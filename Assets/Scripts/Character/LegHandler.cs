using UnityEngine;

[RequireComponent(typeof(GroundCheck))]
public class LegHandler : MonoBehaviour
{
    [SerializeField] Transform targetFoot;
    private GroundCheck groundCheck;

    private void Awake()
    {
        groundCheck = GetComponent<GroundCheck>();
    }

    private void Update()
    {
        SetFootToGround();
    }

    private void SetFootToGround()
    {
        if (groundCheck.GroundHit(out var position, out var normal))
        {
            targetFoot.SetPositionAndRotation(position, Quaternion.Euler(normal));
        }
    }
}
