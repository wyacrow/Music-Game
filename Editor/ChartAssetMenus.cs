using MusicGame.Chart;
using UnityEditor;
using UnityEngine;

namespace MusicGame.Editor
{
    public static class ChartAssetMenus
    {
        const string ChartsFolder = "Assets/Music-Game/Charts";

        [MenuItem("Music Game/Create Odoriko Chart")]
        public static void CreateOdorikoChart()
        {
            EnsureChartsFolder();
            var path = AssetDatabase.GenerateUniqueAssetPath($"{ChartsFolder}/Odoriko.asset");
            var chart = ScriptableObject.CreateInstance<SongChart>();
            chart.songId = "odoriko";
            chart.title = "踊り子";
            chart.bpm = 157f;
            chart.laneCount = 4;
            chart.approachTime = 2f;
            chart.offset = 0f;
            chart.notes = new System.Collections.Generic.List<NoteEventData>();

            AssetDatabase.CreateAsset(chart, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = chart;
            Debug.Log($"[Chart Editor] 已创建《踊り子》谱面模板: {path}。请绑定 AudioClip 后使用 Chart Editor 铺谱。");
        }

        public static SongChart CreateBlankChart(string assetPath)
        {
            var chart = ScriptableObject.CreateInstance<SongChart>();
            chart.notes = new System.Collections.Generic.List<NoteEventData>();
            AssetDatabase.CreateAsset(chart, assetPath);
            return chart;
        }

        public static void EnsureChartsFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Music-Game"))
                AssetDatabase.CreateFolder("Assets", "Music-Game");
            if (!AssetDatabase.IsValidFolder(ChartsFolder))
                AssetDatabase.CreateFolder("Assets/Music-Game", "Charts");
        }

        public static void EnsureAudioFolder()
        {
            const string audioFolder = "Assets/Music-Game/Audio/Songs";
            if (!AssetDatabase.IsValidFolder("Assets/Music-Game/Audio"))
                AssetDatabase.CreateFolder("Assets/Music-Game", "Audio");
            if (!AssetDatabase.IsValidFolder(audioFolder))
                AssetDatabase.CreateFolder("Assets/Music-Game/Audio", "Songs");
        }
    }
}
