using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;

public class Tornado : MonoBehaviour
{
    [Header("TweakParams")]
    public int size = 10;

    [Header("Unit Template")]
    public GameObject tornadoColliderUnit;

    [Header("Live Data")]
    Dictionary<int, Dictionary<int, TornadoChild>> actorIdToPlacedTornadoDic = new Dictionary<int, Dictionary<int, TornadoChild>>();
    int calSize;

    private void Awake()
    {
        calSize = size / 2;
    }

    public void PlaceTornadoObject(int ownerCastingId,Vector3Int cellToPlaceOn)
    {
        //SpawnThe collider here
        GameObject colliderRef = Instantiate(tornadoColliderUnit
            , GridManager.instance.cellToworld(cellToPlaceOn)
            , Quaternion.identity);

        TornadoCollider tornadoCollider = colliderRef.GetComponent<TornadoCollider>();
        tornadoCollider.InitialiseOwner(ownerCastingId);

        //place tile tornado
        GridManager.instance.SetTile(cellToPlaceOn, EnumData.TileType.Tornado, true, false);

        List<Vector3Int> getCellToSolidify = GridManager.instance.GetSizeCells(calSize, cellToPlaceOn);

        if (actorIdToPlacedTornadoDic.ContainsKey(ownerCastingId))
        {
            actorIdToPlacedTornadoDic[ownerCastingId].Add(colliderRef.GetInstanceID(),new TornadoChild(colliderRef,new List<Actor>()));
        }
        else
        {
            Dictionary<int, TornadoChild> tornadoChildrenDictionary = new Dictionary<int, TornadoChild>();
            tornadoChildrenDictionary.Add(colliderRef.GetInstanceID(), new TornadoChild(colliderRef, new List<Actor>()));

            actorIdToPlacedTornadoDic.Add(ownerCastingId, tornadoChildrenDictionary);
        }
        GridManager.instance.SolidifyTiles(getCellToSolidify);

        //Start the coroutine here
        IEnumerator ie = WaitForTornado(ownerCastingId, cellToPlaceOn, colliderRef.GetInstanceID());
        StopCoroutine(ie);
        StartCoroutine(ie);
    }

    IEnumerator WaitForTornado(int ownerCasting, Vector3Int cellToRemoveFrom, int instanceIDCollider)
    {
        yield return new WaitForSeconds(5);

        if (actorIdToPlacedTornadoDic.ContainsKey(ownerCasting))
        {
            if(actorIdToPlacedTornadoDic[ownerCasting].ContainsKey(instanceIDCollider))
            {
                //disable physics
                List<Actor> effectedActors = new List<Actor>(actorIdToPlacedTornadoDic[ownerCasting][instanceIDCollider].actorsEffectedList);
                actorIdToPlacedTornadoDic[ownerCasting][instanceIDCollider].actorsEffectedList.Clear();
                foreach (Actor item in effectedActors)
                {
                    if(item!=null)
                    {
                        item.gamePhysics.RemoveForcePoint(GridManager.instance.cellToworld(cellToRemoveFrom));
                    }
                }

                //destroy colliders
                Destroy(actorIdToPlacedTornadoDic[ownerCasting][instanceIDCollider].tornadoCollider);
                actorIdToPlacedTornadoDic[ownerCasting].Remove(instanceIDCollider);
            }

            

            if (actorIdToPlacedTornadoDic[ownerCasting].Count == 0)
            {
                actorIdToPlacedTornadoDic.Remove(ownerCasting);
            }
        }

        //remove tile tornado
        GridManager.instance.SetTile(cellToRemoveFrom, EnumData.TileType.Tornado, false, false);
        GridManager.instance.SetTile(cellToRemoveFrom, EnumData.TileType.Solid, false, false);

        //set tiles to normal
        List<Vector3Int> getCellToNormalise = GridManager.instance.GetSizeCells(calSize, cellToRemoveFrom);
        foreach (KeyValuePair<int, Dictionary<int, TornadoChild>> kvp in actorIdToPlacedTornadoDic)
        {
            foreach (KeyValuePair<int, TornadoChild> item in kvp.Value)
            {
                List<Vector3Int> regionPositions = GridManager.instance.GetSizeCells(calSize, GridManager.instance.grid.WorldToCell(item.Value.tornadoCollider.transform.position));
                foreach (Vector3Int pos in regionPositions)
                {
                    if (getCellToNormalise.Contains(pos))
                    {
                        getCellToNormalise.Remove(pos);
                    }
                }
            }
        }
        //normalise all tiles in region here

        GridManager.instance.NormaliseTiles(getCellToNormalise);
    }

    public void OnEnterTornadoRegion(TileData tileData,Actor actor)
    {
        int ownerCasting = tileData.GetComponent<TornadoCollider>().ownerCasting;
        if(actorIdToPlacedTornadoDic.ContainsKey(ownerCasting))
        {
            actor.gamePhysics.AddForcePoint(tileData.gameObject.transform.position);
            if(actorIdToPlacedTornadoDic[ownerCasting].ContainsKey(tileData.gameObject.GetInstanceID()))
            {
                actorIdToPlacedTornadoDic[ownerCasting][tileData.gameObject.GetInstanceID()].actorsEffectedList.Add(actor);
            }
        }
        else
        {
            Debug.LogError("Coukd not find any key to add actor to Key: "+ownerCasting);
        }
    }

    public struct TornadoChild
    {
        public GameObject tornadoCollider;
        public List<Actor> actorsEffectedList;

        public TornadoChild(GameObject tornadoCollider, List<Actor> actorsEffectedList)
        {
            this.tornadoCollider = tornadoCollider;
            this.actorsEffectedList = actorsEffectedList;
        }

        public bool ContainsGameObject(GameObject go)
        {
            if(tornadoCollider==go)
            {
                return true;
            }
            return false;
        }
    }
}