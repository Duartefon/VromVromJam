using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TreeMassSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [Tooltip("Drag your tree prefab here.")]
    public GameObject treePrefab;
    
    [Tooltip("How many trees to spawn.")]
    public int treeCount = 50;
    
    [Tooltip("The radius of the circular area where trees will spawn.")]
    public float spawnRadius = 20f;

    [Header("Transform Adjustments")]
    [Tooltip("Adjust this to fix the tree facing downwards. Usually (90, 0, 0) or (-90, 0, 0) on the X axis stands it up.")]
    public Vector3 rotationCorrection = new Vector3(-90f, 0f, 0f);

    [Tooltip("Scale variance. 0 means exact original scale, 0.3 means a random scale between 70% and 130%.")]
    [Range(0f, 1f)]
    public float scaleRandomness = 0.3f;

    public void SpawnTrees()
    {
        if (treePrefab == null)
        {
            Debug.LogWarning("TreeMassSpawner: Please assign a Tree Prefab in the inspector!");
            return;
        }

        for (int i = 0; i < treeCount; i++)
        {
            // 1. Calculate a random position within the defined radius
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

            // 2. Instantiate the tree (maintaining prefab connection in the Editor)
            GameObject newTree;
            #if UNITY_EDITOR
            newTree = (GameObject)PrefabUtility.InstantiatePrefab(treePrefab);
            newTree.transform.position = spawnPos;
            #else
            newTree = Instantiate(treePrefab, spawnPos, Quaternion.identity);
            #endif

            // 3. Fix the "facing down" issue and add some random Y rotation so they don't look identical
            float randomYRot = Random.Range(0f, 360f);
            Quaternion correctionRot = Quaternion.Euler(rotationCorrection);
            Quaternion randomYQuat = Quaternion.Euler(0f, randomYRot, 0f);
            
            // We apply the Y rotation first, then multiply by the correction to stand it upright
            newTree.transform.rotation = randomYQuat * correctionRot;

            // 4. Apply scale randomness based on the prefab's original scale
            float randomScaleFactor = 1f + Random.Range(-scaleRandomness, scaleRandomness);
            newTree.transform.localScale = treePrefab.transform.localScale * randomScaleFactor;

            // 5. Parent to this spawner GameObject to keep your hierarchy clean
            newTree.transform.SetParent(this.transform);
        }
    }

    public void ClearTrees()
    {
        // Iterate backwards when destroying children to avoid index out of bounds errors
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}

// This block creates the actual buttons in the Unity Inspector
#if UNITY_EDITOR
[CustomEditor(typeof(TreeMassSpawner))]
public class TreeMassSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default properties (Prefab, Radius, etc.)
        DrawDefaultInspector();

        TreeMassSpawner spawner = (TreeMassSpawner)target;

        GUILayout.Space(15); // Add a little visual padding

        // The Spawn Button
        if (GUILayout.Button("Spawn Trees", GUILayout.Height(35)))
        {
            Undo.RegisterFullObjectHierarchyUndo(spawner.gameObject, "Spawn Trees");
            spawner.SpawnTrees();
        }

        GUILayout.Space(5);

        // The Clear Button
        if (GUILayout.Button("Clear Spawned Trees"))
        {
            Undo.RegisterFullObjectHierarchyUndo(spawner.gameObject, "Clear Trees");
            spawner.ClearTrees();
        }
    }
}
#endif