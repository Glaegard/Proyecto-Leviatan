using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    public Slider energySlider;
    public TextMeshProUGUI energyText;

    public GameObject pauseMenuPanel;
    public GameObject gameResultPanel;
    public TextMeshProUGUI resultText;

    [Header("Detalle de Carta")]
    public GameObject cardDetailPanel;
    public Image detailCardImage;
    public TextMeshProUGUI detailNameText;
    public TextMeshProUGUI detailAbilityText;
    public TextMeshProUGUI detailLoreText;
    public TextMeshProUGUI detailManaText;
    public TextMeshProUGUI detailAttackText;
    public TextMeshProUGUI detailDefenseText;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            energySlider.maxValue = GameManager.Instance.maxEnergy;
            GameManager.Instance.OnEnergyChanged.AddListener(UpdateEnergyUI);
            UpdateEnergyUI(GameManager.Instance.playerEnergy);
        }

        pauseMenuPanel?.SetActive(false);
        gameResultPanel?.SetActive(false);
        cardDetailPanel?.SetActive(false);
    }

    public void UpdateEnergyUI(int currentEnergy)
    {
        energySlider.value = currentEnergy;
        energyText.text = currentEnergy.ToString();
    }

    public void ShowCardDetail(CardData cardData)
    {
        if (cardDetailPanel == null || cardData == null) return;

        detailCardImage.sprite = cardData.fullSprite;
        detailNameText.text = cardData.GetName();
        detailAbilityText.text = cardData.GetAbilityText();
        detailLoreText.text = cardData.GetLoreText();

        detailManaText.text = cardData.energyCost.ToString();
        detailAttackText.text = cardData.attack.ToString();
        detailDefenseText.text = cardData.defense.ToString();

        cardDetailPanel.SetActive(true);
    }

    public void HideCardDetail()
    {
        cardDetailPanel?.SetActive(false);
    }

    public void ShowGameResult(bool playerWon)
    {
        gameResultPanel.SetActive(true);
        resultText.text = playerWon ? "¡Victoria!" : "Derrota";
    }

    public void OnPauseButtonPressed()
    {
        GameManager.Instance.PauseGame();
        pauseMenuPanel.SetActive(true);
    }

    public void OnResumeButtonPressed()
    {
        GameManager.Instance.ResumeGame();
        pauseMenuPanel.SetActive(false);
    }

    /// <summary>
    /// Llamado al pulsar el botón de “Reiniciar partida”.
    /// Recarga la escena actual y restaura todo al estado inicial.
    /// </summary>
    public void OnRestartButtonPressed()
    {
        // Asegurarnos de que el tiempo no esté pausado
        Time.timeScale = 1f;

        // Recargar la escena en la que estamos
        var current = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(current);
    }

    public void OnContinueButtonPressed()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadGameButton()
    {
        SceneManager.LoadScene("Game");
    }
}
