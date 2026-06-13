using TheAdventure.Models;
using TheAdventure.Map;

namespace TheAdventure.Models;

public enum GamePhase
{
    Exploring,
    Combat,
    GameOver,
    Victory
}

public class GameState
{
    public Player Player { get; }
    public DungeonMap CurrentMap { get; private set; }
    public List<Enemy> Enemies { get; private set; } = new();
    public List<Item> Items { get; private set; } = new();
    public GamePhase Phase { get; set; } = GamePhase.Exploring;
    public Enemy? CurrentEnemy { get; set; }
    public int Score { get; private set; }
    public List<string> Log { get; } = new();

    private static readonly Random _rng = new();

    public GameState()
    {
        CurrentMap = DungeonMap.Generate(40, 25, floor: 1);
        var startRoom = CurrentMap.Rooms.First();
        var (sx, sy) = startRoom.Center;
        Player = new Player(sx, sy);
        SpawnEnemiesAndItems();
    }

    public void NextFloor()
    {
        Player.Floor++;
        CurrentMap = DungeonMap.Generate(40, 25, Player.Floor);
        var startRoom = CurrentMap.Rooms.First();
        var (sx, sy) = startRoom.Center;
        Player.MoveTo(sx, sy);
        Enemies.Clear();
        Items.Clear();
        SpawnEnemiesAndItems();
        AddLog($"You descend to floor {Player.Floor}...");
    }

    private void SpawnEnemiesAndItems()
    {
        // Skip first room (spawn), use LINQ to get remaining rooms
        var spawnRooms = CurrentMap.Rooms.Skip(1).ToList();

        foreach (var room in spawnRooms)
        {
            int enemyCount = _rng.Next(1, 3 + Player.Floor);
            for (int i = 0; i < enemyCount; i++)
            {
                int ex = _rng.Next(room.X + 1, room.X + room.Width - 1);
                int ey = _rng.Next(room.Y + 1, room.Y + room.Height - 1);
                Enemies.Add(CreateEnemy(ex, ey));
            }

            // Randomly place items
            if (_rng.Next(3) == 0)
            {
                int ix = _rng.Next(room.X + 1, room.X + room.Width - 1);
                int iy = _rng.Next(room.Y + 1, room.Y + room.Height - 1);
                var itemType = (ItemType)_rng.Next(Enum.GetValues<ItemType>().Length - 1); // exclude Key
                Items.Add(new Item(itemType, ix, iy));
            }
        }

        // Place key in a random room
        if (spawnRooms.Count > 0)
        {
            var keyRoom = spawnRooms[_rng.Next(spawnRooms.Count)];
            var (kx, ky) = keyRoom.Center;
            Items.Add(new Item(ItemType.Key, kx, ky));
        }

        // Boss on floor 3
        if (Player.Floor == 3)
        {
            var bossRoom = CurrentMap.Rooms.Last();
            var (bx, by) = bossRoom.Center;
            Enemies.Add(new BossEnemy(bx, by));
        }
    }

    private Enemy CreateEnemy(int x, int y) => Player.Floor switch
    {
        1 => new Goblin(x, y),
        2 => _rng.Next(2) == 0 ? new Goblin(x, y) : new Skeleton(x, y),
        _ => _rng.Next(3) switch
        {
            0 => new Goblin(x, y),
            1 => new Skeleton(x, y),
            _ => new Troll(x, y)
        }
    };

    public void AddScore(int points)
    {
        Score += points;
    }

    public void AddLog(string message)
    {
        Log.Add(message);
        if (Log.Count > 8) Log.RemoveAt(0);
    }
}