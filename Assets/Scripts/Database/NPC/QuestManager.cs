using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// ABM Academy — Quest Manager (Step 2 update).
/// Coins are now awarded by QuizManager based on quiz score.
/// REPLACE your existing QuestManager.cs with this file.
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public UnityEvent onMrLedgerMet;
    public UnityEvent onAllSpecialistsComplete;

    [Header("HUD — optional")]
    public TMP_Text coinText;

    private bool mrLedgerMet = false;
    private int totalCoins = 0;
    private readonly HashSet<string> completedIDs = new HashSet<string>();
    private const int TOTAL_SPECIALISTS = 6;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool IsMrLedgerMet() => mrLedgerMet;
    public bool IsComplete(string id) => completedIDs.Contains(id);
    public bool AllDone() => completedIDs.Count >= TOTAL_SPECIALISTS;
    public int GetCoins() => totalCoins;

    public void NotifyMrLedgerMet()
    {
        if (mrLedgerMet) return;
        mrLedgerMet = true;
        MinimapPinManager.Instance?.SetPinVisible("mr_ledger", false);
        MinimapPinManager.Instance?.ShowAllSpecialistPins();
        onMrLedgerMet?.Invoke();
        Debug.Log("[QuestManager] Mr. Ledger met. Specialists unlocked.");
    }

    /// <summary>
    /// Called by SpecialistNPCController.OnQuizComplete().
    /// coinsToAdd is passed as 0 — QuizManager calls AddCoins() separately.
    /// </summary>
    public void NotifySpecialistComplete(string npcID, int coinsToAdd)
    {
        if (completedIDs.Contains(npcID)) return;
        completedIDs.Add(npcID);

        MinimapPinManager.Instance?.SetPinVisible(npcID, false);

        if (coinsToAdd > 0) AddCoins(coinsToAdd);

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
        Debug.Log($"[QuestManager] Coins: {totalCoins} (+{amount})");
    }
}