using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerDamageHandler : MonoBehaviour
{
    private Player player;
    private PlayerStats stats;
    private PlayerNetworkData networkData;
    private PlayerUIHandler uiHandler;

    private float lastFallDamageTime = -999f;
    private float lastCollisionDamageTime = -999f;

    private Vector3 peakYPos;
    private bool wasGrounded = true;
    private bool isFalling = false;
    private bool fallResetProtection = false;

    private int groundedFramesAfterTeleport = 0;
    private float teleportGraceUntil = -1f;

    public void Initialize(Player player, PlayerStats stats, PlayerNetworkData networkData, PlayerUIHandler uiHandler)
    {
        this.player = player;
        this.stats = stats;
        this.networkData = networkData;
        this.uiHandler = uiHandler;
    }

    public void RequestDamage(float amount, AudioClip hitSound = null)
    {
        if (Time.time - lastFallDamageTime < stats.damageCooldown && Time.time - lastCollisionDamageTime < stats.damageCooldown) return;

        networkData.TakeDamageServerRpc(amount);
        uiHandler?.OnDamageTaken();

        if (hitSound != null) player.GetComponent<AudioSource>()?.PlayOneShot(hitSound);
    }

    #region Fall / Collision Handling
    public void HandleFallDamage()
    {
        // �׷��̽�: �ð� �Ǵ� '���� ���� N������'���� ���� ����
        if (fallResetProtection)
        {
            bool grounded = IsGrounded();

            // �ð� �׷��̽� �켱
            if (Time.time < teleportGraceUntil)
            {
                wasGrounded = true;
                isFalling = false;
                peakYPos = player.transform.position;
                return;
            }

            // �ð� �������� '���� ���� 2������' Ȯ��
            if (grounded)
            {
                groundedFramesAfterTeleport++;
                if (groundedFramesAfterTeleport >= 2)
                {
                    fallResetProtection = false;
                    groundedFramesAfterTeleport = 0;
                }
            }
            else
            {
                groundedFramesAfterTeleport = 0;
            }

            wasGrounded = true;
            isFalling = false;
            peakYPos = player.transform.position;
            return;
        }

        // ���� ����
        bool grounded2 = IsGrounded();
        if (wasGrounded && !grounded2)
            peakYPos = player.transform.position;

        // CC.velocity ��� ���� ������ ���� �� ����(�Ʒ� �������� ����)
        if (!grounded2 && !isFalling && player.characterController.velocity.y < -0.1f)
            isFalling = true;

        if (grounded2 && !wasGrounded && isFalling)
        {
            isFalling = false;
            float fallDistance = peakYPos.y - player.transform.position.y;
            float effective = Mathf.Max(0f, fallDistance - stats.fallThreshold);
            if (effective > 0f && Time.time - lastFallDamageTime >= stats.damageCooldown)
            {
                float dmg = effective * stats.damageMultiplier;
                RequestDamage(dmg);
                lastFallDamageTime = Time.time;
            }
        }

        wasGrounded = grounded2;
    }

    private bool IsGrounded()
    {
        // Prefer character controller grounding
        if (player.characterController != null) return player.characterController.isGrounded;
        // Fallback raycast
        return Physics.Raycast(player.transform.position, Vector3.down, 0.2f);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.normal.y > 0.5f) return;

        float speed = player.characterController.velocity.magnitude;
        if (speed >= stats.collisionSpeedThreshold && Time.time - lastCollisionDamageTime >= stats.damageCooldown)
        {
            float excess = speed - stats.collisionSpeedThreshold;
            float dmg = excess * stats.collisionDamageMultiplier;
            RequestDamage(dmg);
            lastCollisionDamageTime = Time.time;
        }
    }

    // �����̵� ���� ȣ��
    public void BeginTeleportGrace(float seconds = 0.25f)
    {
        teleportGraceUntil = Time.time + seconds;
        groundedFramesAfterTeleport = 0;

        wasGrounded = true;
        isFalling = false;
        peakYPos = player.transform.position;

        // ��ٿ �ʱ�ȭ(���� ������ ����)
        lastFallDamageTime = Time.time;
        fallResetProtection = true;
    }
    #endregion
}
