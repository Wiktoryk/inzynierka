using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenManager : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }
    
    public void loadStartScene()
    {
        SceneManager.LoadScene("Scenes/Start");
    }

    public void loadInfo()
    {
        SceneManager.LoadScene("Scenes/Instrukcja");
    }
}
