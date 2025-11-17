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

        private void Reset()
        {
            // No polling; nothing to set here by default.
        }

        private void OnEnable()
        {
            RefreshFromGameManager();
        }

        /// <summary>
        /// Update the displayed health value.
        /// Call this from systems that modify player health (e.g., player damage).
        /// </summary>
        public void SetHealth(int health)
        {
            if (healthText != null)
            {
                healthText.text = health.ToString();
            }
        }

        /// <summary>
        /// Update the displayed run coins value.
        /// Call this after modifying run coins (e.g., collecting an item).
        /// </summary>
        public void SetRunCoins(int coins)
        {
            if (runCoinsText != null)
            {
                runCoinsText.text = coins.ToString();
            }
        }

        /// <summary>
        /// Convenience method to add an amount to the currently displayed run coins value.
        /// This only updates the UI text; the game coin state should be updated separately (e.g., via GameManager.AddRunCoins).
        /// </summary>
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

        /// <summary>
        /// Refresh UI values from the GameManager's current state. Useful on enable or when the entire state changed.
        /// </summary>
        public void RefreshFromGameManager()
        {
            if (GameController.Instance == null) return;

            SetHealth(GameController.Instance.Health);
            SetRunCoins(GameController.Instance.Coins);
        }

        // Optional: expose an explicit force refresh method used by other systems or the UI manager.
        public void ForceRefresh()
        {
            RefreshFromGameManager();
        }
    }
}
