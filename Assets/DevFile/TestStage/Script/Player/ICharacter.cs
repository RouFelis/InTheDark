public interface ICharacter
{
    string Name { get; set; }
    int Health { get; set; }
    int Damage { get; set; }

    void TakeDamage(int amount);
    void Attack(ICharacter target);
}