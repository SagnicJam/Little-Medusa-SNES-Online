using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnumData
{
    public enum MedusaInputs
    {
        Up=0,
        Left,
        Down,
        Right,
        Shoot,
        Push,
        PlaceRemovalBoulder,
        RespawnPlayer,
        LandPlayer,
        UseItem
    }

    public enum PosidannaInputs
    {
        Up = 0,
        Left,
        Down,
        Right,
        ShootTidalWave,
        CastBubbleShield,
        RespawnPlayer,
        LandPlayer,
        UseItem
    }

    public enum ErmolaiInputs
    {
        Up = 0,
        Left,
        Down,
        Right,
        CastPitfall,
        CastEarthquake,
        RespawnPlayer,
        LandPlayer,
        UseItem
    }

    public enum HeliemisInputs
    {
        Up = 0,
        Left,
        Down,
        Right,
        ShootMightyWind,
        PlaceTornado,
        RespawnPlayer,
        LandPlayer,
        UseItem
    }

    public enum AvernaInputs
    {
        Up = 0,
        Left,
        Down,
        Right,
        ShootFireBall,
        CastFlamePillar,
        RespawnPlayer,
        LandPlayer,
        UseItem
    }

    public enum CastItemTypes
    {
        SpawnnableItems=0,
        ServerProjectiles,
        ClientProjectiles
    }

    public enum UsableItemTypes
    {
        Boulder=0,
        Pitfall,
        Earthquake,
        Tornado,
        Minion,
        CereberausHead,
        TidalWave,
        BubbleShield,
        MightyWind,
        FlamePillar,
        EyeLaser,
        Fireball,
        Portal
    }

    public enum PlayerStates
    {
        Local,
        Remote,
        ServerPredicted
    }

    public enum Worlds
    {
        Water,
        Wind,
        Earth,
        Ether,
        Fire,
        Mirror
    }

    public enum TileType
    {
        None=0,
        Empty,
        Normal,
        Boulder,
        Hourglass,
        UpArrow,
        DownArrow,
        LeftArrow,
        RightArrow,
        UpCereberusHead,
        DownCereberusHead,
        LeftCereberusHead,
        RightCereberusHead,
        SpawnJar,
        Hole,
        Tornado,
        NoBoulder,
        Wall,
        Solid,
        VoidDeathTiles,
        Mirror,
        ItemSpawner,
        IcarusWingsItem,
        HeartItem,
        CereberausHeadItem,
        MinionItem,
        EyeLaserItem,
        BoulderItem,
        TidalWaveItem,
        BubbleShieldItem,
        MightyWindItem,
        TornadoPullItem,
        PitfallItem,
        EarthQuakeItem,
        FireballItem,
        FlamePillarItem,
        PortalItem,
        Portal
    }

    public enum GameObjectEnums
    {
        None=0,
        ThunderStruck,
        LightningBolt,
        Earthquake,
        BoulderAppearing,
        BoulderDisappearing,
        LeftCereberusFire,
        RightCereberusFire,
        UpCereberusFire,
        DownCereberusFire,
        TornadoEffect,
        Hero,
        Monster
    }

    public enum StaticAnimatingTiles
    {
        SmallExplosion,
        BigExplosion,
        Lightning,
        Earthquake
    }

    public enum Projectiles
    {
        None,
        Arrow,
        EyeLaser,
        WaterWave,
        Water,
        HurricaneWind,
        BoulderProjectile,
        FireBall,
        FireShieldActor,
        FireTitanJuggler,
        FioraSpark,
        CardinalProjectile,
        DispersedBoulder,
        TidalWave,
        BubbleShield,
        MightyWind,
        FlamePillar,
        EyeLaserMirrorKnight,
        MightyWindMirrorKnight,
        FireBallMirrorKnight,
        DispersedFireBallMirrorKnight,
        EyeLaserPortal
    }

    public enum MonsterBreed
    {
        Cyclops=0,
        Snakes,
        Centaur,
        ZeusHead,
        Minotaur,
        MirrorKnight,
        BlindFold,
        WaterTitan,
        WindTitan,
        EarthTitan,
        FireTitan,
        Fiora,
        Kronus,
        FireShieldActor
    }

    public enum Heroes
    {
        Medusa=0,
        Posidanna,
        Ermolai,
        Heliemis,
        Averna
    }
    
    public enum AttackTypes
    {
        None,
        ProjectileAttack,
        TouchAttack
    }

    public enum GameState
    {
        CharacterSelection=0,
        Gameplay
    }

    public enum EnemyState
    {
        Idle=0,
        Walking,
        PhysicsControlled,
        Pushed,
        Petrified,
        PrimaryMoveUse,
        SecondaryMoveUse
    }

    public enum BattleRoyaleMaps
    {
        Earth_1,
        Earth_2,
        Ether_3,
        Ether_4,
        Fire_5,
        Fire_6,
        Mirror_7,
        Mirror_8,
        Water_9,
        Water_10,
        Wind_11,
        Wind_12,
    }
}
