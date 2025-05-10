using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Gestiona la interfaz de usuario: energía del jugador, menús de pausa/resultado
/// y panel de detalle de carta (vista extendida) con campos separados para maná, ataque y defensa.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI de Energía del Jugador")]
    public Slider energySlider;
    public TextMeshProUGUI energyText;

    [Header("Paneles de Menú / Resultado")]
    public GameObject pauseMenuPanel;
    public GameObject gameResultPanel;
    public TextMeshProUGUI resultText;

    [Header("Panel de Detalle de Carta")]
    public GameObject cardDetailPanel;
    public Image detailCardImage;
    public TextMeshProUGUI detailNameText;
    public TextMeshProUGUI detailAbilityText;
    public TextMeshProUGUI detailLoreText;

    // ————————————————————————————————————————————
    // Tres campos separados para stats
    [Header("Stats Separadas")]
    public TextMeshProUGUI detailManaText;
    public TextMeshProUGUI detailAttackText;
    public TextMeshProUGUI detailDefenseText;
    // ————————————————————————————————————————————

    private void Start()
    {
        // Configurar energía
        if (GameManager.Instance != null && energySlider != null && energyText != null)
        {
            energySlider.maxValue = GameManager.Instance.maxEnergy;
            UpdateEnergyUI(GameManager.Instance.playerEnergy);
            GameManager.Instance.OnEnergyChanged.AddListener(UpdateEnergyUI);
        }

        // Ocultar paneles al inicio
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameResultPanel != null) gameResultPanel.SetActive(false);
        if (cardDetailPanel != null) cardDetailPanel.SetActive(false);
    }

    /// <summary>
    /// Actualiza el slider y el texto con la energía actual.
    /// </summary>
    public void UpdateEnergyUI(int currentEnergy)
    {
        if (energySlider != null)
            energySlider.value = currentEnergy;
        if (energyText != null)
            energyText.text = $"Energía: {currentEnergy}/{GameManager.Instance.maxEnergy}";
    }

    /// <summary>
    /// Muestra u oculta el menú de pausa.
    /// </summary>
    public void ShowPauseMenu(bool show)
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(show);
    }

    /// <summary>
    /// Muestra el panel de resultado con victoria o derrota.
    /// </summary>
    public void ShowGameResult(bool playerWon)
    {
        if (gameResultPanel != null)
        {
            gameResultPanel.SetActive(true);
            if (resultText != null)
                resultText.text = playerWon ? "¡Victoria!" : "Derrota";
        }
    }

    /// <summary>
    /// Muestra la vista extendida de la carta con datos separados de maná, ataque y defensa.
    /// </summary>
    public void ShowCardDetail(CardData cardData)
    {
        if (cardDetailPanel == null || cardData == null)
            return;

        // Imagen y textos generales
        if (detailCardImage != null && cardData.fullSprite != null) detailCardImage.sprite = cardData.fullSprite;
        if (detailNameText != null) detailNameText.text = cardData.cardName;
        if (detailAbilityText != null) detailAbilityText.text = cardData.abilityText;
        if (detailLoreText != null) detailLoreText.text = cardData.loreText;

        // Stats separados
        if (detailManaText != null) detailManaText.text = $"{cardData.energyCost}";
        if (detailAttackText != null) detailAttackText.text = $"{cardData.attack}";
        if (detailDefenseText != null) detailDefenseText.text = $"{cardData.defense}";

        cardDetailPanel.SetActive(true);
    }

    /// <summary>
    /// Oculta el panel de detalle de carta.
    /// </summary>
    public void HideCardDetail()
    {
        if (cardDetailPanel != null)
            cardDetailPanel.SetActive(false);
    }

    // ===== Métodos vinculados a botones de UI =====

    public void OnContinueButtonPressed()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnPauseButtonPressed()
    {
        GameManager.Instance?.PauseGame();
        ShowPauseMenu(true);
    }

    public void OnResumeButtonPressed()
    {
        GameManager.Instance?.ResumeGame();
        ShowPauseMenu(false);
    }
}
