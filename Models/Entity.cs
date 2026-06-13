namespace TheAdventure.Models;

public interface IEntity
{
    int X { get; }
    int Y { get; }
    int Hp { get; }
    bool IsAlive { get; }
    void TakeDamage(int amount);
}

public interface IPickable
{
    string Name { get; }
    void Apply(Player player);
}

public abstract class Entity : IEntity
{
    public int X { get; protected set; }
    public int Y { get; protected set; }
    public int Hp { get; protected set; }
    public int MaxHp { get; protected set; }
    public bool IsAlive => Hp > 0;

    protected Entity(int x, int y, int maxHp)
    {
        X = x;
        Y = y;
        MaxHp = maxHp;
        Hp = maxHp;
    }

    public virtual void TakeDamage(int amount)
    {
        Hp = Math.Max(0, Hp - amount);
    }

    public void Heal(int amount)
    {
        Hp = Math.Min(MaxHp, Hp + amount);
    }
}