using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveUIManager : MonoBehaviour
{
    public GameObject saveMenue;
    public GameObject exitMenue;
    
    void Start()
    {
        saveMenue.gameObject.SetActive(false);
        exitMenue.gameObject.SetActive(false);
    }

    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            saveMenue.SetActive(!saveMenue.activeSelf);
            Cursor.lockState = saveMenue.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = saveMenue.activeSelf;
        }
    }
    public void OnSaveButtonClicked()
    {
        SaveManager.SaveGame();
    }
    public void OnBeforeExitButtonClicked()
    {
        exitMenue.gameObject.SetActive(true);
    }
    public void OnExitButtonClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
