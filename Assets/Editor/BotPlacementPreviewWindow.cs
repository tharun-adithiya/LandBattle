using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BotPlacementPreviewWindow : EditorWindow
{
    private BotShipPlacementData placementData;
    private Tilemap tilemap;

    // Scene ships (exactly 5)
    private Transform[] sceneShips = new Transform[5];

    [MenuItem("Tools/Apply Bot Placement")]
    public static void Open()
    {
        GetWindow<BotPlacementPreviewWindow>("Apply Bot Placement");
    }

    void OnGUI()
    {
        GUILayout.Label("Apply Placement To Existing Ships", EditorStyles.boldLabel);

        placementData = (BotShipPlacementData)EditorGUILayout.ObjectField(
            "Placement SO",
            placementData,
            typeof(BotShipPlacementData),
            false);

        tilemap = (Tilemap)EditorGUILayout.ObjectField(
            "Tilemap",
            tilemap,
            typeof(Tilemap),
            true);

        EditorGUILayout.Space();
        GUILayout.Label("Scene Ships (5)", EditorStyles.boldLabel);

        for (int i = 0; i < 5; i++)
        {
            sceneShips[i] = (Transform)EditorGUILayout.ObjectField(
                $"Ship {i + 1}",
                sceneShips[i],
                typeof(Transform),
                true);
        }

        EditorGUILayout.Space();

        GUI.enabled = placementData && tilemap && sceneShips.All(s => s);

        if (GUILayout.Button("Apply Placement"))
        {
            ApplyPlacement();
        }

        GUI.enabled = true;
    }

    // ================= APPLY =================

    void ApplyPlacement()
    {
        if (placementData.ships == null || placementData.ships.Length != 5)
        {
            Debug.LogError("Placement SO must contain exactly 5 ships.");
            return;
        }

        foreach (var soShip in placementData.ships)
        {
            Transform match = FindSceneShipBySize(soShip.size);

            if (!match)
            {
                Debug.LogError($"No scene ship found with size {soShip.size}");
                return;
            }

            Undo.RecordObject(match, "Apply Bot Placement");

            Vector3 world = tilemap.GetCellCenterWorld(soShip.cellPosition);

            match.position = world;
            match.rotation = Quaternion.Euler(0, 0, soShip.rotatedAngle);
        }

        Debug.Log("Bot placement applied to existing ships.");
    }

    Transform FindSceneShipBySize(int size)
    {
        foreach (var t in sceneShips)
        {
            BotShipController c = t.GetComponent<BotShipController>();

            if (c && c.shipSize == size)
                return t;
        }

        return null;
    }
}
