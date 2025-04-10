using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        // Scène 1 du jeu 
        SceneManager.LoadSceneAsync(1);
    }

    public void QuitGame()
    {
        // Quitte l'application
        Application.Quit();
    }

    public void LoadGameLevel()
{
    // Désactivez d'abord la caméra du menu si elle existe
    Camera menuCamera = Camera.main;
    if (menuCamera != null)
    {
        menuCamera.gameObject.SetActive(false);
    }
    
    // Chargez votre niveau
    SceneManager.LoadScene("Niveau");
}
}
