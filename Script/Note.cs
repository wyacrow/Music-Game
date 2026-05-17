using MusicGame.Audio;
using MusicGame.Chart;
using MusicGame.Scoring;
using UnityEngine;

namespace MusicGame.Gameplay
{
    [DisallowMultipleComponent]
    public class Note : MonoBehaviour
    {
        NoteEventData data;
        SongChart chart;
        LaneConfig laneConfig;
        MusicConductor conductor;
        bool resolved;

        public int Lane => data.lane;
        public float HitTime => data.time;
        public NoteType Type => data.type;
        public bool IsResolved => resolved;

        public void Initialize(NoteEventData noteData, SongChart songChart, LaneConfig lanes, MusicConductor musicConductor)
        {
            data = noteData;
            chart = songChart;
            laneConfig = lanes;
            conductor = musicConductor;
            resolved = false;
            UpdatePosition();
        }

        void Update()
        {
            if (resolved || chart == null || conductor == null) return;
            UpdatePosition();
        }

        void UpdatePosition()
        {
            float timeUntilHit = data.time - conductor.SongTime;
            float t = 1f - Mathf.Clamp01(timeUntilHit / chart.approachTime);
            float x = laneConfig.GetLaneX(data.lane, chart.laneCount);
            float y = Mathf.Lerp(laneConfig.spawnY, laneConfig.judgeY, t);
            transform.position = new Vector3(x, y, 0f);
        }

        public JudgmentType Evaluate(float delta, JudgmentSettings settings)
        {
            float abs = Mathf.Abs(delta);
            if (abs <= settings.perfectWindow) return JudgmentType.Perfect;
            if (abs <= settings.goodWindow) return JudgmentType.Good;
            if (abs <= settings.missWindow) return JudgmentType.Miss;
            return JudgmentType.None;
        }

        public void MarkResolved() => resolved = true;

        public void ResolveAsMiss()
        {
            if (resolved) return;
            resolved = true;
            Destroy(gameObject);
        }
    }
}
