using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System;
using TMPro;

public class SceneChanger : MonoBehaviour
{
    public GameObject saveListPanel;
    public GameObject saveSlotButton;
    public Transform contentParent;
    private void Start()
    {
        saveListPanel.SetActive(false);
    }
    private void Update()
    {
        if (saveListPanel.activeSelf == true)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                saveListPanel.SetActive(false);
            }
        }
    }
    public void OnESCButtonClicked()
    {
        saveListPanel.SetActive(false);
    }
    public void OnNewGameClicked()
    {
        PlayerPrefs.SetInt("LoadGame", 0);
        SceneManager.LoadScene("SampleScene");
    }
    public void OnContinueClicked()
    {
        saveListPanel.SetActive(true);
        PopulateSaveButtons();
    }
    private void PopulateSaveButtons()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        string[] saveFiles = Directory.GetFiles(Application.persistentDataPath, "save_*.json");
        Array.Reverse(saveFiles);
        foreach (string file in saveFiles)
        {
            string fileName = Path.GetFileName(file);
            DateTime lastWirteTime = File.GetLastWriteTime(file);
            string displayName = $"{lastWirteTime:yyyy-MM-dd HH:mm:ss}";
            GameObject btnObj = Instantiate(saveSlotButton, contentParent);
            btnObj.GetComponentInChildren<TextMeshProUGUI>().text = displayName;
            string capturedFilePath = file;
            btnObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                PlayerPrefs.SetString("SaveToLoad", capturedFilePath);
                PlayerPrefs.SetInt("LoadGame", 1);
                SceneManager.LoadScene("SampleScene");
            });
        }
    }
        public void OnExitButtonClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}