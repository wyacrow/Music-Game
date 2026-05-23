using UnityEngine;

namespace MusicGame.Editor
{
    public static class BeatSnapUtility
    {
        public static float GetBeatDuration(float bpm) => bpm > 0f ? 60f / bpm : 1f;

        public static float GetGridDuration(float bpm, int subdiv)
        {
            if (subdiv <= 0) return 0f;
            return GetBeatDuration(bpm) / subdiv;
        }

        public static float SnapTime(float time, float bpm, float offset, int subdiv)
        {
            if (subdiv <= 0) return time;
            float grid = GetGridDuration(bpm, subdiv);
            if (grid <= 0f) return time;
            return offset + Mathf.Round((time - offset) / grid) * grid;
        }
    }
}
