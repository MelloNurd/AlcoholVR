using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SliderClick : MonoBehaviour, IPointerDownHandler
{
    [Tooltip("This event is invoked once when the slider is initially clicked (pointer down).")]
    public UnityEvent onClick;

    public void OnPointerDown(PointerEventData eventData)
    {
        onClick?.Invoke();
    }
}
