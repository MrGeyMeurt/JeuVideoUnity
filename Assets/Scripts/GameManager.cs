using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    private void Start()
    {
        Cursor.visible = true; 
        Cursor.lockState = CursorLockMode.Locked; 
        Debug.Log("GameManager started. Cursor locked.");
    }


}
