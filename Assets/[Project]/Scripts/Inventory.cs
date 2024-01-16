using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Inventory : MonoBehaviour
{
    [SerializeField] private TerrainManager _terrainManager;
    Vector2 _useDirection;
    
    void FixedUpdate()
    {
        if(_useDirection == Vector2.zero)
            return;
        
    }

    private void OnPlaceItem(InputValue value)
    {
        _useDirection = value.Get<Vector2>();
    }
}
