using UnityEngine;

public class PlotTrigger : MonoBehaviour
{
    [Tooltip("Drag the UI Buy Button here")]
    public GameObject buyButton;

    [Tooltip("The exact point where the house will spawn")]
    public Transform spawnPoint;

    [Tooltip("Drag the object with the Mesh Renderer here")]
    public MeshRenderer meshRenderer;

    // Stores the current plot so ChooseHouseUI can access it later
    public static PlotTrigger ActivePlot { get; private set; }

    void Start()
    {
        buyButton.SetActive(false);

        // Start with the mesh hidden
        if (meshRenderer != null) meshRenderer.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ActivePlot = this;
            buyButton.SetActive(true);

            // Turn on the Mesh Renderer
            if (meshRenderer != null) meshRenderer.enabled = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (ActivePlot == this) ActivePlot = null;
            buyButton.SetActive(false);

            // Turn off the Mesh Renderer
            if (meshRenderer != null) meshRenderer.enabled = false;
        }
    }
}