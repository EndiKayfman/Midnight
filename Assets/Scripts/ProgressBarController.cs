using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarController : MonoBehaviour
{
    [SerializeField] private Image _fillImage;
    
    public void UpdateProgress(float progress)
    {
        _fillImage.fillAmount = progress;
    }

    private void Update()
    {
        transform.LookAt(Camera.main.transform);
    }
}