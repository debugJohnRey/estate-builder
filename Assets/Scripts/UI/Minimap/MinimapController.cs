using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MinimapController : MonoBehaviour
{
    [Header("References")]
    public RectTransform minimapPanel;
    public RectTransform expandedPanel;
    public Button minimapClickArea;
    public Button closeButton;

    [Header("Sizes")]
    public Vector2 minimapSize = new Vector2(200, 200);
    public Vector2 expandedSize = new Vector2(700, 700);

    [Header("Camera")]
    public Camera minimapCamera;
    public Camera expandCamera;
    public float minimapZoom = 20f;
    public float expandedZoom = 60f;

    [Header("Overlay")]
    public GameObject mapOverlay;

    private bool isExpanded = false;
    private Coroutine animCoroutine;

    void Start()
    {
        expandedPanel.gameObject.SetActive(false);
        expandCamera.gameObject.SetActive(false);
        minimapClickArea.onClick.AddListener(OpenMap);
        closeButton.onClick.AddListener(CloseMap);
    }

    void OpenMap()
    {
        isExpanded = true;
        expandedPanel.gameObject.SetActive(true);
        expandCamera.gameObject.SetActive(true);
        mapOverlay.SetActive(true);

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateSize(expandedPanel, expandedSize, 0.25f));
    }

    void CloseMap()
    {
        isExpanded = false;
        expandCamera.gameObject.SetActive(false);
        mapOverlay.SetActive(false);

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateClose(expandedPanel, 0.2f));
    }

    IEnumerator AnimateSize(RectTransform target, Vector2 toSize, float duration)
    {
        Vector2 fromSize = target.sizeDelta;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration); // smooth easing
            target.sizeDelta = Vector2.Lerp(fromSize, toSize, t);
            yield return null;
        }

        target.sizeDelta = toSize;
    }

    IEnumerator AnimateClose(RectTransform target, float duration)
    {
        Vector2 fromSize = target.sizeDelta;
        Vector2 toSize = minimapSize;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            target.sizeDelta = Vector2.Lerp(fromSize, toSize, t);
            yield return null;
        }

        target.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (isExpanded) CloseMap();
            else OpenMap();
        }
    }
}