using UnityEngine;

public class AnimationRelay : MonoBehaviour
{
    [SerializeField] private Animator firstpersonAnimator;
    [SerializeField] private Animator thirdpersonAnimator;
    [SerializeField] private MeleeWeaponHitbox weaponBox;    


    // 타격 판정 타이밍에 애니메이션 이벤트로 호출
    public void OnAttackHit()
    {
        weaponBox?.ApplyDamage();
    }
}
