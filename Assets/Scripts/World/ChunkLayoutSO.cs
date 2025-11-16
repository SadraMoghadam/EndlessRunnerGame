using UnityEngine;

namespace World
{
    [CreateAssetMenu(fileName = "ChunkLayout", menuName = "World/Chunk Layout", order = 1)]
    public class ChunkLayoutSO : ScriptableObject
    {
        public enum CellType
        {
            Empty = 0,
            Moving = 1,
            Static = 2,
            Jump = 3,
            Collectible = 10
        }

        [Header("Layout Settings")]
        public Difficulty difficulty = Difficulty.Easy;

        [Tooltip("Number of Z segments (rows). Default50.")]
        public int segments = 50;

        [Tooltip("Number of lanes (columns). Default3.")]
        public int lanes = 3;

        [Tooltip("Cell size in Z direction (meters). Default5.")]
        public float cellSize = 5f;

        [Tooltip("Flattened cells array: index = segment * lanes + lane.")]
        public CellType[] cells;

        private void OnEnable()
        {
            EnsureSize();
        }

        private void OnValidate()
        {
            EnsureSize();
        }

        public void EnsureSize()
        {
            int s = Mathf.Max(0, segments) * Mathf.Max(1, lanes);
            if (cells == null || cells.Length != s)
            {
                var newArr = new CellType[s];
                if (cells != null)
                {
                    int copy = Mathf.Min(cells.Length, s);
                    for (int i = 0; i < copy; i++)
                    {
                        newArr[i] = cells[i];
                    }
                }
                cells = newArr;
            }
        }

        public CellType GetCell(int segment, int lane)
        {
            if (cells == null)
            {
                return CellType.Empty;
            }

            int seg = Mathf.Clamp(segment, 0, Mathf.Max(0, segments - 1));
            int ln = Mathf.Clamp(lane, 0, Mathf.Max(0, lanes - 1));
            int idx = seg * lanes + ln;
            if (idx < 0 || idx >= cells.Length)
            {
                return CellType.Empty;
            }

            return cells[idx];
        }

        public void SetCell(int segment, int lane, CellType t)
        {
            EnsureSize();

            int seg = Mathf.Clamp(segment, 0, Mathf.Max(0, segments - 1));
            int ln = Mathf.Clamp(lane, 0, Mathf.Max(0, lanes - 1));
            int idx = seg * lanes + ln;
            if (idx < 0 || idx >= cells.Length)
            {
                return;
            }

            cells[idx] = t;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
