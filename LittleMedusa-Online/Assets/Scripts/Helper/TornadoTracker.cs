using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;
namespace MedusaMultiplayer
{
    public class TornadoTracker : MonoBehaviour
    {
        [Header("Unit Template")]
        public GameObject tornadoColliderUnit;

        [Header("Live Data")]
        Dictionary<int, Dictionary<int, TornadoChild>> actorIdToPlacedTornadoDic = new Dictionary<int, Dictionary<int, TornadoChild>>();


        public void PlaceTornadoObject(Hero heroOwner, int direction, Vector3Int cellToPlaceOn, OnWorkDone onItemSuccessfullyUsed)
        {
            if (heroOwner.IsHeroAbleToFireProjectiles((FaceDirection)direction))
            {
                Debug.Log("CastTornadoForPlayerImplementation ");
                //SpawnThe collider here
                GameObject colliderRef = Instantiate(tornadoColliderUnit
                    , GridManager.instance.cellToworld(cellToPlaceOn)
                    , Quaternion.identity);

                TornadoCollider tornadoCollider = colliderRef.GetComponent<TornadoCollider>();
                tornadoCollider.InitialiseOwner(heroOwner.ownerId);

                //place tile tornado
                GridManager.instance.SetTile(cellToPlaceOn, EnumData.TileType.Tornado, true, false);
                GridManager.instance.SetTile(cellToPlaceOn, EnumData.TileType.Solid, true, false);
                List<Vector3Int> getCellToSolidify = GridManager.instance.GetSizeCells(GameConfig.tornadoSize / 2, cellToPlaceOn);

                if (actorIdToPlacedTornadoDic.ContainsKey(heroOwner.ownerId))
                {
                    actorIdToPlacedTornadoDic[heroOwner.ownerId].Add(colliderRef.GetInstanceID(), new TornadoChild(colliderRef, new List<Actor>()));
                }
                else
                {
                    Dictionary<int, TornadoChild> tornadoChildrenDictionary = new Dictionary<int, TornadoChild>();
                    tornadoChildrenDictionary.Add(colliderRef.GetInstanceID(), new TornadoChild(colliderRef, new List<Actor>()));

                    actorIdToPlacedTornadoDic.Add(heroOwner.ownerId, tornadoChildrenDictionary);
                }
                GridManager.instance.SolidifyTiles(getCellToSolidify);

                //Start the coroutine here
                IEnumerator ie = WaitForTornado(heroOwner, cellToPlaceOn, colliderRef.GetInstanceID());
                StopCoroutine(ie);
                StartCoroutine(ie);

                heroOwner.tornadoPlacedUsedCount--;
                onItemSuccessfullyUsed?.Invoke();

            }
            else
            {
                Debug.Log("Tornado-Hero is not able to fire projectiles");
            }

        }

        IEnumerator WaitForTornado(Hero ownerHero, Vector3Int cellToRemoveFrom, int instanceIDCollider)
        {
            yield return new WaitForSeconds(5);

            if (actorIdToPlacedTornadoDic.ContainsKey(ownerHero.ownerId))
            {
                if (actorIdToPlacedTornadoDic[ownerHero.ownerId].ContainsKey(instanceIDCollider))
                {
                    //disable physics
                    List<Actor> effectedActors = new List<Actor>(actorIdToPlacedTornadoDic[ownerHero.ownerId][instanceIDCollider].actorsEffectedList);
                    actorIdToPlacedTornadoDic[ownerHero.ownerId][instanceIDCollider].actorsEffectedList.Clear();
                    foreach (Actor item in effectedActors)
                    {
                        if (item != null)
                        {
                            item.gamePhysics.RemoveForcePoint(GridManager.instance.cellToworld(cellToRemoveFrom));
                        }
                    }

                    //destroy colliders
                    Destroy(actorIdToPlacedTornadoDic[ownerHero.ownerId][instanceIDCollider].tornadoCollider);
                    ownerHero.tornadoPlacedUsedCount++;
                    actorIdToPlacedTornadoDic[ownerHero.ownerId].Remove(instanceIDCollider);
                }



                if (actorIdToPlacedTornadoDic[ownerHero.ownerId].Count == 0)
                {
                    actorIdToPlacedTornadoDic.Remove(ownerHero.ownerId);
                }
            }

            //remove tile tornado
            GridManager.instance.SetTile(cellToRemoveFrom, EnumData.TileType.Tornado, false, false);
            GridManager.instance.SetTile(cellToRemoveFrom, EnumData.TileType.Solid, false, false);

            //set tiles to normal
            List<Vector3Int> getCellToNormalise = GridManager.instance.GetSizeCells(GameConfig.tornadoSize / 2, cellToRemoveFrom);
            foreach (KeyValuePair<int, Dictionary<int, TornadoChild>> kvp in actorIdToPlacedTornadoDic)
            {
                foreach (KeyValuePair<int, TornadoChild> item in kvp.Value)
                {
                    List<Vector3Int> regionPositions = GridManager.instance.GetSizeCells(GameConfig.tornadoSize / 2, GridManager.instance.grid.WorldToCell(item.Value.tornadoCollider.transform.position));
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

        public void OnEnterTornadoRegion(TileData tileData, Actor actor)
        {
            int ownerCasting = tileData.GetComponent<TornadoCollider>().ownerCasting;
            if (actorIdToPlacedTornadoDic.ContainsKey(ownerCasting))
            {
                actor.gamePhysics.AddForcePoint(tileData.gameObject.transform.position);
                if (actorIdToPlacedTornadoDic[ownerCasting].ContainsKey(tileData.gameObject.GetInstanceID()))
                {
                    actorIdToPlacedTornadoDic[ownerCasting][tileData.gameObject.GetInstanceID()].actorsEffectedList.Add(actor);
                }
            }
            else
            {
                Debug.LogError("Coukd not find any key to add actor to Key: " + ownerCasting);
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
                if (tornadoCollider == go)
                {
                    return true;
                }
                return false;
            }
        }
    }
}