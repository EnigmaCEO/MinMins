
namespace GameConstants
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
        public const string QUEST_SELECTION = "Quest Selection";
        public const string GLOBAL_SYSTEM_QUEST = "Global System Quest";
    }

    public class EnjinTokenTypes
    {
        public const string COMMON = "Common";
        public const string PREMIUM = "Premium";
        public const string SPECIAL = "Special";
        public const string ULTIMATE = "Ultimate";
    }

    public class TeamBoostCategory
    {
        public const string DAMAGE = "Damage";
        public const string DEFENSE = "Defense";
        public const string HEALTH = "Health";
        public const string POWER = "Power";
        public const string SIZE = "Size";
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
        public const string SIZE = "Enjin Size Ore Item";
    }

    public class OreTier
    {
        public const string RAW = "Raw";
        public const string POLISHED = "Polished";
        public const string PERFECT = "Perfect";
    }

    public class UiMessages
    {
        public const string PRIVATE_ROOM_ALREADY_USED = "Name is already used. Please provide another.";
        public const string LOADING = "Loading...";
        public const string PERFORMING_WITHDRAWAL = "Performing withdrawal...";
        public const string WITHDRAWAL_DESCRIPTION = "Press OK to start withdrawal process. Close window to go back.";
        public const string GET_ENJIN_WALLET_DESCRIPTION = "Press OK to request an Enjin Wallet so you can withdraw your tokens. Close window to go back.";
        public const string WITHDRAWAL_COMPLETED = "Withdrawal completed!";
        public const string WITHDRAWAL_FAILED = "Withdrawal failed. Please try again.";
        public const string ENJIN_AUTO_WITHDRAWAL = "* Items are immediately sent to your Enjin wallet after trade approval";
    }

    public class Terms
    {
        public const string PING = "Ping:";
    }

    public class SoundNames
    {
        public const string GUARD = "guard";
        public const string HEAL = "heal";
        public const string BOMB = "sorc";
        public const string DESTROY = "storm";
        public const string SCOUT = "stealth";

        public const string DEATH = "death";

        public const string LOSE = "lose";
        public const string WIN = "win";

        public const string UI_ADVANCE = "ui";
        public const string UI_BACK = "ui2";
    }

    public class EnjinTokenKeys
    {
        public const string MINMINS_TOKEN = "minmins_token";

        public const string ENJIN_MAXIM = "enjin_maxim";
        public const string ENJIN_WITEK = "enjin_witek";
        public const string ENJIN_BRYANA = "enjin_bryana";
        public const string ENJIN_TASSIO = "enjin_tassio";
        public const string ENJIN_SIMON = "enjin_simon";

        public const string ENJIN_ESTHER = "enjin_esther"; //fairy 124
        public const string ENJIN_ALEX = "enjin_alex";  //black 122
        public const string ENJIN_LIZZ = "enjin_lizz";  //fire 126
        public const string ENJIN_EVAN = "enjin_evan";  //wizard 123
        public const string ENJIN_BRAD = "enjin_brad";  //book 125

        public const string ENJIN_SWORD = "enjin_sword";
        public const string ENJIN_ARMOR = "enjin_armor";
        public const string ENJIN_SHADOWSONG = "enjin_shadowsong";
        public const string ENJIN_BULL = "enjin_bull";

        public const string KNIGHT_HEALER = "knight_healer";
        public const string KNIGHT_BOMBER = "knight_bomber";
        public const string KNIGHT_SCOUT = "knight_scout";
        public const string KNIGHT_DESTROYER = "knight_destroyer";
        public const string KNIGHT_TANK = "knight_tank";

        public const string DEMON_HEALER = "demon_healer";
        public const string DEMON_BOMBER = "demon_bomber";
        public const string DEMON_SCOUT = "demon_scout";
        public const string DEMON_DESTROYER = "demon_destroyer";
        public const string DEMON_TANK = "demon_tank";

        public const string GOD_HEALER = "god_healer";
        public const string GOD_BOMBER = "god_bomber";
        public const string GOD_SCOUT = "god_scout";
        public const string GOD_DESTROYER_1 = "god_destroyer_1";
        public const string GOD_DESTROYER_2 = "god_destroyer_2";
        public const string GOD_TANK_1 = "god_tank_1";
        public const string GOD_TANK_2 = "god_tank_2";

        public const string SWISSBORG_CYBORG = "swissborg_cyborg";

        /*
        public const string ENJIN_DEFENSE_ORE_ITEM_1 = "enjin_defense_ore_1";
        public const string ENJIN_DEFENSE_ORE_ITEM_2 = "enjin_defense_ore_2";
        public const string ENJIN_DEFENSE_ORE_ITEM_3 = "enjin_defense_ore_3";
        public const string ENJIN_DEFENSE_ORE_ITEM_4 = "enjin_defense_ore_4";
        public const string ENJIN_DEFENSE_ORE_ITEM_5 = "enjin_defense_ore_5";
        public const string ENJIN_DEFENSE_ORE_ITEM_6 = "enjin_defense_ore_6";
        public const string ENJIN_DEFENSE_ORE_ITEM_7 = "enjin_defense_ore_7";
        public const string ENJIN_DEFENSE_ORE_ITEM_8 = "enjin_defense_ore_8";
        public const string ENJIN_DEFENSE_ORE_ITEM_9 = "enjin_defense_ore_9";
        public const string ENJIN_DEFENSE_ORE_ITEM_10 = "enjin_defense_ore_10";

        public const string ENJIN_HEALTH_ORE_ITEM_1 = "enjin_health_ore_1";
        public const string ENJIN_HEALTH_ORE_ITEM_2 = "enjin_health_ore_2";
        public const string ENJIN_HEALTH_ORE_ITEM_3 = "enjin_health_ore_3";
        public const string ENJIN_HEALTH_ORE_ITEM_4 = "enjin_health_ore_4";
        public const string ENJIN_HEALTH_ORE_ITEM_5 = "enjin_health_ore_5";
        public const string ENJIN_HEALTH_ORE_ITEM_6 = "enjin_health_ore_6";
        public const string ENJIN_HEALTH_ORE_ITEM_7 = "enjin_health_ore_7";
        public const string ENJIN_HEALTH_ORE_ITEM_8 = "enjin_health_ore_8";
        public const string ENJIN_HEALTH_ORE_ITEM_9 = "enjin_health_ore_9";
        public const string ENJIN_HEALTH_ORE_ITEM_10 = "enjin_health_ore_10";

        public const string ENJIN_POWER_ORE_ITEM_1 = "enjin_power_ore_1";
        public const string ENJIN_POWER_ORE_ITEM_2 = "enjin_power_ore_2";
        public const string ENJIN_POWER_ORE_ITEM_3 = "enjin_power_ore_3";
        public const string ENJIN_POWER_ORE_ITEM_4 = "enjin_power_ore_4";
        public const string ENJIN_POWER_ORE_ITEM_5 = "enjin_power_ore_5";
        public const string ENJIN_POWER_ORE_ITEM_6 = "enjin_power_ore_6";
        public const string ENJIN_POWER_ORE_ITEM_7 = "enjin_power_ore_7";
        public const string ENJIN_POWER_ORE_ITEM_8 = "enjin_power_ore_8";
        public const string ENJIN_POWER_ORE_ITEM_9 = "enjin_power_ore_9";
        public const string ENJIN_POWER_ORE_ITEM_10 = "enjin_power_ore_10";

        public const string ENJIN_DAMAGE_ORE_ITEM_1 = "enjin_damage_ore_1";
        public const string ENJIN_DAMAGE_ORE_ITEM_2 = "enjin_damage_ore_2";
        public const string ENJIN_DAMAGE_ORE_ITEM_3 = "enjin_damage_ore_3";
        public const string ENJIN_DAMAGE_ORE_ITEM_4 = "enjin_damage_ore_4";
        public const string ENJIN_DAMAGE_ORE_ITEM_5 = "enjin_damage_ore_5";
        public const string ENJIN_DAMAGE_ORE_ITEM_6 = "enjin_damage_ore_6";
        public const string ENJIN_DAMAGE_ORE_ITEM_7 = "enjin_damage_ore_7";
        public const string ENJIN_DAMAGE_ORE_ITEM_8 = "enjin_damage_ore_8";
        public const string ENJIN_DAMAGE_ORE_ITEM_9 = "enjin_damage_ore_9";
        public const string ENJIN_DAMAGE_ORE_ITEM_10 = "enjin_damage_ore_10";

        public const string ENJIN_SIZE_ORE_ITEM_1 = "enjin_size_ore_1";
        public const string ENJIN_SIZE_ORE_ITEM_2 = "enjin_size_ore_2";
        public const string ENJIN_SIZE_ORE_ITEM_3 = "enjin_size_ore_3";
        public const string ENJIN_SIZE_ORE_ITEM_4 = "enjin_size_ore_4";
        public const string ENJIN_SIZE_ORE_ITEM_5 = "enjin_size_ore_5";
        public const string ENJIN_SIZE_ORE_ITEM_6 = "enjin_size_ore_6";
        public const string ENJIN_SIZE_ORE_ITEM_7 = "enjin_size_ore_7";
        public const string ENJIN_SIZE_ORE_ITEM_8 = "enjin_size_ore_8";
        public const string ENJIN_SIZE_ORE_ITEM_9 = "enjin_size_ore_9";
        public const string ENJIN_SIZE_ORE_ITEM_10 = "enjin_size_ore_10";
        */
    }

    public class BoxIndexes
    {
        public const int STARTER = 0;
        public const int PREMIUM = 1;
        public const int MASTER = 2;
        public const int SPECIAL = 3;
        public const int DEMON = 4;
        public const int LEGEND = 5;
    }
}
 