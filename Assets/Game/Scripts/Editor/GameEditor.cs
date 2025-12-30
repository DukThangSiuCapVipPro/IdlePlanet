using UnityEngine;
using UnityEditor;
using System.IO;
using System.Globalization;
using ThangDD;

public class CreatureDataImporter : EditorWindow
{
    private string csvPath = "Assets/Game/Datas/CreaturesData.csv";
    private string outputFolder = "Assets/Game/Prefabs/CreatureDatas";

    [MenuItem("Idle Planet/Import Creatures from CSV")]
    public static void ShowWindow()
    {
        GetWindow<CreatureDataImporter>("Creature Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Creature Data Importer", EditorStyles.boldLabel);
        GUILayout.Space(10);

        csvPath = EditorGUILayout.TextField("CSV Path:", csvPath);
        outputFolder = EditorGUILayout.TextField("Output Folder:", outputFolder);

        GUILayout.Space(10);

        if (GUILayout.Button("Import Creatures", GUILayout.Height(30)))
        {
            ImportCreatures();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Assign Prefabs to Creature Data", GUILayout.Height(30)))
        {
            AssignPrefabsToCreatureData();
        }

        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "Import: Read the CSV file and create ScriptableObject assets for each creature.\n\n" +
            "Assign Prefabs: Automatically match and assign prefabs from Prefabs/Creatures to CreatureData assets in Prefabs/CreatureDatas by name.",
            MessageType.Info
        );
    }

    private void ImportCreatures()
    {
        if (!File.Exists(csvPath))
        {
            EditorUtility.DisplayDialog("Error", $"CSV file not found at: {csvPath}", "OK");
            return;
        }

        // Create output folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder(outputFolder))
        {
            string[] folders = outputFolder.Split('/');
            string currentPath = folders[0];

            for (int i = 1; i < folders.Length; i++)
            {
                string newPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath = newPath;
            }
        }

        string[] lines = File.ReadAllLines(csvPath);
        int updatedCount = 0;
        int createdCount = 0;

        // Skip header line (index 0)
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] values = line.Split(',');
            if (values.Length < 10) continue;

