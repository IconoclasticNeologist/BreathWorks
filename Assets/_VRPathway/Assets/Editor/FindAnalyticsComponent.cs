using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

public class FindAnalyticsComponent : EditorWindow
{
    [MenuItem("Tools/Find Socket Interactors Analytics")]
    public static void FindComponent()
    {
        var analyticsType = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.Name == "XrcSocketInteractorsStationAnalytics");

        if (analyticsType == null)
        {
            Debug.Log("Could not find XrcSocketInteractorsStationAnalytics type in any loaded assembly");
            return;
        }

        var components = GameObject.FindObjectsOfType(analyticsType, true);

        if (components.Length == 0)
        {
            Debug.Log("No XrcSocketInteractorsStationAnalytics components found in scene");
            return;
        }

        foreach (var component in components)
        {
            var gameObj = ((Component)component).gameObject;
            Debug.Log($"Found component on GameObject: {gameObj.name}", gameObj);
            Debug.Log($"GameObject path: {GetGameObjectPath(gameObj)}");
            Debug.Log($"Active in hierarchy: {gameObj.activeInHierarchy}");
            Debug.Log($"Component is enabled: {((Behaviour)component).enabled}");
            Debug.Log($"Scene path: {gameObj.scene.path}");
            Debug.Log($"HideFlags: {gameObj.hideFlags}");

            // Try to select it
            Selection.activeGameObject = gameObj;

            // Print all components on the object
            Debug.Log("Components on this GameObject:");
            foreach (var comp in gameObj.GetComponents<Component>())
            {
                Debug.Log($"- {comp.GetType().Name}");
            }
        }
    }

    static string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = obj.name + "/" + path;
        }
        return path;
    }
}