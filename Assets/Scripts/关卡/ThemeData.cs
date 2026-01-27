using UnityEngine;

[CreateAssetMenu(fileName = "ThemeData", menuName = "Scriptable Objects/ThemeData")]
public class ThemeData : ScriptableObject
{
    [SerializeField, ChineseLabel("关卡数据")] private LevelData[] levels;
    public LevelData[] Levels => levels;
}
