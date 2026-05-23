using MusicGame.Core;
using MusicGame.Scoring;
using TMPro;
using UnityEngine;

namespace MusicGame.UI
{
    [DisallowMultipleComponent]
    public class GameplayHUD : MonoBehaviour
    {
        [SerializeField] ScoreManager scoreManager;
        [SerializeField] TextMeshProUGUI scoreText;
        [SerializeField] TextMeshProUGUI comboText;
        [SerializeField] TextMeshProUGUI judgmentText;

        void Awake() => EnsureScoreManager();

        void Start() => EnsureScoreManager();

        void OnEnable()
        {
            MusicGameEvents.OnJudgment += OnJudgment;
            MusicGameEvents.OnSongStarted += OnSongStarted;
        }

        void OnDisable()
        {
            MusicGameEvents.OnJudgment -= OnJudgment;
            MusicGameEvents.OnSongStarted -= OnSongStarted;
        }

        void OnSongStarted()
        {
            EnsureScoreManager();
            RefreshAll();
        }

        void EnsureScoreManager()
        {
            if (scoreManager == null)
                scoreManager = FindObjectOfType<ScoreManager>();
        }

        void OnJudgment(JudgmentResult result)
        {
            RefreshAll();
            if (judgmentText != null)
                judgmentText.text = result.Type.ToString();
        }

        void RefreshAll()
        {
            EnsureScoreManager();
            if (scoreManager == null) return;
            if (scoreText != null)
                scoreText.text = scoreManager.Score.ToString();
            if (comboText != null)
                comboText.text = scoreManager.Combo > 0 ? $"{scoreManager.Combo}" : "";
        }
    }
}
