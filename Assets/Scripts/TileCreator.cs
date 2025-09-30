#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class TileCreator
{
    [MenuItem("Assets/Create/Custom/Tile")]
    public static void CreateTile()
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();

        string path = EditorUtility.SaveFilePanelInProject(
            "Save Tile",
            "New Tile",
            "asset",
            "Save Tile"
        );

        if (path == "")
            return;

        AssetDatabase.CreateAsset(tile, path);
        AssetDatabase.SaveAssets();
    }
}
#endif