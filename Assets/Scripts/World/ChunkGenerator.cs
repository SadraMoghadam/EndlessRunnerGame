using UnityEngine;
using System.Collections.Generic;

namespace World
{
    public static class ChunkGenerator
    {
        public static void GenerateObstacles(
            WorldChunk chunk, 
            GameObject obstaclePrefab, 
            float density = 0.3f, 
            float minSpacing = 3f)
        {
            if (chunk == null || obstaclePrefab == null) return;
            
            float chunkLength = chunk.ChunkLength;
            int obstacleCount = Mathf.RoundToInt(chunkLength * density);
            
            List<float> usedPositions = new List<float>();
            
            for (int i = 0; i < obstacleCount; i++)
            {
                float zPos = Random.Range(2f, chunkLength - 2f);
                
                bool validPosition = true;
                foreach (float usedPos in usedPositions)
                {
                    if (Mathf.Abs(zPos - usedPos) < minSpacing)
                    {
                        validPosition = false;
                        break;
                    }
                }
                
                if (!validPosition) continue;
                
                LaneNumber lane = (LaneNumber)Random.Range(-1, 2);
                float xPos = chunk.GetLaneXPosition(lane);
                
                Vector3 position = new Vector3(xPos, 0, chunk.StartZ + zPos);
                GameObject obstacle = Object.Instantiate(obstaclePrefab, position, Quaternion.identity, chunk.transform);
                
                usedPositions.Add(zPos);
            }
        }
        
        public static void GenerateCollectibles(
            WorldChunk chunk, 
            GameObject collectiblePrefab, 
            int count = 5,
            bool useLanes = true)
        {
            if (chunk == null || collectiblePrefab == null) return;
            
            float chunkLength = chunk.ChunkLength;
            float spacing = chunkLength / (count + 1);
            
            for (int i = 0; i < count; i++)
            {
                float zPos = spacing * (i + 1);
                
                if (useLanes)
                {
                    LaneNumber lane = (LaneNumber)Random.Range(-1, 2);
                    float xPos = chunk.GetLaneXPosition(lane);
                    Vector3 position = new Vector3(xPos, 0.5f, chunk.StartZ + zPos);
                    Object.Instantiate(collectiblePrefab, position, Quaternion.identity, chunk.transform);
                }
                else
                {
                    float xPos = chunk.GetLaneXPosition(LaneNumber.Center);
                    Vector3 position = new Vector3(xPos, 0.5f, chunk.StartZ + zPos);
                    Object.Instantiate(collectiblePrefab, position, Quaternion.identity, chunk.transform);
                }
            }
        }
        
        public static void GenerateCollectibleLine(
            WorldChunk chunk, 
            GameObject collectiblePrefab, 
            float zPosition)
        {
            if (chunk == null || collectiblePrefab == null) return;
            
            foreach (LaneNumber lane in System.Enum.GetValues(typeof(LaneNumber)))
            {
                float xPos = chunk.GetLaneXPosition(lane);
                Vector3 position = new Vector3(xPos, 0.5f, chunk.StartZ + zPosition);
                Object.Instantiate(collectiblePrefab, position, Quaternion.identity, chunk.transform);
            }
        }
        
        public static void GenerateObstaclePattern(
            WorldChunk chunk, 
            GameObject obstaclePrefab, 
            ObstaclePattern pattern)
        {
            if (chunk == null || obstaclePrefab == null) return;
            
            float chunkLength = chunk.ChunkLength;
            int segments = Mathf.RoundToInt(chunkLength / 5f);
            
            for (int i = 0; i < segments; i++)
            {
                float zPos = (i + 0.5f) * 5f;
                
                switch (pattern)
                {
                    case ObstaclePattern.ZigZag:
                        LaneNumber lane = (i % 2 == 0) ? LaneNumber.Left : LaneNumber.Right;
                        float xPos = chunk.GetLaneXPosition(lane);
                        Vector3 position = new Vector3(xPos, 0, chunk.StartZ + zPos);
                        Object.Instantiate(obstaclePrefab, position, Quaternion.identity, chunk.transform);
                        break;
                        
                    case ObstaclePattern.Wall:
                        LaneNumber openLane = (LaneNumber)Random.Range(-1, 2);
                        foreach (LaneNumber l in System.Enum.GetValues(typeof(LaneNumber)))
                        {
                            if (l != openLane)
                            {
                                float x = chunk.GetLaneXPosition(l);
                                Vector3 pos = new Vector3(x, 0, chunk.StartZ + zPos);
                                Object.Instantiate(obstaclePrefab, pos, Quaternion.identity, chunk.transform);
                            }
                        }
                        break;
                        
                    case ObstaclePattern.Random:
                        if (Random.value > 0.5f)
                        {
                            LaneNumber randomLane = (LaneNumber)Random.Range(-1, 2);
                            float x = chunk.GetLaneXPosition(randomLane);
                            Vector3 pos = new Vector3(x, 0, chunk.StartZ + zPos);
                            Object.Instantiate(obstaclePrefab, pos, Quaternion.identity, chunk.transform);
                        }
                        break;
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

