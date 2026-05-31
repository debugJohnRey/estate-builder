using UnityEngine;

/// <summary>
/// ABM Academy — Quest Interaction Zone.
/// REPLACES InteractionZone.cs on the trigger child of:
///   - MrLedgerNPC
///   - All 6 specialist NPCs
///
/// Still calls your existing NPCController.SetPlayerInRange()
/// and DialogManager.ShowPrompt() exactly as before.
/// Additionally notifies MrLedgerController or SpecialistNPCController.
///
/// SETUP:
///   1. On the trigger child GO, remove InteractionZone.cs
///   2. Add this script instead
///   3. No other changes needed
/// </summary>
public class InteractionZone_Quest : MonoBehaviour
{
    private NPCController            npcController;
    private MrLedgerController       mrLedger;
    private SpecialistNPCController  specialist;

    void Start()
    {
        npcController = GetComponentInParent<NPCController>();
        mrLedger      = GetComponentInParent<MrLedgerController>();
        specialist    = GetComponentInParent<SpecialistNPCController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // ── Your existing system ──────────────────────────────────────────
        npcController?.SetPlayerInRange(true);
        DialogManager.Instance?.ShowPrompt(true);

        // ── ABM quest system ──────────────────────────────────────────────
        mrLedger?.OnPlayerEnter();
        specialist?.OnPlayerEnter();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // ── Your existing system ──────────────────────────────────────────
        npcController?.SetPlayerInRange(false);
        DialogManager.Instance?.ShowPrompt(false);

        // ── ABM quest system ──────────────────────────────────────────────
        mrLedger?.OnPlayerExit();
        specialist?.OnPlayerExit();
    }
}
