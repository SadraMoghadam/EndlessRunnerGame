using UnityEngine;

namespace World
{
    public interface IWorldObject
    {
        void MoveWithWorld(float deltaMovement);
        void OnDespawn();
    }
}

