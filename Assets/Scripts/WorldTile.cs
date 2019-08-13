using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTile : MonoBehaviour
{
    [SerializeField] private Transform m_TileStart = null;
    [SerializeField] private Transform m_TileEnd = null;
    
    [SerializeField] public float RotationMod;
    [SerializeField] private Setpieces.Setpiece[] m_Setpieces = null;

    private float m_totalDistance = 0.0f;
    private void Awake() 
    {
        m_totalDistance = (m_TileEnd.position - m_TileStart.position).magnitude;
    }
    public Vector3 GetCameraLerp(float t)
    {
        return Vector3.LerpUnclamped(m_TileStart.position, m_TileEnd.position, t);
    }
    public float GetPlayerPercent(Vector3 playerPos)
    {
        Vector3 total = m_TileEnd.position - m_TileStart.position;
        Vector3 reletivePlayerPos = playerPos - m_TileStart.position;
        Vector3 projected = Vector3.Project(reletivePlayerPos, total);
        float percent = projected.magnitude / m_totalDistance;
        //TODO: Do not do this here dear god
        if(m_Setpieces != null)
        {
            foreach (var set in m_Setpieces)
            {
                if(set.TilePercentActivation < percent && !set.Activated) set.ActivateSetpiece();
            }
        }

        return percent;
    }
    public float GetWorldAngleAtPercent()
    {
        //todo
        return 0.0f;
    }
}
