using MusicGame.Audio;
using MusicGame.Chart;
using MusicGame.Gameplay;
using UnityEngine;

namespace MusicGame.Core
{
    [DisallowMultipleComponent]
    public class MusicGameManager : MonoBehaviour
    {
        [SerializeField] SongChart chart;
        [SerializeField] MusicConductor conductor;
        [SerializeField] NoteSpawner noteSpawner;
        [SerializeField] JudgmentController judgmentController;
        [SerializeField] bool autoStartOnPlay;

        GameState state = GameState.Idle;

        public GameState State => state;
        public SongChart CurrentChart => chart;

        public void Configure(MusicConductor musicConductor, NoteSpawner spawner, JudgmentController judgment)
        {
            if (musicConductor != null) conductor = musicConductor;
            if (spawner != null) noteSpawner = spawner;
            if (judgment != null) judgmentController = judgment;
        }

        void Awake()
        {
            if (conductor == null) conductor = GetComponentInChildren<MusicConductor>();
            if (noteSpawner == null) noteSpawner = GetComponentInChildren<NoteSpawner>();
            if (judgmentController == null) judgmentController = GetComponentInChildren<JudgmentController>();
        }

        void Start()
        {
            if (chart != null)
                LoadChart(chart);

            if (autoStartOnPlay && chart != null)
                StartSong();
        }

        void Update()
        {
            if (state != GameState.Playing || conductor == null || chart == null)
                return;

            if (conductor.IsFinished)
                FinishSong();
        }

        public void LoadChart(SongChart newChart)
        {
            chart = newChart;
            noteSpawner?.Initialize(chart, conductor);
            judgmentController?.Initialize(chart);
            conductor?.Prepare(chart);
            SetState(GameState.Ready);
            MusicGameEvents.RaiseChartLoaded(chart);
        }

        public void StartSong()
        {
            if (chart == null)
            {
                Debug.LogWarning("[MusicGameManager] 未指定谱面，无法开始。");
                return;
            }

            noteSpawner?.ResetSpawner();
            conductor?.Play();
            SetState(GameState.Playing);
            MusicGameEvents.RaiseSongStarted();
        }

        public void PauseSong()
        {
            if (state != GameState.Playing) return;
            conductor?.Pause();
            SetState(GameState.Paused);
        }

        public void ResumeSong()
        {
            if (state != GameState.Paused) return;
            conductor?.Resume();
            SetState(GameState.Playing);
        }

        public void FinishSong()
        {
            if (state == GameState.Finished) return;
            conductor?.Stop();
            SetState(GameState.Finished);
            MusicGameEvents.RaiseSongFinished();
        }

        void SetState(GameState newState)
        {
            if (state == newState) return;
            state = newState;
            MusicGameEvents.RaiseGameStateChanged(state);
        }
    }
}
