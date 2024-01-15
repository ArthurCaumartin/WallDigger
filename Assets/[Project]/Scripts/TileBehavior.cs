using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TileBehavior : MonoBehaviour
{
    [SerializeField] private Vector2Int _terrainPosition;
    public Vector2Int TerrainPosition {get{return _terrainPosition;} set{_terrainPosition = value;}}
    [SerializeField] private TileType _type;
    private SpriteRenderer _spriteRenderer;

    private bool _killPlayerOnCollision = false;

    void Start()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
    
    public void BoulderFall(TerrainManager terrainManager)
    {
        _killPlayerOnCollision = true;
        _spriteRenderer.transform.DOShakePosition(1f, .3f, 8);
        _spriteRenderer.transform.DOShakeRotation(1f, .3f, 8)
        .OnComplete(() =>
        {
            transform.DOMove(transform.position + Vector3.down, .3f)
            .OnComplete(() =>
            {
                terrainManager.SetTileInArray(_terrainPosition, this);
                _killPlayerOnCollision = false;
            });
        });
    }


    void OnCollisionEnter2D(Collision2D other)
    {
        if(other.collider.tag == "Player" && _killPlayerOnCollision)
        {
            GameManager.instance.KillPlayer();
        }
    }

    public void Dig()
    {
        Destroy(gameObject);
    }

    public TileType GetTileType()
    {
        return _type;
    }
}
