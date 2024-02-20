using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public event Action<int> ScoreChanged = delegate(int i) {  };
    public event Action<int> HighScoreChanged = delegate(int i) {  };
    
    public int Score { get; private set; }
    public int HighScore { get; private set; }

    public void ResetScore()
    {
        Score = 0;
        ScoreChanged(Score);
    }

    public void AddPoints(int points)
    {
        Score += points;
        ScoreChanged(Score);
        if (Score <= HighScore) return;
        HighScore = Score;
        HighScoreChanged(HighScore);
    }
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}
