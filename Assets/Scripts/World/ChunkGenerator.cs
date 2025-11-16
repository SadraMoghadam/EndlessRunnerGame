using UnityEngine;
using System.Collections.Generic;
using Managers;
using System.Linq;

namespace World
{
    public static class ChunkGenerator
    {
        // Default cell size along Z (meters). Matches the user's5m suggestion.
        private const float DefaultCellSize =5f;
        private const int LaneCount =3; // Left, Center, Right

        public static void GenerateObstacles(
            WorldChunk chunk, 
            WorldManager worldManager,
            GameObject obstaclePrefab, 
            float density =0.3f, 
            float minSpacing =3f)
        {
            if (chunk == null || obstaclePrefab == null || worldManager == null) return;

            float chunkLength = chunk.ChunkLength;
            float cellSize = DefaultCellSize;
            int segments = Mathf.Max(1, Mathf.FloorToInt(chunkLength / cellSize));

            // occupancy arrays [segment, laneIndex]
            bool[,] occupiedByObstacle = new bool[segments, LaneCount];
            bool[,] occupiedByCollectible = new bool[segments, LaneCount];

            // determine number of obstacles targeted (based on density and chunk length)
            int targetObstacles = Mathf.RoundToInt(chunkLength * density);
            int placed =0;
            int maxAttempts = targetObstacles *6 +20;

            System.Random rng = new System.Random();

            // helper to get lane index from LaneNumber
            int LaneToIndex(LaneNumber l) => l == LaneNumber.Left ?0 : (l == LaneNumber.Center ?1 :2);

            // radius in segments for minSpacing
            int radius = Mathf.CeilToInt(minSpacing / cellSize);

            int attempts =0;
            while (placed < targetObstacles && attempts < maxAttempts)
            {
                attempts++;

                int seg = rng.Next(0, segments);
                LaneNumber lane = (LaneNumber)Random.Range(-1,2);
                int laneIdx = LaneToIndex(lane);

                // if occupied by anything in this lane/segment -> skip
                if (occupiedByObstacle[seg, laneIdx] || occupiedByCollectible[seg, laneIdx])
                    continue;

                // check per-segment occupancy (no more than2 items total in the same segment)
                int itemsInSegment =0;
                for (int l =0; l < LaneCount; l++)
                {
                    if (occupiedByObstacle[seg, l] || occupiedByCollectible[seg, l]) itemsInSegment++;
                }
                if (itemsInSegment >=2) continue;

                // check nearby obstacles count within radius (front/back)
                int nearbyObstacles =0;
                for (int s = Mathf.Max(0, seg - radius); s <= Mathf.Min(segments -1, seg + radius); s++)
                {
                    for (int l =0; l < LaneCount; l++)
                    {
                        if (occupiedByObstacle[s, l]) nearbyObstacles++;
                    }
                }

                if (nearbyObstacles >=2) continue; // placing this would make3 or more within spacing

                // passed checks -> place obstacle
                float zPos = (seg +0.5f) * cellSize; // center of cell
                float xPos = worldManager.GetLaneXPosition(lane);
                Vector3 position = new Vector3(xPos,0, chunk.StartZ + zPos);
                Object.Instantiate(obstaclePrefab, position, Quaternion.identity, chunk.transform);

                occupiedByObstacle[seg, laneIdx] = true;
                placed++;
            }
        }

        public static void GenerateCollectibles(
            WorldChunk chunk, 
            WorldManager worldManager,
            GameObject collectiblePrefab, 
            int count =5,
            bool useLanes = true)
        {
            if (chunk == null || collectiblePrefab == null || worldManager == null) return;

            float chunkLength = chunk.ChunkLength;
            float cellSize = DefaultCellSize;
            int segments = Mathf.Max(1, Mathf.FloorToInt(chunkLength / cellSize));

            bool[,] occupiedByObstacle = new bool[segments, LaneCount];
            bool[,] occupiedByCollectible = new bool[segments, LaneCount];

            // NOTE: If obstacles were already generated in the chunk by a different call, those occupancy arrays
            // won't reflect them here. To fully prevent overlap you should generate obstacles and collectibles
            // from the same generator call or share occupancy state. We'll attempt to be conservative by
            // checking existing children on the chunk for obstacles/collectibles and mark them occupied.

            MarkExistingObjects(chunk, occupiedByObstacle, occupiedByCollectible);

            System.Random rng = new System.Random();
            int attempts =0;
            int placed =0;
            int maxAttempts = count *8 +20;

            int LaneToIndex(LaneNumber l) => l == LaneNumber.Left ?0 : (l == LaneNumber.Center ?1 :2);

            while (placed < count && attempts < maxAttempts)
            {
                attempts++;
                int seg = rng.Next(0, segments);
                LaneNumber lane = useLanes ? (LaneNumber)Random.Range(-1,2) : LaneNumber.Center;
                int laneIdx = LaneToIndex(lane);

                // can't place where obstacle exists
                if (occupiedByObstacle[seg, laneIdx]) continue;

                // per-segment occupancy cap (no more than2 items total)
                int itemsInSegment =0;
                for (int l =0; l < LaneCount; l++)
                {
                    if (occupiedByObstacle[seg, l] || occupiedByCollectible[seg, l]) itemsInSegment++;
                }
                if (itemsInSegment >=2) continue;

                // place collectible
                float zPos = (seg +0.5f) * cellSize;
                float xPos = worldManager.GetLaneXPosition(lane);
                Vector3 position = new Vector3(xPos,0.5f, chunk.StartZ + zPos);
                Object.Instantiate(collectiblePrefab, position, Quaternion.identity, chunk.transform);

                occupiedByCollectible[seg, laneIdx] = true;
                placed++;
            }
        }

