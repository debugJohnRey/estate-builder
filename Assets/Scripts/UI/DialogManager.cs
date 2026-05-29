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

        nextButton.onClick.AddListener(NextLine);
        closeButton.onClick.AddListener(CloseDialog);
    }

    public void ShowPrompt(bool show)
    {
        interactionPrompt.SetActive(show);
    }

    public void StartDialog(NPCDialog dialog, System.Action onEnd = null)
    {
        currentDialog = dialog;
        currentLineIndex = 0;
        onDialogEndCallback = onEnd;

        interactionPrompt.SetActive(false);
        dialogPanel.SetActive(true);

        npcNameText.text = dialog.npcName;

        // pause player movement
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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
        nextButton.GetComponentInChildren<TMP_Text>().text = "Skip";

        foreach (char c in line)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;
        nextButton.GetComponentInChildren<TMP_Text>().text =
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
            nextButton.GetComponentInChildren<TMP_Text>().text =
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
        onDialogEndCallback?.Invoke();

        // restore cursor state based on POV
        bool isFirstPerson = PlayerPrefs.GetInt("POVMode", 0) == 1;
        if (isFirstPerson)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}