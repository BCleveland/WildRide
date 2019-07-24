using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform m_PlayerTransform = null;

    private Vector3 m_PlayerOffset;
    private void Awake() 
    {
        m_PlayerOffset = transform.position - m_PlayerTransform.position;
    }
    void Update()
    {
        Vector3 playerPos = m_PlayerTransform.position;
        playerPos.y = 0;
        Vector3 targetPos = playerPos + m_PlayerOffset;
        targetPos.x *= 0.6f;
        transform.position = targetPos;
    }
}
