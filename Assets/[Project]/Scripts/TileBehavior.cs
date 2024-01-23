using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TileBehavior : MonoBehaviour
{
    [SerializeField] private int _maxDurability;
    [SerializeField] private Vector2Int _terrainPosition;
    public Vector2Int TerrainPosition { get { return _terrainPosition; } set { _terrainPosition = value; } }
    [SerializeField] private TileType _type;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteRenderer _durabilitySpriteRenderer;
    public TileType TileType { get { return _type; } set { _type = value; } }
    private int _currentDurability;

    private bool _killPlayerOnCollision = false;

    void Start()
    {
        _currentDurability = _maxDurability;
    }

    public void BoulderFall(TerrainManager terrainManager)
    {
        // print("Boulder will fall");
        _killPlayerOnCollision = true;

        _spriteRenderer.transform.DOShakePosition(1f, .3f, 8);
        _spriteRenderer.transform.DOShakeRotation(1f, .3f, 8)
        .OnComplete(() =>
        {
            terrainManager.BoulderCheck(_terrainPosition);
            terrainManager.SetTileInArray(new Vector2Int((int)transform.position.x, (int)transform.position.y));
            Vector3 nextPosition = terrainManager.GetDepthTilePose(transform);
            print("N : " + nextPosition + " Actu : " + transform.position);
            transform.DOMove(nextPosition, .3f)
            .OnComplete(() =>
            {
                _terrainPosition.y = (int)transform.position.y;
                terrainManager.SetTileInArray(_terrainPosition, this);
                _killPlayerOnCollision = false;
            });
        });
    }


    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.tag == "Player" && _killPlayerOnCollision)
        {
            GameManager.instance.KillPlayer();
        }
    }

    public bool Dig()
    {
        _currentDurability--;
        _durabilitySpriteRenderer.sprite = SpriteBank.instance.TileDurability[_currentDurability % SpriteBank.instance.TileDurability.Count];
        // (int)Mathf.InverseLerp(SpriteBank.instance.TileDurability.Count, 0, _currentDurability)];
        
        if(_currentDurability <= 0)
        {
            Destroy(gameObject);
            return true;
        }
        return false;
    }

    public TileType GetTileType()
    {
        return _type;
    }

    void OnMouseDown()
    {
        print("Terrain Pos : " + _terrainPosition);
        print("Tile name :" + TerrainManager.instance._tileDictionary[_terrainPosition].gameObject.name);
        print("Tile position :" + _terrainPosition);
    }
}
