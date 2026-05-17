using System.Collections.Generic;
using UnityEngine;

namespace MusicGame.Chart
{
    [CreateAssetMenu(fileName = "NewSongChart", menuName = "Music Game/Song Chart")]
    public class SongChart : ScriptableObject
    {
        public string songId = "untitled";
        public string title = "Untitled";
        [Min(1f)] public float bpm = 120f;
        public float offset;
        [Min(0.1f)] public float approachTime = 2f;
        [Min(1)] public int laneCount = 4;
        public AudioClip music;
        public List<NoteEventData> notes = new();

        public float SongLength
        {
            get
            {
                if (notes == null || notes.Count == 0) return 0f;
                float end = 0f;
                foreach (var n in notes)
                {
                    float noteEnd = n.time + (n.type == NoteType.Hold ? n.duration : 0f);
                    if (noteEnd > end) end = noteEnd;
                }
                return end;
            }
        }

        public void SortNotesByTime()
        {
            notes?.Sort((a, b) => a.time.CompareTo(b.time));
        }
    }
}
