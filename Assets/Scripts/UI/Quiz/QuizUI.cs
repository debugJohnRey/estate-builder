using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ABM Academy — Quiz UI Controller.
/// Attach to the QuizPanel GameObject.
///
/// HIERARCHY:
///   QuizPanel  ← attach this script
///   ├── SubjectBadge       (Image)
///   │   └── SubjectLabel   (TMP)
///   ├── ProgressText       (TMP)  "Question 1 of 3"
///   ├── QuestionText       (TMP)
///   ├── ChoicesContainer   (VerticalLayoutGroup)
///   │   ├── ChoiceButton_A (Button → ChoiceButtonUI)
///   │   ├── ChoiceButton_B
///   │   ├── ChoiceButton_C
///   │   └── ChoiceButton_D
///   ├── FeedbackPanel      (Image — hidden by default)
///   │   ├── FeedbackIcon   (TMP)   "✓" or "✗"
///   │   └── FeedbackText   (TMP)   explanation text
///   └── ContinueButton     (Button)
/// </summary>
public class QuizUI : MonoBehaviour
{
    [Header("Header")]
    public Image subjectBadge;
    public TMP_Text subjectLabel;
    public TMP_Text progressText;

    [Header("Question")]
    public TMP_Text questionText;

    [Header("Choices — assign A B C D in order")]
    public Button[] choiceButtons = new Button[4];
    public TMP_Text[] choiceLabels = new TMP_Text[4];
    public Image[] choiceImages = new Image[4];  // backgrounds for color feedback

    [Header("Feedback")]
    public GameObject feedbackPanel;
    public TMP_Text feedbackIcon;   // "✓" or "✗"
    public TMP_Text feedbackText;   // explanation

    [Header("Continue")]
    public Button continueButton;
    public TMP_Text continueLabel;

    [Header("Panel Animation")]
    public CanvasGroup canvasGroup;
    public float fadeSpeed = 0.3f;

    [Header("Colors")]
    public Color correctColor = new Color(0.22f, 0.60f, 0.29f); // green
    public Color wrongColor = new Color(0.75f, 0.22f, 0.22f); // red
    public Color neutralColor = new Color(0.20f, 0.20f, 0.30f); // default button bg
    public Color feedbackCorrectBg = new Color(0.18f, 0.45f, 0.22f);
    public Color feedbackWrongBg = new Color(0.55f, 0.15f, 0.15f);

    // ── Private ───────────────────────────────────────────────────────────────
    private QuizData quizData;
    private int questionIndex;
    private System.Action<bool> onAnswerCallback;
    private System.Action onFinishedCallback;
    private bool hasAnswered;
    private List<QuizResultsUI.ResultEntry> resultEntries = new List<QuizResultsUI.ResultEntry>();

    [Header("Results Screen")]
    public QuizResultsUI resultsUI;   // assign QuizResultsUI component

    // ─────────────────────────────────────────────────────────────────────────
    // Called by QuizManager.StartQuiz()
    // ─────────────────────────────────────────────────────────────────────────
    public void OpenQuiz(QuizData data, System.Action<bool> onAnswer, System.Action onFinished)
    {
        quizData = data;
        questionIndex = 0;
        onAnswerCallback = onAnswer;
        onFinishedCallback = onFinished;
        resultEntries.Clear();

        // Subject badge color
        if (subjectBadge != null) subjectBadge.color = data.subjectColor;
        if (subjectLabel != null) subjectLabel.text = data.subjectName;

        gameObject.SetActive(true);
        feedbackPanel.SetActive(false);
        continueButton.gameObject.SetActive(false);

        StartCoroutine(FadeIn());
        LoadQuestion(questionIndex);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Load a question into the UI
    // ─────────────────────────────────────────────────────────────────────────
    void LoadQuestion(int index)
    {
        if (index >= quizData.questions.Length) return;

        hasAnswered = false;
        var q = quizData.questions[index];

        progressText.text = $"Question {index + 1} of {quizData.questions.Length}";
        questionText.text = q.questionText;

        feedbackPanel.SetActive(false);
        continueButton.gameObject.SetActive(false);

        // Wire choice buttons
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int capturedIndex = i; // capture for lambda
            choiceLabels[i].text = $"{ChoiceLetter(i)})  {q.choices[i]}";

            // Reset color
            if (choiceImages[i] != null)
                choiceImages[i].color = neutralColor;

            // Re-enable and clear listeners
            choiceButtons[i].interactable = true;
            choiceButtons[i].onClick.RemoveAllListeners();
            choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(capturedIndex));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Player picks an answer
    // ─────────────────────────────────────────────────────────────────────────
    void OnChoiceSelected(int selectedIndex)
    {
        if (hasAnswered) return;
        hasAnswered = true;

        var q = quizData.questions[questionIndex];
        bool correct = selectedIndex == q.correctIndex;

        // Disable all buttons
        foreach (var btn in choiceButtons)
            btn.interactable = false;

        // Color correct green, selected-wrong red
        if (choiceImages[q.correctIndex] != null)
            choiceImages[q.correctIndex].color = correctColor;

        if (!correct && choiceImages[selectedIndex] != null)
            choiceImages[selectedIndex].color = wrongColor;

        // Feedback panel
        feedbackPanel.SetActive(true);
        if (subjectBadge != null)
            feedbackPanel.GetComponent<Image>().color =
                correct ? feedbackCorrectBg : feedbackWrongBg;

        feedbackIcon.text = correct ? "✓  Correct!" : "✗  Incorrect";
        feedbackText.text = q.explanation;

        // Record result for results screen
        resultEntries.Add(new QuizResultsUI.ResultEntry
        {
            questionText = q.questionText,
            yourAnswer = q.choices[selectedIndex],
            correctAnswer = q.choices[q.correctIndex],
            wasCorrect = correct
        });

        // Notify QuizManager
        onAnswerCallback?.Invoke(correct);

        // Show continue button
        bool isLast = questionIndex >= quizData.questions.Length - 1;
        continueLabel.text = isLast ? "See Results" : "Next Question";
        continueButton.gameObject.SetActive(true);
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(isLast ? OnLastContinue : OnNextContinue);
    }

    void OnNextContinue()
    {
        questionIndex++;
        LoadQuestion(questionIndex);
    }

    void OnLastContinue()
    {
        StartCoroutine(FadeOut(() =>
        {
            gameObject.SetActive(false);
            onFinishedCallback?.Invoke();
        }));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Called by QuizManager to show results screen
    // ─────────────────────────────────────────────────────────────────────────
    public void ShowResults(int correct, int total, int coinsEarned,
                            bool firstTime, System.Action onDismiss)
    {
        resultsUI?.Show(correct, total, coinsEarned, firstTime,
                        quizData.subjectName, quizData.subjectColor,
                        resultEntries, onDismiss);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────
    string ChoiceLetter(int i)
    {
        switch (i)
        {
            case 0: return "A";
            case 1: return "B";
            case 2: return "C";
            case 3: return "D";
            default: return "?";
        }
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