using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerReset : MonoBehaviour
{
    // R�f�rence au script Teleportation
    private Teleportation teleportation;

    // Type du contr�leur pour le recr�er
    private System.Type controllerType;

    // R�f�rence au CharacterController
    private CharacterController charController;

    void Start()
    {
        teleportation = GetComponent<Teleportation>();
        charController = GetComponent<CharacterController>();

        // Trouver le ThirdPersonController
        foreach (MonoBehaviour script in GetComponents<MonoBehaviour>())
        {
            if (script.GetType().Name.Contains("Controller") &&
                script.GetType().Name != "CharacterController" &&
                !(script is Teleportation) &&
                !(script is GrapplingRaycast) &&
                !(script is ControllerReset))
            {
                // Stocker son type pour pouvoir le recr�er
                controllerType = script.GetType();
                break;
            }
        }

        // S'abonner � l'�v�nement de fin de t�l�portation
        if (teleportation != null)
        {
            var grapplingRaycast = GetComponent<GrapplingRaycast>();
            if (grapplingRaycast != null)
            {
                grapplingRaycast.OnTeleportComplete += ResetController;
            }
        }
    }

    void OnDestroy()
    {
        // Se d�sabonner des �v�nements
        var grapplingRaycast = GetComponent<GrapplingRaycast>();
        if (grapplingRaycast != null)
        {
            grapplingRaycast.OnTeleportComplete -= ResetController;
        }
    }

    // R�initialiser le contr�leur apr�s la t�l�portation
    void ResetController()
    {
        StartCoroutine(ResetControllerRoutine());
    }

    IEnumerator ResetControllerRoutine()
    {
        if (controllerType != null)
        {
            // Rechercher le contr�leur existant
            MonoBehaviour controller = null;
            foreach (MonoBehaviour script in GetComponents<MonoBehaviour>())
            {
                if (script.GetType() == controllerType)
                {
                    controller = script;
                    break;
                }
            }

            if (controller != null)
            {
                // D�sactiver temporairement
                bool wasEnabled = controller.enabled;
                controller.enabled = false;

                // Attendre une frame
                yield return null;

                // R�activer
                if (wasEnabled)
                {
                    controller.enabled = true;
                }

                // Forcer le contact avec le sol
                if (charController && charController.enabled)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        charController.Move(new Vector3(0, -0.05f, 0));
                        yield return null;
                    }
                }
            }
        }
    }

    // M�thode publique pour force la r�initialisation
    public void ForceReset()
    {
        StartCoroutine(ResetControllerRoutine());
    }
}