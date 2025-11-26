using Managers;
using UI;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    public int GameHealth { get; private set; }
    public int GameCoins { get; private set; } = 0;
    public bool IsGameOver { get; private set; } = false;

    private GameManager _gameManager;
    private UIManager _uiManager;
    private int _defaultHealth = 3;

    public PlayerController PlayerController;
    [HideInInspector] public WorldManager WorldManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
        _gameManager = GameManager.Instance;
        _uiManager = UIManager.Instance;
        if (PlayerController == null)
        {
            PlayerController = FindFirstObjectByType<PlayerController>();
        }
    }

    private void Start()
    {
        WorldManager = GetComponent<WorldManager>();
        SetHealth(_defaultHealth);
        SetCoins(0);
    }

    public int GetCoins()
    {
        return GameCoins;
    }

    public void SetCoins(int amount)
    {
        GameCoins = amount;
        _uiManager.PlayerHUD?.SetRunCoins(GameCoins);
    }

    public int GetHealth()
    {
        return GameHealth;
    }

    public void SetHealth(int amount)
    {
        GameHealth = amount;
        _uiManager.PlayerHUD?.SetHealth(amount);
    }

    public void GameOver()
    {
        int currentHealth = GetHealth();
        currentHealth = Mathf.Max(0, --currentHealth);
        SetHealth(currentHealth);
        SaveProgress();
        IsGameOver = true;
        WorldManager.ResetWorld();
        ResetGame(currentHealth);
        _uiManager.PlayerHUD.RefreshFromGameManager();
    }

    public void ResetGame(int health)
    {
        GameHealth = health;
        GameCoins = 0;
        IsGameOver = false;
    }

    public void SaveProgress()
    {
        _gameManager.SetPlayerCoins(PlayerPrefsManager.GetInt(PlayerPrefsKeys.Coins, 0) + GameCoins);
        //_gameManager.SetPlayerHealth(Health);
        _gameManager.IncrementGameNumber();
    }
}
