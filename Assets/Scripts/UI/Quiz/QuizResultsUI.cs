using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ABM Academy — Quiz Results Screen.
/// Shown after the player answers all questions.
///
/// HIERARCHY:
///   ResultsPanel  ← attach this script
///   ├── SubjectLabel      (TMP)
///   ├── ScoreText         (TMP)   "2 / 3 Correct"
///   ├── CoinsText         (TMP)   "+30 Coins Earned!"
///   ├── CoinsPanel        (Image) — hidden if 0 coins / retry
///   ├── ReviewContainer   (ScrollRect > VerticalLayoutGroup)
///   │   └── ReviewItemPrefab (spawned per question)
///   │       ├── StatusIcon  (TMP) "✓" or "✗"
///   │       ├── QuestionText(TMP)
///   │       ├── YourAnswer  (TMP)
///   │       └── CorrectAnswer(TMP)
///   ├── RetryButton       (Button) — shown if score < 100%
///   └── ContinueButton    (Button)
/// </summary>
public class QuizResultsUI : MonoBehaviour
{
    [System.Serializable]
    public class ResultEntry
    {
        public string questionText;
        public string yourAnswer;
        public string correctAnswer;
        public bool wasCorrect;
    }

    [Header("Header")]
    public TMP_Text subjectLabel;
    public Image subjectBadge;

    [Header("Score")]
    public TMP_Text scoreText;      // "2 / 3 Correct"
    public TMP_Text gradeText;      // "Great Job!" / "Keep Practicing!"

    [Header("Coins")]
    public GameObject coinsPanel;
    public TMP_Text coinsText;    // "+30 Coins Earned!"

    [Header("Review List")]
    public Transform reviewContainer;  // parent for review items
    public GameObject reviewItemPrefab; // prefab with status, question, answers

    [Header("Buttons")]
    public Button retryButton;
    public Button continueButton;

    [Header("Panel")]
    public CanvasGroup canvasGroup;
    public float fadeSpeed = 0.3f;

    [Header("Colors")]
    public Color correctColor = new Color(0.22f, 0.60f, 0.29f);
    public Color wrongColor = new Color(0.75f, 0.22f, 0.22f);

    private System.Action onDismissCallback;
    private System.Action onRetryCallback;

    // ─────────────────────────────────────────────────────────────────────────
    // Called by QuizUI.ShowResults()
    // ─────────────────────────────────────────────────────────────────────────
    public void Show(int correct, int total, int coinsEarned, bool firstTime,
                     string subject, Color subjectColor,
                     List<ResultEntry> entries, System.Action onDismiss)
    {
        onDismissCallback = onDismiss;

        // Subject
        if (subjectLabel != null) subjectLabel.text = subject;
        if (subjectBadge != null) subjectBadge.color = subjectColor;

        // Score
        scoreText.text = $"{correct} / {total} Correct";
        gradeText.text = GetGradeMessage(correct, total);

        // Coins
        bool showCoins = coinsEarned > 0 && firstTime;
        if (coinsPanel != null) coinsPanel.SetActive(showCoins);
        if (coinsText != null) coinsText.text = $"+{coinsEarned} Coins Earned!";

        // Review list
        BuildReviewList(entries);

        // Retry button — show if not perfect
        bool perfect = correct == total;
        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(!perfect);
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(OnRetryPressed);
        }

        // Continue button
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(OnContinuePressed);
        continueButton.GetComponentInChildren<TMP_Text>().text =
            perfect ? "Continue  ✓" : "Continue";

        gameObject.SetActive(true);
        StartCoroutine(FadeIn());
    }

    void BuildReviewList(List<ResultEntry> entries)
    {
        if (reviewContainer == null || reviewItemPrefab == null) return;

        // Clear old items
        foreach (Transform child in reviewContainer)
            Destroy(child.gameObject);

        foreach (var entry in entries)
        {
            GameObject item = Instantiate(reviewItemPrefab, reviewContainer);

            // Find child TMP components by name
            SetChildText(item, "StatusIcon", entry.wasCorrect ? "✓" : "✗");
            SetChildText(item, "QuestionText", entry.questionText);
            SetChildText(item, "YourAnswer", $"Your answer: {entry.yourAnswer}");
            SetChildText(item, "CorrectAnswer",
                entry.wasCorrect ? "" : $"Correct: {entry.correctAnswer}");

            // Color the status icon
            var icon = item.transform.Find("StatusIcon")?.GetComponent<TMP_Text>();
            if (icon != null)
                icon.color = entry.wasCorrect ? correctColor : wrongColor;

            // Hide correct answer line if they got it right
            var correctLine = item.transform.Find("CorrectAnswer")?.gameObject;
            if (correctLine != null)
                correctLine.SetActive(!entry.wasCorrect);
        }
    }

    void SetChildText(GameObject parent, string childName, string text)
    {
        var child = parent.transform.Find(childName);
        if (child != null)
        {
            var tmp = child.GetComponent<TMP_Text>();
            if (tmp != null) tmp.text = text;
        }
    }

    string GetGradeMessage(int correct, int total)
    {
        float ratio = total > 0 ? (float)correct / total : 0f;
        if (ratio >= 1f) return "Perfect Score! Outstanding!";
        if (ratio >= 0.67f) return "Great Job! Keep it up!";
        if (ratio >= 0.33f) return "Not bad — review and try again!";
        return "Keep studying — you can do it!";
    }

    void OnContinuePressed()
    {
        StartCoroutine(FadeOut(() =>
        {
            gameObject.SetActive(false);
            onDismissCallback?.Invoke();
        }));
    }

    void OnRetryPressed()
    {
        // Hide results, reopen quiz from QuizManager
        StartCoroutine(FadeOut(() =>
        {
            gameObject.SetActive(false);
            // Re-trigger the same quiz — QuizManager handles reset
            // The specialist NPC will need to be talked to again for a full retry
            // or you can store the last npcID and re-call StartQuiz
            onDismissCallback?.Invoke();
        }));
    }

    IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        canvasGroup.alpha = 0f;
        float elapsed = 0f;
        while (elapsed < fadeSpeed)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeSpeed);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    IEnumerator FadeOut(System.Action onDone)
    {
        if (canvasGroup == null) { onDone?.Invoke(); yield break; }
        float elapsed = 0f;
        while (elapsed < fadeSpeed)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeSpeed);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        onDone?.Invoke();
    }
}