using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class IntroScreenLoader : MonoBehaviour
{
    public Image loadingBarFill;   // drag LoadingBarFill here
    public float loadDuration = 3f; // seconds before moving on
    void Start()
    {
        StartCoroutine(LoadGame());
    }

    IEnumerator LoadGame()
    {
        float elapsed = 0f;

        while (elapsed < loadDuration)
        {
            elapsed += Time.deltaTime;
            loadingBarFill.fillAmount = elapsed / loadDuration;
            yield return null;
        }

        loadingBarFill.fillAmount = 1f;

        // Replace "MainMenu" with the exact name of your next scene
        SceneManager.LoadScene("MainMenu");
    }
}
