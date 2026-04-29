using UnityEditor;
using UnityEngine;

public static class HumanCitySceneValidator
{
    [MenuItem("Golden Keep/Human City/Validate Current Scene")]
    public static void ValidateCurrentScene()
    {
        HumanCityRuntimeBootstrap bootstrap = Object.FindFirstObjectByType<HumanCityRuntimeBootstrap>();
        if (bootstrap == null)
        {
            Debug.LogError("Human City validation failed: scene has no HumanCityRuntimeBootstrap.");
            return;
        }

        int errorCount = 0;
        errorCount += RequireReference(bootstrap.buildingsJson, "buildingsJson");
        errorCount += RequireReference(bootstrap.citizensJson, "citizensJson");
        errorCount += RequireReference(bootstrap.routeGraphJson, "routeGraphJson");
        errorCount += RequireReference(bootstrap.backdropSprite, "backdropSprite");
        errorCount += RequireReference(bootstrap.civilianSprite, "civilianSprite");
        errorCount += RequireReference(bootstrap.guardSprite, "guardSprite");
        errorCount += RequireReference(bootstrap.playerSprite, "playerSprite");

        if (bootstrap.buildingsJson != null)
        {
            CityBuildingDatabase buildings = JsonUtility.FromJson<CityBuildingDatabase>(bootstrap.buildingsJson.text);
            if (buildings == null || buildings.buildings == null || buildings.buildings.Count != 25)
            {
                Debug.LogError("Human City validation failed: expected 25 buildings.");
                errorCount++;
            }
        }

        if (bootstrap.citizensJson != null)
        {
            CitizenDatabase citizens = JsonUtility.FromJson<CitizenDatabase>(bootstrap.citizensJson.text);
            if (citizens == null || citizens.citizens == null || citizens.citizens.Count != 50)
            {
                Debug.LogError("Human City validation failed: expected 50 citizens.");
                errorCount++;
            }
        }

        if (errorCount == 0)
            Debug.Log("Human City validation passed: bootstrap, data, and art references are present.");
    }

    private static int RequireReference(Object value, string label)
    {
        if (value != null) return 0;
        Debug.LogError("Human City validation failed: missing " + label + ".");
        return 1;
    }
}
