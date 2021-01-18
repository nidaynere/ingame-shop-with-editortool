#pragma warning disable CS0649

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class UITouchEvents : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private UnityEvent onPointerClick, onPointerEnter, onPointerExit;

    public void OnPointerClick(PointerEventData eventData)
    {
        onPointerClick.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onPointerEnter.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onPointerExit.Invoke();
    }
}
