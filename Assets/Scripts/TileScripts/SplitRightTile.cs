using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SplitRightTile : SplitTile {

    public List<GameObject> frontRoad = new List<GameObject>();
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
            frontRoad.Add(spawnedTileFront);

            GameObject spawnedTileRight = Road.Instance.GetRegularTile();
            float spawnedTileRightRotation = transform.eulerAngles.y + 90.0f;
            spawnedTileRight.transform.eulerAngles = new Vector3(0, spawnedTileRightRotation, 0);
            spawnedTileRight.transform.position = transform.Find("AttachPointRight").position;
            spawnedTileRight.name = "Tile" + (int.Parse(name.Substring(4)) + 1).ToString();
            spawnedTileRight.SetActive(true);
            rightRoad.Add(spawnedTileRight);
        }
        else
        {
            GameObject[] lastTile = new GameObject[2];
            lastTile[0] = frontRoad.Last();
            lastTile[1] = rightRoad.Last();
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
                    if (i == 0)
                    {
                        frontRoad.Add(spawnedTile);
                    }
                    else
                    {
                        rightRoad.Add(spawnedTile);
                    }
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
        }
        frontRoad.Clear();
        rightRoad.Clear();
    }
}
