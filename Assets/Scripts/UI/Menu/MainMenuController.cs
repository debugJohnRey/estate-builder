using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Call this from the Play button
    public void OnPlayButtonClicked()
    {
        // Replace "GameScene" with your actual game scene name
        SceneManager.LoadScene("GenderMenu");
    }

    // Call this from the Settings button
    public void OnSettingsButtonClicked()
    {
        // Replace "Settings" with your actual settings scene name
        // OR open a settings panel instead (we can build that next)
        Debug.Log("Settings clicked");
    }
}