using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TileData : MonoBehaviour
{
    public EnumData.TileType tileType;
    public bool blockFlyingUnits;
    public bool blockToSpawnObjectsPlacement;
    public bool blockUnitMotion;
    public bool blockPetrifiedObjects;
    public bool blockProjectiles;
    public bool solidifyTile;
    public bool isPlayer;
    public bool killUnitsInstantlyIfInTheirRegion;
    public bool isPhysicsControlled;


    public bool overrideInspectorValues;

    public virtual void Awake()
    {
        if(!overrideInspectorValues)
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
                case EnumData.TileType.WaterChannels:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = true;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    break;
                case EnumData.TileType.Up:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    break;
                case EnumData.TileType.Hourglass:
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
                case EnumData.TileType.IcarusWings:
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
                case EnumData.TileType.UpCereberusFire:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = true;
                    solidifyTile = false;
                    killUnitsInstantlyIfInTheirRegion = true;
                    break;
                case EnumData.TileType.DownCereberusFire:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = true;
                    solidifyTile = false;
                    killUnitsInstantlyIfInTheirRegion = true;
                    break;
                case EnumData.TileType.LeftCereberusFire:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = true;
                    solidifyTile = false;
                    killUnitsInstantlyIfInTheirRegion = true;
                    break;
                case EnumData.TileType.RightCereberusFire:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = true;
                    solidifyTile = false;
                    killUnitsInstantlyIfInTheirRegion = true;
                    break;
                case EnumData.TileType.Star:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    break;
                case EnumData.TileType.Key:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    solidifyTile = false;
                    break;
                case EnumData.TileType.Chest:
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
                    break;
                case EnumData.TileType.NoBoulder:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
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
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
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
                case EnumData.TileType.Monster:
                    blockFlyingUnits = false;
                    blockToSpawnObjectsPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    break;
            }
        }
    }
}
