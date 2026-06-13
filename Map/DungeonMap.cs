namespace TheAdventure.Map;

public enum TileType
{
    Wall,
    Floor,
    Door,
    StairsDown,
    BossRoom
}

public record Tile(TileType Type, int X, int Y)
{
    public bool IsWalkable => Type is TileType.Floor or TileType.Door or TileType.StairsDown or TileType.BossRoom;
}

public class Room
{
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }

    public Room(int x, int y, int w, int h)
    {
        X = x; Y = y; Width = w; Height = h;
    }

    public (int cx, int cy) Center => (X + Width / 2, Y + Height / 2);

    public bool Intersects(Room other) =>
        X <= other.X + other.Width  && X + Width  >= other.X &&
        Y <= other.Y + other.Height && Y + Height >= other.Y;
}

public class DungeonMap
{
    public int Width { get; }
    public int Height { get; }
    private readonly Tile[,] _tiles;
    public List<Room> Rooms { get; } = new();

    private static readonly Random _rng = new();

    public DungeonMap(int width, int height)
    {
        Width = width;
        Height = height;
        _tiles = new Tile[width, height];
        FillWalls();
    }

    private void FillWalls()
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                _tiles[x, y] = new Tile(TileType.Wall, x, y);
    }

    public Tile GetTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return new Tile(TileType.Wall, x, y);
        return _tiles[x, y];
    }

    public void SetTile(int x, int y, TileType type)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
            _tiles[x, y] = new Tile(type, x, y);
    }

    public static DungeonMap Generate(int width, int height, int floor)
    {
        var map = new DungeonMap(width, height);
        int roomCount = 6 + floor * 2;
        int attempts = 200;

        for (int i = 0; i < attempts && map.Rooms.Count < roomCount; i++)
        {
            int rw = _rng.Next(4, 10);
            int rh = _rng.Next(4, 8);
            int rx = _rng.Next(1, width - rw - 1);
            int ry = _rng.Next(1, height - rh - 1);
            var newRoom = new Room(rx, ry, rw, rh);

            // LINQ: check no overlap
            if (map.Rooms.Any(r => r.Intersects(newRoom))) continue;

            map.CarveRoom(newRoom);
            if (map.Rooms.Count > 0)
                map.CarveCorridorBetween(map.Rooms[^1], newRoom);
            map.Rooms.Add(newRoom);
        }

        // Place stairs in last room
        if (map.Rooms.Count > 0)
        {
            var (sx, sy) = map.Rooms[^1].Center;
            map.SetTile(sx, sy, floor == 3 ? TileType.BossRoom : TileType.StairsDown);
        }

        return map;
    }

    private void CarveRoom(Room room)
    {
        for (int x = room.X; x < room.X + room.Width; x++)
            for (int y = room.Y; y < room.Y + room.Height; y++)
                SetTile(x, y, TileType.Floor);
    }

    private void CarveCorridorBetween(Room a, Room b)
    {
        var (ax, ay) = a.Center;
        var (bx, by) = b.Center;

        // Horizontal then vertical
        int cx = ax;
        while (cx != bx) { SetTile(cx, ay, TileType.Floor); cx += cx < bx ? 1 : -1; }
        int cy = ay;
        while (cy != by) { SetTile(bx, cy, TileType.Floor); cy += cy < by ? 1 : -1; }
    }

    // LINQ: all walkable tiles
    public IEnumerable<Tile> WalkableTiles()
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                if (_tiles[x, y].IsWalkable)
                    yield return _tiles[x, y];
    }
}