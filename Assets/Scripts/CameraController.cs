using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform m_PlayerTransform = null;

    private Vector3 m_PlayerOffset;
    private float m_XRot = 22.76f;
    private void Awake() 
    {
        m_PlayerOffset = transform.position - m_PlayerTransform.position;
    }
    void Update()
    {
        transform.rotation = Quaternion.Euler(m_XRot, LevelManager.Instance.WorldAngle, 0);
    }
    public Vector3 GetPositionOnSection(WorldTile tile)
    {
        float tilePercent = tile.GetPlayerPercent(m_PlayerTransform.position);
        Vector3 playerOnLine = tile.GetCameraLerp(tilePercent);
        Vector3 lineToPlayer = m_PlayerTransform.position - playerOnLine;
        Vector3 cameraSidewaysOffset = lineToPlayer * 0.6f;
        cameraSidewaysOffset.y = 0;
        Vector3 backwardsAmount = (-tile.transform.forward * 5.0f);
        return playerOnLine + cameraSidewaysOffset + backwardsAmount;
    }
}
