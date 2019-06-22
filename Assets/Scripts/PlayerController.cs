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
    [Header("Slide Values")]
    [SerializeField] private float m_SlideTransitionTime = 0.2f;
    [SerializeField] private float m_SlideMaxRotationAngle =  -90.0f;
    [SerializeField] private Vector3 m_SlideMaxPosition = Vector3.zero;
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

    private void Update()
    {
        if(m_CurrentState == PlayerState.Halfpipe)
        {
            HalfpipeLogic();
        }
        else
        {
            if(m_CurrentState == PlayerState.Dead) return;

            if(m_CurrentState == PlayerState.Base)
            {
                if(Input.GetKeyDown(KeyCode.Space))
                {
                    StartCoroutine(Jump());
                }
                else if(Input.GetKeyDown(KeyCode.S))
                {
                    StartCoroutine(Slide());
                }
            }
            Turn();
            Move();
            if(transform.position.x < -5)
            {
                transform.parent = m_HalfPipePivot.GetChild(0);
                m_CurrentState = PlayerState.Halfpipe;
                Vector3 temp = transform.localPosition;
                temp.x = 0;
                transform.localPosition = temp;
            }
        }
    }
    private void Move()
    {
        transform.position += transform.forward * m_Speed * Time.deltaTime;
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
	private void Halfpipe2()
	{
		HalfPipe pipe = null;
		Vector3 velocity = Vector3.zero;
		Vector3 reletive = pipe.TransformDirToLocal(velocity);
		pipe.AddVelocityToAngle(reletive);
		//remove the x from the reletive velocity, as it has already been used
		reletive.x = 0;
		//back to regular space
		velocity = pipe.TransformDirFromLocal(reletive);
		//apply the remaining velocity
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
        if(other.gameObject.CompareTag("Hazard") && m_CurrentState != PlayerState.Sliding)
        {
            MassSetActive(m_ToDisableOnDie, false);
            MassSetActive(m_ToEnableOnDie, true);
            m_CurrentState = PlayerState.Dead;
            m_RagdollForcePoint.AddForce(Vector3.one*100, ForceMode.Impulse);
            StopAllCoroutines();
        }    
    }
    private void MassSetActive(GameObject[] objects, bool state)
    {
        for(int j = 0; j < objects.Length; j++)
        {
            objects[j].SetActive(state);
        }
    }
    private IEnumerator Slide()
    {
        m_CurrentState = PlayerState.Sliding;
        float slideTime = 0.0f;
        while(Input.GetKey(KeyCode.S) && slideTime < m_SlideTransitionTime)
        {
            slideTime += Time.deltaTime;
            UpdateSlideRotation(slideTime/m_SlideTransitionTime);
            yield return null;
        }
        if(slideTime >= m_SlideTransitionTime)
        {
            //is in slide
            while(Input.GetKey(KeyCode.S))
            {
                yield return null;
            }
        }
        while(slideTime > 0)
        {
            slideTime -= Time.deltaTime;
            UpdateSlideRotation(slideTime/m_SlideTransitionTime);
            yield return null;
        }
        m_CurrentState = PlayerState.Base;
    }
    private void UpdateSlideRotation(float percent)
    {
        float interpt = Interpolation.SmoothStep(percent);
        m_Model.transform.localPosition = Vector3.Lerp(Vector3.zero, m_SlideMaxPosition, interpt);
        m_Model.localRotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(m_SlideMaxRotationAngle, 0, 0), interpt);
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
