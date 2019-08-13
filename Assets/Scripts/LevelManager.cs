using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    [SerializeField] private CameraController m_Camera;
    [SerializeField] private PlayerController m_Player;
    [SerializeField] private WorldTile[] m_WorldTiles;

    public WorldTile CurrentTile{get{return m_WorldTiles[m_CurrentTile];}}
    public float WorldAngle { get; private set; }
    public bool InTransition = false;

    private int m_CurrentTile = 0;
    /*
    
        Manage the camera (and the player's) progress through the level
        Activate 'setpiece' events at specific intervals
        Camera spline along the main track
        Player's downhill direction
     */
     private void Awake() 
     {
         Instance = this;
     }
     private void Update() 
     {
        if(GameController.IsGameOver) return;
        if(m_WorldTiles[m_CurrentTile].GetPlayerPercent(m_Player.transform.position) > 1)
        {
            if(m_CurrentTile == m_WorldTiles.Length-1)
            {
                GameController.Instance.GameWin();
            }
            else
            {
                StartCoroutine(LerpBetweenTiles(m_WorldTiles[m_CurrentTile], m_WorldTiles[m_CurrentTile+1]));
                m_CurrentTile++;
            }
        }
         if(!InTransition)
         {
             m_Camera.transform.position = m_Camera.GetPositionOnSection(CurrentTile);
         }
     }
     private IEnumerator LerpBetweenTiles(WorldTile a, WorldTile b)
     {
         InTransition = true;
         float lerpTime = 1.4f;
         for(float t = 0.0f; t < lerpTime; t+=Time.deltaTime)
         {
             float delta = Interpolation.SmoothStep(t/lerpTime);
             WorldAngle = Mathf.Lerp(a.RotationMod, b.RotationMod, delta);
             m_Camera.transform.position = Vector3.Lerp(m_Camera.GetPositionOnSection(a), m_Camera.GetPositionOnSection(b), delta);
             yield return null;
         }
         WorldAngle = b.RotationMod;
         m_Camera.transform.position = m_Camera.GetPositionOnSection(b);
         InTransition = false;
     }
}
