using UnityEngine;

namespace MusicGame.Core
{
    [DisallowMultipleComponent]
    public class GameplayFlowDebug : MonoBehaviour
    {
        [SerializeField] bool logStateChanges = true;
        [SerializeField] bool logJudgments;

        void OnEnable()
        {
            if (logStateChanges)
            {
                MusicGameEvents.OnGameStateChanged += OnState;
                MusicGameEvents.OnChartLoaded += c => Debug.Log($"[MusicGame] Chart loaded: {c?.title} ({c?.notes?.Count ?? 0} notes)");
                MusicGameEvents.OnSongStarted += () => Debug.Log("[MusicGame] Song started");
                MusicGameEvents.OnSongFinished += () => Debug.Log("[MusicGame] Song finished");
            }

            if (logJudgments)
                MusicGameEvents.OnJudgment += r => Debug.Log($"[MusicGame] {r.Type} lane={r.Lane} combo={r.Combo}");
        }

        void OnDisable()
        {
            MusicGameEvents.OnGameStateChanged -= OnState;
        }

        void OnState(GameState state) => Debug.Log($"[MusicGame] State -> {state}");
    }
}
