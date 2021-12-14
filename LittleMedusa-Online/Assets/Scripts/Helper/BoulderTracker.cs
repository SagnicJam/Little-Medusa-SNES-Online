using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MedusaMultiplayer
{
    public class BoulderTracker : MonoBehaviour
    {
        //This data structure will be only in the server side build
        [Header("Live Data")]
        Dictionary<Vector3Int, int> cellPositionToOwnerDic = new Dictionary<Vector3Int, int>();

        //will be called when medusa removes a boulder
        public void RemoveBoulder(Vector3Int cellPositionToRemoveBoulder)
        {
            if (GridManager.instance.HasTileAtCellPoint(cellPositionToRemoveBoulder, EnumData.TileType.Boulder))
            {
                Debug.Log("Remove tile boulder on " + cellPositionToRemoveBoulder);
                GridManager.instance.SetTile(cellPositionToRemoveBoulder, EnumData.TileType.Boulder, false, false);

                if (cellPositionToOwnerDic.ContainsKey(cellPositionToRemoveBoulder))
                {
                    if (Server.clients.ContainsKey(cellPositionToOwnerDic[cellPositionToRemoveBoulder]))
                    {
                        if (Server.clients[cellPositionToOwnerDic[cellPositionToRemoveBoulder]].serverMasterController != null)
                        {
                            if (Server.clients[cellPositionToOwnerDic[cellPositionToRemoveBoulder]].serverMasterController.serverInstanceHero is Medusa serverMedusa)
                            {
                                serverMedusa.boulderUsedCount++;
                            }
                        }
                    }
                    cellPositionToOwnerDic.Remove(cellPositionToRemoveBoulder);
                }
            }
            else
            {
                Debug.LogError("Doesnot have any tile at cell point: " + cellPositionToRemoveBoulder);
            }
        }

        public void PlaceMedusaBoulderObject(Hero serverHero, Vector3Int cellPositionToPlaceBoulder, OnWorkDone onItemSuccessfullyUsed)
        {
            //place boulder
            if (!GridManager.instance.IsCellBlockedForSpawnObjectPlacementAtPos(cellPositionToPlaceBoulder))
            {
                if (!cellPositionToOwnerDic.ContainsKey(cellPositionToPlaceBoulder))
                {
                    Debug.Log("Setting tile non item boulder on " + cellPositionToPlaceBoulder);
                    GridManager.instance.SetTile(cellPositionToPlaceBoulder, EnumData.TileType.Boulder, true, false);
                    serverHero.boulderUsedCount--;
                    cellPositionToOwnerDic.Add(cellPositionToPlaceBoulder, serverHero.ownerId);
                    onItemSuccessfullyUsed?.Invoke();
                }
                else
                {
                    Debug.LogError("Dictionary already contains the key : " + cellPositionToPlaceBoulder);
                }
            }
        }
    }
}
