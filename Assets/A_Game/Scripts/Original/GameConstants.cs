
namespace GameConstants
{
    public class BoxTiers
    {
        public const int BRONZE = 1;
        public const int SILVER = 2;
        public const int GOLD = 3;
    }

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
        public const string SCOUT_QUEST = "Scout Quest";
    }

    public class EnjinTokenTypes
    {
        public const string COMMON = "Common";
        public const string PREMIUM = "Premium";
        public const string SPECIAL = "Special";
        public const string ULTIMATE = "Ultimate";
    }

    public class BoostCategory
    {
        public const string DAMAGE = "Damage";
        public const string DEFENSE = "Defense";
        public const string HEALTH = "Health";
        public const string POWER = "Power";
        public const string SIZE = "Size";
    }

    public class BoostEnjinTokenKeys
    {
        public const string SWORD = "Apochrome Sword";
        public const string ARMOR = "Soulshift Armor";
        public const string SHADOW_SONG = "Shadowsong";
        public const string BULL = "Mark of the Bull";
    }

    public class BoostEnjinOreItems
    {
        public const string DAMAGE = "Enjin Damage Ore Item";
        public const string DEFENSE = "Enjin Defense Ore Item";
        public const string HEALTH = "Enjin Health Ore Item";
        public const string POWER = "Enjin Power Ore Item";
        public const string SIZE = "Enjin Size Ore Item";
    }

    public class OreTiers
    {
        public const string RAW = "Raw";
        public const string POLISHED = "Polished";
        public const string PERFECT = "Perfect";
        public const string QUEST_COMPLETED = "Quest completed";
    }

    public class OreBonuses
    {
        public const int PERFECT_ORE_MIN = 10;
        public const int POLISHED_ORE_MIN = 6;
        public const int RAW_ORE_MIN = 1;
    }

    public class UiMessages
    {
        public const string PRIVATE_ROOM_ALREADY_USED = "Name is already used. Please provide another.";
        public const string LOADING = "Loading...";
        public const string ENJIN_AUTO_WITHDRAWAL = "* Units can be tokenized";
    }

    public class LocalizationTerms
    {
        public const string PING = "Ping:";
        public const string ORE = "Ore";
        public const string REQUIRES = "Requires";
        public const string OR = "or";
        public const string REQUIRES_ENOUGH_GLOBAL_SYSTEM_QUEST_POINTS = "Requires enough Global System Quest points.";
        public const string QUEST_COMPLETED = "Quest is already completed.";
        public const string TOKEN = "Token";
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

    public class BoxIndexes
    {
        public const int STARTER = 0;
        public const int PREMIUM = 1;
        public const int MASTER = 2;
        public const int SPECIAL = 3;
        public const int DEMON = 4;
        public const int LEGEND = 5;
    }

    public class ResourcePaths
    {
        public const string UNIT_IMAGES = "Images/Units/";
    }

    public class RewardsChances
    {
        public const int GUARANTEED_ODDS = 100;
        public const int ORE_ODDS = 20;

        public const int ENJIN_REWARDS_ODDS = 5;
        public const int ENJIN_REWARDS_ODDS_WITH_MINMINS_TOKEN = 25;

        public const int ENJIN_WINS_FOR_GUARANTEED_ENJIN_REWARD = 10;
        public const int ENJIN_WINS_FOR_GUARANTEED_ENJIN_REWARD_WITH_MINMINS_TOKEN = 5;
    }
}
 