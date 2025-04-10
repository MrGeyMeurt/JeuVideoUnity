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
        // S'assurer que le menu est caché au démarrage
        if (pauseScreen != null)
            pauseScreen.SetActive(false);
    }
    
    private void Update()
    {
        // Vérifier l'appui sur la touche P
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }
    
    public void TogglePause()
    {
        isPaused = !isPaused;
        if (isPaused)
            Pause();
        else
            Resume();
    }
    
    public void Pause()
    {
        // Debug.Log("Pause activée");
        Time.timeScale = 0f; 
        if (pauseScreen != null)
            pauseScreen.SetActive(true); 
        isPaused = true;
    }
    
    public void Home() 
    {
        Time.timeScale = 1f; // Rétablir le temps normal
        SceneManager.LoadScene("Main Menu");
    }
    
    public void Resume()
    {
        Debug.Log("Resume game");
        Time.timeScale = 1f; // Reprend le temps
        if (pauseScreen != null)
            pauseScreen.SetActive(false); // Cache le menu de pause
        isPaused = false;
    }
    
    public void QuitGame()
    {
        Debug.Log("Exit game");
        Application.Quit(); // Quitte l'application
    }
}