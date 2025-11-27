using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HUD : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private TMP_Text runCoinsText;

        private void OnEnable()
        {
            RefreshFromGameManager();
        }

        public void SetHealth(int health)
        {
            if (healthText != null)
            {
                healthText.text = health.ToString();
            }
        }

        public void SetRunCoins(int coins)
        {
            if (runCoinsText != null)
            {
                runCoinsText.text = coins.ToString();
            }
        }

        public void AddRunCoins(int amount)
        {
            if (runCoinsText == null) return;

            if (int.TryParse(runCoinsText.text, out int current))
            {
                current += amount;
                runCoinsText.text = current.ToString();
            }
            else
            {
                runCoinsText.text = amount.ToString();
            }
        }

        public void RefreshFromGameManager()
        {
            if (GameController.Instance == null) return;

            SetHealth(GameController.Instance.GameHealth);
            SetRunCoins(GameController.Instance.GameCoins);
        }

        public void ForceRefresh()
        {
            RefreshFromGameManager();
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }   
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
