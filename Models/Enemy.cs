namespace TheAdventure.Models;

public abstract class Enemy : Entity
{
    public string Name { get; }
    public int AttackPower { get; protected set; }
    public int GoldReward { get; protected set; }
    public int XpReward { get; protected set; }

    protected Enemy(string name, int x, int y, int maxHp, int attack, int gold, int xp)
        : base(x, y, maxHp)
    {
        Name = name;
        AttackPower = attack;
        GoldReward = gold;
        XpReward = xp;
    }

    public abstract int RollDamage();

    public void MoveTo(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public class Goblin : Enemy
{
    private static readonly Random _rng = new();

    public Goblin(int x, int y) : base("Goblin", x, y, maxHp: 20, attack: 6, gold: 5, xp: 10) { }

    public override int RollDamage() => _rng.Next(AttackPower - 2, AttackPower + 3);
}

public class Skeleton : Enemy
{
    private static readonly Random _rng = new();

    public Skeleton(int x, int y) : base("Skeleton", x, y, maxHp: 35, attack: 9, gold: 10, xp: 20) { }

    public override int RollDamage() => _rng.Next(AttackPower - 3, AttackPower + 4);
}

public class Troll : Enemy
{
    private static readonly Random _rng = new();

    public Troll(int x, int y) : base("Troll", x, y, maxHp: 60, attack: 14, gold: 20, xp: 40) { }

    public override int RollDamage() => _rng.Next(AttackPower - 4, AttackPower + 6);

    public override void TakeDamage(int amount)
    {
        // Troll regenerates a bit each hit
        base.TakeDamage(amount);
        if (IsAlive) Heal(2);
    }
}

public class BossEnemy : Enemy
{
    private static readonly Random _rng = new();
    private int _hitCount = 0;

    public BossEnemy(int x, int y) : base("Dragon Boss", x, y, maxHp: 150, attack: 20, gold: 100, xp: 200) { }

    public override int RollDamage()
    {
        _hitCount++;
        // Every 3rd hit is a power attack
        return _hitCount % 3 == 0
            ? AttackPower * 2
            : _rng.Next(AttackPower - 5, AttackPower + 5);
    }
}