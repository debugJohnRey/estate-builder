using UnityEngine;

public class InteractionZone : MonoBehaviour
{
    private NPCController npcController;

    void Start()
    {
        npcController = GetComponentInParent<NPCController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            npcController.SetPlayerInRange(true);
            DialogManager.Instance.ShowPrompt(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            npcController.SetPlayerInRange(false);
            DialogManager.Instance.ShowPrompt(false);
        }
    }
}