using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSoundHandler : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    public AudioClip hoverSound; // 마우스 오버 또는 선택 소리
    public AudioClip selectSound; // 마우스 오버 또는 선택 소리

    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.Instance.PlaySound(hoverSound);
    }

    public void OnSelect(BaseEventData eventData)
    {
        AudioManager.Instance.PlaySound(selectSound);
    }
}
