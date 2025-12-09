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
            GameObject collectiblePrefab,
            ObjectConfigSO staticObstacleConfig = null,
            ObjectConfigSO jumpObstacleConfig = null)
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
                        case ChunkLayoutSO.CellType.Moving:
                            pos = new Vector3(x,0, chunk.StartZ + zPos);

                            var pool = DynamicObstaclePool.Instance;
                            if (pool != null)
                            {
                                var pooled = pool.Get();
                                if (pooled != null)
                                {
                                    pooled.transform.position = pos;
                                    pooled.transform.rotation = Quaternion.identity;
                                    
                                    // keep it dormant until the spawner activates it
                                    pooled.SetDormant(true);
                                    // return to pool so it remains available for activation later
                                    pool.Return(pooled);
                                    continue;
                                }
                            }

                            continue;
                        case ChunkLayoutSO.CellType.Static:
                            pos = new Vector3(x,0, chunk.StartZ + zPos);
                            
                            // Try to get random object from staticObstacleConfig
                            if (staticObstacleConfig != null)
                            {
                                var objectData = staticObstacleConfig.GetRandomObject();
                                if (objectData != null && objectData.objectPrefab != null)
                                {
                                    prefabToInstantiate = objectData.objectPrefab;
                                    // Store objectData for configuration after instantiation
                                    var instantiated = Object.Instantiate(prefabToInstantiate, pos, Quaternion.identity, chunk.transform);
                                    var obstacle = instantiated?.GetComponent<WorldObstacle>();
                                    if (obstacle != null)
                                    {
                                        obstacle.ConfigureFromObjectData(objectData);
                                    }
                                    continue;
                                }
                            }
                            
                            // Fallback to staticObstaclePrefab if config not available
                            prefabToInstantiate = staticObstaclePrefab;
                            break;
                        case ChunkLayoutSO.CellType.Jump:
                            pos = new Vector3(x,0, chunk.StartZ + zPos);
                            
                            // Try to get random object from jumpObstacleConfig first, then fallback to staticObstacleConfig
                            ObjectConfigSO configToUse = jumpObstacleConfig ?? staticObstacleConfig;
                            if (configToUse != null)
                            {
                                var objectData = configToUse.GetRandomObject();
                                if (objectData != null && objectData.objectPrefab != null)
                                {
                                    prefabToInstantiate = objectData.objectPrefab;
                                    // Store objectData for configuration after instantiation
                                    var instantiated = Object.Instantiate(prefabToInstantiate, pos, Quaternion.identity, chunk.transform);
                                    var obstacle = instantiated?.GetComponent<WorldObstacle>();
                                    if (obstacle != null)
                                    {
                                        obstacle.ConfigureFromObjectData(objectData);
                                    }
                                    continue;
                                }
                            }
                            
                            // Fallback to jumpObstaclePrefab or staticObstaclePrefab
                            prefabToInstantiate = jumpObstaclePrefab ?? staticObstaclePrefab;
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

