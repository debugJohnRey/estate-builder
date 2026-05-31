using UnityEngine;
using EstateBuilder.Models;

public class TerrainPlacer : MonoBehaviour
{
    public static TerrainPlacer Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Place(PropertyData property, Transform plot)
    {
        // Loads the prefab from the Resources folder
        GameObject housePrefab = Resources.Load<GameObject>(property.asset_ref);

        if (housePrefab != null)
        {
            Instantiate(housePrefab, plot.position, plot.rotation);
        }
        else
        {
            Debug.LogError($"Failed to load prefab at path: {property.asset_ref}");
        }
    }
}