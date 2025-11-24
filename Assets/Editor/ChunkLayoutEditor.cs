using UnityEditor;
using UnityEngine;
using World;

[CustomEditor(typeof(ChunkLayoutSO))]
public class ChunkLayoutEditor : Editor
{
    private ChunkLayoutSO so;
    private Vector2 scroll;

    private void OnEnable()
    {
        so = (ChunkLayoutSO)target;
    }

    public override void OnInspectorGUI()
    {
        if (so == null) return;

        EditorGUILayout.LabelField("Chunk Layout Editor", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        so.segments = EditorGUILayout.IntField("Segments (rows)", so.segments);
        so.lanes = EditorGUILayout.IntField("Lanes (cols)", so.lanes);
        so.cellSize = EditorGUILayout.FloatField("Cell Size (Z)", so.cellSize);

        so.EnsureSize();

        EditorGUILayout.Space();

        // legend
        EditorGUILayout.LabelField("Legend");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Empty")) { GUIUtility.keyboardControl = 0; }
        //if (GUILayout.Button("Moving (1)")) { GUIUtility.keyboardControl = 0; }
        if (GUILayout.Button("Static (2)")) { GUIUtility.keyboardControl = 0; }
        if (GUILayout.Button("Jump (3)")) { GUIUtility.keyboardControl = 0; }
        if (GUILayout.Button("Collectible (10)")) { GUIUtility.keyboardControl = 0; }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // grid
        int rows = so.segments;
        int cols = so.lanes;

        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(400));
        for (int r = 0; r < rows; r++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{r:000}", GUILayout.Width(40));
            for (int c = 0; c < cols; c++)
            {
                var cur = so.GetCell(r, c);
                var newVal = (ChunkLayoutSO.CellType)EditorGUILayout.EnumPopup(cur, GUILayout.Width(90));
                if (newVal != cur)
                {
                    Undo.RecordObject(so, "Edit Chunk Cell");
                    so.SetCell(r, c, newVal);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        if (EditorGUI.EndChangeCheck())
        {
            so.EnsureSize();
        }

        if (GUILayout.Button("Fill Random"))
        {
            UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
            for (int i = 0; i < so.cells.Length; i++)
            {
                int v = UnityEngine.Random.Range(0, 12);
                if (v == 10) so.cells[i] = ChunkLayoutSO.CellType.Collectible;
                else if (v <= 3) so.cells[i] = (ChunkLayoutSO.CellType)v;
                else so.cells[i] = ChunkLayoutSO.CellType.Empty;
            }
            EditorUtility.SetDirty(so);
        }

        if (GUILayout.Button("Clear"))
        {
            for (int i = 0; i < so.cells.Length; i++) so.cells[i] = ChunkLayoutSO.CellType.Empty;
            EditorUtility.SetDirty(so);
        }

        // default inspector fold
        EditorGUILayout.Space();
        DrawDefaultInspector();
    }
}
