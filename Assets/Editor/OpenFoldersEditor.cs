using UnityEditor;
using UnityEngine;
using System.Diagnostics; // For opening the folder

public class OpenFoldersEditor
{
    [MenuItem("Tools/Open Persistent Data Path")]
    public static void OpenPersistentPath()
    {
        // Get the path
        string path = Application.persistentDataPath;

        // Check if the path exists to avoid errors
        if (System.IO.Directory.Exists(path))
        {
            // Open the folder in File Explorer (Windows) or Finder (Mac)
            EditorUtility.RevealInFinder(path);
        }
        else
        {
            UnityEngine.Debug.LogError("Persistent Data Path does not exist yet. Save something first!");
        }
    }
}