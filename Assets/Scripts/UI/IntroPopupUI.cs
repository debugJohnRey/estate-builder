using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class IntroPopupUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text titleText;
    public TMP_Text messageText;
    public Button okButton;

    [Header("Fade Settings")]
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.3f;

    const string TITLE =
        "Welcome to ABM Academy!";

    const string MESSAGE =
        "Before your journey begins, you must first speak with\n" +
        "<color=#FFD700><b>Mr. Ledger</b></color> — the Academy Director.\n\n" +
        "He is waiting at the <color=#FFD700><b>Academy Lobby</b></color>.\n\n" +
        "Find him and he will guide you through your quest!";

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Hide visually but do NOT touch PlayerController yet
        // (PlayerControllers may not be initialized yet in Awake)
        canvasGroup.alpha = 0f;
    }

    void Start()
    {
        // Skip popup if Mr. Ledger was already met
        if (QuestManager.Instance != null && QuestManager.Instance.IsMrLedgerMet())
        {
            gameObject.SetActive(false);
            return;
        }

        titleText.text = TITLE;
        messageText.text = MESSAGE;
        okButton.GetComponentInChildren<TMP_Text>().text = "Got it — let's go!";
        okButton.onClick.AddListener(OnOKPressed);

        // Block input now that all controllers are initialized
        SetPlayerInput(false);

        StartCoroutine(Fade(0f, 1f, fadeInDuration));
    }

    void OnOKPressed()
    {
        okButton.interactable = false;
        StartCoroutine(FadeAndClose());
    }

    IEnumerator FadeAndClose()
    {
        yield return StartCoroutine(Fade(1f, 0f, fadeOutDuration));
        gameObject.SetActive(false);

        // Restore player input
        SetPlayerInput(true);

        // Show Mr. Ledger's minimap pin
        MinimapPinManager.Instance?.ShowMrLedgerPin();
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        canvasGroup.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }

    void SetPlayerInput(bool enabled)
    {
        var players = FindObjectsByType<PlayerController>();
        foreach (var p in players)
        {
            if (enabled) p.EnableController();
            else p.DisableController();
        }
    }
}