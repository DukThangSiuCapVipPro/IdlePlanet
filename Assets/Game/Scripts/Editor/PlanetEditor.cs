using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Planet))]
public class PlanetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Planet planet = (Planet)target;

        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            $"Generate {planet.spawnPointCount} spawn points on the planet boundary.\n" +
            $"Radius: {planet.spawnRadius} units\n" +
            $"Min Distance: {planet.minDistanceBetweenSpawns} units\n" +
            $"Boundary Offset: {planet.boundaryOffset:F2} (0 = exactly on edge)",
            MessageType.Info
        );

        GUILayout.Space(5);

        if (GUILayout.Button("Generate Spawn Points", GUILayout.Height(40)))
        {
            GenerateSpawnPoints(planet);
        }

        GUILayout.Space(5);

        if (planet.spawnPointsContainer != null && planet.spawnPointsContainer.childCount > 0)
        {
            if (GUILayout.Button("Clear Spawn Points", GUILayout.Height(30)))
            {
                ClearSpawnPoints(planet);
            }
        }
    }

    private void GenerateSpawnPoints(Planet planet)
    {
        // Validate
        if (planet.spawnPointCount <= 0)
        {
            EditorUtility.DisplayDialog("Error", "Spawn point count must be greater than 0!", "OK");
            return;
        }

        if (planet.spawnRadius <= 0)
        {
            EditorUtility.DisplayDialog("Error", "Spawn radius must be greater than 0!", "OK");
            return;
        }

        // Create or get container
        Transform container = planet.spawnPointsContainer;

        if (container == null)
        {
            GameObject containerObj = new GameObject("SpawnPoints");
            containerObj.transform.SetParent(planet.transform);
            containerObj.transform.localPosition = Vector3.zero;
            containerObj.transform.localRotation = Quaternion.identity;
            containerObj.transform.localScale = Vector3.one;
            container = containerObj.transform;

            planet.spawnPointsContainer = container;
            EditorUtility.SetDirty(planet);
        }
        else
        {
            // Clear existing spawn points
            ClearSpawnPoints(planet);
        }

        // Generate spawn points with progressive fallback (boundary -> inside)
        List<Vector2> spawnPositions = new List<Vector2>();
        int maxAttemptsPerPoint = 30; // Max attempts per point to find valid position
        int generatedCount = 0;

        for (int i = 0; i < planet.spawnPointCount; i++)
        {
            Vector2 validPosition = Vector2.zero;
            bool foundValidPosition = false;

            // Progressive fallback: try boundary first, then move inward
            float radiusStep = 0.1f; // How much to reduce radius each fallback step
            int maxFallbackSteps = 10; // Number of inward steps to try

            for (int fallbackStep = 0; fallbackStep <= maxFallbackSteps && !foundValidPosition; fallbackStep++)
            {
                // Calculate current radius range (starts at boundary, moves inward)
                float currentMaxRadius = planet.spawnRadius * (1f - (fallbackStep * radiusStep));
                float currentMinRadius = Mathf.Max(0.1f, currentMaxRadius * (1f - planet.boundaryOffset));

                // Skip if radius too small
                if (currentMaxRadius <= 0.1f) break;

                // Try to find a valid position at this radius level
                for (int attempt = 0; attempt < maxAttemptsPerPoint; attempt++)
                {
                    // Generate random angle around the circle
                    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

                    // Generate radius at current level
                    float randomRadius = Random.Range(currentMinRadius, currentMaxRadius);

                    // Convert polar to cartesian
                    Vector2 randomPos = new Vector2(
                        Mathf.Cos(angle) * randomRadius,
                        Mathf.Sin(angle) * randomRadius
                    );

                    // Check if position is far enough from all existing positions
                    bool isFarEnough = true;
                    foreach (Vector2 existingPos in spawnPositions)
                    {
                        float distance = Vector2.Distance(randomPos, existingPos);
                        if (distance < planet.minDistanceBetweenSpawns)
                        {
                            isFarEnough = false;
                            break;
                        }
                    }

                    if (isFarEnough)
                    {
                        validPosition = randomPos;
                        foundValidPosition = true;
                        break;
                    }
                }
            }

            // Create spawn point if valid position found
            if (foundValidPosition)
            {
                spawnPositions.Add(validPosition);

                GameObject spawnPoint = new GameObject($"SpawnPoint_{generatedCount:000}");
                spawnPoint.transform.SetParent(container);
                spawnPoint.transform.localPosition = new Vector3(validPosition.x, validPosition.y, 0);
                spawnPoint.transform.localRotation = Quaternion.identity;
                spawnPoint.transform.localScale = Vector3.one;

                // Optional: Add a visual gizmo component
                var gizmo = spawnPoint.AddComponent<SpawnPointGizmo>();
                gizmo.index = generatedCount;

                generatedCount++;
            }
        }

        // Warn if couldn't generate all requested points
        if (generatedCount < planet.spawnPointCount)
        {
            Debug.LogWarning($"Could only generate {generatedCount}/{planet.spawnPointCount} spawn points. " +
                           $"Try increasing spawn radius or decreasing minimum distance.");
        }

        Debug.Log($"Generated {generatedCount} spawn points (requested: {planet.spawnPointCount})");

        EditorUtility.SetDirty(planet);
        EditorUtility.DisplayDialog(
            "Success",
            $"Generated {planet.spawnPointCount} spawn points!\n\n" +
            $"Check '{container.name}' in hierarchy.",
            "OK"
        );
    }

    private void ClearSpawnPoints(Planet planet)
    {
        if (planet.spawnPointsContainer == null) return;

        if (!EditorUtility.DisplayDialog(
            "Clear Spawn Points?",
            $"This will delete all {planet.spawnPointsContainer.childCount} spawn points.\n\nAre you sure?",
            "Yes, Clear",
            "Cancel"))
        {
            return;
        }

        // Delete all children
        while (planet.spawnPointsContainer.childCount > 0)
        {
            DestroyImmediate(planet.spawnPointsContainer.GetChild(0).gameObject);
        }

        EditorUtility.SetDirty(planet);
        Debug.Log("Spawn points cleared!");
    }
}

/// <summary>
/// Helper component to visualize spawn points in the scene view
/// </summary>
public class SpawnPointGizmo : MonoBehaviour
{
    public int index;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.1f);

        // Draw line to parent (planet)
        if (transform.parent != null && transform.parent.parent != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawLine(transform.parent.parent.position, transform.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.2f);

        // Draw label
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 0.3f,
            $"Spawn {index}",
            new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = Color.yellow },
                fontSize = 10
            }
        );
    }
#endif
}
