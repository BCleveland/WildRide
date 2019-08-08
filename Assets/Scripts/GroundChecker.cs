using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [SerializeField] private PlayerController m_PlayerController;
    private List<Collider> m_TouchingFloorTiles = new List<Collider>();
    private void Update() 
    {
        if(m_TouchingFloorTiles.Count == 0)
        {
            m_PlayerController.OnNoGround();
        }
    }
    private void OnTriggerEnter(Collider other) 
    {
        m_TouchingFloorTiles.Add(other);
    }
    private void OnTriggerExit(Collider other) 
    {
        m_TouchingFloorTiles.Remove(other);
    }
}
