using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cyclops : Enemy
{
    public float lineRangeForDetection;
    
    public override void Awake()
    {
        base.Awake();
        
        lineRangeForDetection = GameConfig.cyclopsLineRangeInTileUnitsForFollowing;
        walkSpeed = GameConfig.cyclopsMovementSpeed;

        mapperList.Add(new AimlessWandererMapper());
        mapperList.Add(new AStarPathFindMapper());

        //primaryMoveUseAction.SetAnimationSpeedAndSpritesOnUsage(GameConfig.cyclopsWalkAnimationDuration,GameConfig.cyclopsWalkAnimationDuration,);
    }
}
