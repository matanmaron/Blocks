using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;
using System;

public class TitleMenu : MonoBehaviour
{
    public GameObject mainMenuObject;
    public GameObject settingsObject;
    
    [Header("Main MenuUI Elements")]
    public TextMeshProUGUI seedField;

    [Header("Settings MenuUI Elements")]
    public Slider viewDistSlider;
    public TextMeshProUGUI viewDistText;
    public Slider mouseSlider;
    public TextMeshProUGUI mouseText;
    public Toggle threadingToggle;
    public Toggle chunkAnimToggle;

    Settings settings;

    private void Awake()
    {
        settings = new Settings();
        if (!File.Exists(Application.dataPath+"/settings.dat"))
        {
            Debug.LogWarning("No Settings File, Reverting To Default");
            ExportSettings();
        }
        else
        {
            Debug.Log("Found Settings File, Loading File");
            ImportSettings();
        }
    }

    public void StartGame()
    {
        VoxelData.seed = Mathf.Abs(seedField.text.GetHashCode()) / VoxelData.WorldSizeInChunks;
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void EnterSettings()
    {
        viewDistSlider.value = settings.ViewDistance;
        mouseSlider.value = settings.MouseSensitivity;
        UpdateViewDistSlider();
        UpdateMouseSlider();

        threadingToggle.isOn = settings.EnableThreading;
        chunkAnimToggle.isOn = settings.EnableAnimatedChunks;

        mainMenuObject.SetActive(false);
        settingsObject.SetActive(true);
    }

    public void LeaveSettings()
    {
        settings.ViewDistance = (int)viewDistSlider.value;
        settings.MouseSensitivity = mouseSlider.value;
        settings.EnableThreading = threadingToggle.isOn;
        settings.EnableAnimatedChunks = chunkAnimToggle.isOn;
        ExportSettings();
        mainMenuObject.SetActive(true);
        settingsObject.SetActive(false);
    }
    public void UpdateViewDistSlider()
    {
        viewDistText.text = $"View Distance: {viewDistSlider.value}";
    }

    public void UpdateMouseSlider()
    {
        mouseText.text = $"Mouse Sensitivity : {mouseSlider.value:F1}";
    }

    void ExportSettings()
    {
        string jsonExport = JsonUtility.ToJson(settings);
        File.WriteAllText(Application.dataPath + "/settings.dat", jsonExport);
    }

    void ImportSettings()
    {
        string jsonImport = File.ReadAllText(Application.dataPath + "/settings.dat");
        settings = JsonUtility.FromJson<Settings>(jsonImport);
    }
}
