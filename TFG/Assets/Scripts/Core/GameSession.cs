// Estat compartit entre escenes: recorda el nivell triat al menu.
public static class GameSession
{
    public const string LevelResourcePath = "Levels/";
    public const int TotalLevels = 3;

    public static int SelectedLevelNumber = 1;
    public static string SelectedLevel = "level1";

    public static bool HasNextLevel => SelectedLevelNumber < TotalLevels;

    public static void SetLevel(int levelNumber)
    {
        SelectedLevelNumber = levelNumber;
        SelectedLevel = "level" + levelNumber;
    }

    public static void GoToNextLevel()
    {
        if (HasNextLevel) SetLevel(SelectedLevelNumber + 1);
    }
}
