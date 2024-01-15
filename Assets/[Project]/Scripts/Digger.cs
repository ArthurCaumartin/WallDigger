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

    private void OnDig()
    {
        if(_hit)
        {
            _hit.collider.GetComponent<TileBehavior>().Dig();
        }
    }
    
    void FixedUpdate()
    {
        Vector2 mouseDirection = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
        _hit = Physics2D.Raycast(transform.position
                                , mouseDirection
                                , _grabDistance
                                , _layerMask);
        
        Debug.DrawRay(transform.position, mouseDirection * _grabDistance, Color.red);

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
