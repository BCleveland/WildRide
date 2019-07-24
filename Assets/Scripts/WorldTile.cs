using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTile : MonoBehaviour
{
    [SerializeField] private Transform m_tileStart = null;
    [SerializeField] private Transform m_tileEnd = null;
    
    public float RotationMod { get{return transform.eulerAngles.y;} }
}
