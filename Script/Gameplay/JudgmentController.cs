using MusicGame.Audio;
using MusicGame.Chart;
using MusicGame.Core;
using MusicGame.Scoring;
using UnityEngine;

namespace MusicGame.Gameplay
{
    [DisallowMultipleComponent]
    public class JudgmentController : MonoBehaviour
    {
        [SerializeField] NoteSpawner noteSpawner;
        [SerializeField] MusicConductor conductor;
        [SerializeField] JudgmentSettings settings;
        [SerializeField] ScoreManager scoreManager;

        static readonly KeyCode[] DefaultLaneKeys =
        {
            KeyCode.D,
            KeyCode.F,
            KeyCode.J,
            KeyCode.K
        };

        SongChart chart;

        public void Configure(NoteSpawner spawner, MusicConductor musicConductor, JudgmentSettings judgment, ScoreManager score)
        {
            if (spawner != null) noteSpawner = spawner;
            if (musicConductor != null) conductor = musicConductor;
            if (judgment != null) settings = judgment;
            if (score != null) scoreManager = score;
        }

        public void Initialize(SongChart songChart)
        {
            chart = songChart;
            if (noteSpawner == null) noteSpawner = GetComponent<NoteSpawner>();
            if (conductor == null) conductor = GetComponentInParent<MusicConductor>();
            if (scoreManager == null) scoreManager = GetComponent<ScoreManager>();
        }

        void Update()
        {
            if (chart == null || noteSpawner == null || conductor == null || !conductor.IsPlaying) return;

            int laneCount = Mathf.Min(chart.laneCount, DefaultLaneKeys.Length);
            for (int lane = 0; lane < laneCount; lane++)
            {
                if (Input.GetKeyDown(DefaultLaneKeys[lane]))
                    TryJudgeLane(lane);
            }
        }

        void TryJudgeLane(int lane)
        {
            if (settings == null)
                settings = ScriptableObject.CreateInstance<JudgmentSettings>();

            Note best = null;
            float bestAbsDelta = float.MaxValue;
            float songTime = conductor.SongTime;

            foreach (var note in noteSpawner.ActiveNotes)
            {
                if (note == null || note.IsResolved || note.Lane != lane) continue;

                float delta = songTime - note.HitTime;
                float abs = Mathf.Abs(delta);
                if (abs <= settings.missWindow && abs < bestAbsDelta)
                {
                    bestAbsDelta = abs;
                    best = note;
                }
            }

            if (best == null) return;

            float hitDelta = songTime - best.HitTime;
            var type = best.Evaluate(hitDelta, settings);
            if (type == JudgmentType.None || type == JudgmentType.Miss) return;

            best.MarkResolved();
            noteSpawner.UnregisterNote(best);
            Destroy(best.gameObject);

            var result = scoreManager != null
                ? scoreManager.ApplyJudgment(type, lane, hitDelta)
                : new JudgmentResult(type, lane, hitDelta, 0, 0);

            MusicGameEvents.RaiseJudgment(result);
        }
    }
}
