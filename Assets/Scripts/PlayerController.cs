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
    [SerializeField] private float m_JumpMinHeight = 1.0f;
    [SerializeField] private float m_JumpMaxHeight = 5.0f;
    [SerializeField] private float m_JumpMinTime = 0.4f;
    [SerializeField] private float m_JumpMaxTime = 0.6f;
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
    [Header("Debug Links")]
    [SerializeField] private Transform m_HalfPipePivot = null;

    private float m_CurrentAngle = 0.0f;
    private float m_PipeAngle = 0.0f;
    private PlayerState m_CurrentState = PlayerState.Base;
    private HalfPipe m_CurrentHalfPipe = null;

    private Vector3 m_GroundedPosition = Vector3.zero;

    private void Update()
    {
        if(m_CurrentState == PlayerState.Base)
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(Jump());
            }
        }
        if(m_CurrentHalfPipe != null)
        {
            if(m_CurrentState == PlayerState.Base)
            {
                Turn();
                HalfpipeGrounded(m_CurrentHalfPipe);
            }
            else if(m_CurrentState == PlayerState.PipeAerial)
                HalfpipeAerial(m_CurrentHalfPipe);
        }
        else
        {
            Turn();
            Move();
        }
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
        float baseRotateAmount = Input.GetAxis("Horizontal") * m_BaseRotationSpeed * Time.deltaTime;;
        float mod = 1.0f;
        if(Mathf.Abs(m_CurrentAngle) < Mathf.Abs(baseRotateAmount + m_CurrentAngle))
        {
            mod = Interpolation.QuinticOut(1-Mathf.Abs(m_CurrentAngle + baseRotateAmount) / m_MaxRotation) * m_BackTurnMod;
        }
        m_CurrentAngle += baseRotateAmount * mod;
        transform.Rotate(0.0f, baseRotateAmount * mod, 0.0f);
    }
	private void HalfpipeGrounded(HalfPipe pipe)
	{
        Vector3 velocity = Quaternion.Euler(0,m_CurrentAngle,0) * Vector3.forward * m_Speed;
		/*Vector3 reletive = pipe.TransformDirToLocal(velocity);
		pipe.AddVelocityToAngle(reletive);
		//remove the x from the reletive velocity, as it has already been used
		reletive.x = 0;
		//back to regular space
		velocity = pipe.TransformDirFromLocal(reletive);*/
        velocity = pipe.ApplyVector(velocity);
		//apply the remaining velocity
		ApplyVelocity(velocity);
		//is this the right order? TODO
		transform.rotation = pipe.GetRotation() * Quaternion.Euler(0, m_CurrentAngle, 0);
		transform.position = m_GroundedPosition + pipe.GetTestPos();

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
		if (transform.position.y <= pipe.m_Pivot.position.y)
        {
            m_CurrentState = PlayerState.Base;
        }
	}
	private void HalfpipeWhileJumping(HalfPipe pipe)
	{
		//Rotate the player based on the angle below them in worldspace
		//if player has landed from the jump, setup the angle properly
		//no velocity applied, as it is done elsewhere
	}
    private void HalfpipeLogic()
    {
        //not in air
        if(m_PipeAngle > -90.0f)
        {
            Turn();
            //get the movement in terms of local space
            Vector3 baseMovement = Quaternion.Euler(0,m_CurrentAngle,0) * Vector3.forward * m_Speed * Time.deltaTime;
            //apply z to global z pos, apply x to rotation of pivot
            //As player attempts to move from -5 to -8, rotate 90
			//3f is x scale
            m_PipeAngle += baseMovement.x * (90.0f/3f);
            m_HalfPipePivot.rotation = Quaternion.Euler(0, 0, m_PipeAngle);
            baseMovement.x = 0;
            transform.position += baseMovement;
        }
        //is in air
        else
        {
            //rotate player's y back towards the ground
            m_CurrentAngle += 120.0f*Time.deltaTime;
            transform.Rotate(0, 120.0f*Time.deltaTime, 0, Space.Self);
            //move forward along local z
            transform.position += transform.forward * m_Speed * Time.deltaTime;
            //if player local x is negative, apply the negative amount to the euler rotation
            if(transform.localPosition.x > 0)
            {
                m_PipeAngle += transform.localPosition.x * Time.deltaTime * 270 * 2;
                m_HalfPipePivot.rotation = Quaternion.Euler(0, 0, m_PipeAngle);
                transform.localPosition = new Vector3(0, transform.localPosition.y, transform.localPosition.z);
            }
        }

        if(transform.position.x > -5)
        {
            transform.parent = null;
            m_CurrentState = PlayerState.Base;
            m_CurrentAngle = transform.localEulerAngles.y;
            transform.rotation = Quaternion.Euler(0, m_CurrentAngle, 0);
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
    private IEnumerator Jump()
    {
        m_CurrentState = PlayerState.Jumping;
        //charge
        float chargeTime = 0.0f;
        while(Input.GetKey(KeyCode.Space))
        {
            chargeTime += Time.deltaTime;
            if(chargeTime > m_JumpMaxChargeTime) chargeTime = m_JumpMaxChargeTime;
            m_Model.localScale = new Vector3(1, Mathf.Lerp(1.0f, m_JumpMaxSquashAmount, Interpolation.CubicOut(chargeTime/m_JumpMaxChargeTime)), 1);
            yield return null;
        }
        m_Model.localScale = Vector3.one;

        //Jump
        float chargePercentage = Interpolation.CubicOut(chargeTime/m_JumpMaxChargeTime);
        float upTime = Mathf.Lerp(m_JumpMinTime, m_JumpMaxTime, chargePercentage);
        float peak = Mathf.Lerp(m_JumpMinHeight, m_JumpMaxHeight, chargePercentage);
        for(float timer = 0.0f; timer < upTime; timer += Time.deltaTime)
        {
            yield return null;
            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(0.0f, peak, Interpolation.QuadraticOut(timer/upTime));
            transform.position = pos;
        }
        for(float timer = upTime; timer > 0.0f; timer -= Time.deltaTime)
        {
            yield return null;
            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(0.0f, peak, Interpolation.QuadraticOut(timer/upTime));
            transform.position = pos;
        }
        Vector3 resetPos = transform.position;
        resetPos.y = 0;
        transform.position = resetPos;
        m_CurrentState = PlayerState.Base;
    }
}
