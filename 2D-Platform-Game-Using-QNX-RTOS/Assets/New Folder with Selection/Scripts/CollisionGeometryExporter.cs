using UnityEngine;

public class CollisionGeometryExporter : MonoBehaviour
{
    [SerializeField] private CompositeCollider2D groundCollider;

    [ContextMenu("Export Ground Collision")]
    public void ExportGroundCollision()
    {
        if (groundCollider == null)
        {
            groundCollider = GetComponent<CompositeCollider2D>();
        }

        if (groundCollider == null)
        {
            Debug.LogError(
                "[CollisionExporter] No CompositeCollider2D found."
            );
            return;
        }

        Debug.Log("========================================");
        Debug.Log("[CollisionExporter] GROUND COLLISION");
        Debug.Log($"Path Count: {groundCollider.pathCount}");
        Debug.Log("========================================");

        for (int pathIndex = 0;
             pathIndex < groundCollider.pathCount;
             pathIndex++)
        {
            int pointCount =
                groundCollider.GetPathPointCount(pathIndex);

            Vector2[] localPoints =
                new Vector2[pointCount];

            groundCollider.GetPath(
                pathIndex,
                localPoints
            );

            Debug.Log(
                $"--- PATH {pathIndex} " +
                $"({pointCount} points) ---"
            );

            for (int i = 0; i < pointCount; i++)
            {
                Vector3 worldPoint =
                    groundCollider.transform.TransformPoint(
                        localPoints[i]
                    );

                Debug.Log(
                    $"P{i}: " +
                    $"X={worldPoint.x:F4}, " +
                    $"Y={worldPoint.y:F4}"
                );
            }
        }

        Debug.Log("========================================");
        Debug.Log("[CollisionExporter] EXPORT COMPLETE");
        Debug.Log("========================================");
    }
}