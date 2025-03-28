using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Toggle vibrationToggle;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button deleteSaveButton;
    [SerializeField] private Button openSettingsButton;
    [SerializeField] private Button closeSettingsButton;

    private string savePath;

    private void Start()
    {
        savePath = SaveLoadManager.Instance != null 
            ? Path.Combine(Application.persistentDataPath, "game_save.json")
            : string.Empty;
        
        LoadSettings();
        
        vibrationToggle.onValueChanged.AddListener(OnVibrationChanged);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        deleteSaveButton.onClick.AddListener(OnDeleteSaveClicked);
        openSettingsButton.onClick.AddListener(ToggleSettingsPanel);
        closeSettingsButton.onClick.AddListener(ToggleSettingsPanel);
    }
    
    public void ToggleSettingsPanel()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
        
        if (settingsPanel.activeSelf)
        {
            LoadSettings();
        }
    }
    
    private void OnVibrationChanged(bool isOn)
    {
        PlayerPrefs.SetInt("VibrationEnabled", isOn ? 1 : 0);
        PlayerPrefs.Save();
        
        if (isOn && Application.platform == RuntimePlatform.Android)
        {
            Handheld.Vibrate();
        }
    }
    
    private void OnVolumeChanged(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }
    
    private void OnDeleteSaveClicked()
    {
        if (string.IsNullOrEmpty(savePath))
        {
            Debug.LogWarning("Save path is not initialized");
            return;
        }

        // Создаем подтверждение удаления (простая версия)
        #if UNITY_EDITOR
        bool confirmDelete = UnityEditor.EditorUtility.DisplayDialog(
            "Удаление сохранения",
            "Вы уверены, что хотите удалить сохранение?",
            "Да", "Нет");
        #else
        // В билде можно использовать свою систему диалогов
        bool confirmDelete = true; // Для теста, в реальном проекте заменить на вызов диалога
        #endif

        if (confirmDelete)
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log("Сохранение удалено: " + savePath);
                
                if (SaveLoadManager.Instance != null)
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(
                        UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                }
            }
            else
            {
                Debug.Log("Файл сохранения не найден: " + savePath);
            }
        }
    }
    
    private void LoadSettings()
    {
        bool vibrationEnabled = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;
        vibrationToggle.isOn = vibrationEnabled;
        
        float volume = PlayerPrefs.GetFloat("MasterVolume", 0.7f);
        volumeSlider.value = volume;
        AudioListener.volume = volume;
    }
}