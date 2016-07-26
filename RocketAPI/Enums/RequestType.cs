#region


using pbr = global::Google.Protobuf.Reflection;



#endregion


namespace PokemonGo.RocketAPI.Enums{    public enum RequestType    {
        MethodUnset = 0,
        PlayerUpdate = 1,
        GetPlayer = 2,
        GetInventory = 4,
        DownloadSettings = 5,
        DownloadItemTemplates = 6,
        DownloadRemoteConfigVersion = 7,
        FortSearch = 101,
        Encounter = 102,
        CatchPokemon = 103,
        FortDetails = 104,
        ItemUse = 105,
        GetMapObjects = 106,
        FortDeployPokemon = 110,
        FortRecallPokemon = 111,
        ReleasePokemon = 112,
        UseItemPotion = 113,
        UseItemCapture = 114,
        UseItemFlee = 115,
        UseItemRevive = 116,
        TradeSearch = 117,
        TradeOffer = 118,
        TradeResponse = 119,
        TradeResult = 120,
        GetPlayerProfile = 121,
        GetItemPack = 122,
        BuyItemPack = 123,
        BuyGemPack = 124,
        EvolvePokemon = 125,
        GetHatchedEggs = 126,
        EncounterTutorialComplete = 127,
        LevelUpRewards = 128,
        CheckAwardedBadges = 129,
        UseItemGym = 133,
        GetGymDetails = 134,
        StartGymBattle = 135,
        AttackGym = 136,
        RecycleInventoryItem = 137,
        CollectDailyBonus = 138,
        UseItemXpBoost = 139,
        UseItemEggIncubator = 140,
        UseIncense = 141,
        GetIncensePokemon = 142,
        IncenseEncounter = 143,
        AddFortModifier = 144,
        DiskEncounter = 145,
        CollectDailyDefenderBonus = 146,
        UpgradePokemon = 147,
        SetFavoritePokemon = 148,
        NicknamePokemon = 149,
        EquipBadge = 150,
        SetContactSettings = 151,
        GetAssetDigest = 300,
        GetDownloadUrls = 301,
        GetSuggestedCodenames = 401,
        CheckCodenameAvailable = 402,
        ClaimCodename = 403,
        SetAvatar = 404,
        SetPlayerTeam = 405,
        MarkTutorialComplete = 406,
        LoadSpawnPoints = 500,
        Echo = 666,
        DebugUpdateInventory = 700,
        DebugDeletePlayer = 701,
        SfidaRegistration = 800,
        SfidaActionLog = 801,
        SfidaCertification = 802,
        SfidaUpdate = 803,
        SfidaAction = 804,
        SfidaDowser = 805,
        SfidaCapture = 806    }}