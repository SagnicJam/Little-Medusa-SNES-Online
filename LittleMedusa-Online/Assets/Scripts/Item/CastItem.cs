using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CastItem
{
    public EnumData.CastItemTypes castableItemType;
    public EnumData.UsableItemTypes usableItemType;
    public int itemCount;

    public CastItem(EnumData.CastItemTypes castableItemType,
        EnumData.UsableItemTypes usableItemType, int itemCount)
    {
        this.castableItemType = castableItemType;
        this.usableItemType = usableItemType;
        this.itemCount = itemCount;
    }


}
