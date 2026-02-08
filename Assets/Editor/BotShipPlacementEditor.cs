using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(BotShipPlacementData))]
public class BotShipPlacementEditor : Editor
{
    private Tilemap tilemap;
    private Transform[] sceneShips = new Transform[5];

    // NEW
    private BotBoardManager botBoardManager;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BotShipPlacementData data = (BotShipPlacementData)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Bot Placement Tool", EditorStyles.boldLabel);

        tilemap = (Tilemap)EditorGUILayout.ObjectField(
            "Bot Tilemap",
            tilemap,
            typeof(Tilemap),
            true);

        // NEW
        botBoardManager = (BotBoardManager)EditorGUILayout.ObjectField(
            "Bot Board Manager",
            botBoardManager,
            typeof(BotBoardManager),
            true);

        EditorGUILayout.Space();

        DrawShipDropArea();

        EditorGUILayout.Space();

        GUI.enabled = tilemap != null && botBoardManager != null;

        if (GUILayout.Button("Capture Ship Placements"))
        {
            CaptureShips(data);
        }

        GUI.enabled = true;
    }

    // ================= DROP AREA =================

    void DrawShipDropArea()
    {
        Rect dropArea = GUILayoutUtility.GetRect(0, 70, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drag 5 Ships Here");

        Event evt = Event.current;

        if (!dropArea.Contains(evt.mousePosition))
            return;

        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                var dropped = DragAndDrop.objectReferences
                    .OfType<GameObject>()
                    .Select(g => g.transform)
                    .ToArray();

                if (dropped.Length != 5)
                {
                    Debug.LogWarning("Drop EXACTLY 5 ships.");
                    return;
                }

                sceneShips = dropped;

                Debug.Log("Ships assigned.");
            }

            evt.Use();
        }
    }

    // ================= CAPTURE =================

    void CaptureShips(BotShipPlacementData data)
    {
        if (tilemap == null || botBoardManager == null)
        {
            Debug.LogError("Tilemap or BotBoardManager missing.");
            return;
        }

        if (sceneShips.Any(s => s == null))
        {
            Debug.LogError("Missing ship references.");
            return;
        }

        data.ships = new BotShipPlacementData.Ship[sceneShips.Length];

        for (int i = 0; i < sceneShips.Length; i++)
        {
            Transform t = sceneShips[i];

            BotShipController controller = t.GetComponent<BotShipController>();

            if (!controller)
            {
                Debug.LogError($"BotShipController missing on {t.name}");
                return;
            }

            float angle = t.eulerAngles.z;
            bool horizontal = Mathf.Abs(angle % 180f) < 1f;

            // FIX — bake true start cell instead of transform center
            Vector3Int cell = botBoardManager.GetStartCell(
                t.position,
                controller.shipSize,
                horizontal);

            data.ships[i] = new BotShipPlacementData.Ship
            {
                cellPosition = cell,
                rotatedAngle = angle,
                size = controller.shipSize,
                isHorizontal = horizontal
            };
        }

        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();

        Debug.Log($"Captured {sceneShips.Length} ships.");
    }
}
