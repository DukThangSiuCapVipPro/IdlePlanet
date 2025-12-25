using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [Header("Transform")]
    public float ROTATE_SPEED = 1;
    public float ROTATE_SMOOTH = 5f;

    [Header("Creature Spawns")]
    [Tooltip("Radius of the circle where creatures will be placed")]
    public float spawnRadius = 5f;
    [Tooltip("Number of spawn points around the planet")]
    public int spawnPointCount = 100;
    [Tooltip("Minimum distance between spawn points to prevent overlap")]
    public float minDistanceBetweenSpawns = 0.5f;
    [Tooltip("How much spawn points can deviate inward from the boundary (0 = exactly on boundary)")]
    [Range(0f, 1f)]
    public float boundaryOffset = 0.1f;
    [Tooltip("Parent transform that holds all spawn points")]
    public Transform spawnPointsContainer;

    private Quaternion targetRotation;

    #region Unity Methods
    void Start()
    {
        targetRotation = transform.rotation;
    }

    void Update()
    {
        targetRotation *= Quaternion.Euler(0, 0, ROTATE_SPEED * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, ROTATE_SMOOTH * Time.deltaTime);
    }
    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Draw spawn radius circle
        Gizmos.color = Color.cyan;
        DrawCircle(transform.position, spawnRadius, 64);

        // Draw filled semi-transparent circle
        UnityEditor.Handles.color = new Color(0, 1, 1, 0.1f);
        UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.forward, spawnRadius);

        // Draw label
        UnityEditor.Handles.Label(
            transform.position + Vector3.right * spawnRadius,
            $"Spawn Radius: {spawnRadius}",
            new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = Color.cyan },
                fontSize = 12,
                fontStyle = FontStyle.Bold
            }
        );
    }

    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(Mathf.Cos(0) * radius, Mathf.Sin(0) * radius, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
#endif
}
