using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEndTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {   
            // Le joueur entre dans la zone de fin => on passe au niveau suivant
            Debug.Log("Plateforme d'arrivée atteinte !");
            LevelManager.Instance.LoadNextLevel();
        }
    }
}
