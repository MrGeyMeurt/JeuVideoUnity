using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerReset : MonoBehaviour
{
    // Référence au script Teleportation
    private Teleportation teleportation;

    // Type du contrôleur pour le recréer
    private System.Type controllerType;

    // Référence au CharacterController
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
                // Stocker son type pour pouvoir le recréer
                controllerType = script.GetType();
                break;
            }
        }

        // S'abonner à l'événement de fin de téléportation
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
        // Se désabonner des événements
        var grapplingRaycast = GetComponent<GrapplingRaycast>();
        if (grapplingRaycast != null)
        {
            grapplingRaycast.OnTeleportComplete -= ResetController;
        }
    }

    // Réinitialiser le contrôleur après la téléportation
    void ResetController()
    {
        StartCoroutine(ResetControllerRoutine());
    }

    IEnumerator ResetControllerRoutine()
    {
        if (controllerType != null)
        {
            // Rechercher le contrôleur existant
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
                // Désactiver temporairement
                bool wasEnabled = controller.enabled;
                controller.enabled = false;

                // Attendre une frame
                yield return null;

                // Réactiver
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

    // Méthode publique pour force la réinitialisation
    public void ForceReset()
    {
        StartCoroutine(ResetControllerRoutine());
    }
}