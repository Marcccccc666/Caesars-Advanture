#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BattleRoomController), true)]
public class RoomControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BattleRoomController rc = (BattleRoomController)target;

        GUILayout.Space(10);

        if (GUILayout.Button("收集子物体中的敌人"))
        {
            rc.GetAllEnemies();
            EditorUtility.SetDirty(rc);
        }
    }
}
#endif
