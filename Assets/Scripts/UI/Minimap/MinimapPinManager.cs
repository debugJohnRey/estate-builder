using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ABM Academy — Minimap Pin Manager.
/// Attach to a persistent empty GameObject named "MinimapPinManager" in GameScene.
///
/// This is the ONLY script that controls minimap pin visibility.
/// QuestManager calls it. No other script touches pins directly.
///
/// HOW PINS WORK:
///   Each NPC has a child GameObject with a SpriteRenderer on the MinimapVisible layer.
///   That child is registered here by npcID.
///   This manager shows/hides them — the minimap camera sees them, main camera does not.
///
/// SETUP:
///   1. Create this GameObject in scene, attach MinimapPinManager.cs
///   2. In Inspector, fill the pins array with each NPC's pin entry
///   3. Make sure each pin SpriteRenderer is on Layer: MinimapVisible
///   4. Main camera Culling Mask: UNCHECK MinimapVisible
///   5. MinimapCamera Culling Mask: CHECK MinimapVisible
/// </summary>
public class MinimapPinManager : MonoBehaviour
{
    public static MinimapPinManager Instance { get; private set; }

    [System.Serializable]
    public class NPCPin
    {
        public string npcID;            // must match exactly: "mr_ledger", "fabm", etc.
        public GameObject pinObject;    // the child GO with SpriteRenderer on MinimapVisible layer
    }

    [Header("Register every NPC pin here")]
    public NPCPin[] pins;

    private Dictionary<string, GameObject> pinMap = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Build lookup dictionary
        foreach (var pin in pins)
        {
            if (pin.pinObject != null)
            {
                pinMap[pin.npcID] = pin.pinObject;
                pin.pinObject.SetActive(false); // all hidden at start
            }
        }
    }

    // ── Called by QuestManager ────────────────────────────────────────────────

    public void SetPinVisible(string npcID, bool visible)
    {
        if (pinMap.TryGetValue(npcID, out GameObject pin))
            pin.SetActive(visible);
        else
            Debug.LogWarning($"[MinimapPinManager] No pin registered for ID: '{npcID}'");
    }

    /// Shows Mr. Ledger's pin — called by IntroPopupUI after OK is pressed
    public void ShowMrLedgerPin()
    {
        SetPinVisible("mr_ledger", true);
    }

    /// Shows all 6 specialist pins — called by QuestManager after Mr. Ledger met
    public void ShowAllSpecialistPins()
    {
        SetPinVisible("fabm",  true);
        SetPinVisible("bmath", true);
        SetPinVisible("bfin",  true);
        SetPinVisible("org",   true);
        SetPinVisible("mkt",   true);
        SetPinVisible("bes",   true);
    }
}
