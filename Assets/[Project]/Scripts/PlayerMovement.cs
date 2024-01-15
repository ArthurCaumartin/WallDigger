using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using Unity.VisualScripting;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private TerrainManager _terrainManager;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private Vector3 _moveAxis;
    [SerializeField] private LayerMask _layerMask;

    private Rigidbody2D _rigidbody;
    private RaycastHit2D _hit;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // print(_moveAxis);
        if(_moveAxis == Vector3.zero)
            return;

        _hit = Physics2D.Raycast(transform.position, _moveAxis, 1f, _layerMask);

        if(_hit.collider)
        {
            TileBehavior tileHit = _hit.collider.GetComponent<TileBehavior>();
            _terrainManager.DigTile(tileHit.TerrainPosition);
        }
        else
        {
            _rigidbody.MovePosition(transform.position + _moveAxis);
        }
        _moveAxis = Vector3.zero;
    }


    public void OnMove(InputValue value)
    {
        Vector2 valueVector = value.Get<Vector2>();

        if(_moveAxis == Vector3.zero)
        {
            // print(valueVector);
            if(valueVector == Vector2.down | valueVector == Vector2.left | valueVector == Vector2.right | valueVector == Vector2.up)
                _moveAxis = valueVector;
        }
    }
}
