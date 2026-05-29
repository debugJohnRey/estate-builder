using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Gender + name selection screen.
///
/// BUTTON:  When clicked → Image tint goes gray (sprite darkens).
///          Other button  → resets to white (sprite normal).
///
/// OUTLINE: m_outline (blue neon)  shown/hidden for Male selection.
///          f_outline (pink neon)  shown/hidden for Female selection.
///          Assign both GameObjects in the Inspector on UIManager.
/// </summary>
public class CharacterSelect : MonoBehaviour
{
    // ── Inspector fields ─────────────────────────────────────────────────────

    [Header("Input / Confirm")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button confirmButton;

    [Header("Gender Buttons")]
    [SerializeField] private Button maleButton;
    [SerializeField] private Button femaleButton;

    [Header("Character Outline GameObjects")]
    [Tooltip("The 'm_outline' GameObject (blue neon sprite) — shown when Male is selected.")]
    [SerializeField] private GameObject maleOutlineObject;
    [Tooltip("The 'f_outline' GameObject (pink neon sprite) — shown when Female is selected.")]
    [SerializeField] private GameObject femaleOutlineObject;

    [Header("Button Tint Colours")]
    [SerializeField] private Color selectedButtonColor = new Color(0.55f, 0.55f, 0.55f, 1f); // gray
    [SerializeField] private Color defaultButtonColor = Color.white;

    // ── Private state ─────────────────────────────────────────────────────────

    private string selectedGender = "";
    private string playerName = "";

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    void Start()
    {
        maleButton.onClick.AddListener(() => SelectGender("Male", maleButton, maleOutlineObject));
        femaleButton.onClick.AddListener(() => SelectGender("Female", femaleButton, femaleOutlineObject));
        confirmButton.onClick.AddListener(ConfirmSelection);

        nameInputField.onValueChanged.AddListener(_ => OnNameChanged());

        ResetVisuals();
        confirmButton.interactable = false;
    }

    // ── Gender selection ──────────────────────────────────────────────────────

    void SelectGender(string gender, Button clickedBtn, GameObject outlineToShow)
    {
        selectedGender = gender;

        // 1. Reset all visuals first
        ResetVisuals();

        // 2. Gray-out the selected button
        clickedBtn.image.color = selectedButtonColor;

        // 3. Show the matching neon outline (blue for male, pink for female)
        if (outlineToShow != null)
            outlineToShow.SetActive(true);

        CheckValidation();
    }

    void ResetVisuals()
    {
        // Buttons → white (normal sprite colour)
        if (maleButton != null) maleButton.image.color = defaultButtonColor;
        if (femaleButton != null) femaleButton.image.color = defaultButtonColor;

        // Outline GameObjects → hidden
        if (maleOutlineObject != null) maleOutlineObject.SetActive(false);
        if (femaleOutlineObject != null) femaleOutlineObject.SetActive(false);
    }

    // ── Name input ────────────────────────────────────────────────────────────

    public void OnNameChanged()
    {
        playerName = nameInputField.text;
        CheckValidation();
    }

    void CheckValidation()
    {
        confirmButton.interactable =
            !string.IsNullOrEmpty(playerName) &&
            !string.IsNullOrEmpty(selectedGender);
    }

    // ── Confirm ───────────────────────────────────────────────────────────────

    void ConfirmSelection()
    {
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.SetString("PlayerGender", selectedGender);
        PlayerPrefs.Save();

        Debug.Log($"[CharacterSelect] Player: {playerName} | Gender: {selectedGender}");
        SceneManager.LoadScene("GameScene");
    }
}