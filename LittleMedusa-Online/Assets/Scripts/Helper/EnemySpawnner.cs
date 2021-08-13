using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnner : MonoBehaviour
{
    public List<GameObject> enemyPrefab;
    public EnumData.MonsterBreed monsterToSpawn;

    public List<Vector3Int> spawnIndexList = new List<Vector3Int>();

    private void Start()
    {
        spawnIndexList = GridManager.instance.GetAllPositionForTileMap(EnumData.TileType.SpawnJar);
    }

    public void SpawnEnemy(EnumData.MonsterBreed monsterBreedToSpawn,Vector3Int cell)
    {
        IEnumerator ie = waitAndStartCor(monsterBreedToSpawn, cell);
        StopCoroutine(ie);
        StartCoroutine(ie);
    }

    IEnumerator waitAndStartCor(EnumData.MonsterBreed monsterBreedToSpawn, Vector3Int cell)
    {
        yield return new WaitForSeconds(1.5f);

        GameObject enemy = Instantiate(enemyPrefab[(int)monsterBreedToSpawn]);
        Actor actor = enemy.GetComponentInChildren<Actor>();
        actor.transform.position = GridManager.instance.cellToworld(cell);
        actor.transform.rotation = Quaternion.identity;
        actor.currentMovePointCellPosition = GridManager.instance.grid.WorldToCell(actor.transform.position);
    }

}
