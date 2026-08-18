using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Board))]
public class BoardEditor : Editor
{
    private Board board;

    private bool editingCells;

    private void OnEnable()
    {
        board = (Board)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        if (!editingCells)
        {
            if (GUILayout.Button("Start Cell Editing"))
            {
                EnsureLogicalBoard();

                editingCells = true;

                SceneView.RepaintAll();
            }
        }
        else
        {
            EditorGUILayout.HelpBox(
                "Cell editing is active.\n" +
                "Click on the plane to create cells.\n" +
                "Keep clicking to create cells sequentially.",
                MessageType.Info
            );

            if (GUILayout.Button("Stop Cell Editing"))
            {
                editingCells = false;

                SceneView.RepaintAll();
            }
        }
    }

    private void OnSceneGUI()
    {
        DrawPlane();

        if (!editingCells)
            return;

        Event e = Event.current;

        if (e.type != EventType.MouseDown)
            return;

        if (e.button != 0)
            return;

        // Alt + Click = Scene View navigation
        if (e.alt)
            return;

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        Plane plane = GetEditingPlane();

        if (!plane.Raycast(ray, out float distance))
            return;

        Vector3 position = ray.GetPoint(distance);

        CreateCell(position);

        e.Use();
    }

    private Plane GetEditingPlane()
    {
        Transform boardTransform = board.transform;

        Vector3 normal = boardTransform.up;

        Vector3 planePoint =
            boardTransform.position +
            normal * board.planeHeightOffset;

        return new Plane(normal, planePoint);
    }

    private void CreateCell(Vector3 worldPosition)
    {
        EnsureLogicalBoard();

        Undo.RecordObject(board, "Add Cell");

        int index = board.Cells?.Length ?? 0;

        GameObject cellObject = new GameObject(
            $"Cell_{index}"
        );

        Undo.RegisterCreatedObjectUndo(
            cellObject,
            "Create Cell"
        );

        // تمام Cellها فرزند LogicalBoard می‌شوند
        cellObject.transform.SetParent(
            board.LogicalBoard,
            true
        );

        cellObject.transform.position = worldPosition;

        cellObject.transform.rotation =
            board.transform.rotation;

        Cell cell = Undo.AddComponent<Cell>(
            cellObject
        );

        SetCellIndex(cell, index);

        board.AddCell(cell);

        EditorUtility.SetDirty(board);

        // Cell تازه ساخته شده انتخاب می‌شود
        Selection.activeGameObject = cellObject;

        SceneView.RepaintAll();
    }

    private void SetCellIndex(Cell cell, int index)
    {
        SerializedObject serializedCell =
            new SerializedObject(cell);

        SerializedProperty indexProperty =
            serializedCell.FindProperty(
                "<Index>k__BackingField"
            );

        if (indexProperty != null)
        {
            indexProperty.intValue = index;

            serializedCell.ApplyModifiedProperties();
        }
    }

    private void EnsureLogicalBoard()
    {
        if (board.LogicalBoard != null)
            return;

        GameObject logicalBoard =
            new GameObject("LogicalBoard");

        Undo.RegisterCreatedObjectUndo(
            logicalBoard,
            "Create LogicalBoard"
        );

        logicalBoard.transform.SetParent(
            board.transform,
            false
        );

        board.SetLogicalBoard(
            logicalBoard.transform
        );

        EditorUtility.SetDirty(board);
    }

    private void DrawPlane()
    {
        Transform boardTransform = board.transform;

        Vector3 normal = boardTransform.up;

        Vector3 center =
            boardTransform.position +
            normal * board.planeHeightOffset;

        float halfSize = board.planeSize * 0.5f;

        Vector3 right =
            boardTransform.right * halfSize;

        Vector3 forward =
            boardTransform.forward * halfSize;

        Vector3 p1 = center - right - forward;
        Vector3 p2 = center - right + forward;
        Vector3 p3 = center + right + forward;
        Vector3 p4 = center + right - forward;

        Handles.color =
            new Color(0f, 1f, 0f, 0.15f);

        Handles.DrawSolidRectangleWithOutline(
            new[]
            {
                p1,
                p2,
                p3,
                p4
            },
            new Color(0f, 1f, 0f, 0.05f),
            Color.green
        );
    }
}