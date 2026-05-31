using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance;

    [Header("UI References")]
    public GameObject interactionPrompt;
    public GameObject dialogPanel;
    public TMP_Text npcNameText;
    public TMP_Text dialogText;
    public Button nextButton;
    public TMP_Text nextButtonLabel;
    public Button closeButton;

    [Header("Typewriter Settings")]
    public float typewriterSpeed = 0.03f;

    private NPCDialog currentDialog;
    private int currentLineIndex = 0;
    private System.Action onDialogEndCallback;
    private Coroutine typewriterCoroutine;
    private bool isTyping = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        interactionPrompt.SetActive(false);
        dialogPanel.SetActive(false);

        // Fallback: auto-find the label if it wasn't wired in the Inspector
        if (nextButtonLabel == null)
            nextButtonLabel = nextButton.GetComponentInChildren<TMP_Text>();

        if (nextButtonLabel == null)
            Debug.LogError("[DialogManager] nextButtonLabel is not assigned and could not be found on nextButton's children.", this);

        nextButton.onClick.AddListener(NextLine);
        closeButton.onClick.AddListener(CloseDialog);
    }

    public void ShowPrompt(bool show)
    {
        interactionPrompt.SetActive(show);
    }

    public void StartDialog(NPCDialog dialog, System.Action onEnd = null)
    {
        if (dialog == null)
        {
            Debug.LogError("[DialogManager] StartDialog called with a null NPCDialog. Assign the dialog asset in the Inspector.");
            return;
        }

        if (dialog.lines == null || dialog.lines.Length == 0)
        {
            Debug.LogError($"[DialogManager] NPCDialog '{dialog.name}' has no lines. Add at least one line to the asset.");
            return;
        }

        currentDialog = dialog;
        currentLineIndex = 0;
        onDialogEndCallback = onEnd;

        interactionPrompt.SetActive(false);
        dialogPanel.SetActive(true);

        npcNameText.text = dialog.npcName;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SetPlayerInput(false);

        ShowLine(currentLineIndex);
    }

    void ShowLine(int index)
    {
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        typewriterCoroutine = StartCoroutine(
            TypewriterEffect(currentDialog.lines[index]));
    }

    IEnumerator TypewriterEffect(string line)
    {
        isTyping = true;
        dialogText.text = "";
        nextButtonLabel.text = "Skip";

        foreach (char c in line)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;
        nextButtonLabel.text =
            currentLineIndex < currentDialog.lines.Length - 1 ? "Next" : "Close";
    }

    void NextLine()
    {
        // if still typing, skip to end of current line
        if (isTyping)
        {
            StopCoroutine(typewriterCoroutine);
            dialogText.text = currentDialog.lines[currentLineIndex];
            isTyping = false;
            nextButtonLabel.text =
                currentLineIndex < currentDialog.lines.Length - 1 ? "Next" : "Close";
            return;
        }

        currentLineIndex++;

        if (currentLineIndex < currentDialog.lines.Length)
            ShowLine(currentLineIndex);
        else
            CloseDialog();
    }

    public void CloseDialog()
    {
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        dialogPanel.SetActive(false);
        SetPlayerInput(true);
        onDialogEndCallback?.Invoke();

        bool isFirstPerson = PlayerPrefs.GetInt("POVMode", 0) == 1;
        if (isFirstPerson)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void SetPlayerInput(bool enable)
    {
        foreach (var p in FindObjectsByType<PlayerController>())
        {
            if (enable) p.EnableController();
            else        p.DisableController();
        }
    }
}