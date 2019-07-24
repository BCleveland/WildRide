using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private Transform m_camera;
    [SerializeField] private Setpieces.Setpiece m_setpiece;
    /*
    
        Manage the camera (and the player's) progress through the level
        Activate 'setpiece' events at specific intervals
        Camera spline along the main track
        Player's downhill direction
     */
     private void Update() 
     {
         float levelProgress = m_camera.position.z / 90.0f;
         if(levelProgress > m_setpiece.LevelPercentActivation && !m_setpiece.Activated)
         {
             m_setpiece.ActivateSetpiece();
             m_setpiece.Activated = true;
         }
     }
}
