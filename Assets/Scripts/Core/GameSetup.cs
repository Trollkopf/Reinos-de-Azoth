public enum PlayerType
{
    Empty,
    Human,
    Bot,
}

public enum BotDifficulty
{
    Easy,
    Normal,
    Hard,
}

public static class GameSetup
{
    public static PlayerType[] Players =
    {
        PlayerType.Human,
        PlayerType.Bot,
        PlayerType.Empty,
        PlayerType.Empty,
    };

    public static BotDifficulty Difficulty = BotDifficulty.Normal;
}
