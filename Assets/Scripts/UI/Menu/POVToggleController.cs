using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class POVToggleController : MonoBehaviour
{
    [Header("Toggle UI")]
    public Toggle povToggle;
    public RectTransform knob;
    public Image trackImage;

    [Header("Labels")]
    public TMP_Text firstPersonLabel;
    public TMP_Text thirdPersonLabel;

    [Header("Knob Positions")]
    public float knobOffX = -15f;
    public float knobOnX = 15f;

    [Header("Track Colors")]
    public Color offColor = new Color(0.75f, 0.75f, 0.75f, 1f);
    public Color onColor = new Color(0.18f, 0.85f, 0.27f, 1f);

    [Header("Cameras")]
    public GameObject firstPersonCameraObj;
    public GameObject thirdPersonCameraObj;
    public FirstPersonCameraFollow firstPersonFollow;
    public PlayerController playerController;

    private Coroutine animCoroutine;
    private bool isInitialized = false;
    private bool isFirstPerson = false;



    void Start()
    {
        bool savedState = PlayerPrefs.GetInt("POVMode", 0) == 1;

        // remove listener BEFORE setting isOn to prevent false trigger
        povToggle.onValueChanged.RemoveAllListeners();
        povToggle.isOn = savedState;
        SetVisualImmediate(savedState);
        ApplyCameras(savedState);

        // add listener AFTER initial setup
        povToggle.onValueChanged.AddListener(OnToggleChanged);
        isInitialized = true;
    }

    void OnToggleChanged(bool isOn)
    {
        if (!isInitialized) return;

        AudioManager.Instance?.PlayToggleSFX();

        isFirstPerson = isOn;
        PlayerPrefs.SetInt("POVMode", isOn ? 1 : 0);
        UpdateLabelHighlight(isOn);
        ApplyCameras(isOn);

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateToggle(isOn));
    }

    void ApplyCameras(bool isOn)
    {
        // delay camera switch until menu closes
        if (PauseMenuController.IsMenuOpen)
        {
            PlayerPrefs.SetInt("POVMode", isOn ? 1 : 0);
            return;
        }

        firstPersonCameraObj.SetActive(isOn);
        thirdPersonCameraObj.SetActive(!isOn);

        if (playerController != null)
            playerController.isFirstPerson = isOn;
    }

    // called by PauseMenuController when menu closes
    public void ApplyCamerasExternal(bool isOn)
    {

        firstPersonCameraObj.SetActive(isOn);
        thirdPersonCameraObj.SetActive(!isOn);

        if (playerController != null)
            playerController.isFirstPerson = isOn;
    }

    void SetVisualImmediate(bool isOn)
    {
        knob.anchoredPosition = new Vector2(isOn ? knobOnX : knobOffX, 0f);
        trackImage.color = isOn ? onColor : offColor;
        UpdateLabelHighlight(isOn);
    }

    void UpdateLabelHighlight(bool isOn)
    {
        if (firstPersonLabel != null)
        {
            Color c = firstPersonLabel.color;
            c.a = isOn ? 1f : 0.4f;
            firstPersonLabel.color = c;
        }

        if (thirdPersonLabel != null)
        {
            Color c = thirdPersonLabel.color;
            c.a = !isOn ? 1f : 0.4f;
            thirdPersonLabel.color = c;
        }
    }

    IEnumerator AnimateToggle(bool isOn)
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector2 fromPos = knob.anchoredPosition;
        Vector2 toPos = new Vector2(isOn ? knobOnX : knobOffX, 0f);
        Color fromColor = trackImage.color;
        Color toColor = isOn ? onColor : offColor;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            knob.anchoredPosition = Vector2.Lerp(fromPos, toPos, t);
            trackImage.color = Color.Lerp(fromColor, toColor, t);
            yield return null;
        }

        knob.anchoredPosition = toPos;
        trackImage.color = toColor;
    }
}