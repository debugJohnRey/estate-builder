using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    [Header("Character Root GameObjects")]
    [Tooltip("Drag the Boy GameObject (child of Characters) here.")]
    public GameObject boyCharacter;

    [Tooltip("Drag the Girl GameObject (child of Characters) here.")]
    public GameObject girlCharacter;

    [Header("Camera Targets (optional auto-wire)")]
    [Tooltip("Third-person camera – will have its cameraTarget re-pointed at active character.")]
    public ThirdPersonCamera thirdPersonCamera;

    [Tooltip("CameraTargetFollow helper – will have its player re-pointed at active character.")]
    public CameraTargetFollow cameraTargetFollow;

    [Tooltip("First-person camera – will have its playerBody re-pointed at active character.")]
    public FirstPersonCameraFollow firstPersonCamera;

    [Header("Player Controller (optional auto-wire)")]
    [Tooltip("PlayerController on the Characters parent or elsewhere – animator will be re-wired.")]
    public PlayerController playerController;

    // ─────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        string gender = PlayerPrefs.GetString("PlayerGender", "Male");
        ApplyGender(gender);
        Debug.Log($"[CharacterSpawner] Gender='{gender}' → Active='{(gender == "Male" ? "Boy" : "Girl")}'");
    }

    // ─────────────────────────────────────────────────────────────────────────

    void ApplyGender(string gender)
    {
        bool isMale = gender == "Male";

        // Activate / deactivate
        if (boyCharacter != null) boyCharacter.SetActive(isMale);
        if (girlCharacter != null) girlCharacter.SetActive(!isMale);

        GameObject activeChar = isMale ? boyCharacter : girlCharacter;
        if (activeChar == null) return;

        // Tag as Player so NPCController, InteractionZone, etc. find it
        activeChar.tag = "Player";
        Debug.Log($"[CharacterSpawner] Tag set to 'Player' on '{activeChar.name}'");

        // Remove Player tag from the inactive character to avoid duplicates
        GameObject inactiveChar = isMale ? girlCharacter : boyCharacter;
        if (inactiveChar != null && inactiveChar.CompareTag("Player"))
            inactiveChar.tag = "Untagged";

        Transform activeTransform = activeChar.transform;

        // ── Re-wire cameras ───────────────────────────────────────────────

        // ThirdPersonCamera needs a cameraTarget (usually a CameraTargetFollow child)
        if (thirdPersonCamera != null && thirdPersonCamera.cameraTarget == null)
            thirdPersonCamera.cameraTarget = activeTransform;

        // CameraTargetFollow follows the player root
        if (cameraTargetFollow != null)
            cameraTargetFollow.player = activeTransform;

        // FirstPersonCamera needs the player body
        if (firstPersonCamera != null)
            firstPersonCamera.playerBody = activeTransform;

        // ── Re-wire PlayerController animator ────────────────────────────
        if (playerController != null)
        {
            Animator anim = activeChar.GetComponentInChildren<Animator>();
            if (anim != null)
                playerController.animator = anim;
        }
    }
}