using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using Unity.VisualScripting;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Digger _digger;
    [SerializeField] private Transform _direction;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private Vector3 _moveAxis;

    private Rigidbody2D _rigidbody;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        _rigidbody.velocity = new Vector2(_moveAxis.x * _moveSpeed, _rigidbody.velocity.y);
    }

    public void OnMove(InputValue value)
    {
        Vector2 valueVector = value.Get<Vector2>();
        _moveAxis = valueVector;

        if(valueVector.x != 0)
            _direction.right = new Vector2(valueVector.x, 0);
    }
}
