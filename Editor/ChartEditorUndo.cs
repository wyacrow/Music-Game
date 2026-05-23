using MusicGame.Chart;
using UnityEditor;

namespace MusicGame.Editor
{
    public static class ChartEditorUndo
    {
        public static void Record(SongChart chart, string actionName)
        {
            if (chart != null)
                Undo.RecordObject(chart, actionName);
        }
    }
}
