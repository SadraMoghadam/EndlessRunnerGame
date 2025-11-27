using UI;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] Button playAgainButton;
    [SerializeField] Button lastCheckpointButton;

    private void Start()
    {
        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        }
        if (lastCheckpointButton != null)
        {
            lastCheckpointButton.onClick.AddListener(OnLastCheckpointClicked);
        }
    }

    private void OnPlayAgainClicked()
    {
        GameController.Instance?.ResetGame();
        Hide();
    }

    private void OnLastCheckpointClicked()
    {
        GameController.Instance?.ResetToLastCheckpoint();
        Hide();
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
