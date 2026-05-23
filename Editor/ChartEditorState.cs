using MusicGame.Chart;

namespace MusicGame.Editor
{
    public class ChartEditorState
    {
        public SongChart chart;
        public float playheadTime;
        public float scrollTime;
        public float secondsVisible = 20f;
        public int selectedNoteIndex = -1;
        public int snapSubdiv = 2;
        public bool isPlaying;
        public bool isDraggingNote;
        public int dragNoteIndex = -1;
        public bool isDraggingPlayhead;

        public float fillStartTime;
        public float fillEndTime = 30f;
        public int fillLane = 0;
        public int fillLanePatternIndex;
        public bool showFillPanel = true;

        public const float MinSecondsVisible = 5f;
        public const float MaxSecondsVisible = 60f;

        public void ResetSelection()
        {
            selectedNoteIndex = -1;
            isDraggingNote = false;
            dragNoteIndex = -1;
        }

        public void OnChartChanged()
        {
            ResetSelection();
            playheadTime = 0f;
            scrollTime = 0f;
            isPlaying = false;
            isDraggingPlayhead = false;
            fillStartTime = 0f;
            fillEndTime = 30f;
        }
    }
}
