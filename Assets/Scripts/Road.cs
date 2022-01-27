﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ObstacleType
{
    None,
    JumpOver,
    FallInto,
    RollUnder
}

public class Road : MonoBehaviour {

    // Singleton Pattern
    private static Road instance;
    public static Road Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Road>();
            }
            return instance;
        }
    }

    public string theme;

    private GameObject RegularTilePrefab;
    private GameObject CornerRightTilePrefab;
    private GameObject CornerLeftTilePrefab;
    private GameObject SplitSidesTilePrefab;
    private GameObject SplitRightTilePrefab;
    private GameObject SplitLeftTilePrefab;
    private GameObject SplitTriTilePrefab;

    // the storage max count value should be chosen to minimize memory overhead
    private const int tileStorageMaxCount = 30;
    private List<GameObject> regularTilesStorage = new List<GameObject>();
    private List<GameObject> cornerRightTilesStorage = new List<GameObject>();
    private List<GameObject> cornerLeftTilesStorage = new List<GameObject>();
    private List<GameObject> splitSidesTilesStorage = new List<GameObject>();
    private List<GameObject> splitRightTilesStorage = new List<GameObject>();
    private List<GameObject> splitLeftTilesStorage = new List<GameObject>();
    private List<GameObject> splitTriTilesStorage = new List<GameObject>();

    private const int maxTilesAhead = 4;
    private const int maxTilesBehind = 2;
    private int tilesBehind = 0;
    private List<GameObject> road = new List<GameObject>();


    private const int numObstaclesJumpOverTypes = 4;
    private List<GameObject> ObstaclesJumpOverPrefabs = new List<GameObject>();
    private const int numObstaclesFallIntoTypes = 1;
    private List<GameObject> ObstaclesFallIntoPrefabs = new List<GameObject>();
    private const int numObstaclesRollUnderTypes = 1;
    private List<GameObject> ObstaclesRollUnderPrefabs = new List<GameObject>();

    private const int obstacleStorageCountPerType = 5;
    private List<GameObject> obstaclesJumpOverStorage = new List<GameObject>();
    private List<GameObject> obstaclesFallIntoStorage = new List<GameObject>();
    private List<GameObject> obstaclesRollUnderStorage = new List<GameObject>();


    private GameObject gemPrefab;
    private const int gemStorageMaxCount = 100;
    private List<GameObject> gemStorage = new List<GameObject>();
    public bool spawningGems = false;
    public int numTilesWithGemsRemaining = 0;
    public int gemSpawnPercentage = 10;
    public int minConsecutiveTilesWithGems = 5;
    public int maxConsecutiveTilesWithGems = 10;


    private const int numDecorationTypes = 8;
    private List<GameObject> DecorationPrefabs = new List<GameObject>();
    private const int decorationStorageCountPerType = 10;
    private List<GameObject> decorationStorage = new List<GameObject>();
    private int decorationSpawnPercentage = 30;


    public void IncrementTilesBehind()
    {
        tilesBehind++;
    }
    
    public GameObject GetRegularTile()
    {
        GameObject tile = regularTilesStorage.Last();
        regularTilesStorage.RemoveAt(regularTilesStorage.Count - 1);
        return tile;
    }

    public GameObject GetRandomTile()
    {
        GameObject tile;
        int type = Random.Range(0, 100);
        if (type < 20)
        {
            tile = cornerRightTilesStorage.Last();
            cornerRightTilesStorage.RemoveAt(cornerRightTilesStorage.Count - 1);
        }
        else if (type < 40)
        {
            tile = cornerLeftTilesStorage.Last();
            cornerLeftTilesStorage.RemoveAt(cornerLeftTilesStorage.Count - 1);
        }
        else if (type < 44)
        {
            tile = splitSidesTilesStorage.Last();
            splitSidesTilesStorage.RemoveAt(splitSidesTilesStorage.Count - 1);
        }
        else if (type < 48)
        {
            tile = splitRightTilesStorage.Last();
            splitRightTilesStorage.RemoveAt(splitRightTilesStorage.Count - 1);
        }
        else if (type < 52)
        {
            tile = splitLeftTilesStorage.Last();
            splitLeftTilesStorage.RemoveAt(splitLeftTilesStorage.Count - 1);
        }
        else if (type < 55)
        {
            tile = splitTriTilesStorage.Last();
            splitTriTilesStorage.RemoveAt(splitTriTilesStorage.Count - 1);
        }
        else
        {
            tile = regularTilesStorage.Last();
            regularTilesStorage.RemoveAt(regularTilesStorage.Count - 1);
        }
        return tile;
    }

    public void RandomizeObstacle(GameObject tile)
    {
        int obstacleType = Random.Range(0, 100);
        if (obstacleType < 10)
        {
            // obstacle of 'jump over' type
            int index = Random.Range(0, obstaclesJumpOverStorage.Count);
            GameObject obstacle = obstaclesJumpOverStorage[index];
            obstaclesJumpOverStorage.RemoveAt(index);
            // vary the position and rotation
            float positionXOffset = (Random.Range(0, 100) - 50.0f) / 70.0f;
            float positionZOffset = (Random.Range(0, 100) - 50.0f) / 70.0f;
            obstacle.transform.position = new Vector3(tile.transform.position.x + positionXOffset, obstacle.transform.position.y, tile.transform.position.z + positionZOffset);
            float rotationOffset = (Random.Range(0, 100) - 50.0f) * 1.2f;
            obstacle.transform.rotation = tile.transform.rotation;
            obstacle.transform.eulerAngles += new Vector3(0.0f, rotationOffset, 0.0f);
            obstacle.SetActive(true);
            Tile tileComponent = tile.GetComponent<Tile>();
            tileComponent.obstacle = obstacle;
            tileComponent.obstacleType = ObstacleType.JumpOver;
        }
        else if (obstacleType < 20)
        {
            // obstacle of 'fall into' type
            int index = Random.Range(0, obstaclesFallIntoStorage.Count);
            GameObject obstacle = obstaclesFallIntoStorage[index];
            obstaclesFallIntoStorage.RemoveAt(index);
            // vary the position
            float positionXOffset = (Random.Range(0, 100) - 50.0f) / 70.0f;
            float positionZOffset = (Random.Range(0, 100) - 50.0f) / 70.0f;
            obstacle.transform.position = new Vector3(tile.transform.position.x + positionXOffset, obstacle.transform.position.y, tile.transform.position.z + positionZOffset);            
            obstacle.transform.rotation = tile.transform.rotation;
            obstacle.SetActive(true);
            Tile tileComponent = tile.GetComponent<Tile>();
            tileComponent.obstacle = obstacle;
            tileComponent.obstacleType = ObstacleType.FallInto;
        }
        else if (obstacleType < 30 && tile.tag == "TileRegular")
        {
            // obstacle of 'roll under' type
            int index = Random.Range(0, obstaclesRollUnderStorage.Count);
            GameObject obstacle = obstaclesRollUnderStorage[index];
            obstaclesRollUnderStorage.RemoveAt(index);
            // vary the position on the direction of movement
            float positionOffset = (Random.Range(0, 100) - 50.0f) / 40.0f;
            obstacle.transform.position = new Vector3(tile.transform.position.x, obstacle.transform.position.y, tile.transform.position.z) + tile.transform.forward * positionOffset;
            obstacle.transform.rotation = tile.transform.rotation;
            obstacle.SetActive(true);
            Tile tileComponent = tile.GetComponent<Tile>();
            tileComponent.obstacle = obstacle;
            tileComponent.obstacleType = ObstacleType.RollUnder;
        }
    }

    public void SpawnGems(GameObject tile)
    {
        Tile tileComponent = tile.GetComponent<Tile>();
        for (int i = 0; i < tileComponent.numGemPositions; ++i)
        {
            GameObject gem = gemStorage.Last();
            gemStorage.RemoveAt(gemStorage.Count - 1);
            //gem.transform.position = tile.transform.position + tileComponent.gemPositions[i];
            gem.transform.position = tile.transform.Find("GemPosition" + i).transform.position;
            gem.SetActive(true);
            tileComponent.gems.Add(gem);
        }
    }

    public void SpawnDecorations(GameObject tile)
    {
        Tile tileComponent = tile.GetComponent<Tile>();
        List<int> decorationPositionsRemaining = new List<int>();
        for (int i = 0; i < tileComponent.numDecorationPositions; ++i)
        {
            decorationPositionsRemaining.Add(i);
        }
        int spawnPercentage = decorationSpawnPercentage;
        while (decorationPositionsRemaining.Count > 0)
        {
            if (Random.Range(0, 100) < spawnPercentage)
            {
                int index = Random.Range(0, decorationStorage.Count);
                GameObject decoration = decorationStorage[index];
                decorationStorage.RemoveAt(index);
                index = Random.Range(0, decorationPositionsRemaining.Count);
                decoration.transform.position = tile.transform.Find("DecorationPosition" + decorationPositionsRemaining[index]).transform.position;
                decorationPositionsRemaining.RemoveAt(index);
                // randomize the rotation
                int rotation= Random.Range(0, 360);
                decoration.transform.eulerAngles = new Vector3(0, rotation, 0);
                decoration.SetActive(true);
                tileComponent.decorations.Add(decoration);
                // reduce the probability to spawn additional decorations for the same tile
                spawnPercentage /= 2;
            }
            else
            {
                break;
            }
        }
    }

    public void RecycleTile(GameObject tile)
    {
        Tile tileComponent = tile.GetComponent<Tile>();
        // recycle obstacle
        if (tileComponent.obstacle != null)
        {
            tileComponent.obstacle.SetActive(false);
            if (tileComponent.obstacleType == ObstacleType.JumpOver)
            {
                obstaclesJumpOverStorage.Add(tileComponent.obstacle);
            }
            else if (tileComponent.obstacleType == ObstacleType.FallInto)
            {
                obstaclesFallIntoStorage.Add(tileComponent.obstacle);
            }
            else if (tileComponent.obstacleType == ObstacleType.RollUnder)
            {
                obstaclesRollUnderStorage.Add(tileComponent.obstacle);
            }
            tileComponent.obstacle = null;
            tileComponent.obstacleType = ObstacleType.None;
        }
        // recycle gems
        for (int i = 0; i < tileComponent.gems.Count; ++i)
        {
            tileComponent.gems[i].SetActive(false);
            gemStorage.Add(tileComponent.gems[i]);
        }
        tileComponent.gems.Clear();
        // recycle decorations
        for (int i = 0; i < tileComponent.decorations.Count; ++i)
        {
            tileComponent.decorations[i].SetActive(false);
            decorationStorage.Add(tileComponent.decorations[i]);
        }
        tileComponent.decorations.Clear();
        // recycle tile
        tile.SetActive(false);
        if (tile.tag == "TileCornerRight")
        {
            cornerRightTilesStorage.Add(tile);
        }
        else if (tile.tag == "TileCornerLeft")
        {
            cornerLeftTilesStorage.Add(tile);
        }
        // recycle split tiles recursively
        else if (tile.tag == "TileSplitSides")
        {
            SplitSidesTile splitSidesTileComponent = tile.GetComponent<SplitSidesTile>();
            List<GameObject> leftRoad = splitSidesTileComponent.leftRoad;
            List<GameObject> rightRoad = splitSidesTileComponent.rightRoad;
            foreach (GameObject t in leftRoad)
            {
                RecycleTile(t);
            }
            foreach (GameObject t in rightRoad)
            {
                RecycleTile(t);
            }
            leftRoad.Clear();
            rightRoad.Clear();
            splitSidesTilesStorage.Add(tile);
        }
        else if (tile.tag == "TileSplitRight")
        {
            SplitRightTile splitRightTileComponent = tile.GetComponent<SplitRightTile>();
            List<GameObject> frontRoad = splitRightTileComponent.frontRoad;
            List<GameObject> rightRoad = splitRightTileComponent.rightRoad;
            foreach (GameObject t in frontRoad)
            {
                RecycleTile(t);
            }
            foreach (GameObject t in rightRoad)
            {
                RecycleTile(t);
            }
            frontRoad.Clear();
            rightRoad.Clear();
            splitRightTilesStorage.Add(tile);
        }
        else if (tile.tag == "TileSplitLeft")
        {
            SplitLeftTile splitLeftTileComponent = tile.GetComponent<SplitLeftTile>();
            List<GameObject> frontRoad = splitLeftTileComponent.frontRoad;
            List<GameObject> leftRoad = splitLeftTileComponent.leftRoad;
            foreach (GameObject t in frontRoad)
            {
                RecycleTile(t);
            }
            foreach (GameObject t in leftRoad)
            {
                RecycleTile(t);
            }
            frontRoad.Clear();
            leftRoad.Clear();
            splitLeftTilesStorage.Add(tile);
        }
        else if (tile.tag == "TileSplitTri")
        {
            SplitTriTile splitTriTileComponent = tile.GetComponent<SplitTriTile>();
            List<GameObject> frontRoad = splitTriTileComponent.frontRoad;
            List<GameObject> leftRoad = splitTriTileComponent.leftRoad;
            List<GameObject> rightRoad = splitTriTileComponent.rightRoad;
            foreach (GameObject t in frontRoad)
            {
                RecycleTile(t);
            }
            foreach (GameObject t in leftRoad)
            {
                RecycleTile(t);
            }
            foreach (GameObject t in rightRoad)
            {
                RecycleTile(t);
            }
            frontRoad.Clear();
            leftRoad.Clear();
            rightRoad.Clear();
            splitTriTilesStorage.Add(tile);
        }
        else
        {
            regularTilesStorage.Add(tile);
        }
    }

    public static void SetTilePositionAndRotation(GameObject spawnedTile, GameObject lastTile)
    {
        float spawnedTileRotation;
        if (lastTile.tag == "TileCornerRight")
        {
            spawnedTileRotation = lastTile.transform.eulerAngles.y + 90.0f;
        }
        else if (lastTile.tag == "TileCornerLeft")
        {
            spawnedTileRotation = lastTile.transform.eulerAngles.y - 90.0f;
        }
        else
        {
            spawnedTileRotation = lastTile.transform.eulerAngles.y;
        }
        spawnedTile.transform.position = lastTile.transform.Find("AttachPoint").transform.position;
        spawnedTile.transform.eulerAngles = new Vector3(0, spawnedTileRotation, 0);
    }

    public void SpawnTile()
    {
        if (tilesBehind > maxTilesBehind)
        {
            // erase road's oldest tile
            GameObject tile = road[0];
            RecycleTile(tile);
            road.RemoveAt(0);
        }

        GameObject lastTile = road.Last();
        if (lastTile.tag.Contains("TileSplit"))
        {
            lastTile.GetComponent<SplitTile>().Spawn();
        }
        else
        {
            GameObject spawnedTile;
            if (lastTile.tag.Contains("TileCorner"))
            {
                // there can't be consecutive corner tiles (must avoid overlapping)
                spawnedTile = GetRegularTile();
            }
            else
            {
                spawnedTile = GetRandomTile();
            }
            // compute tile's position and rotation based on previous tile
            SetTilePositionAndRotation(spawnedTile, lastTile);
            // set tile's name according to its index
            if (lastTile.name == "TileStart")
            {
                spawnedTile.name = "Tile1";
            }
            else
            {
                spawnedTile.name = "Tile" + (int.Parse(lastTile.name.Substring(4)) + 1).ToString();
            }
            spawnedTile.SetActive(true);
            // randomize obstacle (there can't be consecutive tiles with obstacles)
            if (lastTile.GetComponent<Tile>().obstacle == null)
            {
                RandomizeObstacle(spawnedTile);
            }
            // spawn gems
            Tile tileComponent = spawnedTile.GetComponent<Tile>();
            if (spawningGems && (tileComponent.obstacle == null || tileComponent.obstacleType != ObstacleType.RollUnder))
            {
                SpawnGems(spawnedTile);
                numTilesWithGemsRemaining--;
                if (numTilesWithGemsRemaining == 0)
                {
                    spawningGems = false;
                }
            }
            else
            {
                if (Random.Range(0, 100) < gemSpawnPercentage)
                {
                    spawningGems = true;
                    numTilesWithGemsRemaining = Random.Range(minConsecutiveTilesWithGems, maxConsecutiveTilesWithGems);
                }
            }
            if (spawnedTile.tag.Contains("TileSplit"))
            {
                SplitTile splitTileComponent = spawnedTile.GetComponent<SplitTile>();
                splitTileComponent.spawningGems = spawningGems;
                splitTileComponent.numTilesWithGemsRemaining = numTilesWithGemsRemaining;
            }
            // spawn decorations
            SpawnDecorations(spawnedTile);
            road.Add(spawnedTile);
        }
    }

    public void MergeTilesIntoRoad(List<GameObject> tiles)
    {
        road.AddRange(tiles);
    }

	// Use this for initialization
	void Start ()
    {
        string prefabsPath = "Prefabs/" + theme + "/";
        // load tile prefabs
        string tilesPath = prefabsPath + "Tiles/";
        RegularTilePrefab = Resources.Load<GameObject>(tilesPath + "TileRegular");
        CornerRightTilePrefab = Resources.Load<GameObject>(tilesPath + "TileCornerRight");
        CornerLeftTilePrefab = Resources.Load<GameObject>(tilesPath + "TileCornerLeft");
        SplitSidesTilePrefab = Resources.Load<GameObject>(tilesPath + "TileSplitSides");
        SplitRightTilePrefab = Resources.Load<GameObject>(tilesPath + "TileSplitRight");
        SplitLeftTilePrefab = Resources.Load<GameObject>(tilesPath + "TileSplitLeft");
        SplitTriTilePrefab = Resources.Load<GameObject>(tilesPath + "TileSplitTri");
        // load obstacle prefabs
        string obstaclesPath = prefabsPath + "Obstacles/";
        for (int i = 0; i < numObstaclesJumpOverTypes; ++i)
        {
            ObstaclesJumpOverPrefabs.Add(Resources.Load<GameObject>(obstaclesPath + "JumpOver" + (i + 1)));
        }
        for (int i = 0; i < numObstaclesFallIntoTypes; ++i)
        {
            ObstaclesFallIntoPrefabs.Add(Resources.Load<GameObject>(obstaclesPath + "FallInto" + (i + 1)));
        }
        for (int i = 0; i < numObstaclesRollUnderTypes; ++i)
        {
            ObstaclesRollUnderPrefabs.Add(Resources.Load<GameObject>(obstaclesPath + "RollUnder" + (i + 1)));
        }
        // load gem prefab
        string collectiblesPath = prefabsPath + "Collectibles/";
        gemPrefab = Resources.Load<GameObject>(collectiblesPath + "Gem");
        // load decoration prefabs
        string environmentPath = prefabsPath + "Environment/";
        for (int i = 0; i < numDecorationTypes; ++i)
        {
            DecorationPrefabs.Add(Resources.Load<GameObject>(environmentPath + "Decoration" + (i + 1)));
        }

        // instantiate tiles
        for (int i = 0; i < tileStorageMaxCount; ++i)
        {
            // regular tile
            GameObject newTile = Instantiate(RegularTilePrefab, Vector3.zero, Quaternion.identity);
            newTile.SetActive(false);
            regularTilesStorage.Add(newTile);
            // corner right tile
            newTile = Instantiate(CornerRightTilePrefab, Vector3.zero, Quaternion.identity);
            newTile.SetActive(false);
            cornerRightTilesStorage.Add(newTile);
            // corner left tile
            newTile = Instantiate(CornerLeftTilePrefab, Vector3.zero, Quaternion.identity);
            newTile.SetActive(false);
            cornerLeftTilesStorage.Add(newTile);
            // split sides tile
            newTile = Instantiate(SplitSidesTilePrefab, Vector3.zero, Quaternion.identity);
            newTile.SetActive(false);
            splitSidesTilesStorage.Add(newTile);
            // split right tile
            newTile = Instantiate(SplitRightTilePrefab, Vector3.zero, Quaternion.identity);
            newTile.SetActive(false);
            splitRightTilesStorage.Add(newTile);
            // split left tile
            newTile = Instantiate(SplitLeftTilePrefab, Vector3.zero, Quaternion.identity);
            newTile.SetActive(false);
            splitLeftTilesStorage.Add(newTile);
            // split tri tile
            newTile = Instantiate(SplitTriTilePrefab, Vector3.zero, Quaternion.identity);
            newTile.SetActive(false);
            splitTriTilesStorage.Add(newTile);
        }
        // instantiate obstacles
        for (int i = 0; i < numObstaclesJumpOverTypes; ++i)
        {
            for (int j = 0; j < obstacleStorageCountPerType; ++j)
            {
                GameObject obstacle = Instantiate(ObstaclesJumpOverPrefabs[i], ObstaclesJumpOverPrefabs[i].transform.position, Quaternion.identity);
                obstacle.SetActive(false);
                obstaclesJumpOverStorage.Add(obstacle);
            }
        }
        for (int i = 0; i < numObstaclesFallIntoTypes; ++i)
        {
            for (int j = 0; j < obstacleStorageCountPerType; ++j)
            {
                GameObject obstacle = Instantiate(ObstaclesFallIntoPrefabs[i], ObstaclesFallIntoPrefabs[i].transform.position, Quaternion.identity);
                obstacle.SetActive(false);
                obstaclesFallIntoStorage.Add(obstacle);
            }
        }
        for (int i = 0; i < numObstaclesRollUnderTypes; ++i)
        {
            for (int j = 0; j < obstacleStorageCountPerType; ++j)
            {
                GameObject obstacle = Instantiate(ObstaclesRollUnderPrefabs[i], ObstaclesRollUnderPrefabs[i].transform.position, Quaternion.identity);
                obstacle.SetActive(false);
                obstaclesRollUnderStorage.Add(obstacle);
            }
        }
        // instantiate gems
        for (int i = 0; i < gemStorageMaxCount; ++i)
        {
            GameObject gem = Instantiate(gemPrefab, Vector3.zero, Quaternion.identity);
            gem.SetActive(false);
            gemStorage.Add(gem);
        }
        // instantiate decorations
        for (int i = 0; i < numDecorationTypes; ++i)
        {
            for (int j = 0; j < decorationStorageCountPerType; ++j)
            {
                GameObject decoration = Instantiate(DecorationPrefabs[i], Vector3.zero, Quaternion.identity);
                decoration.SetActive(false);
                decorationStorage.Add(decoration);
            }
        }

        // initialize road
        road.Add(GameObject.Find("TileStart"));
        for (int i = 0; i < maxTilesAhead; ++i)
        {
            SpawnTile();
        }
        // TODO - fog

    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}