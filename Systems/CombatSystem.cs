using TheAdventure.Models;

namespace TheAdventure.Systems;

public static class CombatSystem
{
    private static readonly Random _rng = new();

    public static string PlayerAttacks(Player player, Enemy enemy)
    {
        int damage = _rng.Next(player.AttackPower - 3, player.AttackPower + 4);
        damage = Math.Max(1, damage);
        enemy.TakeDamage(damage);

        return enemy.IsAlive
            ? $"You hit {enemy.Name} for {damage} damage! ({enemy.Hp}/{enemy.MaxHp} HP)"
            : $"You defeated {enemy.Name}! (+{enemy.GoldReward} gold, +{enemy.XpReward} xp)";
    }

    public static string EnemyAttacks(Enemy enemy, Player player)
    {
        int damage = enemy.RollDamage();
        int before = player.Hp;
        player.TakeDamage(damage);
        int actual = before - player.Hp;

        return $"{enemy.Name} hits you for {actual} damage! ({player.Hp}/{player.MaxHp} HP)";
    }

    public static void RewardPlayer(Player player, Enemy enemy)
    {
        player.Gold += enemy.GoldReward;
    }
}