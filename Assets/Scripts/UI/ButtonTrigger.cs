using UnityEngine;

public class ButtonTrigger : MonoBehaviour
{
    public GameObject uiButton;
    private MeshRenderer areaMesh;

    void Start()
    {
        areaMesh = GetComponent<MeshRenderer>();
        areaMesh.enabled = false; 
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            uiButton.SetActive(true);
            areaMesh.enabled = true; 
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            uiButton.SetActive(false);
            areaMesh.enabled = false; 
        }
    }
}