using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class Digger : MonoBehaviour
{
    [SerializeField] private Transform _direction;
    [SerializeField] private float _grabDistance;
    [SerializeField] private LayerMask _layerMask;
    private RaycastHit2D _hit;
    
    void FixedUpdate()
    {
        _hit = Physics2D.BoxCast(transform.position
                                , Vector2.one * .5f
                                , 0f
                                , _direction.right
                                , _grabDistance
                                ,_layerMask);

        if(_hit.collider)
        {
            // print("hit " + _hit.collider.name);
        }
        else
        {
            // print("not hit");
        }
    }

    void OnDrawGizmos()
    {
        if(_hit.collider)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(_hit.point, Vector2.one * 0.2f);
        }
    }
}
