using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using Unity.VisualScripting;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private TerrainManager _terrainManager;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private Vector2 _moveAxis;
    [SerializeField] private LayerMask _layerMask;

    private Rigidbody2D _rigidbody;
    private RaycastHit2D _hit;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    void OnMoveInput()
    {
        _hit = Physics2D.Raycast(transform.position, _moveAxis, 1f, _layerMask);
        if(_hit.collider)
        {
            Dig();
        }
        else
        {
            Move();
        }
    }

    void Dig()
    {
        TileBehavior tileHit = _hit.collider.GetComponent<TileBehavior>();
        _terrainManager.DigTile(tileHit.TerrainPosition);
    }

    void Move()
    {
        _rigidbody.MovePosition(transform.position + new Vector3(_moveAxis.x, _moveAxis.y, 0));
    }

    public void OnMove(InputValue value)
    {
        Vector2 valueVector = value.Get<Vector2>();

        if(valueVector == Vector2.down | valueVector == Vector2.left | 
            valueVector == Vector2.right | valueVector == Vector2.up)
            _moveAxis = valueVector;

        OnMoveInput();
        _moveAxis = Vector2.zero;
    }
}
