using System;
using MusicGame.Chart;
using MusicGame.Scoring;

namespace MusicGame.Core
{
    public static class MusicGameEvents
    {
        public static event Action<GameState> OnGameStateChanged;
        public static event Action<SongChart> OnChartLoaded;
        public static event Action OnSongStarted;
        public static event Action OnSongFinished;
        public static event Action<JudgmentResult> OnJudgment;

        public static void RaiseGameStateChanged(GameState state) => OnGameStateChanged?.Invoke(state);
        public static void RaiseChartLoaded(SongChart chart) => OnChartLoaded?.Invoke(chart);
        public static void RaiseSongStarted() => OnSongStarted?.Invoke();
        public static void RaiseSongFinished() => OnSongFinished?.Invoke();
        public static void RaiseJudgment(JudgmentResult result) => OnJudgment?.Invoke(result);
    }
}
