using UnityEngine;
using System.Collections.Generic;
using Managers;
using System.Linq;

namespace World
{
    public static class ChunkGenerator
    {
        private const float DefaultCellSize =5f;
        
        public static void GenerateFromScriptable(
            WorldChunk chunk,
            WorldManager worldManager,
            ChunkLayoutSO layout,
            GameObject movingObstaclePrefab,
            GameObject staticObstaclePrefab,
            GameObject jumpObstaclePrefab,
            GameObject collectiblePrefab)
        {
            if (chunk == null || worldManager == null || layout == null) return;

            int segments = Mathf.Max(1, layout.segments);
            int lanes = Mathf.Max(1, layout.lanes);
            float cellSize = layout.cellSize >0 ? layout.cellSize : DefaultCellSize;

            for (int seg =0; seg < segments; seg++)
            {
                float zPos = (seg +0.5f) * cellSize;
                for (int laneIdx =0; laneIdx < lanes; laneIdx++)
                {
                    var cell = layout.GetCell(seg, laneIdx);
                    if (cell == ChunkLayoutSO.CellType.Empty) continue;

                    LaneNumber lane = laneIdx ==0 ? LaneNumber.Left : (laneIdx ==1 ? LaneNumber.Center : LaneNumber.Right);
                    float x = worldManager.GetLaneXPosition(lane);
                    Vector3 pos;
                    GameObject prefabToInstantiate = null;

                    switch (cell)
                    {
                        // MOVING obstacles are not instantiated here. They will be spawned by the ChunkSpawner/ChunkMovingSpawner
                        case ChunkLayoutSO.CellType.Moving:
                            continue;
                        case ChunkLayoutSO.CellType.Static:
                            prefabToInstantiate = staticObstaclePrefab;
                            pos = new Vector3(x,0, chunk.StartZ + zPos);
                            break;
                        case ChunkLayoutSO.CellType.Jump:
                            prefabToInstantiate = jumpObstaclePrefab ?? staticObstaclePrefab;
                            pos = new Vector3(x,0, chunk.StartZ + zPos);
                            break;
                        case ChunkLayoutSO.CellType.Collectible:
                            prefabToInstantiate = collectiblePrefab;
                            pos = new Vector3(x,0.5f, chunk.StartZ + zPos);
                            break;
                        default:
                            continue;
                    }

                    if (prefabToInstantiate == null)
                    {
                        Debug.LogWarning($"ChunkGenerator: prefab for cell {cell} is null. Skipping.");
                        continue;
                    }

                    Object.Instantiate(prefabToInstantiate, pos, Quaternion.identity, chunk.transform);
                }
            }
        }
    }
}

