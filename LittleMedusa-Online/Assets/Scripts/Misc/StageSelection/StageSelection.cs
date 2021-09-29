using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class StageSelection : MonoBehaviour
{
    public MatchConditionManager matchConditionManager;
    public Snapper snapper;
    public Image[] imageOptionArray;
    public Image[] imagePreviewArray;
    private void Awake()
    {
        snapper.InitialiseSnapper();
    }
    public void SetAllImageSelectedPanelOff()
    {
        foreach (Image item in imageOptionArray)
        {
            item.enabled = false;
        }
    }

    public void TurnOnSelectedPanel()
    {
        imageOptionArray[snapper.selectedIndex].enabled = true;
        foreach (Image item in imagePreviewArray)
        {
            item.enabled = false;
        }
        imagePreviewArray[snapper.selectedIndex].enabled = true;
        matchConditionManager.SetMap((int)imageOptionArray[snapper.selectedIndex].GetComponent<StageSelect>().battleRoyaleMaps);
    }

}
