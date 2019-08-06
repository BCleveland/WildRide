using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalfPipe : MonoBehaviour
{
	[SerializeField] private float m_XScale = 3.0f;
	[SerializeField] private float m_Rotation = 0.0f;
	[SerializeField] private bool m_IsLeftFacing = false;

	public bool CrossedJumpThreshhold{get{return m_CurrentAngle >= 90.0f;}}
	public bool CrossedGroundThreshhold{get{return m_CurrentAngle <= 0.0f;}}
	public float DirMod{get{return m_IsLeftFacing ? -1.0f : 1.0f;}}

	private float m_CurrentAngle = 0.0f;

	//Manually override the current angle
	public void SetAngle(float angle)
	{
		m_CurrentAngle = angle;
	}
	//Applies the given velocity, in local space, to the pipes current angle
	public void AddVelocityToAngle(Vector3 localVelocity)
	{
		float dist = m_XScale * Mathf.PI / 2f;
		localVelocity = Quaternion.Euler(0, -m_Rotation, 0) * localVelocity;
		m_CurrentAngle += Mathf.LerpUnclamped(0, 90, localVelocity.x*Time.deltaTime / dist);
	}
	//Consumes the needed part of the velocity and returns the unused portion 
	public Vector3 ApplyVector(Vector3 velocity)
	{
		AddVelocityToAngle(Vector3.Project(velocity, transform.right * DirMod));
		return ProjectAlongForward(velocity);
	}
	//Get the forward direction of the halfpipe, taking into account the leftmod
	public Vector3 ProjectAlongForward(Vector3 velocity)
	{
		return Vector3.Project(velocity, transform.forward * DirMod);
	}
	//1 if left facing, -1 if right facing
	public float GetAerialRotationDirection()
	{
		return -DirMod;
	}
	//A quaternion that rotates along the pipe without changing forward direction
	public Quaternion GetRotation()
	{
		float yRot = transform.eulerAngles.y;
		return
			  Quaternion.Euler(0, m_Rotation, 0)
			* Quaternion.Euler(0, 0, m_CurrentAngle)
			* Quaternion.Euler(0, -m_Rotation, 0);
	}
	//Get the position up the ramp at the current angle
	public Vector3 GetAnglePosition()
	{
		return (Vector3.up*m_XScale + GetRotation() * Vector3.down*m_XScale);
	}
	//Set the angle of the pipe to below the player when they are airborne
	//While airborne, the circular movement mod does not apply
	public void SetAngleToAirbornePos(Vector3 playerPos)
	{
		m_CurrentAngle = Mathf.Lerp(0, 90, Interpolation.CircularIn(GetAirborneDelta(playerPos)));
	}
	//Get the height below the given vector
	public float GetLandingHeight(Vector3 playerPos)
	{
		return Interpolation.CircularIn(GetAirborneDelta(playerPos)) * m_XScale;
	}
	//returns a percent of the players perpendicular progress over the ramp
	private float GetAirborneDelta(Vector3 pos)
	{
		Vector3 reletivePos = pos - transform.position;
		//Find pos in local x space
		//where local x (0, width]
		Vector3 projected = Vector3.Project(reletivePos, transform.right);
		return Mathf.Clamp01(DirMod * projected.x / m_XScale);
	}
}
