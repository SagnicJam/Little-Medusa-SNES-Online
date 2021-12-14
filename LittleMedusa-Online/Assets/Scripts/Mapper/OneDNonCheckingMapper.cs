using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MedusaMultiplayer
{
    public class OneDNonCheckingMapper : Mapper
    {
        public FaceDirection face;
        public OneDNonCheckingMapper(FaceDirection face)
        {
            this.face = face;
        }

        public override Vector3Int GetNewPathPoint(Actor tobeMappedActor)
        {
            Vector3Int posToAnalyseForNextPoint = tobeMappedActor.currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(face));

            return posToAnalyseForNextPoint;
        }
    }
}