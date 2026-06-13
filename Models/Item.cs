namespace TheAdventure.Models;

public enum ItemType
{
    HealthPotion,
    AttackBoost,
    Shield,
    Key
}

public class Item : IPickable
{
    public string Name { get; }
    public ItemType Type { get; }
    public int Value { get; }
    public int X { get; set; }
    public int Y { get; set; }

    public Item(ItemType type, int x, int y)
    {
        Type = type;
        X = x;
        Y = y;
        (Name, Value) = type switch
        {
            ItemType.HealthPotion => ("Health Potion", 30),
            ItemType.AttackBoost  => ("Attack Boost", 5),
            ItemType.Shield       => ("Shield", 10),
            ItemType.Key          => ("Dungeon Key", 1),
            _                     => ("Unknown", 0)
        };
    }

    public void Apply(Player player)
    {
        switch (Type)
        {
            case ItemType.HealthPotion:
                player.Heal(Value);
                break;
            case ItemType.AttackBoost:
                player.AttackPower += Value;
                break;
            case ItemType.Shield:
                player.Defense += Value;
                break;
            case ItemType.Key:
                player.HasKey = true;
                break;
        }
    }
}