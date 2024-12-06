using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

public class RemoveSocketAnalytics : EditorWindow
{
    [MenuItem("Tools/XR/Remove Socket Analytics")]
    public static void RemoveAnalytics()
    {
        if (!EditorUtility.DisplayDialog("Remove Socket Analytics?",
            "This will remove only the XR Socket Interactors analytics components from your scene. Other analytics will remain intact. Make sure you have a backup of your scene before proceeding. Continue?",
            "Yes, Remove Socket Analytics", "Cancel"))
        {
            return;
        }

        try
        {
            // Find the specific analytics type
            var analyticsType = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == "XrcSocketInteractorsStationAnalytics");

            if (analyticsType == null)
            {
                Debug.Log("No socket analytics type found in the project.");
                return;
            }

            // Find all instances of this component
            var components = GameObject.FindObjectsOfType(analyticsType, true);
            int count = components.Length;

            if (count == 0)
            {
                Debug.Log("No socket analytics components found in the scene.");
                return;
            }

            // Remove each instance
            foreach (var component in components)
            {
                var go = ((Component)component).gameObject;
                Debug.Log($"Removing socket analytics from: {go.name}", go);
                Undo.DestroyObjectImmediate(component);
            }

            Debug.Log($"Successfully removed {count} socket analytics components. Other analytics components remain intact.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error while removing socket analytics: {e.Message}\n{e.StackTrace}");
        }
    }
}