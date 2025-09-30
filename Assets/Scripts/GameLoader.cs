using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameLoader : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(DelayLoad());
    }

    IEnumerator DelayLoad()
    {
        yield return null;
        yield return null;
        if (PlayerPrefs.GetInt("LoadGame", 0) == 1)
        {
            PlayerPrefs.SetInt("LoadGame", 0);
            string path = PlayerPrefs.GetString("SaveToLoad", "");
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                SaveManager.LoadGame(path);
            }
            else
            {
                Debug.Log("저장경로가 유효하지않음");
            }
        }
    }

}
