using UnityEngine;

public class NPCController : MonoBehaviour
{
    [Header("Dialog")]
    public NPCDialog dialog;

    [Header("Settings")]
    public float interactionDistance = 3f;
    public KeyCode interactionKey = KeyCode.E;

    [Header("References")]
    public Transform playerTransform;   // drag Girl here

    // face player when talking
    public float turnSpeed = 5f;

    private Animator animator;
    private bool playerInRange = false;
    private bool isTalking = false;

    private static readonly int IsWavingHash = Animator.StringToHash("Wave");

    void Start()
    {
        animator = GetComponent<Animator>();

        // auto find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey) && !isTalking)
            StartTalking();
    }

    void LateUpdate()
    {
        // face player while talking
        if (isTalking && playerTransform != null)
        {
            Vector3 dir = playerTransform.position - transform.position;
            dir.y = 0f;

            if (dir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            }
        }
    }

    void StartTalking()
    {
        isTalking = true;

        // trigger wave/talk animation
        if (animator != null)
            animator.SetBool(IsWavingHash, true);

        // open dialog via DialogManager
        if (dialog != null)
            DialogManager.Instance.StartDialog(dialog, OnDialogEnd);
    }

    void OnDialogEnd()
    {
        isTalking = false;

        if (animator != null)
            animator.SetBool(IsWavingHash, false);
    }

    // called by InteractionZone trigger
    public void SetPlayerInRange(bool inRange)
    {
        playerInRange = inRange;

        if (!inRange && isTalking)
        {
            DialogManager.Instance.CloseDialog();
            OnDialogEnd();
        }
    }
}