        public static void GenerateCollectibleLine(
            WorldChunk chunk, 
            WorldManager worldManager,
            GameObject collectiblePrefab, 
            float zPosition)
        {
            if (chunk == null || collectiblePrefab == null || worldManager == null) return;

            float cellSize = DefaultCellSize;
            int segments = Mathf.Max(1, Mathf.FloorToInt(chunk.ChunkLength / cellSize));
            int seg = Mathf.Clamp(Mathf.FloorToInt(zPosition / cellSize),0, segments -1);

            // inspect existing children to avoid overlapping with obstacles
            bool[,] occupiedByObstacle = new bool[segments, LaneCount];
            bool[,] occupiedByCollectible = new bool[segments, LaneCount];
            MarkExistingObjects(chunk, occupiedByObstacle, occupiedByCollectible);

            int LaneToIndex(LaneNumber l) => l == LaneNumber.Left ?0 : (l == LaneNumber.Center ?1 :2);

            foreach (LaneNumber lane in System.Enum.GetValues(typeof(LaneNumber)))
            {
                int laneIdx = LaneToIndex(lane);
                // don't place if obstacle or another collectible already occupies this lane/segment
                if (occupiedByObstacle[seg, laneIdx] || occupiedByCollectible[seg, laneIdx]) continue;

                // check per-segment cap
                int itemsInSegment =0;
                for (int l =0; l < LaneCount; l++)
                {
                    if (occupiedByObstacle[seg, l] || occupiedByCollectible[seg, l]) itemsInSegment++;
                }
                if (itemsInSegment >=2) continue;

                float xPos = worldManager.GetLaneXPosition(lane);
                Vector3 position = new Vector3(xPos,0.5f, chunk.StartZ + (seg +0.5f) * cellSize);
                Object.Instantiate(collectiblePrefab, position, Quaternion.identity, chunk.transform);

                occupiedByCollectible[seg, laneIdx] = true;
            }
        }

        private static void MarkExistingObjects(WorldChunk chunk, bool[,] occupiedByObstacle, bool[,] occupiedByCollectible)
        {
            if (chunk == null) return;

            float cellSize = DefaultCellSize;
            int segments = occupiedByObstacle.GetLength(0);

            foreach (Transform child in chunk.transform)
            {
                if (child == null) continue;

                var obstacle = child.GetComponent<WorldObstacle>();
                var collectible = child.GetComponent<WorldCollectible>();

                if (obstacle == null && collectible == null) continue;

                float localZ = child.localPosition.z; // local to chunk
                int seg = Mathf.Clamp(Mathf.FloorToInt(localZ / cellSize),0, segments -1);
                int laneIdx =1; // default center

                // crudely determine lane by x position (relative)
                float x = child.localPosition.x;
                if (x < -1f) laneIdx =0; // left
                else if (x >1f) laneIdx =2; // right
                else laneIdx =1; // center

                if (obstacle != null)
                    occupiedByObstacle[seg, laneIdx] = true;
                if (collectible != null)
                    occupiedByCollectible[seg, laneIdx] = true;
            }
        }

        public static void GenerateObstaclePattern(
            WorldChunk chunk, 
            WorldManager worldManager,
            GameObject obstaclePrefab, 
            ObstaclePattern pattern)
        {
            if (chunk == null || obstaclePrefab == null) return;

            float chunkLength = chunk.ChunkLength;
            int segments = Mathf.RoundToInt(chunkLength /5f);

            for (int i =0; i < segments; i++)
            {
                float zPos = (i +0.5f) *5f;

                switch (pattern)
                {
                    case ObstaclePattern.ZigZag:
                        LaneNumber lane = (i %2 ==0) ? LaneNumber.Left : LaneNumber.Right;
                        float xPos = worldManager.GetLaneXPosition(lane);
                        Vector3 position = new Vector3(xPos,0, chunk.StartZ + zPos);
                        Object.Instantiate(obstaclePrefab, position, Quaternion.identity, chunk.transform);
                        break;

                    case ObstaclePattern.Wall:
                        LaneNumber openLane = (LaneNumber)Random.Range(-1,2);
                        foreach (LaneNumber l in System.Enum.GetValues(typeof(LaneNumber)))
                        {
                            if (l != openLane)
                            {
                                float x = worldManager.GetLaneXPosition(l);
                                Vector3 pos = new Vector3(x,0, chunk.StartZ + zPos);
                                Object.Instantiate(obstaclePrefab, pos, Quaternion.identity, chunk.transform);
                            }
                        }
                        break;

                    case ObstaclePattern.Random:
                        if (Random.value >0.5f)
                        {
                            LaneNumber randomLane = (LaneNumber)Random.Range(-1,2);
                            float x = worldManager.GetLaneXPosition(randomLane);
                            Vector3 pos = new Vector3(x,0, chunk.StartZ + zPos);
                            Object.Instantiate(obstaclePrefab, pos, Quaternion.identity, chunk.transform);
                        }
                        break;
                }
            }
        }

        // --- NEW: SCRIPTABLEOBJECT-based generator ------------------------------------------------
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
                        case ChunkLayoutSO.CellType.Moving:
                            prefabToInstantiate = movingObstaclePrefab;
                            pos = new Vector3(x,0, chunk.StartZ + zPos);
                            break;
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

    public enum ObstaclePattern
    {
        Random,
        ZigZag,
        Wall
    }
}

