using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance;
    public static bool IsGameOver{get{return Instance.CurrentState != GameState.InProgress;}}

    public enum GameState{InProgress, Win, Lose}
    public GameState CurrentState{get; private set;}

    [SerializeField] private GameObject m_TESTOBJ = null;

    private void Awake() 
    {
        Instance = this;    
        CurrentState = GameState.InProgress;
    }
    public void GameWin()
    {
        m_TESTOBJ.SetActive(true);
        CurrentState = GameState.Win;
    }
    public void GameLose()
    {
        StartCoroutine(GameoverLoop());
        CurrentState = GameState.Lose;
    }
    private IEnumerator GameoverLoop()
    {
        //do cool gameover stuff
        while(!Input.GetKeyDown(KeyCode.R)) yield return null;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
