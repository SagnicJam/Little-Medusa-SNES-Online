using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MedusaMultiplayer
{
    public class GorgonGlassTracker : MonoBehaviour
    {
        public void PlaceGorgonTile(OnWorkDone onSuccessfullyItemPlaced)
        {
            List<Vector3Int> allNormalFloorCellPos = GridManager.instance.GetAllPositionForTileMap(EnumData.TileType.Normal);
            List<Vector3Int> allNoBoulderFloorCellPos = GridManager.instance.GetAllPositionForTileMap(EnumData.TileType.NoBoulder);

            foreach (Vector3Int item in allNormalFloorCellPos)
            {
                GridManager.instance.SetTile(item, EnumData.TileType.Mirror, true, false);
                GridManager.instance.SetTile(item, EnumData.TileType.Normal, false, false);
            }

            foreach (Vector3Int item in allNoBoulderFloorCellPos)
            {
                GridManager.instance.SetTile(item, EnumData.TileType.Mirror, true, false);
                GridManager.instance.SetTile(item, EnumData.TileType.NoBoulder, false, false);
            }

            IEnumerator ie = GorgonGlassTimer(allNormalFloorCellPos, allNoBoulderFloorCellPos);
            StopCoroutine(ie);
            StartCoroutine(ie);
            onSuccessfullyItemPlaced?.Invoke();
        }

        IEnumerator GorgonGlassTimer(List<Vector3Int> allNormalFloorCellPos, List<Vector3Int> allNoBoulderFloorCellPos)
        {
            int temp = 0;
            while (temp < GameConfig.gorgonGlassTickCount)
            {
                yield return new WaitForFixedUpdate();
                temp++;
            }
            foreach (Vector3Int item in allNormalFloorCellPos)
            {
                GridManager.instance.SetTile(item, EnumData.TileType.Mirror, false, false);
                GridManager.instance.SetTile(item, EnumData.TileType.Normal, true, false);
            }

            foreach (Vector3Int item in allNoBoulderFloorCellPos)
            {
                GridManager.instance.SetTile(item, EnumData.TileType.Mirror, false, false);
                GridManager.instance.SetTile(item, EnumData.TileType.NoBoulder, true, false);
            }
            yield break;
        }
    }
}