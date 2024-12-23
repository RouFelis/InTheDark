using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSoundHandler : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    public AudioClip hoverSound; // ���콺 ���� �Ǵ� ���� �Ҹ�
    public AudioClip selectSound; // ���콺 ���� �Ǵ� ���� �Ҹ�

    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.Instance.PlaySound(hoverSound);
    }

    public void OnSelect(BaseEventData eventData)
    {
        AudioManager.Instance.PlaySound(selectSound);
    }
}
