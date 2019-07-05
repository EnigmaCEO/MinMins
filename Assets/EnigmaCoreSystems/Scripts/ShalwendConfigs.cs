using System.Collections.Generic;

public static class ShalwendConfigs
{
    public const int SEND_WAVE_TRANSACTION_ID = 1;
    public const int GET_KILLED_COUNT_TRANSACTION_ID = 2;
    public const int RANK_CHANGED_TRANSACTION_ID = 16;
    public const int ENJIN_ITEM_COLLECTED_TRANSACTION = 17;

    public const string TRANSACTION_GAME_NAME = "Shalwend";

    public static string[] AchievementNames = new string[]
    {
        "ELF_KILLS",
        "THROW_WEAPON_KILLS",
        "KICK_KILLS",
        "POWERUPS_GAINED"
    };

    public static Dictionary<string, string> AchievementNamesForClient = new Dictionary<string, string>
    {
        { "ELF_KILLS", "Elf Kills" },
        { "THROW_WEAPON_KILLS", "Thrown Weapon Kills" },
        { "KICK_KILLS", "Kick Kills" },
        { "POWERUPS_GAINED", "Powerups Gained" }
    };
}
