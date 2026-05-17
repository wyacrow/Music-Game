using UnityEngine;

namespace MusicGame.Gameplay
{
    [CreateAssetMenu(fileName = "LaneConfig", menuName = "Music Game/Lane Config")]
    public class LaneConfig : ScriptableObject
    {
        [Min(1)] public int laneCount = 4;
        public float laneSpacing = 1.5f;
        public float spawnY = 6f;
        public float judgeY = 0f;

        public float GetLaneX(int lane, int totalLanes)
        {
            if (totalLanes <= 1) return 0f;
            float center = (totalLanes - 1) * 0.5f;
            return (lane - center) * laneSpacing;
        }
    }
}
