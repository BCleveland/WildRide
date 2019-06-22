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
        transform.position = m_PlayerTransform.position + m_PlayerOffset;
    }
}
