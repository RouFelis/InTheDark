public interface IDamaged
{	
	public int Health { get; set; }
	public int Damage { get; set; }
	public void TakeDamage(int amount);
	public void Die();
}