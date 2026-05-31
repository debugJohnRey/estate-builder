using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ABM Academy — Quiz Manager.
/// Attach to a persistent empty GameObject named "QuizManager" in GameScene.
///
/// RESPONSIBILITIES:
///   • Holds all QuizData assets (drag them in Inspector)
///   • Launched by SpecialistNPCController.OnDialogueEnd()
///   • Passes questions to QuizUI
///   • Calculates coins earned based on score
///   • Calls back SpecialistNPCController when quiz is done
///
/// SETUP:
///   1. Create empty GO → name "QuizManager"
///   2. Attach QuizManager.cs
///   3. Drag all 6 QuizData assets into the quizDataList array
/// </summary>
public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance { get; private set; }

    [Header("Quiz Data — drag all 6 QuizData assets here")]
    public QuizData[] quizDataList;

    [Header("References")]
    public QuizUI quizUI;           // drag QuizUI GameObject here

    // ── Runtime ───────────────────────────────────────────────────────────────
    private QuizData activeQuizData;
    private int currentQuestionIndex;
    private int correctAnswers;
    private int totalQuestions;
    private System.Action<bool> onQuizCompleteCallback;

    // Tracks which npcIDs have already been REWARDED (not just completed)
    // so retry doesn't give coins again — QuestManager handles that too,
    // but we track locally for the results screen display
    private HashSet<string> rewardedIDs = new HashSet<string>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── Called by SpecialistNPCController.OnDialogueEnd() ────────────────────
    public void StartQuiz(string npcID, System.Action<bool> onComplete)
    {
        QuizData data = FindQuizData(npcID);

        if (data == null)
        {
            Debug.LogWarning($"[QuizManager] No QuizData found for npcID: '{npcID}'");
            onComplete?.Invoke(false);
            return;
        }

        activeQuizData = data;
        currentQuestionIndex = 0;
        correctAnswers = 0;
        totalQuestions = data.questions.Length;
        onQuizCompleteCallback = onComplete;

        // Disable player movement during quiz
        SetPlayerMovement(false);

        // Tell QuizUI to show itself and load first question
        quizUI?.OpenQuiz(data, OnAnswerSubmitted, OnQuizFinished);

        Debug.Log($"[QuizManager] Starting quiz for: {npcID} ({totalQuestions} questions)");
    }

    // ── Called by QuizUI each time player picks an answer ────────────────────
    public void OnAnswerSubmitted(bool wasCorrect)
    {
        if (wasCorrect) correctAnswers++;
    }

    // ── Called by QuizUI after last question's Continue is pressed ───────────
    public void OnQuizFinished()
    {
        bool passed = correctAnswers > 0;

        // Calculate coins: proportional to score
        int coinsEarned = CalculateCoins();
        bool firstTime = !rewardedIDs.Contains(activeQuizData.npcID);

        if (firstTime)
            rewardedIDs.Add(activeQuizData.npcID);

        // Re-enable player
        SetPlayerMovement(true);

        // Show results screen
        quizUI?.ShowResults(
            correctAnswers,
            totalQuestions,
            coinsEarned,
            firstTime,
            OnResultsDismissed
        );

        Debug.Log($"[QuizManager] Quiz done. {correctAnswers}/{totalQuestions} correct. " +
                  $"Coins: {coinsEarned}. FirstTime: {firstTime}");
    }

    // ── Called by QuizUI when player taps Continue on results screen ─────────
    void OnResultsDismissed()
    {
        bool passed = correctAnswers > 0;
        onQuizCompleteCallback?.Invoke(passed);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    int CalculateCoins()
    {
        if (activeQuizData == null || totalQuestions == 0) return 0;

        // Base coins come from SpecialistNPCController.coinsOnComplete
        // Here we calculate the proportion earned
        float ratio = (float)correctAnswers / totalQuestions;

        // Get base coins from QuestManager specialist data
        // We use a flat lookup — you can wire this per-specialist if preferred
        int baseCoins = GetBaseCoins(activeQuizData.npcID);

        if (ratio >= 1f) return baseCoins;           // all correct  → full
        if (ratio >= 0.67f) return Mathf.RoundToInt(baseCoins * 0.75f); // 2/3 → 75%
        if (ratio >= 0.33f) return Mathf.RoundToInt(baseCoins * 0.5f);  // 1/3 → 50%
        return 0;                                       // 0 correct   → no coins
    }

    int GetBaseCoins(string npcID)
    {
        // Matches coinsOnComplete in SpecialistNPCController
        switch (npcID)
        {
            case "fabm": return 40;
            case "bmath": return 35;
            case "bfin": return 45;
            case "org": return 35;
            case "mkt": return 40;
            case "bes": return 60;
            default: return 30;
        }
    }

    QuizData FindQuizData(string npcID)
    {
        foreach (var data in quizDataList)
            if (data != null && data.npcID == npcID)
                return data;
        return null;
    }

    void SetPlayerMovement(bool enabled)
    {
        var players = FindObjectsByType<PlayerController>();
        foreach (var p in players)
        {
            if (enabled) p.EnableController();
            else p.DisableController();
        }
    }
}