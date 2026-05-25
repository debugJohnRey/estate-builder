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

    [Header("Audio")]
    public AudioSource sfxAudioSource;
    public AudioSource musicAudioSource;

    private bool isMenuOpen = false;
    private bool isOptionsOpen = false;
    private Coroutine fadeCoroutine;

    void Start()
    {
        pauseMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);

        // load saved values
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);

        // listeners
        menuButton.onClick.AddListener(ToggleMenu);
        resumeButton.onClick.AddListener(CloseMenu);
        optionsButton.onClick.AddListener(ToggleOptions);
        quitButton.onClick.AddListener(QuitGame);
        backButton.onClick.AddListener(CloseOptions);

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

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadePanel(pauseMenuPanel, 0f, 1f, 0.2f));
    }

    void CloseMenu()
    {
        isMenuOpen = false;
        isOptionsOpen = false;
        optionsPanel.SetActive(false);

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

        // hide main menu buttons so it looks clean
        resumeButton.gameObject.SetActive(false);
        optionsButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);
    }

    void CloseOptions()
    {
        isOptionsOpen = false;
        optionsPanel.SetActive(false);

        // show main menu buttons again
        resumeButton.gameObject.SetActive(true);
        optionsButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(true);
    }
    void SetSFXVolume(float value)
    {
        if (sfxAudioSource != null) sfxAudioSource.volume = value;
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    void SetMusicVolume(float value)
    {
        if (musicAudioSource != null) musicAudioSource.volume = value;
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();

        // in editor this won't quit, so:
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Fade coroutine with optional callback
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
        // press Escape to close menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isMenuOpen) CloseMenu();
        }
    }

    //     void ToggleOptions()
    // {
    //     isOptionsOpen = !isOptionsOpen;

    //     if (isOptionsOpen)
    //         OpenOptions();
    //     else
    //         CloseOptions();
    // }


}