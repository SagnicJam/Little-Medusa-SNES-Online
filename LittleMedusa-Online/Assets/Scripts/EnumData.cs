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
        PlaceRemovalBoulder
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
        NoBoulder,
        Wall,
        Mirror,
        Switch,
        ThunderStruck,
        LightningBolt,
        Monster,
        Hero,
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

    public enum Projectiles
    {
        None,
        Arrow,
        EyeLaser,
        Water,
        HurricaneWind,
        BoulderProjectile,
        FireBall,
        FireShieldActor,
        FireTitanJuggler,
        FioraSpark,
        CardinalProjectile,
        DispersedBoulder
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

    public enum AttackTypes
    {
        None,
        ProjectileAttack,
        TouchAttack
    }

    public enum PlayerAnimation
    {
        PlayerWalk=0,
        PlayerFiringPrimaryProjectile
    }
}
