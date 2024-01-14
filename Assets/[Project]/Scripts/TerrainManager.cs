using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//! torp bien le tutossssssssssssssssssssssssssss
//! https://www.youtube.com/watch?v=_XtOOhxRsWY&list=PLn1X2QyVjFVDE9syarF1HoUFwB_3K7z2y&ab_channel=ErenCode

enum TileType
{
    Empty,
    Dirt,
    Grass,
    Stone,
    Mineral,
}

public class TerrainManager : MonoBehaviour
{
    [Header("Tile Prefabs :")]
    [SerializeField] private GameObject _grassTilePrefab;
    [SerializeField] private GameObject _dirtTilePrefab;
    [SerializeField] private GameObject _stoneTilePrefab;
    [SerializeField] private GameObject _mineralTilePrefab;

    [Header("Terrain Parameter :")]
    [SerializeField] private int _worldSizeX = 50;
    [SerializeField] private int _worldSizeY = 50;
    [SerializeField] private int _surfaceDepth = 5;
    [SerializeField] private float _mineralSpawnChance = 0.05f;

    [Header("Noise Parameter :")]
    [SerializeField, Range(0, 1)] private float _stoneSpawnValue = 0.5f;
    [SerializeField] private AnimationCurve _mineralSpawnDepthScale;
    [SerializeField, Range(0.01f, 3.5f)] private float _noiseFrequency = 0.05f;
    [SerializeField] private float _seed;
    [SerializeField] private Texture2D _noiseTexture;
    
    private TileType[,] _terrainState;


    void OnValidate()
    {
        GenerateNoiseTexture();
    }

    void Start()
    {
        for (int i = 0; i < _worldSizeY; i++)
        {
            print("Depth : " + i + " / " + _mineralSpawnDepthScale.Evaluate(Mathf.InverseLerp(0, _worldSizeY, i)));
        }

        _terrainState = new TileType[_worldSizeX, _worldSizeY];

        transform.position = new Vector3Int(-_worldSizeX / 2, -_worldSizeY, 0);
        _seed = Random.Range(-10000, 10000);

        GenerateNoiseTexture();
        ComputeTerrainArray();
        GenerateTerrainTiles();
    }

    private void ComputeTerrainArray()
    {
        print("Compute Array");
        for (int x = 0; x < _worldSizeX; x++)
        {
            for (int y = 0; y < _worldSizeY; y++)
            {

                //! Stone
                if(_noiseTexture.GetPixel(x, y).r > _stoneSpawnValue)
                {
                    _terrainState[x, y] = TileType.Stone;

                    //! Minerals
                    float depthMultiplier = _mineralSpawnDepthScale.Evaluate(Mathf.InverseLerp(_worldSizeY, 0, y));
                    // print(depthMultiplier);
                    if(Random.value < _mineralSpawnChance * depthMultiplier)
                    {
                        _terrainState[x, y] = TileType.Mineral;
                    }
                }
                else
                {
                    _terrainState[x, y] = TileType.Dirt;
                }

                //! Dirt Surface
                if(y > _worldSizeY - _surfaceDepth)
                {
                    _terrainState[x, y] = TileType.Dirt;
                }

                //! Grass
                if(y == _worldSizeY - 1)
                {
                    _terrainState[x, y] = TileType.Grass;
                }
            }
        }
    }

    public void GenerateTerrainTiles()
    {
        print("Generate Tiles");
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
            case TileType.Stone :
                newTile = Instantiate(_stoneTilePrefab);
            break;

            case TileType.Dirt :
                newTile = Instantiate(_dirtTilePrefab);
            break;

            case TileType.Grass :
                newTile = Instantiate(_grassTilePrefab);
            break;

            case TileType.Mineral :
                newTile = Instantiate(_mineralTilePrefab);
            break;
        }

        if(!newTile)
            return;
        
        newTile.transform.parent = transform;
        newTile.transform.localPosition = new Vector2(x, y);
    }

    private void GenerateNoiseTexture()
    {
        print("Compute Noise Texture");
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
}
