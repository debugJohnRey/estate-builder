using UnityEngine;
using UnityEngine.UI;

// Attach to any clickable object to play a sound on click.
// - UI Button: hooks into Button.onClick automatically
// - 3D world object: uses OnMouseDown (requires a Collider on the object)
// Assign overrideClip to use a different sound; leave it empty to use the default button click.
public class ClickableSFX : MonoBehaviour
{
    public AudioClip overrideClip;

    private bool isUIButton;

    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            isUIButton = true;
            btn.onClick.AddListener(PlaySound);
        }
    }

    // fires for 3D objects with a Collider; Unity never calls this on UI elements
    void OnMouseDown()
    {
        if (!isUIButton)
            PlaySound();
    }

    void PlaySound()
    {
        if (overrideClip != null)
            AudioManager.Instance?.PlaySFX(overrideClip);
        else
            AudioManager.Instance?.PlayButtonClick();
    }
}
