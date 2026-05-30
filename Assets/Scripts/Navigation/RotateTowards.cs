using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RotateTowards : MonoBehaviour
{
    [SerializeField] float maxRotationSpeed;
    [SerializeField] float maxDegreeDelta;

    private Rigidbody rb;
    private bool _reachedRotation = false;
    private Vector3 _destination;
    public void SetDestination(Vector3 newDestination)
    {
        _destination = newDestination;
        _reachedRotation = false;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = maxRotationSpeed;
    }

    public void Rotate()
    {
        //rb.MoveRotation
        //    (
        //        Quaternion.LookRotation(
        //            transform.forward, 
        //            transform.up)
        //    );
    }

    private void FixedUpdate()
    {
        if (_reachedRotation) return;

        Rotate();
    }

}
