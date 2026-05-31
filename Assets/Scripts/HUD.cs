using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    public TextMeshProUGUI moneyText;

    void Start()
    {
        // Subscribe to the event after all objects are ready
        PlayerManager.Instance.OnCashChanged += UpdateMoneyDisplay;
        PlayerManager.Instance.RefreshCash();
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (PlayerManager.Instance != null)
            PlayerManager.Instance.OnCashChanged -= UpdateMoneyDisplay;
    }

    void UpdateMoneyDisplay(double newBalance)
    {
        moneyText.text = $"₱{newBalance:N0}";
    }
}