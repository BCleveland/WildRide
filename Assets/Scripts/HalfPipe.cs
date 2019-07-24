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
		return ProjectAlongForward(velocity);
	}
	public Vector3 ProjectAlongForward(Vector3 velocity)
	{
		float dirMod = m_IsLeftFacing ? -1.0f : 1.0f;
		return Vector3.Project(velocity, transform.forward * dirMod);
	}
	public float GetAerialRotationDirection()
	{
		return m_IsLeftFacing ? 1.0f : -1.0f;
	}
	public Quaternion GetRotation()
	{
		float yRot = transform.eulerAngles.y;
		return
			  Quaternion.Euler(0, yRot, 0)
			* Quaternion.Euler(0, 0, m_CurrentAngle)
			* Quaternion.Euler(0, -yRot, 0);
	}
	public Vector3 GetTestPos()
	{
		return (Vector3.up*m_XScale + GetRotation() * Vector3.down*m_XScale);
	}
	public void SetAngleToAirbornePos(Vector3 playerPos)
	{
		m_CurrentAngle = Mathf.Lerp(0, 90, Interpolation.CircularIn(GetAirborneDelta(playerPos)));
	}
	public Quaternion GetAirborneRotation(Vector3 playerPos)
	{
		float yRot = transform.eulerAngles.y;
		float desiredAngle = Mathf.Lerp(0, 90, Interpolation.CircularIn(GetAirborneDelta(playerPos)));
		return
			  Quaternion.Euler(0, yRot, 0)
			* Quaternion.Euler(0, 0, desiredAngle)
			* Quaternion.Euler(0, -yRot, 0);
	}
	public float GetLandingHeight(Vector3 playerPos)
	{
		return Interpolation.CircularIn(GetAirborneDelta(playerPos)) * m_XScale;
	}
	private float GetAirborneDelta(Vector3 pos)
	{
		//get reletive position
		Vector3 reletivePos = pos - transform.position;
		//Find pos in local x space
		//where local x (0, 3]
		Vector3 projected = Vector3.Project(reletivePos, transform.right);
		return Mathf.Clamp01((m_IsLeftFacing ? -1.0f : 1.0f) * projected.x / m_XScale);
	}
}
