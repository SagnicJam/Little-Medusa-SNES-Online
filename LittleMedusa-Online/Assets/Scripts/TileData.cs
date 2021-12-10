using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TileData : MonoBehaviour
{
    public EnumData.TileType tileType;
    public EnumData.GameObjectEnums gameObjectEnums;
    public bool blockFlyingUnits;
    public bool blockToSpawnObjectsPlacement;
    public bool blockUnitMotion;
    public bool blockPetrifiedObjects;
    public bool blockProjectiles;
    public bool isGhostTile;
    public bool solidifyTile;
    public bool isItem;
    public bool killUnitsInstantlyIfInTheirRegion;
    public bool isPhysicsControlled;


    public virtual void Awake()
    {
        if(gameObjectEnums == EnumData.GameObjectEnums.None)
        {
            switch (tileType)
            {
                case EnumData.TileType.None:
                    blockFlyingUnits = true;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = true;
                    blockPetrifiedObjects = true;
                    blockProjectiles = true;
                    break;
                case EnumData.TileType.Empty:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = true;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    killUnitsInstantlyIfInTheirRegion = true;
                    solidifyTile = false;
                    break;
                case EnumData.TileType.Normal:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = false;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    break;
                case EnumData.TileType.Portal:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    break;
                case EnumData.TileType.UpArrow:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    break;
                case EnumData.TileType.DownArrow:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    break;
                case EnumData.TileType.LeftArrow:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    break;
                case EnumData.TileType.RightArrow:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    break;
                case EnumData.TileType.UpCereberusHead:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = true;
                    blockPetrifiedObjects = true;
                    blockProjectiles = true;
                    solidifyTile = true;
                    break;
                case EnumData.TileType.DownCereberusHead:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = true;
                    blockPetrifiedObjects = true;
                    blockProjectiles = true;
                    solidifyTile = true;
                    break;
                case EnumData.TileType.LeftCereberusHead:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = true;
                    blockPetrifiedObjects = true;
                    blockProjectiles = true;
                    solidifyTile = true;
                    break;
                case EnumData.TileType.RightCereberusHead:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = true;
                    blockPetrifiedObjects = true;
                    blockProjectiles = true;
                    solidifyTile = true;
                    break;
                case EnumData.TileType.SpawnJar:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = true;
                    blockPetrifiedObjects = true;
                    blockProjectiles = true;
                    solidifyTile = true;
                    break;
                case EnumData.TileType.Hole:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = true;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    killUnitsInstantlyIfInTheirRegion = true;
                    break;
                case EnumData.TileType.NoBoulder:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = false;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    break;
                case EnumData.TileType.Wall:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = true;
                    blockPetrifiedObjects = true;
                    blockProjectiles = true;
                    solidifyTile = true;
                    break;
                case EnumData.TileType.Boulder:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = true;
                    blockPetrifiedObjects = true;
                    blockProjectiles = true;
                    solidifyTile = true;
                    break;
                case EnumData.TileType.Mirror:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    break;
                case EnumData.TileType.Tornado:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = true;
                    blockPetrifiedObjects = true;
                    blockProjectiles = true;
                    solidifyTile = true;
                    break;
                case EnumData.TileType.Solid:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = true;
                    blockPetrifiedObjects = true;
                    blockProjectiles = true;
                    solidifyTile = true;
                    break;
                case EnumData.TileType.VoidDeathTiles:
                    blockFlyingUnits = true;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = true;
                    blockPetrifiedObjects = false;
                    blockProjectiles = true;
                    killUnitsInstantlyIfInTheirRegion = true;
                    break;
                case EnumData.TileType.ItemSpawner:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    break;
                case EnumData.TileType.Hourglass:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    isItem = true;
                    break;
                case EnumData.TileType.IcarusWingsItem:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    isItem = true;
                    isGhostTile = true;
                    break;
                case EnumData.TileType.HeartItem:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    isItem = true;
                    break;
                case EnumData.TileType.CereberausHeadItem:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    isItem = true;
                    break;
                case EnumData.TileType.MinionItem:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    isItem = true;
                    break;
                case EnumData.TileType.EyeLaserItem:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    isItem = true;
                    break;
                case EnumData.TileType.BoulderItem:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    isItem = true;
                    break;
                case EnumData.TileType.TidalWaveItem:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    isItem = true;
                    break;
                case EnumData.TileType.BubbleShieldItem:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    isItem = true;
                    break;
                case EnumData.TileType.MightyWindItem:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    isItem = true;
                    break;
                case EnumData.TileType.TornadoPullItem:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    isItem = true;
                    break;
                case EnumData.TileType.PitfallItem:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    isItem = true;
                    break;
                case EnumData.TileType.EarthQuakeItem:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    isItem = true;
                    break;
                case EnumData.TileType.FireballItem:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    isItem = true;
                    break;
                case EnumData.TileType.FlamePillarItem:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    isItem = true;
                    break;
                case EnumData.TileType.PortalItem:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    isItem = true;
                    break;
                case EnumData.TileType.AeloianMightItem:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    isItem = true;
                    break;
                case EnumData.TileType.QuickSandItem:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    isItem = true;
                    break;
                case EnumData.TileType.PermamnentBlockItem:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    isItem = true;
                    break;
                case EnumData.TileType.StarShowerItem:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    isItem = true;
                    break;
                case EnumData.TileType.CentaurBowItem:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    isItem = true;
                    break;
                case EnumData.TileType.ArrowDirectionItem:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    isItem = true;
                    break;
                case EnumData.TileType.MirrorItem:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    isItem = true;
                    break;
                case EnumData.TileType.GorgonGlassItem:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    isItem = true;
                    break;
            }
        }
    }
}
