using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class PauseMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pauseMenuPanel;
    public GameObject optionsPanel;

    [Header("Buttons")]
    public Button menuButton;
    public Button resumeButton;
    public Button optionsButton;
    public Button quitButton;
    public Button backButton;

    [Header("Sliders")]
    public Slider sfxSlider;
    public Slider musicSlider;

    private bool isMenuOpen = false;
    private bool isOptionsOpen = false;
    private Coroutine fadeCoroutine;

    void Start()
    {
        pauseMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);

        // Step 8 — load saved volumes into sliders
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);

        // also apply loaded values to AudioManager immediately
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(sfxSlider.value);
            AudioManager.Instance.SetMusicVolume(musicSlider.value);
        }

        // listeners
        menuButton.onClick.AddListener(ToggleMenu);
        resumeButton.onClick.AddListener(CloseMenu);
        optionsButton.onClick.AddListener(ToggleOptions);
        quitButton.onClick.AddListener(QuitGame);
        backButton.onClick.AddListener(CloseOptions);

        // Step 8 — connect sliders to AudioManager
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
    }

    void ToggleMenu()
    {
        if (isMenuOpen) CloseMenu();
        else OpenMenu();
    }

    void OpenMenu()
    {
        isMenuOpen = true;
        pauseMenuPanel.SetActive(true);
        AudioManager.Instance?.PlayButtonClick();   // Step 7

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadePanel(pauseMenuPanel, 0f, 1f, 0.2f));
    }

    void CloseMenu()
    {
        isMenuOpen = false;
        isOptionsOpen = false;
        optionsPanel.SetActive(false);
        AudioManager.Instance?.PlayButtonClick();   // Step 7

        // make sure main buttons visible for next open
        resumeButton.gameObject.SetActive(true);
        optionsButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(true);

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadePanel(pauseMenuPanel, 1f, 0f, 0.2f, () =>
        {
            pauseMenuPanel.SetActive(false);
        }));
    }

    void ToggleOptions()
    {
        isOptionsOpen = !isOptionsOpen;

        if (isOptionsOpen)
            OpenOptions();
        else
            CloseOptions();
    }

    void OpenOptions()
    {
        isOptionsOpen = true;
        optionsPanel.SetActive(true);
        AudioManager.Instance?.PlayButtonClick();   // Step 7

        resumeButton.gameObject.SetActive(false);
        optionsButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);
    }

    void CloseOptions()
    {
        isOptionsOpen = false;
        optionsPanel.SetActive(false);
        AudioManager.Instance?.PlayButtonClick();   // Step 7

        resumeButton.gameObject.SetActive(true);
        optionsButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(true);
    }

    // Step 8 — routes slider value to AudioManager
    void SetSFXVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
    }

    // Step 8 — routes slider value to AudioManager
    void SetMusicVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);
    }

    void QuitGame()
    {
        AudioManager.Instance?.PlayButtonClick();   // Step 7
        Debug.Log("Quit");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    IEnumerator FadePanel(GameObject panel, float fromAlpha, float toAlpha,
                          float duration, System.Action onComplete = null)
    {
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();

        float elapsed = 0f;
        cg.alpha = fromAlpha;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration);
            yield return null;
        }

        cg.alpha = toAlpha;
        onComplete?.Invoke();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isOptionsOpen) CloseOptions();      // escape from options → back to menu
            else if (isMenuOpen) CloseMenu();        // escape from menu → back to game
        }
    }
}