using MusicGame.Core;
using UnityEngine;

namespace MusicGame.Scoring
{
    [DisallowMultipleComponent]
    public class ScoreManager : MonoBehaviour
    {
        [SerializeField] int perfectScore = 300;
        [SerializeField] int goodScore = 100;

        int score;
        int combo;
        int maxCombo;
        int perfectCount;
        int goodCount;
        int missCount;

        public int Score => score;
        public int Combo => combo;
        public int MaxCombo => maxCombo;

        void OnEnable()
        {
            MusicGameEvents.OnSongStarted += ResetStats;
        }

        void OnDisable()
        {
            MusicGameEvents.OnSongStarted -= ResetStats;
        }

        public void ResetStats()
        {
            score = 0;
            combo = 0;
            maxCombo = 0;
            perfectCount = 0;
            goodCount = 0;
            missCount = 0;
        }

        public JudgmentResult ApplyJudgment(JudgmentType type, int lane, float deltaTime)
        {
            int added = 0;

            switch (type)
            {
                case JudgmentType.Perfect:
                    combo++;
                    added = perfectScore + combo;
                    perfectCount++;
                    break;
                case JudgmentType.Good:
                    combo++;
                    added = goodScore + combo / 2;
                    goodCount++;
                    break;
                case JudgmentType.Miss:
                    combo = 0;
                    missCount++;
                    break;
            }

            score += added;
            if (combo > maxCombo) maxCombo = combo;

            return new JudgmentResult(type, lane, deltaTime, added, combo);
        }

        public float GetAccuracy()
        {
            int total = perfectCount + goodCount + missCount;
            if (total == 0) return 1f;
            return (perfectCount + goodCount * 0.5f) / total;
        }
    }
}
