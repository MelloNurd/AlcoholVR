using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class AppButton : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent OnClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke();

        if(Phone.Instance && Phone.Instance._clickSound)
        {
            Phone.Instance.PlayClickSound();
        }
    }
}
