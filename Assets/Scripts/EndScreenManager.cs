using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject endScreenCanvas;
    [SerializeField] private float displayDuration = 4f;
    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private bool fadeIn = true;
    [SerializeField] private float fadeInDuration = 1f;
    
    private CanvasGroup canvasGroup;
    
    private void Awake()
    {
        // Canvas is disabled by default
        if (endScreenCanvas != null)
        {
            endScreenCanvas.SetActive(false);
            
            canvasGroup = endScreenCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null && fadeIn)
            {
                canvasGroup = endScreenCanvas.AddComponent<CanvasGroup>();
            }
        }
    }
    
    // Call the end screen when the game is over
    public void ShowEndScreen()
    {
        if (endScreenCanvas != null)
        {
            endScreenCanvas.SetActive(true);
            
            if (fadeIn && canvasGroup != null)
            {
                StartCoroutine(FadeInCanvas());
            }
            
            // Couroutine to return to the menu after a delay
            StartCoroutine(ReturnToMenuAfterDelay());
        }
        else
        {
            Debug.LogError("EndScreenCanvas non assigné dans l'inspecteur!");
        }
    }
    
    private IEnumerator FadeInCanvas()
    {
        canvasGroup.alpha = 0f;
        
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            canvasGroup.alpha = elapsedTime / fadeInDuration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    private IEnumerator ReturnToMenuAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        
        SceneManager.LoadScene(menuSceneName);
    }
    
    public void TriggerEndScreen()
    {
        ShowEndScreen();
    }
}