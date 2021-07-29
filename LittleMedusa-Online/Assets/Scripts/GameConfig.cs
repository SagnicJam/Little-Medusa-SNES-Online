using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfig
{
    public const float boxCastCellSizePercent = 0.95f;

    #region Cyclops Data
    public const int cyclopsLineRangeInTileUnitsForFollowing = 18;
    public const float cyclopsMovementSpeed= 1.5f;
    public const float cyclopsWalkAnimationDuration = 1f;
    #endregion

    #region Medusa Data
    //Medusa Data
    public const float medusaEyeAttackAnimationDuration = 0.5f;
    public const float medusaWalkAnimationDuration = 0.5f;
    public const int eyeBeamDamage = 2;
    #endregion
}
