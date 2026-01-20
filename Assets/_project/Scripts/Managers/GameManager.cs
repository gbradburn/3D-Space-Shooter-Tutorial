using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public event Action<GameState> GameStateChanged = delegate(GameState state) {  };

    public GameState GameState { get; private set; }
    
    bool ShouldQuitGame => Keyboard.current != null && Keyboard.current.escapeKey.wasReleasedThisFrame;
    
    void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    void SetGameState(GameState gameState)
    {
        if (gameState == GameState) return;
        GameState = gameState;
        GameStateChanged(gameState);
    }

    void OnEnable()
    {
        SetGameState(GameState.Patrol);
        MusicManager.Instance.PlayPatrolMusic();
    }

    void Update()
    {
        if (ShouldQuitGame)
        {
            QuitGame();
        }

        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
        {
            Time.timeScale = 0f;
        }

        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
        {
            Cursor.visible = !Cursor.visible;
            Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Confined;
        }
    }

    public void InCombat(bool inCombat)
    {
        if (GameState == GameState.Combat) return;
        if (inCombat)
        {
            MusicManager.Instance.PlayCombatMusic();
            SetGameState(GameState.Combat); 
            return;
        }

        MusicManager.Instance.PlayPatrolMusic();
    }

    public void PlayerWon()
    {
        MusicManager.Instance.PlayGameOverMusic();
        SetGameState(GameState.GameOver);    
    }
    
    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // todo handle WebGL
        Application.Quit();
#endif
    }
}