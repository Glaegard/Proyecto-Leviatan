using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Gestiona la interfaz de usuario del juego: energ�a, men�s de pausa/resultado, y panel de detalle de cartas.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI de Energ�a del Jugador")]
    public Slider energySlider;
    public TextMeshProUGUI energyText;

    [Header("Paneles de Men�/Resultado")]
    public GameObject pauseMenuPanel;
    public GameObject gameResultPanel;
    public TextMeshProUGUI resultText;

    [Header("Panel Detalle de Carta")]
    public GameObject cardDetailPanel;
    public Image detailCardImage;
    public TextMeshProUGUI detailNameText;
    public TextMeshProUGUI detailAbilityText;
    public TextMeshProUGUI detailLoreText;
    public TextMeshProUGUI detailStatsText;

    private void Start()
    {
        // Configurar el slider de energ�a m�xima basado en la configuraci�n del GameManager
        if (energySlider != null && GameManager.Instance != null)
        {
            energySlider.maxValue = GameManager.Instance.maxEnergy;
            UpdateEnergyUI(GameManager.Instance.playerEnergy);
        }

        // Asegurarse de que los paneles de pausa y resultado est�n ocultos al inicio
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameResultPanel != null) gameResultPanel.SetActive(false);
        if (cardDetailPanel != null) cardDetailPanel.SetActive(false);

        // Suscribirse al evento de cambio de energ�a del GameManager para actualizar la UI de energ�a
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnergyChanged.AddListener(UpdateEnergyUI);
        }
    }

    /// <summary>
    /// Actualiza la visualizaci�n de la energ�a actual del jugador en la interfaz.
    /// </summary>
    public void UpdateEnergyUI(int currentEnergy)
    {
        if (energySlider != null)
        {
            energySlider.value = currentEnergy;
        }
        if (energyText != null)
        {
            energyText.text = $"Energ�a: {currentEnergy}/{GameManager.Instance.maxEnergy}";
        }
    }

    /// <summary>
    /// Muestra u oculta el men� de pausa.
    /// </summary>
    public void ShowPauseMenu(bool show)
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(show);
        }
    }

    /// <summary>
    /// Muestra el resultado de la partida en pantalla (victoria o derrota).
    /// </summary>
    public void ShowGameResult(bool playerWon)
    {
        if (gameResultPanel != null)
        {
            gameResultPanel.SetActive(true);
            if (resultText != null)
            {
                resultText.text = playerWon ? "�Victoria!" : "Derrota";
            }
        }
    }

    /// <summary>
    /// Muestra el panel de detalle de una carta con toda su informaci�n (imagen completa, texto de habilidad, lore, stats).
    /// </summary>
    public void ShowCardDetail(CardData cardData)
    {
        if (cardData == null || cardDetailPanel == null) return;

        // Actualizar los elementos UI del panel de detalle con la informaci�n de la carta
        if (detailCardImage != null && cardData.fullSprite != null)
            detailCardImage.sprite = cardData.fullSprite;
        if (detailNameText != null)
            detailNameText.text = cardData.cardName;
        if (detailAbilityText != null)
            detailAbilityText.text = cardData.abilityText;
        if (detailLoreText != null)
            detailLoreText.text = cardData.loreText;
        if (detailStatsText != null)
            detailStatsText.text = $"Ataque: {cardData.attack}  Defensa: {cardData.defense}  Coste: {cardData.energyCost}";

        // Mostrar el panel de detalle
        cardDetailPanel.SetActive(true);
    }

    /// <summary>
    /// Oculta el panel de detalle de carta.
    /// </summary>
    public void HideCardDetail()
    {
        if (cardDetailPanel != null)
        {
            cardDetailPanel.SetActive(false);
        }
    }

    // === M�todos vinculados a botones de la interfaz ===

    public void OnContinueButtonPressed()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnPauseButtonPressed()
    {
        GameManager.Instance?.PauseGame();
    }

    public void OnResumeButtonPressed()
    {
        GameManager.Instance?.ResumeGame();
    }
}
