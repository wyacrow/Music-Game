using System;
using UnityEngine;

namespace MusicGame.Chart
{
    [Serializable]
    public struct NoteEventData
    {
        public int lane;
        public float time;
        public NoteType type;
        public float duration;

        public NoteEventData(int lane, float time, NoteType type = NoteType.Tap, float duration = 0f)
        {
            this.lane = lane;
            this.time = time;
            this.type = type;
            this.duration = duration;
        }
    }
}
