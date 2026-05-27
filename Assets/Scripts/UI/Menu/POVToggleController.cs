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
    public float knobOffX = -15f;    // left side = 3rd person
    public float knobOnX = 15f;    // right side = 1st person

    [Header("Track Colors")]
    public Color offColor = new Color(0.75f, 0.75f, 0.75f, 1f);  // gray
    public Color onColor = new Color(0.18f, 0.85f, 0.27f, 1f);  // green

    [Header("Cameras")]
    public Camera firstPersonCamera;
    public Camera thirdPersonCamera;

    // optional — your camera controller scripts
    public MonoBehaviour firstPersonController;
    public MonoBehaviour thirdPersonController;

    private Coroutine animCoroutine;

    void Start()
    {
        // load saved preference (default = off = 3rd person)
        bool savedState = PlayerPrefs.GetInt("POVMode", 0) == 1;
        povToggle.isOn = savedState;

        // set visuals immediately without animation on start
        SetVisualImmediate(savedState);

        // apply camera state
        ApplyCameras(savedState);

        // listen for changes
        povToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void OnToggleChanged(bool isOn)
    {
        // save preference
        PlayerPrefs.SetInt("POVMode", isOn ? 1 : 0);

        // apply cameras
        ApplyCameras(isOn);

        // animate toggle
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateToggle(isOn));

        // update label highlights
        UpdateLabelHighlight(isOn);
    }

    void ApplyCameras(bool isOn)
    {
        // isOn = true  → 1st person
        // isOn = false → 3rd person
        if (firstPersonCamera != null)
            firstPersonCamera.gameObject.SetActive(isOn);

        if (thirdPersonCamera != null)
            thirdPersonCamera.gameObject.SetActive(!isOn);

        if (firstPersonController != null)
            firstPersonController.enabled = isOn;

        if (thirdPersonController != null)
            thirdPersonController.enabled = !isOn;
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

        // snap to final values
        knob.anchoredPosition = toPos;
        trackImage.color = toColor;
    }
}