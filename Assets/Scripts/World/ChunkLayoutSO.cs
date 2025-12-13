// LEGACY CODE

using UnityEngine;

namespace World
{
    [CreateAssetMenu(fileName = "ChunkLayout", menuName = "World/Chunk Layout", order = 1)]
    public class ChunkLayoutSO : ScriptableObject
    {

        [Header("Layout Settings")]
        public Difficulty difficulty = Difficulty.Easy;
        public GameObject chunkPrefab;
        public int lanes = 3;
    }
}
