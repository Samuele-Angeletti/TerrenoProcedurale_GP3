using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(MoveTowards), typeof(RotateTowards))]
public class MoveAlongPathHandler : MonoBehaviour
{
    [SerializeField] PathfindingGrid grid;
    [SerializeField] Transform startPosition;
    [SerializeField] Transform endPosition;

    private MoveTowards moveTowards;
    private RotateTowards rotateTowards;
    private List<Vector3> path;
    private int _currentIndex = 0;
    private void Awake()
    {
        moveTowards = GetComponent<MoveTowards>();
        rotateTowards = GetComponent<RotateTowards>();

        path = grid.FindPath(startPosition.position, endPosition.position);
        moveTowards.OnDestinationReached += NextDestination;

        SetPositionAndRotation();
    }

    private void NextDestination()
    {
        _currentIndex++;
        SetPositionAndRotation();
    }

    private void SetPositionAndRotation()
    {
        if (_currentIndex == path.Count) return;

        var destination = path[_currentIndex];

        moveTowards.SetDestination(destination);
        rotateTowards.SetDestination(destination);
    }
}
