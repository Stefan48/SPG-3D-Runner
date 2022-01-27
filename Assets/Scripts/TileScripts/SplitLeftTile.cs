using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SplitLeftTile : SplitTile {

    public List<GameObject> frontRoad = new List<GameObject>();
    public List<GameObject> leftRoad = new List<GameObject>();

    public override void Spawn()
    {
        if (frontRoad.Count == 0)
        {
            GameObject spawnedTileFront = Road.Instance.GetRegularTile();
            float spawnedTileFrontRotation = transform.eulerAngles.y;
            spawnedTileFront.transform.eulerAngles = new Vector3(0, spawnedTileFrontRotation, 0);
            spawnedTileFront.transform.position = transform.Find("AttachPointFront").position;
            spawnedTileFront.name = "Tile" + (int.Parse(name.Substring(4)) + 1).ToString();
            spawnedTileFront.SetActive(true);
            // randomize obstacle
            if (obstacle == null)
            {
                Road.Instance.RandomizeObstacle(spawnedTileFront);
            }
            // spawn decorations
            Road.Instance.SpawnDecorations(spawnedTileFront);
            frontRoad.Add(spawnedTileFront);

            GameObject spawnedTileLeft = Road.Instance.GetRegularTile();
            float spawnedTileLeftRotation = transform.eulerAngles.y - 90.0f;
            spawnedTileLeft.transform.eulerAngles = new Vector3(0, spawnedTileLeftRotation, 0);
            spawnedTileLeft.transform.position = transform.Find("AttachPointLeft").position;
            spawnedTileLeft.name = "Tile" + (int.Parse(name.Substring(4)) + 1).ToString();
            spawnedTileLeft.SetActive(true);
            // randomize obstacle
            if (obstacle == null)
            {
                Road.Instance.RandomizeObstacle(spawnedTileLeft);
            }
            // spawn decorations
            Road.Instance.SpawnDecorations(spawnedTileLeft);
            leftRoad.Add(spawnedTileLeft);
            // spawn gems
            if (spawningGems)
            {
                Road.Instance.SpawnGems(spawnedTileFront);
                Road.Instance.SpawnGems(spawnedTileLeft);
                numTilesWithGemsRemaining--;
                if (numTilesWithGemsRemaining == 0)
                {
                    spawningGems = false;
                }
            }
            else
            {
                if (Random.Range(0, 100) < Road.Instance.gemSpawnPercentage)
                {
                    spawningGems = true;
                    numTilesWithGemsRemaining = Random.Range(Road.Instance.minConsecutiveTilesWithGems, Road.Instance.maxConsecutiveTilesWithGems);
                }
            }
        }
        else
        {
            GameObject[] lastTile = new GameObject[2];
            lastTile[0] = frontRoad.Last();
            lastTile[1] = leftRoad.Last();
            GameObject spawnedTile;
            for (int i = 0; i < 2; ++i)
            {
                if (lastTile[i].tag.Contains("TileSplit"))
                {
                    lastTile[i].GetComponent<SplitTile>().Spawn();
                }
                else
                {
                    if (lastTile[i].tag.Contains("TileCorner"))
                    {
                        // there can't be consecutive corner tiles (must avoid overlapping)
                        spawnedTile = Road.Instance.GetRegularTile();
                    }
                    else
                    {
                        spawnedTile = Road.Instance.GetRandomTile();
                    }
                    // compute tile's position and rotation based on previous tile
                    Road.SetTilePositionAndRotation(spawnedTile, lastTile[i]);
                    // set tile's name according to its index
                    spawnedTile.name = "Tile" + (int.Parse(lastTile[i].name.Substring(4)) + 1).ToString();
                    spawnedTile.SetActive(true);
                    // randomize obstacle
                    if (lastTile[i].GetComponent<Tile>().obstacle == null)
                    {
                        Road.Instance.RandomizeObstacle(spawnedTile);
                    }
                    // spawn decorations
                    Road.Instance.SpawnDecorations(spawnedTile);
                    // spawn gems
                    Tile tileComponent = spawnedTile.GetComponent<Tile>();
                    if (spawningGems && (tileComponent.obstacle == null || tileComponent.obstacleType != ObstacleType.RollUnder))
                    {
                        Road.Instance.SpawnGems(spawnedTile);
                    }
                    if (spawnedTile.tag.Contains("TileSplit"))
                    {
                        SplitTile splitTileComponent = spawnedTile.GetComponent<SplitTile>();
                        splitTileComponent.spawningGems = spawningGems;
                        splitTileComponent.numTilesWithGemsRemaining = numTilesWithGemsRemaining;
                    }
                    if (i == 0)
                    {
                        frontRoad.Add(spawnedTile);
                    }
                    else
                    {
                        leftRoad.Add(spawnedTile);
                    }
                }
            }
            if (spawningGems)
            {
                numTilesWithGemsRemaining--;
                if (numTilesWithGemsRemaining == 0)
                {
                    spawningGems = false;
                }
            }
            else
            {
                if (Random.Range(0, 100) < Road.Instance.gemSpawnPercentage)
                {
                    spawningGems = true;
                    numTilesWithGemsRemaining = Random.Range(Road.Instance.minConsecutiveTilesWithGems, Road.Instance.maxConsecutiveTilesWithGems);
                }
            }
        }
    }

    public override IEnumerator MergeIntoRoad(Vector3 chosenDirection)
    {
        yield return new WaitForSeconds(mergeUnusedBranchDelay);
        if (chosenDirection == Vector3.forward)
        {
            Road.Instance.MergeTilesIntoRoad(frontRoad);
            foreach (GameObject tile in leftRoad)
            {
                Road.Instance.RecycleTile(tile);
            }
        }
        else
        {
            Road.Instance.MergeTilesIntoRoad(leftRoad);
            foreach (GameObject tile in frontRoad)
            {
                Road.Instance.RecycleTile(tile);
            }
        }
        frontRoad.Clear();
        leftRoad.Clear();
    }
}
