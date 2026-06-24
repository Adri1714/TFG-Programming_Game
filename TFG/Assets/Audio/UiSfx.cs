using UnityEngine;
using UnityEngine.EventSystems;

public class UiSfx : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData e) => AudioManager.Play(l => l.buttonHover);
    public void OnPointerClick(PointerEventData e) => AudioManager.Play(l => l.buttonClick);
}