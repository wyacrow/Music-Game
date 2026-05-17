namespace MusicGame.Scoring
{
    public readonly struct JudgmentResult
    {
        public JudgmentType Type { get; }
        public int Lane { get; }
        public float DeltaTime { get; }
        public int ScoreAdded { get; }
        public int Combo { get; }

        public JudgmentResult(JudgmentType type, int lane, float deltaTime, int scoreAdded, int combo)
        {
            Type = type;
            Lane = lane;
            DeltaTime = deltaTime;
            ScoreAdded = scoreAdded;
            Combo = combo;
        }
    }
}
