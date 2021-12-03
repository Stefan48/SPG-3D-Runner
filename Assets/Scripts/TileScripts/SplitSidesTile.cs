using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SplitSidesTile : SplitTile {

    public List<GameObject> leftRoad = new List<GameObject>();
    public List<GameObject> rightRoad = new List<GameObject>();

    public override void Spawn()
    {
        if (leftRoad.Count == 0)
        {
            GameObject spawnedTileLeft = Road.Instance.GetRegularTile();
            float spawnedTileLeftRotation = transform.eulerAngles.y - 90.0f;
            spawnedTileLeft.transform.eulerAngles = new Vector3(0, spawnedTileLeftRotation, 0);
            spawnedTileLeft.transform.position = transform.Find("AttachPointLeft").position;
            spawnedTileLeft.name = "Tile" + (int.Parse(name.Substring(4)) + 1).ToString();
            spawnedTileLeft.SetActive(true);
            leftRoad.Add(spawnedTileLeft);

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
            lastTile[0] = leftRoad.Last();
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
                        leftRoad.Add(spawnedTile);
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
        // TODO - wait time as a function of player's speed
        yield return new WaitForSeconds(mergeUnusedBranchDelay);
        if (chosenDirection == Vector3.left)
        {
            Road.Instance.MergeTilesIntoRoad(leftRoad);
            foreach (GameObject tile in rightRoad)
            {
                Road.Instance.RecycleTile(tile);
            }
        }
        else
        {
            Road.Instance.MergeTilesIntoRoad(rightRoad);
            foreach (GameObject tile in leftRoad)
            {
                Road.Instance.RecycleTile(tile);
            }
        }
        leftRoad.Clear();
        rightRoad.Clear();
    }
}