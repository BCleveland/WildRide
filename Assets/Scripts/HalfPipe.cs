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
		m_CurrentAngle += Mathf.LerpUnclamped(0, 90, localVelocity.x*Time.deltaTime / dist);
	}
	public Vector3 TransformDirToLocal(Vector3 velocity)
	{
		//TODO:this doesn't really work at other angles. X can be negative when it shouldnt?
		//Look into vector projection
		return Quaternion.LookRotation(transform.forward, transform.up) * velocity;
	}
	public Vector3 TransformDirFromLocal(Vector3 localVelocity)
	{
		return Quaternion.Inverse(Quaternion.LookRotation(transform.forward, transform.up))
			* localVelocity;
	}
	public float GetAerialRotationDirection()
	{
		return m_IsLeftFacing ? 1.0f : -1.0f;
	}
	public Quaternion GetRotation()
	{
		return Quaternion.Euler(0,0,m_CurrentAngle * (m_IsLeftFacing ? -1 : 1));
	}
	public Vector3 GetTestPos()
	{
		return transform.rotation * (Vector3.up*m_XScale + Quaternion.Euler(0,0,m_CurrentAngle) * Vector3.down*m_XScale);
	}
}
