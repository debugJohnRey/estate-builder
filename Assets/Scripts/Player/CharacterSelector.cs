using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CharacterSelect : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button maleButton;
    [SerializeField] private Button femaleButton;
    [SerializeField] private Button confirmButton;

    [Header("Selection Settings")]
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField] private Color defaultColor = Color.white;

    private string selectedGender = "";
    private string playerName = "";

    void Start()
    {
        // Add listeners for gender buttons
        maleButton.onClick.AddListener(() => SelectGender("Male", maleButton));
        femaleButton.onClick.AddListener(() => SelectGender("Female", femaleButton));
        confirmButton.onClick.AddListener(ConfirmSelection);

        // IMPORTANT: Add listener for name input field changes
        nameInputField.onValueChanged.AddListener(delegate { OnNameChanged(); });

        confirmButton.interactable = false;
    }

    void SelectGender(string gender, Button clickedButton)
    {
        selectedGender = gender;
        ResetButtonColors();
        clickedButton.image.color = selectedColor;
        CheckValidation();
    }

    void ResetButtonColors()
    {
        maleButton.image.color = defaultColor;
        femaleButton.image.color = defaultColor;
    }

    public void OnNameChanged()
    {
        playerName = nameInputField.text;
        CheckValidation();
    }

    void CheckValidation()
    {
        confirmButton.interactable = !string.IsNullOrEmpty(playerName) && !string.IsNullOrEmpty(selectedGender);
    }

    void ConfirmSelection()
    {
        // Save data to PlayerPrefs
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.SetString("PlayerGender", selectedGender);
        PlayerPrefs.Save();

        Debug.Log($"Character Created: {playerName} as {selectedGender}");

        // Load the GameScene
        SceneManager.LoadScene("GameScene");
    }
}