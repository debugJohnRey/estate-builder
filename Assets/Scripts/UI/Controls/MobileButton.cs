using UnityEngine;
using UnityEngine.EventSystems;

public class MobileButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool IsHeld     { get; private set; }
    public bool WasPressed { get; private set; }

    public void OnPointerDown(PointerEventData e) { IsHeld = true; WasPressed = true; }
    public void OnPointerUp(PointerEventData e)   => IsHeld = false;

    void LateUpdate() => WasPressed = false;
}
