using Managers;
using UI;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    public int Health { get; private set; }
    public int Coins { get; private set; } = 0;
    public bool IsGameOver { get; private set; } = false;

    private GameManager _gameManager;
    private UIManager _uiManager;

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
            DontDestroyOnLoad(this.gameObject);
        }
        _gameManager = GameManager.Instance;
        _uiManager = UIManager.Instance;
    }

    private void Start()
    {
        WorldManager = GetComponent<WorldManager>();
        SetHealth(_gameManager.PlayerHealth);
        SetCoins(0);
    }

    public int GetCoins()
    {
        return Coins;
    }

    public void SetCoins(int amount)
    {
        Coins = amount;
        _uiManager.PlayerHUD?.SetRunCoins(Coins);
    }

    public int GetHealth()
    {
        return Health;
    }

    public void SetHealth(int amount)
    {
        Health = amount;
        _uiManager.PlayerHUD?.SetHealth(amount);
    }

    public void GameOver()
    {
        int currentHealth = GetHealth();
        SetHealth(--currentHealth);
        SaveProgress();
        IsGameOver = true;
        WorldManager.ResetWorld();
        ResetGame(_gameManager.PlayerHealth);
        _uiManager.PlayerHUD.RefreshFromGameManager();
    }

    public void ResetGame(int startingHealth)
    {
        Health = PlayerPrefsManager.GetInt(PlayerPrefsKeys.Health, 0);
        Coins = 0;
        IsGameOver = false;
    }

    public void SaveProgress()
    {
        _gameManager.SetPlayerCoins(PlayerPrefsManager.GetInt(PlayerPrefsKeys.Coins, 0) + Coins);
        _gameManager.SetPlayerHealth(Health);
        _gameManager.IncrementGameNumber();
    }
}
