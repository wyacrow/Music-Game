using UnityEngine;

namespace MusicGame.Gameplay
{
    [CreateAssetMenu(fileName = "JudgmentSettings", menuName = "Music Game/Judgment Settings")]
    public class JudgmentSettings : ScriptableObject
    {
        [Min(0.01f)] public float perfectWindow = 0.05f;
        [Min(0.01f)] public float goodWindow = 0.12f;
        [Min(0.01f)] public float missWindow = 0.2f;
    }
}
