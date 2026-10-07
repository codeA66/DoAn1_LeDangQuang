using LuHanh.Level;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace LuHanh.EditorTools
{
    /// <summary>Keo canh phong trong Scene view; canh tu bam theo luoi 1 unit cho khop tilemap.</summary>
    [CustomEditor(typeof(Room))]
    public class RoomEditor : Editor
    {
        readonly BoxBoundsHandle handle = new() { axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y };

        void OnSceneGUI()
        {
            var room = (Room)target;
            var t = room.transform;

            handle.center = t.position;
            handle.size = room.Size;
            handle.SetColor(new Color(0.4f, 0.95f, 1f));

            EditorGUI.BeginChangeCheck();
            handle.DrawHandle();
            if (!EditorGUI.EndChangeCheck()) return;

            Vector2 min = handle.center - handle.size * 0.5f;
            Vector2 max = handle.center + handle.size * 0.5f;
            min = new Vector2(Mathf.Round(min.x), Mathf.Round(min.y));
            max = new Vector2(Mathf.Round(max.x), Mathf.Round(max.y));

            Undo.RecordObjects(new Object[] { room, t }, "Resize Room");
            room.Size = max - min;
            t.position = new Vector3((min.x + max.x) * 0.5f, (min.y + max.y) * 0.5f, t.position.z);
        }
    }
}
