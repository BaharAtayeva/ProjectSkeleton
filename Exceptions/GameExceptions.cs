namespace TheAdventure.Exceptions;

public class GameOverException : Exception
{
    public bool PlayerWon { get; }

    public GameOverException(bool playerWon, string message) : base(message)
    {
        PlayerWon = playerWon;
    }
}

public class InvalidMoveException : Exception
{
    public InvalidMoveException(string message) : base(message) { }
}

public class SaveLoadException : Exception
{
    public SaveLoadException(string message, Exception? inner = null) : base(message, inner) { }
}