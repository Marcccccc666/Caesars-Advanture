using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuffPool", menuName = "Buff Pool/Buff Pool")]
public class BuffPool : ScriptableObject
{
    [SerializeField] private List<BuffDefinition> buffs = new List<BuffDefinition>();

    public IReadOnlyList<BuffDefinition> Buffs => buffs;
}
