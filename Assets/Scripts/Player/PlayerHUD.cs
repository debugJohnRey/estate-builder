using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image avatarIcon;
    [SerializeField] private TMP_Text playerNameText;

    [Header("Custom Typography")]
    [Tooltip("Assign your TextMeshPro Font Asset here (e.g., Orange Kid, Scrubland, etc.)")]
    [SerializeField] private TMP_FontAsset profileNameFont;

    [Header("Avatar Sprites")]
    [SerializeField] private Sprite maleSprite;
    [SerializeField] private Sprite femaleSprite;
    [SerializeField] private Sprite defaultSprite;

    // ──────────────────────────────────────────────────────────────────────────

    void Start()
    {
        LoadAndDisplayPlayerData();
    }

    // Call this any time you want to refresh the HUD (e.g. after a save/load).
    public void LoadAndDisplayPlayerData()
    {
        string savedName = PlayerPrefs.GetString("PlayerName", "Player");
        string savedGender = PlayerPrefs.GetString("PlayerGender", "");

        DisplayName(savedName);
        DisplayAvatar(savedGender);
    }

    private void DisplayName(string playerName)
    {
        if (playerNameText == null)
        {
            Debug.LogWarning("PlayerHUD: playerNameText is not assigned in the Inspector.");
            return;
        }

        // Apply the custom game font if one is assigned in the Inspector
        if (profileNameFont != null)
        {
            playerNameText.font = profileNameFont;
        }

        playerNameText.text = playerName;
    }

    private void DisplayAvatar(string gender)
    {
        if (avatarIcon == null)
        {
            Debug.LogWarning("PlayerHUD: avatarIcon is not assigned in the Inspector.");
            return;
        }

        switch (gender)
        {
            case "Male":
                avatarIcon.sprite = maleSprite != null ? maleSprite : defaultSprite;
                break;

            case "Female":
                avatarIcon.sprite = femaleSprite != null ? femaleSprite : defaultSprite;
                break;

            default:
                avatarIcon.sprite = defaultSprite;
                Debug.LogWarning($"PlayerHUD: Unknown gender '{gender}' – using default sprite.");
                break;
        }
    }
}