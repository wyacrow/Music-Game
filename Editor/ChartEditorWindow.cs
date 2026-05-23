using MusicGame.Chart;
using UnityEditor;
using UnityEngine;

namespace MusicGame.Editor
{
    public class ChartEditorWindow : EditorWindow
    {
        readonly ChartEditorState state = new ChartEditorState();
        readonly ChartEditorAudio audio = new ChartEditorAudio();

        string[] snapLabels = { "Off", "1 beat", "1/2 beat", "1/4 beat" };
        string[] fillPatternLabels = { "Single lane", "Rotate lanes", "All 4 lanes / step" };
        int snapPopupIndex = 2;

        [MenuItem("Music Game/Chart Editor")]
        public static void Open()
        {
            var w = GetWindow<ChartEditorWindow>("Chart Editor");
            w.minSize = new Vector2(720f, 480f);
        }

        void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            Undo.undoRedoPerformed -= OnUndoRedo;
            audio.Stop();
            audio.Dispose();
            state.isPlaying = false;
        }

        void OnUndoRedo()
        {
            state.ResetSelection();
            Repaint();
        }

        void OnEditorUpdate()
        {
            if (!state.isPlaying) return;
            if (state.chart == null || state.chart.music == null)
            {
                state.isPlaying = false;
                audio.Stop();
                return;
            }

            if (!audio.IsPlaying)
            {
                state.isPlaying = false;
                return;
            }

            state.playheadTime = audio.GetTime();

            if (state.playheadTime >= state.chart.music.length)
            {
                state.isPlaying = false;
                audio.Stop();
                return;
            }

            float follow = state.scrollTime + state.secondsVisible * 0.7f;
            if (state.playheadTime > follow)
                state.scrollTime = Mathf.Max(0f, state.playheadTime - state.secondsVisible * 0.7f);

            Repaint();
        }

        void OnGUI()
        {
            DrawToolbar();

            if (state.chart != null)
            {
                DrawFillPanel();
                DrawStats();
            }

            EditorGUILayout.Space(4f);

            if (state.chart == null)
            {
                EditorGUILayout.HelpBox(
                    "Assign a SongChart asset, or use New / Music Game > Create Odoriko Chart.",
                    MessageType.Info);
                return;
            }

            float timelineHeight = Mathf.Max(180f, position.height - (state.showFillPanel ? 200f : 140f));
            var timelineRect = GUILayoutUtility.GetRect(position.width - 8f, timelineHeight);
            timelineRect.x += 4f;
            timelineRect.width -= 8f;

            var layout = new ChartTimelineLayout
            {
                rect = timelineRect,
                rulerHeight = 22f,
                scrollTime = state.scrollTime,
                secondsVisible = state.secondsVisible,
                laneCount = Mathf.Max(1, state.chart.laneCount)
            };

            var e = Event.current;

            if (timelineRect.Contains(e.mousePosition) && e.type == EventType.ScrollWheel)
                ChartEditorInput.HandleScrollWheel(e, state, timelineRect);

            ChartEditorInput.HandlePlayheadScrub(e, layout, state, SetPlayheadTime);
            ChartEditorInput.HandleDeleteKey(e, state, state.chart, MarkDirty);
            ChartEditorInput.HandleTimelineMouse(e, layout, state, state.chart, MarkDirty);

            ChartTimelineDrawer.Draw(layout, state.chart, state);

            if (state.isDraggingPlayhead)
                EditorGUIUtility.AddCursorRect(layout.rect, MouseCursor.SlideArrow);
        }

