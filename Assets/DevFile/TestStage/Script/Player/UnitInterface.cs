using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface UnitInterface 
{    
    string Name { get; set; }
    int Health { get; set; }
    int Damage { get; set; }

    void TakeDamage(int amount);
    void Attack(ICharacter target);
}
