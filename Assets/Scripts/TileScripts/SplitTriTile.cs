using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SplitTriTile : SplitTile {

    public List<GameObject> frontRoad = new List<GameObject>();
    public List<GameObject> leftRoad = new List<GameObject>();
    public List<GameObject> rightRoad = new List<GameObject>();

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

            GameObject spawnedTileRight = Road.Instance.GetRegularTile();
            float spawnedTileRightRotation = transform.eulerAngles.y + 90.0f;
            spawnedTileRight.transform.eulerAngles = new Vector3(0, spawnedTileRightRotation, 0);
            spawnedTileRight.transform.position = transform.Find("AttachPointRight").position;
            spawnedTileRight.name = "Tile" + (int.Parse(name.Substring(4)) + 1).ToString();
            spawnedTileRight.SetActive(true);
            // randomize obstacle
            if (obstacle == null)
            {
                Road.Instance.RandomizeObstacle(spawnedTileRight);
            }
            // spawn decorations
            Road.Instance.SpawnDecorations(spawnedTileRight);
            rightRoad.Add(spawnedTileRight);
            // spawn gems
            if (spawningGems)
            {
                Road.Instance.SpawnGems(spawnedTileFront);
                Road.Instance.SpawnGems(spawnedTileLeft);
                Road.Instance.SpawnGems(spawnedTileRight);
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
            GameObject[] lastTile = new GameObject[3];
            lastTile[0] = frontRoad.Last();
            lastTile[1] = leftRoad.Last();
            lastTile[2] = rightRoad.Last();
            GameObject spawnedTile;
            for (int i = 0; i < 3; ++i)
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
                    else if (i == 1)
                    {
                        leftRoad.Add(spawnedTile);
                    }
                    else
                    {
                        rightRoad.Add(spawnedTile);
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
            foreach (GameObject tile in rightRoad)
            {
                Road.Instance.RecycleTile(tile);
            }
        }
        else if (chosenDirection == Vector3.left)
        {
            Road.Instance.MergeTilesIntoRoad(leftRoad);
            foreach (GameObject tile in frontRoad)
            {
                Road.Instance.RecycleTile(tile);
            }
            foreach (GameObject tile in rightRoad)
            {
                Road.Instance.RecycleTile(tile);
            }
        }
        else
        {
            Road.Instance.MergeTilesIntoRoad(rightRoad);
            foreach (GameObject tile in frontRoad)
            {
                Road.Instance.RecycleTile(tile);
            }
            foreach (GameObject tile in leftRoad)
            {
                Road.Instance.RecycleTile(tile);
            }
        }
        frontRoad.Clear();
        leftRoad.Clear();
        rightRoad.Clear();
    }
}
