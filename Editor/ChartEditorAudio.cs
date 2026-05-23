using MusicGame.Chart;
using UnityEditor;
using UnityEngine;

namespace MusicGame.Editor
{
    public class ChartEditorAudio
    {
        GameObject previewRoot;
        AudioSource source;

        public bool IsPlaying => source != null && source.isPlaying;

        public void EnsureSource()
        {
            if (previewRoot != null) return;
            previewRoot = EditorUtility.CreateGameObjectWithHideFlags(
                "ChartEditorAudioPreview",
                HideFlags.HideAndDontSave);
            source = previewRoot.AddComponent<AudioSource>();
            source.playOnAwake = false;
        }

        public void Dispose()
        {
            if (previewRoot != null)
            {
                Object.DestroyImmediate(previewRoot);
                previewRoot = null;
                source = null;
            }
        }

        public void Stop()
        {
            if (source == null) return;
            source.Stop();
        }

        public void Play(SongChart chart, float startTime)
        {
            if (chart == null || chart.music == null) return;
            EnsureSource();
            source.clip = chart.music;
            source.time = Mathf.Clamp(startTime, 0f, chart.music.length);
            source.Play();
        }

        public float GetTime()
        {
            if (source == null || source.clip == null) return 0f;
            return source.time;
        }

        public void Seek(float time, SongChart chart)
        {
            if (source == null || chart == null || chart.music == null) return;
            EnsureSource();
            source.clip = chart.music;
            source.time = Mathf.Clamp(time, 0f, chart.music.length);
        }
    }
}
