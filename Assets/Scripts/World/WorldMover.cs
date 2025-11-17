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

        private void Start()
        {
            // Ensure a valid speed multiplier on start. It's possible the inspector serialized this to 0.
            if (speedMultiplier <= 0f)
            {
                speedMultiplier = 1f;
            }

            // Ensure current speed is synced with base speed at start
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
            // set multiplier to zero to pause but remember current multiplier? For now we set to 0.
            speedMultiplier = 0f;
        }
        
        public void Resume()
        {
            // restore to at least 1 if multiplier is zero to ensure world moves
            if (speedMultiplier <= 0f) speedMultiplier = 1f;
        }
    }
}

