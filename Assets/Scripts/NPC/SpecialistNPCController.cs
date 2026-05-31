using UnityEngine;

/// <summary>
/// ABM Academy — Specialist NPC Controller (Step 2 update).
/// Same as Step 1 but with QuizManager.StartQuiz() now active.
/// REPLACE your existing SpecialistNPCController.cs with this file.
/// </summary>
[RequireComponent(typeof(NPCController))]
public class SpecialistNPCController : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("fabm | bmath | bfin | org | mkt | bes")]
    public string npcID;
    public string displayName;
    public int coinsOnComplete = 40;

    [Header("Dialogue")]
    public NPCDialog briefingDialog;
    public NPCDialog revisitDialog;

    [Header("Quest Markers — child GameObjects")]
    public GameObject lockedMarker;
    public GameObject questMarker;
    public GameObject completedMarker;

    // ── Private ───────────────────────────────────────────────────────────────
    private NPCController npcController;
    private bool isUnlocked = false;
    private bool isComplete = false;
    private bool playerInRange = false;
    private bool isTalking = false;

    void Awake()
    {
        npcController = GetComponent<NPCController>();
    }

    void Start()
    {
        if (QuestManager.Instance != null && QuestManager.Instance.IsMrLedgerMet())
            isUnlocked = true;

        if (QuestManager.Instance != null && QuestManager.Instance.IsComplete(npcID))
            isComplete = true;

        if (QuestManager.Instance != null)
            QuestManager.Instance.onMrLedgerMet.AddListener(Unlock);

        UpdateMarkers();
    }

    void OnDestroy()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.onMrLedgerMet.RemoveListener(Unlock);
    }

    void Update()
    {
        if (!playerInRange || !isUnlocked || isTalking) return;
        if (Input.GetKeyDown(KeyCode.E))
            StartDialogue();
    }

    public void OnPlayerEnter() => playerInRange = true;
    public void OnPlayerExit() => playerInRange = false;

    // ── Dialogue ──────────────────────────────────────────────────────────────

    void StartDialogue()
    {
        if (DialogManager.Instance == null || isTalking) return;

        isTalking = true;

        var anim = GetComponent<Animator>();
        if (anim != null) anim.SetBool("Wave", true);

        NPCDialog toPlay = (isComplete && revisitDialog != null)
            ? revisitDialog : briefingDialog;

        DialogManager.Instance.StartDialog(toPlay, OnDialogueEnd);
    }

    void OnDialogueEnd()
    {
        isTalking = false;

        var anim = GetComponent<Animator>();
        if (anim != null) anim.SetBool("Wave", false);

        if (!isComplete)
        {
            // ── LAUNCH QUIZ ───────────────────────────────────────────────
            QuizManager.Instance?.StartQuiz(npcID, OnQuizComplete);
        }
    }

    public void OnQuizComplete(bool passed)
    {
        if (!isComplete)
        {
            isComplete = true;
            UpdateMarkers();
            // Coins are calculated and added inside QuizManager based on score
            // QuestManager just marks it complete and hides the pin
            QuestManager.Instance?.NotifySpecialistComplete(npcID, 0);
            // Note: pass 0 here — QuizManager.AddCoins() handles the actual amount
        }

        Debug.Log($"[SpecialistNPC] {displayName} quiz result: {(passed ? "PASSED" : "FAILED")}");
    }

    void Unlock()
    {
        isUnlocked = true;
        UpdateMarkers();
    }

    void UpdateMarkers()
    {
        if (lockedMarker != null) lockedMarker.SetActive(!isUnlocked);
        if (questMarker != null) questMarker.SetActive(isUnlocked && !isComplete);
        if (completedMarker != null) completedMarker.SetActive(isComplete);
    }
}