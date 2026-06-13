namespace TheAdventure.Models;

public class Player : Entity
{
    public int AttackPower { get; set; }
    public int Defense { get; set; }
    public bool HasKey { get; set; }
    public int Gold { get; set; }
    public int Floor { get; set; } = 1;

    private readonly List<Item> _inventory = new();
    public IReadOnlyList<Item> Inventory => _inventory;

    public Player(int x, int y) : base(x, y, maxHp: 100)
    {
        AttackPower = 10;
        Defense = 2;
    }

    public void Move(int dx, int dy)
    {
        X += dx;
        Y += dy;
    }

    public void MoveTo(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void PickUp(Item item)
    {
        item.Apply(this);
        if (item.Type != ItemType.HealthPotion)
            _inventory.Add(item);
    }

    public override void TakeDamage(int amount)
    {
        var mitigated = Math.Max(1, amount - Defense);
        base.TakeDamage(mitigated);
    }

    // LINQ: total inventory value
    public int TotalInventoryValue => _inventory.Sum(i => i.Value);
}