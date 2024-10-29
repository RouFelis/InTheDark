using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : playerMoveController , IDamaged, ICharacter
{
	public int Health { get; set; }
	public int Damage { get; set; }
	public string Name { get; set; }

	public void Die()
	{
	}

	public void TakeDamage(int amount)
	{
	}
}
