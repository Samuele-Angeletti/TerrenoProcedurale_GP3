using System;
using UnityEngine;

/// <summary>
/// Responsabilità: muovere il gameobject da transform.position a destination
/// Usa una velocità basata sul Rigidbody con accelerazione e freno
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class MoveTowards : MonoBehaviour
{
    private Vector3 _destination;
    
    [SerializeField] float maxSpeed;
    [SerializeField] float accelerationForce;

    private Rigidbody rb;
    private bool _reachedPosition = false;
    public event Action OnDestinationReached;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxLinearVelocity = maxSpeed;
    }

    public void SetDestination(Vector3 newDestination)
    {
        _destination = newDestination;
        _reachedPosition = false;
    }

    public void Move()
    {
        rb.AddForce((_destination - transform.position).normalized * accelerationForce, ForceMode.Force);
    }

    private void FixedUpdate()
    {
        if (_reachedPosition) return;

        Move();
    }

    private void Update()
    {
        if (_reachedPosition) return;

        if (Vector3.Distance(transform.position, _destination) < 0.01f)
        { 
            _reachedPosition = true;
            OnDestinationReached?.Invoke();
        }
    }
}
