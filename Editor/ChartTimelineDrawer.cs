using MusicGame.Chart;
using UnityEditor;
using UnityEngine;

namespace MusicGame.Editor
{
    public static class ChartTimelineDrawer
    {
        static readonly Color NoteColor = new Color(0.2f, 0.75f, 1f, 0.9f);
        static readonly Color SelectedColor = new Color(1f, 0.85f, 0.2f, 1f);
        static readonly Color LaneLineColor = new Color(1f, 1f, 1f, 0.08f);
        static readonly Color RulerColor = new Color(0.25f, 0.25f, 0.28f);
        static readonly Color PlayheadColor = new Color(1f, 0.35f, 0.35f, 0.95f);

        public static void Draw(ChartTimelineLayout layout, SongChart chart, ChartEditorState state)
        {
            if (chart == null) return;

            EditorGUI.DrawRect(layout.rect, new Color(0.12f, 0.12f, 0.14f));

            var rulerRect = new Rect(layout.rect.x, layout.rect.y, layout.rect.width, layout.rulerHeight);
            EditorGUI.DrawRect(rulerRect, RulerColor);
            DrawRulerLabels(layout);

            for (int lane = 0; lane < layout.laneCount; lane++)
            {
                var laneRect = layout.GetLaneRect(lane);
                if (lane % 2 == 0)
                    EditorGUI.DrawRect(laneRect, new Color(1f, 1f, 1f, 0.02f));
                EditorGUI.DrawRect(new Rect(laneRect.x, laneRect.yMax - 1f, laneRect.width, 1f), LaneLineColor);
            }

            DrawBeatGrid(layout, chart, state.snapSubdiv);

            float viewStart = layout.scrollTime;
            float viewEnd = layout.scrollTime + layout.secondsVisible;

            if (chart.notes != null)
            {
                for (int i = 0; i < chart.notes.Count; i++)
                {
                    var n = chart.notes[i];
                    if (n.time < viewStart - 0.5f || n.time > viewEnd + 0.5f) continue;
                    if (n.lane < 0 || n.lane >= layout.laneCount) continue;

                    var noteRect = layout.GetNoteRect(n.time, n.lane);
                    var color = i == state.selectedNoteIndex ? SelectedColor : NoteColor;
                    EditorGUI.DrawRect(noteRect, color);
                }
            }

            float playX = layout.TimeToX(state.playheadTime);
            EditorGUI.DrawRect(new Rect(playX - 1f, layout.rect.y, 2f, layout.rect.height), PlayheadColor);
            var handleTri = new Rect(playX - 6f, layout.rect.y + 2f, 12f, layout.rulerHeight - 4f);
            EditorGUI.DrawRect(handleTri, new Color(1f, 0.45f, 0.45f, 0.9f));
        }

        static void DrawRulerLabels(ChartTimelineLayout layout)
        {
            float step = layout.secondsVisible > 30f ? 5f : layout.secondsVisible > 15f ? 2f : 1f;
            float t0 = Mathf.Floor(layout.scrollTime / step) * step;
            for (float t = t0; t <= layout.scrollTime + layout.secondsVisible; t += step)
            {
                float x = layout.TimeToX(t);
                if (x < layout.rect.x || x > layout.rect.xMax) continue;
                var label = $"{t:0}s";
                GUI.Label(new Rect(x + 2f, layout.rect.y + 2f, 48f, layout.rulerHeight - 2f), label, EditorStyles.miniLabel);
            }
        }

        static void DrawBeatGrid(ChartTimelineLayout layout, SongChart chart, int subdiv)
        {
            if (chart == null || chart.bpm <= 0f || subdiv <= 0) return;

            float grid = BeatSnapUtility.GetGridDuration(chart.bpm, subdiv);
            if (grid <= 0f) return;

            float t0 = chart.offset + Mathf.Floor((layout.scrollTime - chart.offset) / grid) * grid;
            var gridColor = new Color(1f, 1f, 1f, 0.04f);
            for (float t = t0; t <= layout.scrollTime + layout.secondsVisible; t += grid)
            {
                float x = layout.TimeToX(t);
                if (x < layout.rect.x || x > layout.rect.xMax) continue;
                EditorGUI.DrawRect(new Rect(x, layout.rect.y + layout.rulerHeight, 1f, layout.LaneAreaHeight), gridColor);
            }
        }

    }
}
