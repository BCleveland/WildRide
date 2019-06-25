using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalfPipe : MonoBehaviour
{
	[SerializeField] public Transform m_Pivot;
	[SerializeField] private float m_XScale = 3.0f;
	[SerializeField] private bool m_IsLeftFacing = false;

	public bool CrossedJumpThreshhold{get{return m_CurrentAngle >= 90.0f;}}
	public bool CrossedGroundThreshhold{get{return m_CurrentAngle <= 0.0f;}}
	private float m_CurrentAngle = 0.0f;
	public void SetAngle(float angle)
	{
		m_CurrentAngle = angle;
	}
	public void AddVelocityToAngle(Vector3 localVelocity)
	{
		float dist = m_XScale * Mathf.PI / 2f;
		if(m_IsLeftFacing) 
		{
			localVelocity *= -1f;
		}
		m_CurrentAngle += Mathf.LerpUnclamped(0, 90, localVelocity.x*Time.deltaTime / dist);
	}
	public Vector3 ApplyVector(Vector3 velocity)
	{
		float dirMod = m_IsLeftFacing ? -1.0f : 1.0f;
		AddVelocityToAngle(Vector3.Project(velocity, transform.right * dirMod));
		return Vector3.Project(velocity, transform.forward * dirMod);
	}
	public float GetAerialRotationDirection()
	{
		return m_IsLeftFacing ? 1.0f : -1.0f;
	}
	public Quaternion GetRotation()
	{
		float yRot = transform.eulerAngles.y;
		if (m_IsLeftFacing) yRot += 180;
		return
			  Quaternion.Euler(0, yRot, 0)
			* Quaternion.Euler(0, 0, m_CurrentAngle * (m_IsLeftFacing ? -1 : 1))
			* Quaternion.Inverse(Quaternion.Euler(0, -yRot, 0));
	}
	public Vector3 GetTestPos()
	{
		return (Vector3.up*m_XScale + Quaternion.Euler(0,0,m_CurrentAngle * (m_IsLeftFacing ? -1 : 1)) * Vector3.down*m_XScale);
	}
}
