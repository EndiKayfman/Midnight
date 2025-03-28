using UnityEngine;

[CreateAssetMenu(fileName = "NewResourceType", menuName = "Game/Resource Type")]
public class ResourceType : ScriptableObject
{
    public string displayName;
    public Sprite icon;
    public GameObject flyingVisualPrefab;
    public Color resourceColor = Color.white;
}