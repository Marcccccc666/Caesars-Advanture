#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoomController))]
public class RoomControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RoomController rc = (RoomController)target;

        GUILayout.Space(10);

        if (GUILayout.Button("收集子物体中的敌人"))
        {
            rc.GetAllEnemies();
            EditorUtility.SetDirty(rc);
        }
    }
}
#endif
