using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "ResourcesDatabase", menuName = "Game/Resources Database")]
public class ResourcesDatabase : ScriptableObject
{
    public ResourceType[] allResources;

#if UNITY_EDITOR
    [ContextMenu("Auto Populate")]
    private void AutoPopulate()
    {
        string[] guids = AssetDatabase.FindAssets("t:ResourceType");
        allResources = new ResourceType[guids.Length];
        
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            allResources[i] = AssetDatabase.LoadAssetAtPath<ResourceType>(path);
        }
        
        EditorUtility.SetDirty(this);
    }
#endif
}