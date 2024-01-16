//This method was derived from the video by t3ssel8r found here: https://www.youtube.com/watch?v=KPoeNZZ6H4s

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondOrderMotion : MonoBehaviour
{
    [Range(0.001f, 20)]
    public float F = 2;
    [Range(0, 1)]
    public float Z = 1;
    [Range(-1, 1)]
    public float R = 1;
    public Transform target;

    private float _f, _z, _r = 0;
    private Transform _transform;
    private SecondOrderDynamics[] _dymamics = new SecondOrderDynamics[3];

    [SerializeField] private Vector3 _offSet;

    // Start is called before the first frame update
    void Start()
    {
        _f = F;
        _z = Z;
        _r = R;
        _transform = GetComponent<Transform>();
        _dymamics[0] = new SecondOrderDynamics(_f, _z, _r, target.position.x);
        _dymamics[1] = new SecondOrderDynamics(_f, _z, _r, target.position.y);
        _dymamics[2] = new SecondOrderDynamics(_f, _z, _r, target.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        if (_f != F || _z != Z || _r != R)
        {
            _f = F;
            _z = Z;
            _r = R;
            _dymamics[0] = new SecondOrderDynamics(_f, _z, _r, target.position.x);
            _dymamics[1] = new SecondOrderDynamics(_f, _z, _r, target.position.y);
            _dymamics[2] = new SecondOrderDynamics(_f, _z, _r, target.position.z);
        }
        float x = _dymamics[0].Update(Time.deltaTime, target.position.x);
        float y = _dymamics[1].Update(Time.deltaTime, target.position.y);
        float z = _dymamics[2].Update(Time.deltaTime, target.position.z);

        _transform.position = new Vector3(x, y, z) + _offSet;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(target.position, 0.5f);
    }


}

public class SecondOrderDynamics
{
    private float xp; // previous input
    private float y, yd; // state variables
    float _w, _z, _d, k1, k2, k3; // dynamic constraints

    public SecondOrderDynamics(float f, float z, float r, float x0)
    {
        //compute constants
        _w = 2 * Mathf.PI * f;
        _z = z;
        _d = _w * Mathf.Sqrt(Mathf.Abs(z * z - 1));
        k1 = z / (Mathf.PI * f);
        k2 = 1 / ((2*Mathf.PI * f) * (2 * Mathf.PI * f));
        k3 = r * z / (2 * Mathf.PI * f);

        //Initialize Variables
        xp = x0;
        y = x0;
        yd = 0;
    }

    public float Update(float T, float x)
    {
        //Estimate velocity
        float xd = (x - xp) / T;
        xp = x;

        float k1_stable, k2_stable;
        if (_w * T < _z) // clamp k2 to guarantee stability without jitter
        {
            k1_stable = k1;
            k2_stable = Mathf.Max(k2, T*T/2f + T*k1/2f, T*k1);
        }
        else // Use pole matching when the system is very fast
        {
            float t1 = Mathf.Exp(-_z * _w * T);
            float alpha = 2 * t1 * (_z <= 1 ? Mathf.Cos(T * _d) : System.MathF.Cosh(T * _d));
            float beta = t1 * t1;
            float t2 = T / (1 + beta - alpha);
            k1_stable = (1 - beta) * t2;
            k2_stable = T * t2;
        }

        y = y + T * yd; // integrate position by velocity
        yd = yd + T * (x + k3*xd - y - k1*yd)/k2_stable; // integrate velocity by acceleration
        return y;
    }
}