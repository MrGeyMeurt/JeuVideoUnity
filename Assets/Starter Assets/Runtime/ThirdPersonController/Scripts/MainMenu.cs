using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private void Start()
    {
        Cursor.visible = true; 
        Cursor.lockState = CursorLockMode.None; 
    }
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
