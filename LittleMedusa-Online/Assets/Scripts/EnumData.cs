using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnumData
{
    public enum Inputs
    {
        Up=0,
        Left,
        Down,
        Right,
        Shoot,
        Push,
        PlaceRemovalBoulder,
        RespawnPlayer
    }

    public enum PosidannaInputs
    {
        Up = 0,
        Left,
        Down,
        Right,
        ShootTidalWave,
        CastBubbleShield,
        RespawnPlayer
    }

    public enum ErmolaiInputs
    {
        Up = 0,
        Left,
        Down,
        Right,
        CastPitfall,
        CastEarthquake,
        RespawnPlayer
    }

    public enum HeliemisInputs
    {
        Up = 0,
        Left,
        Down,
        Right,
        ShootMightyWind,
        PlaceTornado,
        RespawnPlayer
    }

    public enum AvernaInputs
    {
        Up = 0,
        Left,
        Down,
        Right,
        ShootFireBall,
        CastFlamePillar,
        RespawnPlayer
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
        WaterChannels,
        Up,
        Hourglass,
        UpArrow,
        DownArrow,
        LeftArrow,
        RightArrow,
        IcarusWings,
        UpCereberusHead,
        DownCereberusHead,
        LeftCereberusHead,
        RightCereberusHead,
        Star,
        Key,
        Chest,
        SpawnJar,
        Hole,
        Tornado,
        NoBoulder,
        Wall,
        Mirror,
        Switch,
        ThunderStruck,
        LightningBolt,
        Monster,
        Medusa,
        Poseidanna,
        Heliemis,
        Ermolai,
        Averna,
        WaterTitan,
        WindTitan,
        EarthTitan,
        FireTitan,
        Fiora,
        Kronus,
        BoulderAppearing,
        BoulderDisappearing,
        HurricaneWind,
        BoulderProjectile,
        DispersedBoulder,
        CrackEffect,
        DispersedFire,
        GravityChunks,
        LeftCereberusFire,
        RightCereberusFire,
        UpCereberusFire,
        DownCereberusFire,
        ChestOpenAnimation,
        BossBlockTile,
        SafeTile,
        RightRailIceWorldTile,
        LeftRailIceWorldTile,
        WaveBlocks,
        Stagalmite
    }

    public enum StaticAnimatingTiles
    {
        SmallExplosion,
        BigExplosion,
        Lightning
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
        DispersedFireBallMirrorKnight
    }

    public enum MonsterBreed
    {
        Cyclops=0,
        Snakes,
        Centaur,
        ZeusHead,
        Minotaur,
        BlindFold,
        MirrorKnight,
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
}
