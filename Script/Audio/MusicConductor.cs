using MusicGame.Chart;
using UnityEngine;

namespace MusicGame.Audio
{
    [RequireComponent(typeof(AudioSource))]
    [DisallowMultipleComponent]
    public class MusicConductor : MonoBehaviour
    {
        [SerializeField] AudioSource audioSource;
        [SerializeField] float dspLatencyCompensation;

        SongChart chart;
        bool isPlaying;
        bool useInternalClock;
        float internalTime;
        float pausedTime;

        public float SongTime
        {
            get
            {
                if (chart == null) return 0f;
                if (!isPlaying) return pausedTime;
                float t = useInternalClock ? internalTime : (audioSource != null ? audioSource.time : internalTime);
                return t + chart.offset + dspLatencyCompensation;
            }
        }

        void Update()
        {
            if (isPlaying && useInternalClock)
                internalTime += Time.deltaTime;
        }

        public bool IsPlaying => isPlaying;
        public bool IsFinished => chart != null && isPlaying && SongTime >= chart.SongLength + chart.approachTime;

        void Awake()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }

        public void Prepare(SongChart songChart)
        {
            chart = songChart;
            isPlaying = false;
            pausedTime = 0f;

            if (audioSource == null) return;
            audioSource.clip = chart?.music;
            audioSource.playOnAwake = false;
        }

        public void Play()
        {
            isPlaying = true;
            pausedTime = 0f;
            internalTime = 0f;

            useInternalClock = chart == null || audioSource == null || chart.music == null;
            if (useInternalClock)
            {
                if (chart != null && chart.music == null)
                    Debug.LogWarning("[MusicConductor] 未绑定音频，使用内部时钟驱动谱面。");
                return;
            }

            audioSource.time = 0f;
            audioSource.Play();
        }

        public void Pause()
        {
            if (!isPlaying) return;
            pausedTime = SongTime;
            isPlaying = false;
            if (audioSource != null && audioSource.isPlaying)
                audioSource.Pause();
        }

        public void Resume()
        {
            if (chart == null) return;
            isPlaying = true;
            if (audioSource != null && chart.music != null)
            {
                audioSource.time = Mathf.Max(0f, pausedTime - chart.offset - dspLatencyCompensation);
                audioSource.UnPause();
            }
        }

        public void Stop()
        {
            isPlaying = false;
            if (audioSource != null)
                audioSource.Stop();
        }
    }
}
