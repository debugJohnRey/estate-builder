using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// ABM Academy — Central quest state manager.
/// Attach to a persistent empty GameObject named "QuestManager" in GameScene.
/// Does NOT use DontDestroyOnLoad — lives only in the game scene.
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    // ── Events ────────────────────────────────────────────────────────────────
    /// Fires once when player finishes Mr. Ledger's intro dialogue
    public UnityEvent onMrLedgerMet;
    /// Fires once when all 6 specialists are complete
    public UnityEvent onAllSpecialistsComplete;

    // ── HUD (optional) ────────────────────────────────────────────────────────
    [Header("HUD — optional")]
    public TMP_Text coinText;   // drag your coin counter TMP here

    // ── Runtime state ─────────────────────────────────────────────────────────
    private bool mrLedgerMet   = false;
    private int  totalCoins    = 0;
    private readonly HashSet<string> completedIDs = new HashSet<string>();
    private const int TOTAL_SPECIALISTS = 6;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── Public queries ────────────────────────────────────────────────────────
    public bool IsMrLedgerMet()          => mrLedgerMet;
    public bool IsComplete(string id)    => completedIDs.Contains(id);
    public bool AllDone()                => completedIDs.Count >= TOTAL_SPECIALISTS;
    public int  GetCoins()               => totalCoins;

    // ── Called by MrLedgerController after his dialogue ends ─────────────────
    public void NotifyMrLedgerMet()
    {
        if (mrLedgerMet) return;
        mrLedgerMet = true;

        // Hide Mr. Ledger's minimap pin
        MinimapPinManager.Instance?.SetPinVisible("mr_ledger", false);

        // Show all 6 specialist pins
        MinimapPinManager.Instance?.ShowAllSpecialistPins();

        onMrLedgerMet?.Invoke();
        Debug.Log("[QuestManager] Mr. Ledger met. Specialists unlocked.");
    }

    // ── Called by SpecialistNPCController after task complete ─────────────────
    public void NotifySpecialistComplete(string npcID, int coins)
    {
        if (completedIDs.Contains(npcID)) return;
        completedIDs.Add(npcID);

        // Hide that specialist's pin
        MinimapPinManager.Instance?.SetPinVisible(npcID, false);

        AddCoins(coins);
        Debug.Log($"[QuestManager] {npcID} complete. {completedIDs.Count}/{TOTAL_SPECIALISTS}");

        if (AllDone())
        {
            onAllSpecialistsComplete?.Invoke();
            Debug.Log("[QuestManager] All specialists done!");
        }
    }

    public void AddCoins(int amount)
    {
        totalCoins += amount;
        if (coinText != null) coinText.text = totalCoins.ToString();
    }
}
