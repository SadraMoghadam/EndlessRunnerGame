using UnityEngine;

namespace World
{
    public interface IWorldObject
    {
        void MoveWithWorld();
        void OnDespawn();
        void OnCollided();
    }
}

