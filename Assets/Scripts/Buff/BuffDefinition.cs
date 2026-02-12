using UnityEngine;

public abstract class BuffDefinition : ScriptableObject
{
    [SerializeField] private string buffId;
    [SerializeField] private string displayName;
    [SerializeField, TextArea] private string description;

    public string BuffId => buffId;
    public string DisplayName => displayName;
    public string Description => description;

    public abstract void Apply();
}
