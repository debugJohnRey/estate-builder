using UnityEngine;

/// <summary>
/// ABM Academy — Mr. Ledger (Main NPC) Controller.
/// Attach alongside NPCController on the MrLedgerNPC GameObject.
///
/// BEHAVIOUR:
///   • First visit  → auto-triggers after player enters zone (no E needed)
///   • Revisit      → player presses E to talk again
///   • All done     → plays allDoneDialog instead
///
/// SETUP:
///   1. Attach to MrLedgerNPC alongside existing NPCController
///   2. Set NPCController.dialog = None
///   3. Assign the three NPCDialog assets in Inspector
///   4. Assign questMarker and completedMarker child GameObjects
///   5. Replace InteractionZone.cs on trigger child with InteractionZone_Quest.cs
/// </summary>
[RequireComponent(typeof(NPCController))]
public class MrLedgerController : MonoBehaviour
{
    [Header("Dialogue — create via Right-click > Create > NPC > Dialog")]
    public NPCDialog introDialog;    // First visit
    public NPCDialog revisitDialog;  // After first talk, specialists not all done
    public NPCDialog allDoneDialog;  // After all 6 specialists complete

    [Header("Quest Markers — child GameObjects")]
    public GameObject questMarker;      // "!" shown before player talks to him
    public GameObject completedMarker;  // "✓" shown after player talks to him

    [Header("Auto-trigger Settings")]
    [Tooltip("Delay in seconds before dialogue auto-fires on first entry")]
    public float autoTriggerDelay = 0.8f;

    // ── Private ───────────────────────────────────────────────────────────────
    private NPCController npcController;
    private bool playerInRange     = false;
    private bool hasMetOnce        = false;  // true after first dialogue ends
    private bool isTalking         = false;

    void Awake()
    {
        npcController = GetComponent<NPCController>();
    }

    void Start()
    {
        UpdateMarkers();
    }

    void Update()
    {
        // Revisit — player presses E after first meeting
        if (playerInRange && hasMetOnce && !isTalking
            && Input.GetKeyDown(KeyCode.E))
        {
            StartDialogue();
        }
    }

    // ── Called by InteractionZone_Quest ──────────────────────────────────────

    public void OnPlayerEnter()
    {
        playerInRange = true;

        // First visit only: auto-fire after short delay
        if (!hasMetOnce && !isTalking)
            Invoke(nameof(StartDialogue), autoTriggerDelay);
    }

    public void OnPlayerExit()
    {
        playerInRange = false;
        CancelInvoke(nameof(StartDialogue));
    }

    // ── Dialogue ──────────────────────────────────────────────────────────────

    void StartDialogue()
    {
        // Guard: player must still be in range (may have walked away during delay)
        if (!playerInRange || isTalking || DialogManager.Instance == null) return;

        isTalking = true;

        // Wave animation via existing NPCController animator
        var anim = GetComponent<Animator>();
        if (anim != null) anim.SetBool("Wave", true);

        DialogManager.Instance.StartDialog(PickDialog(), OnDialogueEnd);
    }

    NPCDialog PickDialog()
    {
        if (QuestManager.Instance != null && QuestManager.Instance.AllDone()
            && allDoneDialog != null)
            return allDoneDialog;

        if (hasMetOnce && revisitDialog != null)
            return revisitDialog;

        return introDialog;
    }

    void OnDialogueEnd()
    {
        isTalking  = false;
        hasMetOnce = true;

        // Stop wave animation
        var anim = GetComponent<Animator>();
        if (anim != null) anim.SetBool("Wave", false);

        // Notify QuestManager — unlocks specialist NPCs and shows their pins
        QuestManager.Instance?.NotifyMrLedgerMet();

        UpdateMarkers();
    }

    void UpdateMarkers()
    {
        bool met = hasMetOnce ||
                   (QuestManager.Instance != null && QuestManager.Instance.IsMrLedgerMet());

        if (questMarker     != null) questMarker.SetActive(!met);
        if (completedMarker != null) completedMarker.SetActive(met);
    }
}
