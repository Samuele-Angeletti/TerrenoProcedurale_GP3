using System.Linq;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [SerializeField] Transform groundCheckPivot;
    [SerializeField] Vector3 groundCheckDirection = Vector3.down;
    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundCheckDistance = 1.0f;

    public bool GroundHit(out Vector3 hitPosition, out Vector3 normal)
    {
        RaycastHit[] hits = new RaycastHit[10];
        if (Physics.RaycastNonAlloc(
            groundCheckPivot.position,
            groundCheckDirection,
            hits,
            groundCheckDistance,
            groundMask) > 0)
        {
            var terrain = hits.Where(x => x.collider != null).FirstOrDefault();

            hitPosition = terrain.point;
            normal = terrain.normal;
            return true;
        }

        hitPosition = Vector3.zero;
        normal = Vector3.zero;

        return false;
    }
}
