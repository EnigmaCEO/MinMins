﻿
public class GameConstants
{
    public class Scenes
    {
        public const string LEVELS = "Levels";
        public const string WAR_PREP = "WarPrep";
        public const string WAR = "War";
        public const string STORE = "Store";
        public const string LOBBY = "NewLobby";
        public const string UNIT_SELECT = "UnitSelect";
        public const string TEAM_BOOST = "TeamBoost"; 
    }

    public class TeamBoostCategory
    {
        public const string DAMAGE = "Damage";
        public const string DEFENSE = "Defense";
        public const string HEALTH = "Health";
        public const string POWER = "Power";
    }

    public class TeamBoostEnjinTokens
    {
        public const string SWORD = "Apochrome Sword";
        public const string ARMOR = "Soulshift Armor";
        public const string SHADOW_SONG = "Shadowsong";
        public const string BULL = "Mark of the Bull";
    }

    public class TeamBoostEnjinOreItems
    {
        public const string DAMAGE = "Enjin Damage Ore Item";
        public const string DEFENSE = "Enjin Defense Ore Item";
        public const string HEALTH = "Enjin Health Ore Item";
        public const string POWER = "Enjin Power Ore Item";
    }

    public class OreTier
    {
        public const string RAW = "Raw";
        public const string POLISHED = "Polished";
        public const string PERFECT = "Perfect";
    }

    public class LobbyPopUpMessages
    {
        public const string PROVIDE_NAME_FOR_ROOM_SEARCH = "Please provide a name for private room search.";
        public const string PRIVATE_ROOM_ALREADY_USED = "Name is already used. Please provide another.";
        public const string NO_PRIVATE_WITH_NAME_FOUND = "No private rooms were found with given name.";
    }
}
 