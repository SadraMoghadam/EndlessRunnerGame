using Managers;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private bool _debugMode = true;
    public bool DebugMode => _debugMode;

    [HideInInspector] public WorldManager WorldManager;
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Start()
    {
        WorldManager = GetComponent<WorldManager>();
    }
}
