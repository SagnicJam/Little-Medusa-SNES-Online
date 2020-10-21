using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Mapper
{
    public List<FaceDirection> passableDirectionEnumList=new List<FaceDirection>();

    public abstract Vector3Int GetNewPathPoint(Actor tobeMappedActor);

    
}
