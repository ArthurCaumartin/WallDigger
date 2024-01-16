using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

//! torp bien le tutossssssssssssssssssssssssssss pour poser les bases
//! https://www.youtube.com/watch?v=_XtOOhxRsWY&list=PLn1X2QyVjFVDE9syarF1HoUFwB_3K7z2y&ab_channel=ErenCode

[System.Serializable]
public enum TileType
{
    Empty,
    Dirt,
    Grass,
    Stone,
    Mineral,
    Boulder,
    Ladder,
}

public class TerrainManager : MonoBehaviour
{
    public static TerrainManager instance;

    [Header("Tile Prefabs :")]
    [SerializeField] private GameObject _grassTilePrefab;
    [SerializeField] private GameObject _dirtTilePrefab;
    [SerializeField] private GameObject _stoneTilePrefab;
    [SerializeField] private GameObject _mineralTilePrefab;
    [SerializeField] private GameObject _boulderTilePrefab;

    [Header("Terrain Parameter :")]
    [SerializeField] private int _worldSizeX = 50;
    [SerializeField] private int _worldSizeY = 50;
    [SerializeField] private int _surfaceDepth = 5;
    [SerializeField] private float _mineralSpawnChance = 0.05f;
    [SerializeField] private float _boulderSpawnChance = 0.05f;

    [Header("Noise Parameter :")]
    [SerializeField, Range(0, 1)] private float _stoneSpawnValue = 0.5f;
    [SerializeField] private AnimationCurve _mineralSpawnDepthScale;
    [SerializeField, Range(0.01f, 3.5f)] private float _noiseFrequency = 0.05f;
    [SerializeField] private float _seed;
    [SerializeField] private Texture2D _noiseTexture;

    private TileType[,] _terrainState; //! dois etre localiser au fonction qui en on besoin
    private TileBehavior[,] _tileArray; //! a remplacer par le dictionaire
    [SerializeField] private Dictionary<Vector2Int, TileBehavior> _tileDictionary;

    void Awake()
    {
        instance = this;
    }

    void OnValidate()
    {
        GenerateNoiseTexture();
    }

    void Start()
    {
        _terrainState = new TileType[_worldSizeX, _worldSizeY];
        _tileArray = new TileBehavior[_worldSizeX, _worldSizeY];
        _tileDictionary = new Dictionary<Vector2Int, TileBehavior>();

        transform.position = new Vector3Int(-_worldSizeX / 2, -_worldSizeY, 0);
        CreateNewTerrain();
    }

    public void CreateNewTerrain()
    {
        _seed = Random.Range(-10000, 10000);

        ResetArray();
        GenerateNoiseTexture();
        ComputeTerrainArray();
        GenerateTerrainTiles();
    }

    private void ResetArray()
    {
        for (int x = 0; x < _worldSizeX; x++)
        {
            for (int y = 0; y < _worldSizeY; y++)
            {
                _terrainState[x, y] = TileType.Empty;
                if (_tileArray[x, y])
                    Destroy(_tileArray[x, y].gameObject);
            }
        }
    }

    private void GenerateNoiseTexture()
    {
        // print("Compute Noise Texture");
        _noiseTexture = new Texture2D(_worldSizeX, _worldSizeY);

        for (int x = 0; x < _noiseTexture.width; x++)
        {
            for (int y = 0; y < _noiseTexture.height; y++)
            {
                float noisePixel = Mathf.PerlinNoise((x + _seed) * _noiseFrequency, (y + _seed) * _noiseFrequency);
                _noiseTexture.SetPixel(x, y, new Color(noisePixel, noisePixel, noisePixel));
            }
        }

        //! Transfere de donné entre CPU / GPU ?
        _noiseTexture.Apply();
    }

    private void ComputeTerrainArray()
    {
        // print("Compute Array");
        for (int x = 0; x < _worldSizeX; x++)
        {
            for (int y = 0; y < _worldSizeY; y++)
            {

                //! Stone
                if (_noiseTexture.GetPixel(x, y).r > _stoneSpawnValue)
                {
                    _terrainState[x, y] = TileType.Stone;

                    if (Random.value < _boulderSpawnChance)
                    {
                        _terrainState[x, y] = TileType.Boulder;
                    }

                    //! Minerals
                    float depthMultiplier = _mineralSpawnDepthScale.Evaluate(Mathf.InverseLerp(_worldSizeY, 0, y));
                    if (Random.value < _mineralSpawnChance * depthMultiplier)
                    {
                        _terrainState[x, y] = TileType.Mineral;
                    }
                }
                else
                {
                    _terrainState[x, y] = TileType.Dirt;
                }

                //! Dirt Surface
                if (y > _worldSizeY - _surfaceDepth)
                {
                    _terrainState[x, y] = TileType.Dirt;
                }

                //! Grass
                if (y == _worldSizeY - 1)
                {
                    _terrainState[x, y] = TileType.Grass;
                }
            }
        }
    }

    private void GenerateTerrainTiles()
    {
        // print("Generate Tiles");
        for (int x = 0; x < _worldSizeX; x++)
        {
            for (int y = 0; y < _worldSizeY; y++)
            {
                CreatTile(x, y, _terrainState[x, y]);
            }
        }
    }

    private void CreatTile(int x, int y, TileType type)
    {
        GameObject newTile = null;
        switch (type)
        {
            case TileType.Stone:
                newTile = Instantiate(_stoneTilePrefab);
                newTile.GetComponent<TileBehavior>().TileType = TileType.Stone;
                break;

            case TileType.Dirt:
                newTile = Instantiate(_dirtTilePrefab);
                newTile.GetComponent<TileBehavior>().TileType = TileType.Dirt;
                break;

            case TileType.Grass:
                newTile = Instantiate(_grassTilePrefab);
                newTile.GetComponent<TileBehavior>().TileType = TileType.Grass;
                break;

            case TileType.Mineral:
                newTile = Instantiate(_mineralTilePrefab);
                newTile.GetComponent<TileBehavior>().TileType = TileType.Mineral;
                break;

            case TileType.Boulder:
                newTile = Instantiate(_boulderTilePrefab);
                newTile.GetComponent<TileBehavior>().TileType = TileType.Boulder;
                break;
        }

        if (!newTile)
            return;

        newTile.transform.parent = transform;
        newTile.transform.localPosition = new Vector2(x, y);

        TileBehavior newTileBehavior = newTile.GetComponent<TileBehavior>();
        _tileArray[x, y] = newTileBehavior;
        newTileBehavior.TerrainPosition = new Vector2Int(x, y);

        Vector2Int worldPos = new Vector2Int((int)newTile.transform.position.x, (int)newTile.transform.position.y);
        // _tileDictionary.Add(worldPos, newTileBehavior);
    }

    public void SetTileInArray(Vector2Int position, TileBehavior tileToSet)
    {
        _tileArray[position.x, position.y] = tileToSet;
    }

    public void DigTile(Vector2Int tileToDigPosition)
    {
        _tileArray[tileToDigPosition.x, tileToDigPosition.y].Dig();
        BoulderCheck(tileToDigPosition);
    }

    private void BoulderCheck(Vector2Int digTilePosition)
    {
        if (digTilePosition.y + 1 < _worldSizeY)
        {
            TileBehavior tileAbove = _tileArray[digTilePosition.x, digTilePosition.y + 1];

            if (tileAbove)
                print(tileAbove.name);
            else
                print("Nothhing above");


            if (tileAbove.GetTileType() == TileType.Boulder)
            {
                tileAbove.BoulderFall(this);
            }
        }
    }
}