            try
            {
                // Parse CSV values
                int id = int.Parse(values[0]);
                string name = values[1];
                string description = values[2];
                CreatureSize size = (CreatureSize)System.Enum.Parse(typeof(CreatureSize), values[3]);
                CreatureEra era = (CreatureEra)System.Enum.Parse(typeof(CreatureEra), values[4]);
                double basePriceValue = double.Parse(values[5], CultureInfo.InvariantCulture);
                double baseProductivityValue = double.Parse(values[6], CultureInfo.InvariantCulture);
                float priceScale = float.Parse(values[7], CultureInfo.InvariantCulture);
                float productivityScale = float.Parse(values[8], CultureInfo.InvariantCulture);
                int requiredDayNumber = int.Parse(values[9]);

                // Find existing asset by ID
                string[] existingAssets = System.IO.Directory.GetFiles(outputFolder, $"{id:000}_*.asset");
                CreatureData creature = null;

                if (existingAssets.Length > 0)
                {
                    // Update existing asset
                    string assetPath = existingAssets[0].Replace('\\', '/');
                    creature = AssetDatabase.LoadAssetAtPath<CreatureData>(assetPath);
                    if (creature != null)
                    {
                        updatedCount++;
                    }
                }

                if (creature == null)
                {
                    // Create new ScriptableObject if not found
                    creature = ScriptableObject.CreateInstance<CreatureData>();
                    string assetPath = $"{outputFolder}/{id:000}_{name.Replace(" ", "")}.asset";
                    AssetDatabase.CreateAsset(creature, assetPath);
                    createdCount++;
                }

                // Update properties
                creature.id = id;
                creature.name = name;
                creature.description = description;
                creature.size = size;
                creature.era = era;
                creature.basePrice = new BigNumber(basePriceValue);
                creature.baseProductivity = new BigNumber(baseProductivityValue);
                creature.priceScalePerLevel = priceScale;
                creature.productivityScalePerLevel = productivityScale;
                creature.requiredDayNumber = requiredDayNumber;

                EditorUtility.SetDirty(creature);

                if ((updatedCount + createdCount) % 10 == 0)
                {
                    EditorUtility.DisplayProgressBar("Importing Creatures",
                        $"Processing creature {updatedCount + createdCount}/{lines.Length - 1}",
                        (float)(updatedCount + createdCount) / (lines.Length - 1));
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error importing creature on line {i + 1}: {e.Message}");
            }
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success",
            $"Successfully updated {updatedCount} and created {createdCount} creatures!\n\nCheck: {outputFolder}",
            "OK");
    }

    private void AssignPrefabsToCreatureData()
    {
        string creatureDataFolder = "Assets/Game/Prefabs/CreatureDatas";
        string prefabFolder = "Assets/Game/Prefabs/Creatures";
        string spriteFolder = "Assets/Game/Sprites/Creatures";

        // Load all CreatureData assets
        string[] creatureDataGuids = AssetDatabase.FindAssets("t:CreatureData", new[] { creatureDataFolder });
        if (creatureDataGuids.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", $"No CreatureData assets found in {creatureDataFolder}", "OK");
            return;
        }

        // Load all prefabs
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabFolder });
        if (prefabGuids.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", $"No prefabs found in {prefabFolder}", "OK");
            return;
        }

        // Load all sprites
        string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { spriteFolder });
        if (spriteGuids.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", $"No sprites found in {spriteFolder}", "OK");
            return;
        }

        // Build a dictionary of prefab IDs to GameObjects
        // Expecting prefab names like "Creature 1", "Creature 2", etc.
        System.Collections.Generic.Dictionary<int, GameObject> prefabDict = new System.Collections.Generic.Dictionary<int, GameObject>();
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                // Extract number from "Creature X" format
                string[] parts = prefab.name.Split(' ');
                if (parts.Length >= 2 && int.TryParse(parts[parts.Length - 1], out int prefabId))
                {
                    prefabDict[prefabId] = prefab;
                }
                else
                {
                    Debug.LogWarning($"Could not extract ID from prefab name: '{prefab.name}'");
                }
            }
        }

        // Build a dictionary of sprite IDs to Sprites
        System.Collections.Generic.Dictionary<int, Sprite> spriteDict = new System.Collections.Generic.Dictionary<int, Sprite>();
        foreach (string guid in spriteGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
            {
                // Extract number from "Creature X" format
                string[] parts = sprite.name.Split(' ');
                if (parts.Length >= 2 && int.TryParse(parts[parts.Length - 1], out int spriteId))
                {
                    spriteDict[spriteId] = sprite;
                }
                else
                {
                    Debug.LogWarning($"Could not extract ID from sprite name: '{sprite.name}'");
                }
            }
        }

        int assignedCount = 0;
        int skippedCount = 0;

        // Assign prefabs and sprites to CreatureData
        for (int i = 0; i < creatureDataGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(creatureDataGuids[i]);
            CreatureData creatureData = AssetDatabase.LoadAssetAtPath<CreatureData>(path);

            if (creatureData == null) continue;

            // Use the CreatureData's ID field to match with prefab
            int creatureId = creatureData.id;

            if (prefabDict.ContainsKey(creatureId))
            {
                creatureData.prefab = prefabDict[creatureId];
                creatureData.icon = spriteDict.ContainsKey(creatureId) ? spriteDict[creatureId] : null;
                EditorUtility.SetDirty(creatureData);
                assignedCount++;
                Debug.Log($"Assigned prefab '{prefabDict[creatureId].name}' to CreatureData '{creatureData.name}' (ID: {creatureId})");
            }
            else
            {
                skippedCount++;
                Debug.LogWarning($"No matching prefab found for CreatureData '{creatureData.name}' (ID: {creatureId})");
            }

            if ((i + 1) % 10 == 0)
            {
                EditorUtility.DisplayProgressBar("Assigning Prefabs",
                    $"Processing {i + 1}/{creatureDataGuids.Length}",
                    (float)(i + 1) / creatureDataGuids.Length);
            }
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string message = $"Successfully assigned {assignedCount} prefabs!\n";
        if (skippedCount > 0)
            message += $"\n{skippedCount} CreatureData assets had no matching prefab.";

        EditorUtility.DisplayDialog("Complete", message, "OK");
    }
}
