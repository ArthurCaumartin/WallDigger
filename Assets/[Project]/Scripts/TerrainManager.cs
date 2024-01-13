using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//! torp bien le tutossssssssssssssssssssssssssss
//! https://www.youtube.com/watch?v=_XtOOhxRsWY&list=PLn1X2QyVjFVDE9syarF1HoUFwB_3K7z2y&ab_channel=ErenCode

enum TileType
{
    Ground,
    Void,
    Mineral
}

public class TerrainManager : MonoBehaviour
{
    [Header("Tile Prefabs :")]
    [SerializeField] private GameObject _mineralTilePrefab;
    [SerializeField] private GameObject _groundTilePrefab;
    [Space]
    [SerializeField] private int worldSizeX, worldSizeY;
    [SerializeField] private float _spawnValue;
    [SerializeField, Range(0.01f, 5f)] private float _noiseFrequency = 0.05f;
    [SerializeField] private float _seed;
    [SerializeField] private Texture2D _noiseTexture;


    void Start()
    {
        transform.position = new Vector3Int(-worldSizeX / 2, -worldSizeY, 0);
        _seed = Random.Range(-10000, 10000);

        GenerateNoiseTexture();
        GenerateTerrainArray();
    }

    private void GenerateTerrainArray()
    {
        for (int x = 0; x < worldSizeX; x++)
        {
            for (int y = 0; y < worldSizeY; y++)
            {
                if(_noiseTexture.GetPixel(x, y).r > _spawnValue)
                {
                    GameObject newtile = Instantiate(_groundTilePrefab);
                    newtile.transform.parent = transform;
                    newtile.transform.localPosition = new Vector2(x, y);
                }
            }
        }
    }

    private void CreatTile(int x, int y, TileType type)
    {

    }

    private void GenerateNoiseTexture()
    {
        _noiseTexture = new Texture2D(worldSizeX, worldSizeY);

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
