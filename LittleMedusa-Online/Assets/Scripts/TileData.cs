using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TileData : MonoBehaviour
{
    public EnumData.TileType tileType;
    public bool blockFlyingUnits;
    public bool blockBoulderPlacement;
    public bool blockUnitMotion;
    public bool blockPetrifiedObjects;
    public bool blockProjectiles;
    public bool killUnitsInstantlyIfInTheirRegion;
    public bool isPhysicsControlled;


    public bool overrideInspectorValues;

    private void Awake()
    {
        if(!overrideInspectorValues)
        {
            switch (tileType)
            {
                case EnumData.TileType.None:
                    blockFlyingUnits = true;
                    blockBoulderPlacement = true;
                    blockUnitMotion = true;
                    blockPetrifiedObjects = true;
                    blockProjectiles = true;
                    break;
                case EnumData.TileType.Empty:
                    blockFlyingUnits = false;
                    blockBoulderPlacement = true;
                    blockUnitMotion = true;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    break;
                case EnumData.TileType.Normal:
                    blockFlyingUnits = false;
                    blockBoulderPlacement = false;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    break;
                case EnumData.TileType.WaterChannels:
                    blockFlyingUnits = false;
                    blockBoulderPlacement = true;
                    blockUnitMotion = true;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    break;
                case EnumData.TileType.Up:
                    blockFlyingUnits = false;
                    blockBoulderPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    break;
                case EnumData.TileType.Hourglass:
                    blockFlyingUnits = false;
                    blockBoulderPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    break;
                case EnumData.TileType.UpArrow:
                    blockFlyingUnits = false;
                    blockBoulderPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    break;
                case EnumData.TileType.DownArrow:
                    blockFlyingUnits = false;
                    blockBoulderPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    break;
                case EnumData.TileType.LeftArrow:
                    blockFlyingUnits = false;
                    blockBoulderPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    break;
                case EnumData.TileType.RightArrow:
                    blockFlyingUnits = false;
                    blockBoulderPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    break;
                case EnumData.TileType.IcarusWings:
                    blockFlyingUnits = false;
                    blockBoulderPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    break;
                case EnumData.TileType.UpCereberusHead:
                    blockFlyingUnits = false;
                    blockBoulderPlacement = true;
                    blockUnitMotion = true;
                    blockPetrifiedObjects = true;
                    blockProjectiles = true;
                    break;
                case EnumData.TileType.DownCereberusHead:
                    blockFlyingUnits = false;
                    blockBoulderPlacement = true;
                    blockUnitMotion = true;
                    blockPetrifiedObjects = true;
                    blockProjectiles = true;
                    break;
                case EnumData.TileType.LeftCereberusHead:
                    blockFlyingUnits = false;
                    blockBoulderPlacement = true;
                    blockUnitMotion = true;
                    blockPetrifiedObjects = true;
                    blockProjectiles = true;
                    break;
                case EnumData.TileType.RightCereberusHead:
                    blockFlyingUnits = false;
                    blockBoulderPlacement = true;
                    blockUnitMotion = true;
                    blockPetrifiedObjects = true;
                    blockProjectiles = true;
                    break;
                case EnumData.TileType.Star:
                    blockFlyingUnits = false;
                    blockBoulderPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    break;
                case EnumData.TileType.Key:
                    blockFlyingUnits = false;
                    blockBoulderPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    break;
                case EnumData.TileType.Chest:
                    blockFlyingUnits = false;
                    blockBoulderPlacement = true;
                    blockUnitMotion = true;
                    blockPetrifiedObjects = true;
                    blockProjectiles = true;
                    break;
                case EnumData.TileType.SpawnJar:
                    blockFlyingUnits = false;
                    blockBoulderPlacement = true;
                    blockUnitMotion = true;
                    blockPetrifiedObjects = true;
                    blockProjectiles = true;
                    break;
                case EnumData.TileType.Hole:
                    blockFlyingUnits = false;
                    blockBoulderPlacement = true;
                    blockUnitMotion = true;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    break;
                case EnumData.TileType.NoBoulder:
                    blockFlyingUnits = false;
                    blockBoulderPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    break;
                case EnumData.TileType.Wall:
                    blockFlyingUnits = false;
                    blockBoulderPlacement = true;
                    blockUnitMotion = true;
                    blockPetrifiedObjects = true;
                    blockProjectiles = true;
                    break;
                case EnumData.TileType.Boulder:
                    blockFlyingUnits = false;
                    blockBoulderPlacement = true;
                    blockUnitMotion = true;
                    blockPetrifiedObjects = true;
                    blockProjectiles = true;
                    break;
                case EnumData.TileType.Mirror:
                    blockFlyingUnits = false;
                    blockBoulderPlacement = true;
                    blockUnitMotion = false;
                    blockPetrifiedObjects = false;
                    blockProjectiles = false;
                    break;
            }
        }
        
    }
}
