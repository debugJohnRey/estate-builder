using UnityEngine;
using UnityEngine.EventSystems;

public class MobileButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool IsHeld { get; private set; }

    public event System.Action OnPressed;

    public void OnPointerDown(PointerEventData e)
    {
        IsHeld = true;
        OnPressed?.Invoke();
    }

    public void OnPointerUp(PointerEventData e) => IsHeld = false;
}
