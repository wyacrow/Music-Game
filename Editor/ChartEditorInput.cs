using MusicGame.Chart;
using UnityEditor;
using UnityEngine;

namespace MusicGame.Editor
{
    public static class ChartEditorInput
    {
        const float PlayheadHandleWidth = 14f;

        public static void HandleScrollWheel(Event e, ChartEditorState state, Rect timelineRect)
        {
            if (!timelineRect.Contains(e.mousePosition)) return;

            if (e.control || e.command)
            {
                float delta = -e.delta.y * 0.5f;
                state.secondsVisible = Mathf.Clamp(
                    state.secondsVisible + delta,
                    ChartEditorState.MinSecondsVisible,
                    ChartEditorState.MaxSecondsVisible);
            }
            else
            {
                float deltaTime = e.delta.y * 0.05f * state.secondsVisible;
                state.scrollTime = Mathf.Max(0f, state.scrollTime + deltaTime);
            }
            e.Use();
        }

        public static void HandlePlayheadScrub(
            Event e,
            ChartTimelineLayout layout,
            ChartEditorState state,
            System.Action<float> setPlayhead)
        {
            if (!layout.rect.Contains(e.mousePosition)) return;

            float playX = layout.TimeToX(state.playheadTime);
            var handleRect = new Rect(playX - PlayheadHandleWidth * 0.5f, layout.rect.y, PlayheadHandleWidth, layout.rect.height);
            var rulerRect = new Rect(layout.rect.x, layout.rect.y, layout.rect.width, layout.rulerHeight);
            bool onRuler = rulerRect.Contains(e.mousePosition);
            bool onHandle = handleRect.Contains(e.mousePosition);

            switch (e.type)
            {
                case EventType.MouseDown when e.button == 0 && (onHandle || onRuler):
                    state.isDraggingPlayhead = true;
                    state.isDraggingNote = false;
                    state.dragNoteIndex = -1;
                    setPlayhead(layout.XToTime(e.mousePosition.x));
                    e.Use();
                    break;

                case EventType.MouseDown when e.button == 1 && layout.rect.Contains(e.mousePosition):
                    setPlayhead(layout.XToTime(e.mousePosition.x));
                    e.Use();
                    break;

                case EventType.MouseDrag when e.button == 0 && state.isDraggingPlayhead:
                    setPlayhead(layout.XToTime(e.mousePosition.x));
                    e.Use();
                    break;

                case EventType.MouseUp when e.button == 0:
                    state.isDraggingPlayhead = false;
                    break;
            }
        }

        public static void HandleTimelineMouse(
            Event e,
            ChartTimelineLayout layout,
            ChartEditorState state,
            SongChart chart,
            System.Action markDirty)
        {
            if (chart == null || chart.notes == null) return;
            if (state.isDraggingPlayhead) return;

            int hitIndex = HitTestNote(layout, chart, e.mousePosition);

            switch (e.type)
            {
                case EventType.MouseDown when e.button == 0:
                    if (hitIndex >= 0)
                    {
                        ChartEditorUndo.Record(chart, "Move Note");
                        state.selectedNoteIndex = hitIndex;
                        state.isDraggingNote = true;
                        state.dragNoteIndex = hitIndex;
                    }
                    else if (layout.Contains(e.mousePosition) && layout.rect.y + layout.rulerHeight < e.mousePosition.y)
                    {
                        ChartEditorUndo.Record(chart, "Add Note");
                        TryAddNote(layout, state, chart, e.mousePosition);
                        markDirty?.Invoke();
                    }
                    e.Use();
                    break;

                case EventType.MouseDrag when e.button == 0 && state.isDraggingNote && state.dragNoteIndex >= 0:
                    DragNote(layout, state, chart, state.dragNoteIndex, e.mousePosition);
                    markDirty?.Invoke();
                    e.Use();
                    break;

                case EventType.MouseUp when e.button == 0:
                    state.isDraggingNote = false;
                    state.dragNoteIndex = -1;
                    break;
            }
        }

        public static void HandleDeleteKey(Event e, ChartEditorState state, SongChart chart, System.Action markDirty)
        {
            if (e.type != EventType.KeyDown) return;
            if (e.keyCode != KeyCode.Delete && e.keyCode != KeyCode.Backspace) return;
            if (state.selectedNoteIndex < 0 || chart?.notes == null) return;
            if (state.selectedNoteIndex >= chart.notes.Count) return;

            ChartEditorUndo.Record(chart, "Delete Note");
            chart.notes.RemoveAt(state.selectedNoteIndex);
            state.selectedNoteIndex = -1;
            chart.SortNotesByTime();
            markDirty?.Invoke();
            e.Use();
        }

        static int HitTestNote(ChartTimelineLayout layout, SongChart chart, Vector2 mousePos)
        {
            int best = -1;
            float bestDist = float.MaxValue;
            for (int i = 0; i < chart.notes.Count; i++)
            {
                var n = chart.notes[i];
                if (n.lane < 0 || n.lane >= layout.laneCount) continue;
                var rect = layout.GetNoteRect(n.time, n.lane);
                if (!rect.Contains(mousePos)) continue;
                float d = Vector2.Distance(mousePos, rect.center);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = i;
                }
            }
            return best;
        }

        static void TryAddNote(ChartTimelineLayout layout, ChartEditorState state, SongChart chart, Vector2 mousePos)
        {
            float time = layout.XToTime(mousePos.x);
            int lane = layout.YToLane(mousePos.y);
            time = BeatSnapUtility.SnapTime(time, chart.bpm, chart.offset, state.snapSubdiv);

            float grid = BeatSnapUtility.GetGridDuration(chart.bpm, state.snapSubdiv > 0 ? state.snapSubdiv : 1);
            float threshold = grid > 0f ? grid * 0.25f : 0.05f;

            if (ExistsNear(chart, lane, time, threshold)) return;

            chart.notes.Add(new NoteEventData(lane, time, NoteType.Tap, 0f));
            chart.SortNotesByTime();
            state.selectedNoteIndex = FindNoteIndex(chart, lane, time);
        }

        static void DragNote(ChartTimelineLayout layout, ChartEditorState state, SongChart chart, int index, Vector2 mousePos)
        {
            if (index < 0 || index >= chart.notes.Count) return;

            var n = chart.notes[index];
            float time = layout.XToTime(mousePos.x);
            int lane = layout.YToLane(mousePos.y);
            lane = Mathf.Clamp(lane, 0, chart.laneCount - 1);
            time = BeatSnapUtility.SnapTime(time, chart.bpm, chart.offset, state.snapSubdiv);
            time = Mathf.Max(0f, time);

            n.lane = lane;
            n.time = time;
            chart.notes[index] = n;
            chart.SortNotesByTime();
            state.selectedNoteIndex = FindNoteIndex(chart, lane, time);
        }

        static bool ExistsNear(SongChart chart, int lane, float time, float threshold)
        {
            foreach (var n in chart.notes)
            {
                if (n.lane == lane && Mathf.Abs(n.time - time) < threshold)
                    return true;
            }
            return false;
        }

        static int FindNoteIndex(SongChart chart, int lane, float time)
        {
            for (int i = 0; i < chart.notes.Count; i++)
            {
                var n = chart.notes[i];
                if (n.lane == lane && Mathf.Abs(n.time - time) < 0.001f)
                    return i;
            }
            return -1;
        }
    }
}
