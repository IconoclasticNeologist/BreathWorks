using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.Collections.Generic;

public class AnalyticsCleanup : EditorWindow
{
    [MenuItem("Tools/XR/List Analytics Components")]
    public static void ListAnalytics()
    {
        Debug.Log("Scanning for analytics components...");
        int count = 0;

        // Find all components in the scene
        var allComponents = GameObject.FindObjectsOfType<Component>(true);

        foreach (var component in allComponents)
        {
            if (component != null)
            {
                string typeName = component.GetType().Name;
                // Check if the component name contains "Analytics"
                if (typeName.Contains("Analytics"))
                {
                    count++;
                    Debug.Log($"Found analytics component: {typeName} on GameObject: {component.gameObject.name}", component.gameObject);
                }
            }
        }

        Debug.Log($"Found {count} analytics components in total.");
    }

    [MenuItem("Tools/XR/Remove Analytics Components")]
    public static void RemoveAnalytics()
    {
        if (!EditorUtility.DisplayDialog("Remove Analytics?",
            "This will remove all XR Content analytics components from your scene. This operation cannot be undone. Make sure you have a backup of your scene before proceeding. Continue?",
            "Yes, Remove Analytics", "Cancel"))
        {
            return;
        }

        Debug.Log("Beginning analytics removal...");
        int removedCount = 0;

        // Find all components in the scene
        var allComponents = GameObject.FindObjectsOfType<Component>(true);

        foreach (var component in allComponents)
        {
            if (component != null)
            {
                string typeName = component.GetType().Name;
                // Check if the component name contains "Analytics"
                if (typeName.Contains("Analytics"))
                {
                    Debug.Log($"Removing {typeName} from {component.gameObject.name}", component.gameObject);
                    Undo.DestroyObjectImmediate(component);
                    removedCount++;
                }
            }
        }

        Debug.Log($"Removed {removedCount} analytics components.");
    }
}