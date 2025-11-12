using UnityEngine;

namespace World
{
    public class WorldMover : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float baseSpeed = 10f;
        [SerializeField] private float speedMultiplier = 1f;
        [SerializeField] private bool useAcceleration = true;
        [SerializeField] private float accelerationRate = 0.1f;
        [SerializeField] private float maxSpeed = 30f;
        
        private float _currentSpeed;
        private float _totalDistanceTraveled = 0f;
        
        public float CurrentSpeed => _currentSpeed;
        public float BaseSpeed => baseSpeed;
        public float SpeedMultiplier 
        { 
            get => speedMultiplier; 
            set => speedMultiplier = Mathf.Clamp(value, 0f, 5f); 
        }
        public float TotalDistanceTraveled => _totalDistanceTraveled;
        
        private void Awake()
        {
            _currentSpeed = baseSpeed;
        }
        
        private void Update()
        {
            float deltaTime = Time.deltaTime;
            float movement = _currentSpeed * speedMultiplier * deltaTime;
            
            _totalDistanceTraveled += movement;
            
            if (useAcceleration)
            {
                _currentSpeed = Mathf.Min(_currentSpeed + accelerationRate * deltaTime, maxSpeed);
            }
        }
        
        public float GetMovementDelta()
        {
            return _currentSpeed * speedMultiplier * Time.deltaTime;
        }
        
        public void SetSpeed(float speed)
        {
            baseSpeed = Mathf.Clamp(speed, 0f, maxSpeed);
            _currentSpeed = baseSpeed;
        }
        
        public void ResetSpeed()
        {
            _currentSpeed = baseSpeed;
            _totalDistanceTraveled = 0f;
        }
        
        public void Pause()
        {
            speedMultiplier = 0f;
        }
        
        public void Resume()
        {
            speedMultiplier = 1f;
        }
    }
}

