using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalfPipe : MonoBehaviour
{
	[SerializeField] private Transform m_Pivot;
	[SerializeField] private float m_XScale = 3.0f;

	private float m_CurrentAngle = 0.0f;

	public void AddVelocityToAngle(Vector3 localVelocity)
	{
		float dist = m_XScale * Mathf.PI / 2f;
		m_CurrentAngle += Mathf.LerpUnclamped(0, 90, localVelocity.x / dist);
	}
	public Vector3 TransformDirToLocal(Vector3 velocity)
	{
		return Quaternion.LookRotation(transform.forward, transform.up) * velocity;
	}
	public Vector3 TransformDirFromLocal(Vector3 localVelocity)
	{
		return Quaternion.Inverse(Quaternion.LookRotation(transform.forward, transform.up))
			* localVelocity;
	}
}
