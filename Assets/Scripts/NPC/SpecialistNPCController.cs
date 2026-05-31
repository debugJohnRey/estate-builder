using UnityEngine;

/// <summary>
/// ABM Academy — Specialist NPC Controller.
/// Attach alongside NPCController on each of the 6 specialist NPCs.
///
/// BEHAVIOUR:
///   • Locked (no interaction) until Mr. Ledger is met
///   • Unlocks when QuestManager.onMrLedgerMet fires
///   • Player presses E → plays briefing dialogue
///   • After briefing → hook your quiz here
///   • Revisit after complete → plays revisitDialog
///
/// NPC IDs — use exactly these strings in the npcID field:
///   fabm | bmath | bfin | org | mkt | bes
///
/// SETUP:
///   1. Attach to specialist NPC alongside NPCController
///   2. Set NPCController.dialog = None
///   3. Assign npcID, displayName, coinsOnComplete
///   4. Assign briefingDialog and revisitDialog NPCDialog assets
///   5. Assign lockedMarker, questMarker, completedMarker child GOs
///   6. Replace InteractionZone.cs on trigger child with InteractionZone_Quest.cs
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
    public NPCDialog briefingDialog;  // Played on first interaction
    public NPCDialog revisitDialog;   // Played on repeat visits after completion

    [Header("Quest Markers — child GameObjects")]
    public GameObject lockedMarker;     // "?" visible before Mr. Ledger is met
    public GameObject questMarker;      // "!" visible when available, not done
    public GameObject completedMarker;  // "✓" visible after task done

    // ── Private ───────────────────────────────────────────────────────────────
    private NPCController npcController;
    private bool isUnlocked  = false;
    private bool isComplete  = false;
    private bool playerInRange = false;
    private bool isTalking   = false;

    void Awake()
    {
        npcController = GetComponent<NPCController>();
    }

    void Start()
    {
        // Already unlocked if Mr. Ledger was met before this scene loaded
        if (QuestManager.Instance != null && QuestManager.Instance.IsMrLedgerMet())
            isUnlocked = true;

        if (QuestManager.Instance != null && QuestManager.Instance.IsComplete(npcID))
            isComplete = true;

        // Subscribe to unlock event
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

    // ── Called by InteractionZone_Quest ──────────────────────────────────────

    public void OnPlayerEnter()
    {
        playerInRange = true;
    }

    public void OnPlayerExit()
    {
        playerInRange = false;
    }

    // ── Dialogue ──────────────────────────────────────────────────────────────

    void StartDialogue()
    {
        if (DialogManager.Instance == null || isTalking) return;

        isTalking = true;

        var anim = GetComponent<Animator>();
        if (anim != null) anim.SetBool("Wave", true);

        NPCDialog toPlay = (isComplete && revisitDialog != null)
            ? revisitDialog
            : briefingDialog;

        DialogManager.Instance.StartDialog(toPlay, OnDialogueEnd);
    }

    void OnDialogueEnd()
    {
        isTalking = false;

        var anim = GetComponent<Animator>();
        if (anim != null) anim.SetBool("Wave", false);

        if (!isComplete)
        {
            isComplete = true;
            UpdateMarkers();
            QuestManager.Instance?.NotifySpecialistComplete(npcID, coinsOnComplete);

            // ── HOOK YOUR QUIZ HERE ───────────────────────────────────────
            // QuizManager.Instance?.StartQuiz(npcID, OnQuizComplete);
            // For now, completion is immediate. Replace with your quiz call.
            Debug.Log($"[SpecialistNPC] {displayName} briefing done. Hook quiz for: {npcID}");
        }
    }

    // Call this from your QuizManager when quiz ends
    public void OnQuizComplete(bool passed)
    {
        Debug.Log($"[SpecialistNPC] {displayName} quiz result: {(passed ? "PASSED" : "FAILED")}");
    }

    // ── Event listener ────────────────────────────────────────────────────────

    void Unlock()
    {
        isUnlocked = true;
        UpdateMarkers();
        Debug.Log($"[SpecialistNPC] {displayName} is now available.");
    }

    void UpdateMarkers()
    {
        if (lockedMarker    != null) lockedMarker.SetActive(!isUnlocked);
        if (questMarker     != null) questMarker.SetActive(isUnlocked && !isComplete);
        if (completedMarker != null) completedMarker.SetActive(isComplete);
    }
}
