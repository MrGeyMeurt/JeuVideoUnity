using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    private void Start()
    {
        Cursor.visible = true; 
        Cursor.lockState = CursorLockMode.Locked; // Verrouille le curseur au centre de l'écran
        Debug.Log("GameManager started. Cursor locked.");
    }


}
