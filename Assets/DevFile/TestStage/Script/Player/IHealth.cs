using UnityEngine;
public interface IHealth
{	
	public float Health { get; }
	public void TakeDamage(float amount, AudioClip hitSound);
	public void Die();

	public bool IsDead { get; }
}