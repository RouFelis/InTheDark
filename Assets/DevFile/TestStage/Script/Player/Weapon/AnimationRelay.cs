using UnityEngine;

public class AnimationRelay : MonoBehaviour
{
    [SerializeField] private Animator firstpersonAnimator;
    [SerializeField] private Animator thirdpersonAnimator;
    [SerializeField] private MeleeWeaponHitbox weaponBox;    


    // Ÿ�� ���� Ÿ�ֿ̹� �ִϸ��̼� �̺�Ʈ�� ȣ��
    public void OnAttackHit()
    {
        weaponBox?.ApplyDamage();
    }
}
