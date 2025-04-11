using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseScreen;
    private bool isPaused = false;
    
    private void Start()
    {
        if (pauseScreen != null)
            pauseScreen.SetActive(false);
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }
    
    public void TogglePause()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            Pause();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Resume();
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    
    public void Pause()
    {
        Time.timeScale = 0f; 
        if (pauseScreen != null)
            pauseScreen.SetActive(true); 
        isPaused = true;
    }
    
    public void Home() 
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("MainMenu");
    }
    
    public void Resume()
    {
        Debug.Log("Resume game");
        Time.timeScale = 1f; 
        if (pauseScreen != null)
            pauseScreen.SetActive(false); 
        isPaused = false;
    }
    
    public void QuitGame()
    {
        Debug.Log("Exit game");
        Application.Quit(); 
    }
}