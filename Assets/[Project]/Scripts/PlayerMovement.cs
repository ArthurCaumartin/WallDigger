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

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    void OnMoveInput()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, _moveAxis, 1f, _layerMask);
        if(hit.collider)
        {
            TileBehavior tile = hit.collider.GetComponent<TileBehavior>();
            Dig(tile);
            return;
        }
        Move(_moveAxis);
    }

    private void Dig(TileBehavior tileToDig)
    {
        _terrainManager.DigTile(tileToDig.TerrainPosition);
        CheckVoidUnderPlayer();
    }

    private void Move(Vector2 moveDirection)
    {
        _rigidbody.MovePosition(transform.position + new Vector3(moveDirection.x, moveDirection.y, 0));
        CheckVoidUnderPlayer();
    }

    private void CheckVoidUnderPlayer()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, _layerMask);
        if(!hit.collider)
            return;
        
        TileBehavior tile = hit.collider.GetComponent<TileBehavior>();
        if(Vector3.Distance(tile.transform.position, transform.position) < 1.5)
            return;

        _rigidbody.MovePosition(tile.transform.position + Vector3.up);
        print(hit.collider.name);
    }


    public void OnMove(InputValue value)
    {
        print("Input !");
        Vector2 valueVector = value.Get<Vector2>();

        if(valueVector == Vector2.down | valueVector == Vector2.left | 
            valueVector == Vector2.right/*  | valueVector == Vector2.up */)
            _moveAxis = valueVector;

        if(_moveAxis == Vector2.zero)
            return;
        OnMoveInput();
        _moveAxis = Vector2.zero;
    }
}
