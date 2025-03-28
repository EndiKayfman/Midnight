using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text countText;

    public void Initialize(Sprite icon, string name, int count)
    {
        if (iconImage != null) iconImage.sprite = icon;
        if (nameText != null) nameText.text = name;
        UpdateCount(count);
    }
    
    public void UpdateCount(int newCount)
    {
        if (countText != null) 
        {
            countText.text = newCount.ToString();
        }
    }
}