        void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUI.BeginChangeCheck();
            var newChart = (SongChart)EditorGUILayout.ObjectField(
                state.chart, typeof(SongChart), false, GUILayout.Width(220f));
            if (EditorGUI.EndChangeCheck() && newChart != state.chart)
            {
                if (state.isPlaying)
                {
                    audio.Stop();
                    state.isPlaying = false;
                }
                state.chart = newChart;
                state.OnChartChanged();
                if (state.chart != null && state.chart.music != null)
                    state.fillEndTime = state.chart.music.length;
            }

            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(48f)))
                SaveChart();

            if (GUILayout.Button("New", EditorStyles.toolbarButton, GUILayout.Width(40f)))
                CreateNewChart();

            GUILayout.FlexibleSpace();

            if (state.chart != null)
            {
                EditorGUI.BeginChangeCheck();
                state.chart.songId = EditorGUILayout.TextField(state.chart.songId, GUILayout.Width(80f));
                state.chart.title = EditorGUILayout.TextField(state.chart.title, GUILayout.Width(100f));
                state.chart.bpm = EditorGUILayout.FloatField(state.chart.bpm, GUILayout.Width(48f));
                state.chart.offset = EditorGUILayout.FloatField(state.chart.offset, GUILayout.Width(48f));
                state.chart.approachTime = EditorGUILayout.FloatField(state.chart.approachTime, GUILayout.Width(40f));
                if (EditorGUI.EndChangeCheck())
                {
                    ChartEditorUndo.Record(state.chart, "Edit Chart Metadata");
                    MarkDirty();
                }

                EditorGUI.BeginChangeCheck();
                state.chart.music = (AudioClip)EditorGUILayout.ObjectField(
                    state.chart.music, typeof(AudioClip), false, GUILayout.Width(160f));
                if (EditorGUI.EndChangeCheck())
                {
                    ChartEditorUndo.Record(state.chart, "Edit Music Clip");
                    if (state.chart.music != null)
                        state.fillEndTime = state.chart.music.length;
                    MarkDirty();
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            snapPopupIndex = EditorGUILayout.Popup("Snap", snapPopupIndex, snapLabels, GUILayout.Width(130f));
            state.snapSubdiv = SnapIndexToSubdiv(snapPopupIndex);

            using (new EditorGUI.DisabledScope(state.chart == null || state.chart.music == null))
            {
                if (GUILayout.Button(state.isPlaying ? "Stop" : "Play", EditorStyles.toolbarButton, GUILayout.Width(48f)))
                    TogglePlay();
            }

            if (state.chart != null && state.chart.music == null)
                GUILayout.Label("No AudioClip", EditorStyles.miniLabel);

            if (state.chart != null)
            {
                state.secondsVisible = EditorGUILayout.Slider(
                    state.secondsVisible,
                    ChartEditorState.MinSecondsVisible,
                    ChartEditorState.MaxSecondsVisible,
                    GUILayout.Width(160f));
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawFillPanel()
        {
            state.showFillPanel = EditorGUILayout.Foldout(state.showFillPanel, "Interval Fill (fixed rhythm)", true);
            if (!state.showFillPanel) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            state.fillStartTime = EditorGUILayout.FloatField("Start (s)", state.fillStartTime);
            state.fillEndTime = EditorGUILayout.FloatField("End (s)", state.fillEndTime);
            if (GUILayout.Button("Playhead", GUILayout.Width(64f)))
                state.fillStartTime = state.playheadTime;
            if (GUILayout.Button("Clip end", GUILayout.Width(56f)) && state.chart.music != null)
                state.fillEndTime = state.chart.music.length;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            state.fillLane = EditorGUILayout.IntPopup("Lane", state.fillLane, new[] { "0", "1", "2", "3" }, new[] { 0, 1, 2, 3 });
            state.fillLanePatternIndex = EditorGUILayout.Popup("Pattern", state.fillLanePatternIndex, fillPatternLabels);
            GUILayout.Label($"Step: {GetFillStepSeconds():F3}s", EditorStyles.miniLabel, GUILayout.Width(90f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Fill Range"))
                RunFillRange();
            if (GUILayout.Button("Fill Visible"))
                RunFillVisible();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        void DrawStats()
        {
            if (state.chart == null) return;

            int count = state.chart.notes?.Count ?? 0;
            float last = state.chart.SongLength;
            float clipLen = state.chart.music != null ? state.chart.music.length : 0f;
            float tail = clipLen > 0f ? clipLen - last : 0f;
            float maxTime = clipLen > 0f ? clipLen : Mathf.Max(last + 10f, 60f);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Notes: {count}  |  Last: {last:F2}s  |  Clip: {clipLen:F2}s  |  Tail: ", EditorStyles.miniLabel);
            var prev = GUI.color;
            if (clipLen > 0f && last > clipLen) GUI.color = Color.red;
            GUILayout.Label($"{tail:F2}s", EditorStyles.miniLabel);
            GUI.color = prev;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Playhead", GUILayout.Width(52f));
            EditorGUI.BeginChangeCheck();
            float newPlayhead = EditorGUILayout.Slider(state.playheadTime, 0f, maxTime);
            newPlayhead = EditorGUILayout.FloatField(newPlayhead, GUILayout.Width(56f));
            if (EditorGUI.EndChangeCheck())
                SetPlayheadTime(newPlayhead);
            GUILayout.Label("Drag red line / ruler / slider. Ctrl+Z undo.", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        float GetFillStepSeconds()
        {
            if (state.chart == null || state.chart.bpm <= 0f) return 0f;
            int subdiv = state.snapSubdiv > 0 ? state.snapSubdiv : 1;
            return BeatSnapUtility.GetGridDuration(state.chart.bpm, subdiv);
        }

        void RunFillRange()
        {
            if (state.chart == null) return;
            float step = GetFillStepSeconds();
            if (step <= 0f)
            {
                EditorUtility.DisplayDialog("Fill", "Set BPM > 0 and Snap (not Off).", "OK");
                return;
            }

            ChartEditorUndo.Record(state.chart, "Fill Notes");
            var pattern = (ChartFillLanePattern)state.fillLanePatternIndex;
            int added = ChartNoteFillUtility.FillRange(
                state.chart,
                state.fillStartTime,
                state.fillEndTime,
                step,
                state.fillLane,
                pattern);
            MarkDirty();
            Debug.Log($"[Chart Editor] Filled {added} notes.");
        }

        void RunFillVisible()
        {
            state.fillStartTime = state.scrollTime;
            state.fillEndTime = state.scrollTime + state.secondsVisible;
            RunFillRange();
        }

        void SetPlayheadTime(float time)
        {
            if (state.chart == null) return;

            float max = state.chart.music != null ? state.chart.music.length : Mathf.Max(state.chart.SongLength, time);
            state.playheadTime = Mathf.Clamp(time, 0f, max);

            if (state.isPlaying)
            {
                state.isPlaying = false;
                audio.Stop();
            }

            if (state.chart.music != null)
                audio.Seek(state.playheadTime, state.chart);

            Repaint();
        }

        void TogglePlay()
        {
            if (state.chart == null || state.chart.music == null) return;

            if (state.isPlaying)
            {
                state.isPlaying = false;
                audio.Stop();
                return;
            }

            audio.Play(state.chart, state.playheadTime);
            state.isPlaying = true;
        }

        void SaveChart()
        {
            if (state.chart == null) return;
            state.chart.SortNotesByTime();
            MarkDirty();
            AssetDatabase.SaveAssets();
            Debug.Log("[Chart Editor] Chart saved.");
        }

        void CreateNewChart()
        {
            ChartAssetMenus.EnsureChartsFolder();
            var path = EditorUtility.SaveFilePanelInProject(
                "Create Song Chart",
                "NewSongChart",
                "asset",
                "Choose save location for the chart asset");
            if (string.IsNullOrEmpty(path)) return;

            var chart = ChartAssetMenus.CreateBlankChart(path);
            AssetDatabase.SaveAssets();
            state.chart = chart;
            state.OnChartChanged();
            Selection.activeObject = chart;
        }

        void MarkDirty()
        {
            if (state.chart != null)
                EditorUtility.SetDirty(state.chart);
        }

        static int SnapIndexToSubdiv(int index)
        {
            switch (index)
            {
                case 1: return 1;
                case 2: return 2;
                case 3: return 4;
                default: return 0;
            }
        }
    }
}
