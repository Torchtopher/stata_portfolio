using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FollowCameraNoClipRobust : MonoBehaviour
{
    public Transform cameraTransform;
    public Vector3 offset = new Vector3(0, 2, -5);
    public float followSpeed = 5f;        // meters per second
    public float rotationSpeed = 10f;
    public float lockedX = 90f;           // world-space X locked
    public float skinWidth = 0.05f;       // margin to keep off walls
    public LayerMask obstacleLayers = ~0; // which layers count as obstacles

    private Rigidbody rb;
    private Collider col;
    private CapsuleCollider capsuleCol;
    private float radius = 0.5f;
    private float halfHeight = 0.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        capsuleCol = GetComponent<CapsuleCollider>();

        // physics settings
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // set initial fixed orientation (world X)
        Vector3 e = transform.eulerAngles;
        transform.eulerAngles = new Vector3(lockedX, e.y, 0f);

        // derive cast shape from collider when possible
        if (capsuleCol != null)
        {
            radius = capsuleCol.radius * Mathf.Max(transform.localScale.x, transform.localScale.z);
            halfHeight = Mathf.Max(0.01f, (capsuleCol.height * 0.5f) * transform.localScale.y - radius);
        }
        else if (col != null)
        {
            // approximate radius from bounds (horizontal extent)
            radius = Mathf.Max(col.bounds.extents.x, col.bounds.extents.z);
            halfHeight = Mathf.Max(0.01f, col.bounds.extents.y - 0.01f);
        }
    }

    void FixedUpdate()
    {
        if (cameraTransform == null) return;

        // desired world position
        Vector3 desiredPos = cameraTransform.position + offset;

        // compute movement and clamp to max step (prevents teleporting)
        Vector3 moveVec = desiredPos - rb.position;
        float dist = moveVec.magnitude;
        float maxStep = followSpeed * Time.fixedDeltaTime;
        Vector3 moveDir = dist > 0f ? (moveVec / dist) : Vector3.zero;
        float stepDist = Mathf.Min(dist, maxStep);

        // prepare obstacle mask (exclude self layer)
        int selfLayerMask = 1 << gameObject.layer;
        LayerMask mask = obstacleLayers & ~(selfLayerMask);

        Vector3 newPos = rb.position;

        if (stepDist > 0.0001f)
        {
            // If capsule collider available, use CapsuleCast; otherwise SphereCast.
            RaycastHit hit;
            bool blocked = false;

            if (capsuleCol != null)
            {
                // compute capsule endpoints in world space (along Y)
                Vector3 localUp = Vector3.up;
                Vector3 worldCenter = rb.position + capsuleCol.center;
                Vector3 point0 = worldCenter + localUp * halfHeight;
                Vector3 point1 = worldCenter - localUp * halfHeight;

                // do capsule cast
                blocked = Physics.CapsuleCast(point0, point1, radius - 0.001f, moveDir, out hit, stepDist + skinWidth, mask, QueryTriggerInteraction.Ignore);
            }
            else
            {
                // spherecast from current position
                blocked = Physics.SphereCast(rb.position, radius - 0.001f, moveDir, out hit, stepDist + skinWidth, mask, QueryTriggerInteraction.Ignore);
            }

            if (blocked)
            {
                // stop before the obstacle (leave skinWidth + radius)
                float allowed = Mathf.Max(0f, hit.distance - (skinWidth + 0.01f));
                newPos = rb.position + moveDir * allowed;
            }
            else
            {
                // path clear for this step
                newPos = rb.position + moveDir * stepDist;
            }

            // additional safety: if desiredPos itself is inside geometry, don't teleport there
            // check overlap at newPos
            if (IsOverlappingAtPosition(newPos, mask))
            {
                // if overlapping, keep current position (or slight retreat)
                newPos = rb.position;
            }

            // finally move
            rb.MovePosition(newPos);
        }

        // rotation: only Y (world), keep X/Z fixed
        Vector3 dirToCamera = cameraTransform.position - rb.position;
        dirToCamera.y = 0f;
        if (dirToCamera.sqrMagnitude > 0.001f)
        {
            float targetY = Mathf.Atan2(dirToCamera.x, dirToCamera.z) * Mathf.Rad2Deg;
            float currentY = rb.rotation.eulerAngles.y;
            float y = Mathf.LerpAngle(currentY, targetY, Mathf.Clamp01(rotationSpeed * Time.fixedDeltaTime));
            rb.MoveRotation(Quaternion.Euler(lockedX, y, 0f));
        }
    }

    // check if our collider would overlap obstacles at a candidate world position
    private bool IsOverlappingAtPosition(Vector3 worldPos, LayerMask mask)
    {
        if (capsuleCol != null)
        {
            Vector3 worldCenter = worldPos + capsuleCol.center;
            Vector3 p0 = worldCenter + Vector3.up * halfHeight;
            Vector3 p1 = worldCenter - Vector3.up * halfHeight;
            Collider[] hits = Physics.OverlapCapsule(p0, p1, radius - 0.001f, mask, QueryTriggerInteraction.Ignore);
            return hits.Length > 0;
        }
        else
        {
            Collider[] hits = Physics.OverlapSphere(worldPos, radius - 0.001f, mask, QueryTriggerInteraction.Ignore);
            return hits.Length > 0;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(rb.position, radius);
    }
}
