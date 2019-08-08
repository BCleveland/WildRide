using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Base Movement")]
    [SerializeField] private float m_Speed = 5f;
    [SerializeField] private float m_BaseRotationSpeed = 90f;
    [SerializeField] private float m_MaxRotation = 90f;
    [SerializeField] private float m_BackTurnMod = 1.0f;
    [Header("Jump Values")]
    [SerializeField] private float m_JumpMinForce = 1.0f;
    [SerializeField] private float m_JumpMaxForce = 5.0f;
    [SerializeField] private float m_Gravity = 9.8f;
    [SerializeField] private float m_JumpMaxChargeTime = 3.0f;
    [SerializeField] private float m_JumpMaxSquashAmount = 0.6f;
	[Header("HalfPipe Values")]
	[Tooltip("The amount to rotate while in the air, in degrees/sec")]
	[SerializeField] private float m_AerialRotationalGravity = 90.0f;
    [Header("Components")]
    [SerializeField] private Transform m_Model = null;
    [SerializeField] private Rigidbody m_RagdollForcePoint = null;
    [SerializeField] private GameObject[] m_ToDisableOnDie = null;
    [SerializeField] private GameObject[] m_ToEnableOnDie = null;

    System.Action[,] m_StateMachine = null;
    private PlayerState m_CurrentState = PlayerState.Base;
    private Vector3 m_GroundedPosition = Vector3.zero;
    private float m_CurrentAngle = 0.0f;
    private Vector3 m_AirVelocity = Vector3.zero;
    private HalfPipe m_CurrentHalfPipe = null;

    private void Awake() 
    {
        SetupStateMachine();   
    }
    private void Update()
    {
        if(m_CurrentState == PlayerState.Base)
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(JumpCharge());
            }
        }
        m_StateMachine[(int)m_CurrentState, (m_CurrentHalfPipe) == null ? 0 : 1]?.Invoke();
    }
    private void ApplyVelocity(Vector3 velocity)
    {
        //Applies velocity to *grounded* pos
        m_GroundedPosition += velocity * Time.deltaTime;
        transform.position = m_GroundedPosition;
    }
    private void Move()
    {
        ApplyVelocity(transform.forward * m_Speed);
    }
    private void Turn()
    {
        float worldForwardDir = LevelManager.Instance.WorldAngle;
        float forwardModAngle = m_CurrentAngle - worldForwardDir;

        float baseRotateAmount = Input.GetAxis("Horizontal") * m_BaseRotationSpeed * Time.deltaTime;;
        float mod = 1.0f;
        if(Mathf.Abs(forwardModAngle) < Mathf.Abs(forwardModAngle + baseRotateAmount))
        {
            mod = Interpolation.QuinticOut(1-Mathf.Abs(forwardModAngle + baseRotateAmount) / m_MaxRotation) * m_BackTurnMod;
        }
        m_CurrentAngle += baseRotateAmount * mod;
        transform.Rotate(0.0f, baseRotateAmount * mod, 0.0f);
    }
    private void MoveAir()
    {
        //apply the current airial velocity
        Vector3 velocity = m_AirVelocity;
        //apply gravity to the velocity for next frame
        m_AirVelocity += Vector3.down * m_Gravity * Time.deltaTime;
        //check for landing
        float desiredY = transform.position.y + (velocity.y * Time.deltaTime);
        velocity.y = 0;
        ApplyVelocity(velocity);
        Vector3 pos = transform.position;
        pos.y = desiredY;
        if(pos.y < 0)
        {
            pos.y = 0;
            m_CurrentState = PlayerState.Base;
        }
        transform.position = pos;
    }
    //Air movement while above a half pipe (but not doing an aerial)
    private void MoveAirHalfpipe()
    {
        //apply the current airial velocity
        Vector3 pos = transform.position;
        pos += m_AirVelocity * Time.deltaTime;

        m_CurrentHalfPipe.SetAngleToAirbornePos(pos);
        //Apply to the grounded position
        m_GroundedPosition += m_CurrentHalfPipe.ProjectAlongForward(m_AirVelocity) * Time.deltaTime;

        //rotate
		transform.rotation = m_CurrentHalfPipe.GetRotation() * Quaternion.Euler(0, m_CurrentAngle, 0);
        //apply gravity to the velocity for next frame
        m_AirVelocity += Vector3.down * m_Gravity * Time.deltaTime;
        //check for landing on halfpipe
        float landingPos = m_CurrentHalfPipe.GetLandingHeight(pos);
        if(pos.y < landingPos)
        {
            pos.y = landingPos;
            m_CurrentState = PlayerState.Base;
        }
        if(m_CurrentHalfPipe.CrossedGroundThreshhold)
        {
            m_CurrentHalfPipe = null;
        }
        transform.position = pos;
    }
	private void HalfpipeGrounded(HalfPipe pipe)
	{
        Vector3 velocity = Quaternion.Euler(0,m_CurrentAngle,0) * Vector3.forward * m_Speed;
        velocity = pipe.ApplyVector(velocity);
		//apply the remaining velocity
		ApplyVelocity(velocity);
		transform.rotation = pipe.GetRotation() * Quaternion.Euler(0, m_CurrentAngle, 0);
		transform.position = m_GroundedPosition + pipe.GetAnglePosition();

        if(pipe.CrossedJumpThreshhold)
        {
            pipe.SetAngle(90.0f);
            m_CurrentState = PlayerState.PipeAerial;
        }
        else if(pipe.CrossedGroundThreshhold)
        {
            pipe.SetAngle(0.0f);
            m_CurrentHalfPipe = null;
            transform.rotation = Quaternion.Euler(0,m_CurrentAngle,0);
            transform.position = m_GroundedPosition;
        }
	}
    //doing an aerial (sideways jump)
	private void HalfpipeAerial(HalfPipe pipe)
	{
		//rotate player towards gravity
		m_CurrentAngle += m_AerialRotationalGravity * Time.deltaTime * pipe.GetAerialRotationDirection();
		//generate velocity(nothing else can modify this)
		Vector3 velocity = transform.forward * m_Speed;
		//apply full movement (not ramp local)
        //manually applied to ignore grounded pos
		transform.position += velocity * Time.deltaTime;
        velocity.y = 0f;
        m_GroundedPosition += velocity * Time.deltaTime;
		transform.rotation = pipe.GetRotation() * Quaternion.Euler(0, m_CurrentAngle, 0);
		//check if player is now back on the ramp
		if (transform.position.y <= /*pipe.m_Pivot.position.y*/3)
        {
            m_CurrentState = PlayerState.Base;
        }
	}

    public void OnNoGround()
    {
        if(m_CurrentState == PlayerState.Base)
        {
            MassSetActive(m_ToDisableOnDie, false);
            MassSetActive(m_ToEnableOnDie, true);
            m_CurrentState = PlayerState.Dead;
            m_RagdollForcePoint.AddForce(transform.forward*100, ForceMode.Impulse);
            StopAllCoroutines();
        }
    }
    private void OnCollisionEnter(Collision other) 
    {
        if(other.gameObject.CompareTag("Hazard"))
        {
            MassSetActive(m_ToDisableOnDie, false);
            MassSetActive(m_ToEnableOnDie, true);
            m_CurrentState = PlayerState.Dead;
            m_RagdollForcePoint.AddForce(Vector3.one*100, ForceMode.Impulse);
            StopAllCoroutines();
        }    
    }
    private void OnTriggerEnter(Collider other) 
    {
        if(other.gameObject.CompareTag("Halfpipe"))
        {
            m_CurrentHalfPipe = other.GetComponent<HalfPipe>();
        }    
    }
    private void MassSetActive(GameObject[] objects, bool state)
    {
        for(int j = 0; j < objects.Length; j++)
        {
            objects[j].SetActive(state);
        }
    }
    private IEnumerator JumpCharge()
    {
        float chargeTime = 0.0f;
        while(Input.GetKey(KeyCode.Space))
        {
            chargeTime += Time.deltaTime;
            if(chargeTime > m_JumpMaxChargeTime) chargeTime = m_JumpMaxChargeTime;
            m_Model.localScale = new Vector3(1, Mathf.Lerp(1.0f, m_JumpMaxSquashAmount, Interpolation.CubicOut(chargeTime/m_JumpMaxChargeTime)), 1);
            yield return null;
        }
        m_Model.localScale = Vector3.one;

        float chargePercentage = Interpolation.CubicOut(chargeTime/m_JumpMaxChargeTime);
        float force = Mathf.Lerp(m_JumpMinForce, m_JumpMaxForce, chargePercentage);
        //calculate velocity from force
        Vector3 velocity = transform.up * force;
        velocity += transform.forward * m_Speed;
        m_CurrentState = PlayerState.Airborne;
        m_AirVelocity = velocity;
    }
    private void SetupStateMachine()
    {
        m_StateMachine = new System.Action[4,2];

        m_StateMachine[(int)PlayerState.Base,0] = () => {Turn(); Move();};
        m_StateMachine[(int)PlayerState.Airborne, 0] = () => {Turn(); MoveAir();};

        m_StateMachine[(int)PlayerState.Base, 1] = () => {Turn(); HalfpipeGrounded(m_CurrentHalfPipe);};
        m_StateMachine[(int)PlayerState.Airborne, 1] = () => {Turn(); MoveAirHalfpipe();};
        m_StateMachine[(int)PlayerState.PipeAerial, 1] = () => {HalfpipeAerial(m_CurrentHalfPipe);};
    }

}
