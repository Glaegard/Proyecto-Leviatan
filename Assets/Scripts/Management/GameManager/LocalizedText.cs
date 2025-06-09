using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    public string translationKey;
    private TextMeshProUGUI uiText;

    private void Awake()
    {
        uiText = GetComponent<TextMeshProUGUI>();
        UpdateText();
        LocalizationManager.Instance.OnLanguageChanged += UpdateText;
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
    }

    private void UpdateText()
    {
        uiText.text = LocalizationManager.Instance.Translate(translationKey);
    }
}
