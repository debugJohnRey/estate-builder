using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

    [Header("References")]
    public POVToggleController povToggleController;

    // static flag so other scripts can check if menu is open
    public static bool IsMenuOpen { get; private set; } = false;

    private bool isMenuOpen = false;
    private bool isOptionsOpen = false;
    private Coroutine fadeCoroutine;

    void Start()
    {
        pauseMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);

        // load saved volumes and apply immediately
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(sfxSlider.value);
            AudioManager.Instance.SetMusicVolume(musicSlider.value);
        }

        // button listeners
        menuButton.onClick.AddListener(ToggleMenu);
        resumeButton.onClick.AddListener(CloseMenu);
        optionsButton.onClick.AddListener(ToggleOptions);
        quitButton.onClick.AddListener(QuitGame);
        backButton.onClick.AddListener(CloseOptions);

        // slider listeners
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
    }

    // ─── Menu ────────────────────────────────────────────

    void ToggleMenu()
    {
        if (isMenuOpen) CloseMenu();
        else OpenMenu();
    }

    void OpenMenu()
    {
        IsMenuOpen = true;
        isMenuOpen = true;
        pauseMenuPanel.SetActive(true);
        AudioManager.Instance?.PlayButtonClick();

        // always free cursor when menu opens
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadePanel(pauseMenuPanel, 0f, 1f, 0.2f));
    }

    void CloseMenu()
    {
        IsMenuOpen = false;
        isMenuOpen = false;
        isOptionsOpen = false;
        optionsPanel.SetActive(false);
        AudioManager.Instance?.PlayButtonClick();

        // apply saved POV and restore correct cursor state
        ApplySavedPOV();

        resumeButton.gameObject.SetActive(true);
        optionsButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(true);

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadePanel(pauseMenuPanel, 1f, 0f, 0.2f, () =>
        {
            pauseMenuPanel.SetActive(false);
        }));
    }

    void ApplySavedPOV()
    {
        bool isFirstPerson = PlayerPrefs.GetInt("POVMode", 0) == 1;

        if (povToggleController != null)
            povToggleController.ApplyCamerasExternal(isFirstPerson);

        // restore cursor based on active POV
        if (isFirstPerson)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // ─── Options ─────────────────────────────────────────

    void ToggleOptions()
    {
        if (isOptionsOpen) CloseOptions();
        else OpenOptions();
    }

    void OpenOptions()
    {
        isOptionsOpen = true;
        optionsPanel.SetActive(true);
        AudioManager.Instance?.PlayButtonClick();

        resumeButton.gameObject.SetActive(false);
        optionsButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);
    }

    void CloseOptions()
    {
        isOptionsOpen = false;
        optionsPanel.SetActive(false);
        AudioManager.Instance?.PlayButtonClick();

        resumeButton.gameObject.SetActive(true);
        optionsButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(true);
    }

    // ─── Audio ───────────────────────────────────────────

    void SetSFXVolume(float value)
    {
        AudioManager.Instance?.SetSFXVolume(value);
    }

    void SetMusicVolume(float value)
    {
        AudioManager.Instance?.SetMusicVolume(value);
    }

    // ─── Quit ────────────────────────────────────────────

    void QuitGame()
    {
        AudioManager.Instance?.PlayButtonClick();
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ─── Fade ────────────────────────────────────────────

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

    // ─── Escape Key ──────────────────────────────────────

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isOptionsOpen) CloseOptions();
            else if (isMenuOpen) CloseMenu();
            else OpenMenu();
        }
    }
}