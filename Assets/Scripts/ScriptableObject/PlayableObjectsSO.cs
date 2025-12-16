using System.Collections.Generic;
using UnityEngine;
using World;

namespace Player
{
    public enum PlayableObjectName
    {
        Skateboard,
        Bicycle,
        Scooter,
    }

    [System.Serializable]
    public class PlayableObjectData
    {
        public PlayableObjectName name = PlayableObjectName.Skateboard;
        public GameObject prefab;
        public float laneChangeSpeed = 10;
        public float speed = 0;
        public int health = 0;
    }
    [CreateAssetMenu(fileName = "PlayableObjects", menuName = "World/Playable Objects", order = 1)]
    public class PlayableObjectsSO : ScriptableObject
    {
        public List<PlayableObjectData> PlayableObjectDatas = new List<PlayableObjectData>();

        public PlayableObjectData GetPlayableObjectDataByName(PlayableObjectName name)
        {
            return PlayableObjectDatas.Find(obj => obj.name == name);
        }
    }
}
