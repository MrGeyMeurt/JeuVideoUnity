using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadGameLevel()
{
    Camera menuCamera = Camera.main;
    if (menuCamera != null)
    {
        menuCamera.gameObject.SetActive(false);
    }
    
    SceneManager.LoadScene("Level");
}
}
