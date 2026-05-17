using System.Collections.Generic;
using MusicGame.Audio;
using MusicGame.Chart;
using MusicGame.Core;
using MusicGame.Scoring;
using UnityEngine;

namespace MusicGame.Gameplay
{
    [DisallowMultipleComponent]
    public class NoteSpawner : MonoBehaviour
    {
        [SerializeField] Note notePrefab;
        [SerializeField] LaneConfig laneConfig;
        [SerializeField] JudgmentSettings judgmentSettings;
        [SerializeField] ScoreManager scoreManager;
        [SerializeField] Transform noteRoot;

        readonly List<Note> activeNotes = new();
        SongChart chart;
        MusicConductor conductor;
        int nextNoteIndex;

        public IReadOnlyList<Note> ActiveNotes => activeNotes;

        public void Configure(Note prefab, LaneConfig lanes, JudgmentSettings settings, ScoreManager score)
        {
            if (prefab != null) notePrefab = prefab;
            if (lanes != null) laneConfig = lanes;
            if (settings != null) judgmentSettings = settings;
            if (score != null) scoreManager = score;
        }

        public void Initialize(SongChart songChart, MusicConductor musicConductor)
        {
            chart = songChart;
            conductor = musicConductor;
            if (scoreManager == null)
                scoreManager = GetComponent<ScoreManager>();
            if (judgmentSettings == null)
                judgmentSettings = ScriptableObject.CreateInstance<JudgmentSettings>();
            if (laneConfig == null)
                Debug.LogWarning("[NoteSpawner] 未配置 LaneConfig。");
        }

        public void ResetSpawner()
        {
            ClearNotes();
            nextNoteIndex = 0;
            chart?.SortNotesByTime();
        }

        void Update()
        {
            if (chart == null || conductor == null || !conductor.IsPlaying) return;
            SpawnDueNotes();
            CheckMissedNotes();
        }

        void SpawnDueNotes()
        {
            while (nextNoteIndex < chart.notes.Count)
            {
                var ev = chart.notes[nextNoteIndex];
                float spawnAt = ev.time - chart.approachTime;
                if (conductor.SongTime < spawnAt)
                    break;

                SpawnNote(ev);
                nextNoteIndex++;
            }
        }

        void SpawnNote(NoteEventData ev)
        {
            if (notePrefab == null)
            {
                Debug.LogError("[NoteSpawner] 未指定 Note 预制体。");
                return;
            }

            var parent = noteRoot != null ? noteRoot : transform;
            var note = Instantiate(notePrefab, parent);
            note.Initialize(ev, chart, laneConfig, conductor);
            activeNotes.Add(note);
        }

        void CheckMissedNotes()
        {
            float missAfter = judgmentSettings.missWindow;

            for (int i = activeNotes.Count - 1; i >= 0; i--)
            {
                var note = activeNotes[i];
                if (note == null)
                {
                    activeNotes.RemoveAt(i);
                    continue;
                }

                if (note.IsResolved) continue;

                float delta = conductor.SongTime - note.HitTime;
                if (delta > missAfter)
                {
                    int lane = note.Lane;
                    note.MarkResolved();
                    Destroy(note.gameObject);
                    activeNotes.RemoveAt(i);

                    var result = scoreManager != null
                        ? scoreManager.ApplyJudgment(JudgmentType.Miss, lane, delta)
                        : new JudgmentResult(JudgmentType.Miss, lane, delta, 0, 0);
                    MusicGameEvents.RaiseJudgment(result);
                }
            }
        }

        public void UnregisterNote(Note note) => activeNotes.Remove(note);

        void ClearNotes()
        {
            foreach (var n in activeNotes)
            {
                if (n != null)
                    Destroy(n.gameObject);
            }
            activeNotes.Clear();
        }
    }
}
