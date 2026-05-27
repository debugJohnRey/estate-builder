using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform background;
    public RectTransform knob;
    public float maxRadius = 80f;

    private Vector2 input = Vector2.zero;

    public float Horizontal => input.x;
    public float Vertical   => input.y;

    public void OnPointerDown(PointerEventData e) => OnDrag(e);

    public void OnDrag(PointerEventData e)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, e.position, e.pressEventCamera, out pos))
        {
            pos   = Vector2.ClampMagnitude(pos, maxRadius);
            knob.anchoredPosition = pos;
            input = pos / maxRadius;
        }
    }

    public void OnPointerUp(PointerEventData e)
    {
        input = Vector2.zero;
        knob.anchoredPosition = Vector2.zero;
    }
}
