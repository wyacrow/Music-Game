using System;
using System.Collections.Generic;
using UnityEngine;

namespace MusicGame.Chart
{
    [Serializable]
    class ChartJsonRoot
    {
        public string songId;
        public string title;
        public float bpm = 120f;
        public float offset;
        public float approachTime = 2f;
        public int laneCount = 4;
        public List<NoteEventData> notes = new();
    }

    public static class ChartJsonLoader
    {
        public static SongChart LoadFromText(string json, AudioClip music = null)
        {
            var root = JsonUtility.FromJson<ChartJsonRoot>(json);
            if (root == null)
            {
                Debug.LogError("[ChartJsonLoader] JSON 解析失败。");
                return null;
            }

            var chart = ScriptableObject.CreateInstance<SongChart>();
            chart.songId = root.songId;
            chart.title = root.title;
            chart.bpm = root.bpm;
            chart.offset = root.offset;
            chart.approachTime = root.approachTime;
            chart.laneCount = root.laneCount;
            chart.music = music;
            chart.notes = root.notes ?? new List<NoteEventData>();
            chart.SortNotesByTime();
            return chart;
        }

        public static SongChart LoadFromResource(string resourcePath, AudioClip music = null)
        {
            var textAsset = Resources.Load<TextAsset>(resourcePath);
            if (textAsset == null)
            {
                Debug.LogError($"[ChartJsonLoader] 找不到 Resources/{resourcePath}");
                return null;
            }
            return LoadFromText(textAsset.text, music);
        }
    }
}
