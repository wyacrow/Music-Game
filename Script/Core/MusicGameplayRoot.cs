using MusicGame.Audio;
using MusicGame.Chart;
using MusicGame.Gameplay;
using MusicGame.Scoring;
using UnityEngine;

namespace MusicGame.Core
{
    /// <summary>
    /// 可选的场景引导：在 Inspector 中拖入配置后，自动创建并串联各子系统。
    /// 也可手动搭建层级并分别挂载组件。
    /// </summary>
    [DisallowMultipleComponent]
    public class MusicGameplayRoot : MonoBehaviour
    {
        [SerializeField] SongChart chart;
        [SerializeField] bool loadDemoChartFromResources;
        [SerializeField] Note notePrefab;
        [SerializeField] LaneConfig laneConfig;
        [SerializeField] JudgmentSettings judgmentSettings;
        [SerializeField] bool autoStart = true;

        MusicGameManager manager;
        MusicConductor conductor;
        NoteSpawner spawner;
        JudgmentController judgment;
        ScoreManager score;

        void Awake()
        {
            EnsureComponents();

            if (loadDemoChartFromResources && chart == null)
                chart = ChartJsonLoader.LoadFromResource("Charts/demo");

            spawner.Configure(notePrefab, laneConfig, judgmentSettings, score);
            judgment.Configure(spawner, conductor, judgmentSettings, score);
            manager.Configure(conductor, spawner, judgment);

            if (chart != null)
                manager.LoadChart(chart);
        }

        void Start()
        {
            if (autoStart && chart != null)
                manager.StartSong();
        }

        void EnsureComponents()
        {
            manager = GetComponent<MusicGameManager>();
            if (manager == null) manager = gameObject.AddComponent<MusicGameManager>();

            conductor = GetComponentInChildren<MusicConductor>();
            if (conductor == null)
            {
                var audioGo = new GameObject("Audio");
                audioGo.transform.SetParent(transform);
                audioGo.AddComponent<AudioSource>();
                conductor = audioGo.AddComponent<MusicConductor>();
            }

            spawner = GetComponentInChildren<NoteSpawner>();
            judgment = GetComponentInChildren<JudgmentController>();
            score = GetComponentInChildren<ScoreManager>();

            if (spawner == null)
            {
                var gameplayGo = new GameObject("Gameplay");
                gameplayGo.transform.SetParent(transform);
                spawner = gameplayGo.AddComponent<NoteSpawner>();
                judgment = gameplayGo.AddComponent<JudgmentController>();
                score = gameplayGo.AddComponent<ScoreManager>();
            }
            else
            {
                if (judgment == null) judgment = spawner.gameObject.AddComponent<JudgmentController>();
                if (score == null) score = spawner.gameObject.AddComponent<ScoreManager>();
            }
        }
    }
}
