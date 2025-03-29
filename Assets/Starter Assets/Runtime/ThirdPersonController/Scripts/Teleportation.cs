using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Teleportation : MonoBehaviour
{
    public float preTeleportDelay = 0.1f;
    public float postTeleportDelay = 0.1f;

    // Références aux composants
    private GrapplingRaycast grapplingRaycast;
    private CharacterController charController;
    private Animator characterAnimator;
    private MonoBehaviour playerController;

    void Start()
    {
        // Récupérer les références
        grapplingRaycast = GetComponent<GrapplingRaycast>();
        charController = GetComponent<CharacterController>();
        characterAnimator = GetComponentInChildren<Animator>();

        // Trouver le controller du joueur
        foreach (MonoBehaviour script in GetComponents<MonoBehaviour>())
        {
            if (script != this && script != grapplingRaycast && script.GetType().Name.Contains("Controller"))
            {
                playerController = script;
                break;
            }
        }

        // S'abonner aux événements
        if (grapplingRaycast != null)
        {
            grapplingRaycast.OnGrapplingHit += HandleGrapplingHit;
        }
    }

    void OnDestroy()
    {
        // Se désabonner des événements
        if (grapplingRaycast != null)
        {
            grapplingRaycast.OnGrapplingHit -= HandleGrapplingHit;
        }
    }

    void HandleGrapplingHit(Vector3 hitPoint)
    {
        StartCoroutine(TeleportSequence());
    }

    IEnumerator TeleportSequence()
    {
        // Débuter la téléportation
        grapplingRaycast.StartTeleportation();

        // Désactiver les contrôles
        DisableControls();

        // Attendre avant la téléportation
        yield return new WaitForSeconds(preTeleportDelay);

        // Désactiver temporairement le CharacterController
        bool controllerWasEnabled = false;
        if (charController && charController.enabled)
        {
            controllerWasEnabled = true;
            charController.enabled = false;
        }

        // Téléporter
        grapplingRaycast.Teleport();

        // Réactiver le CharacterController
        if (controllerWasEnabled && charController)
        {
            charController.enabled = true;
        }

        // Forcer le contact avec le sol
        ForceGroundContact();

        // Attendre après la téléportation
        yield return new WaitForSeconds(postTeleportDelay);

        // Réactiver les contrôles
        EnableControls();

        // Terminer la téléportation
        grapplingRaycast.FinishTeleportation();
    }

    void DisableControls()
    {
        if (playerController)
            playerController.enabled = false;

        if (characterAnimator)
            characterAnimator.speed = 0;
    }

    void EnableControls()
    {
        if (playerController)
            playerController.enabled = true;

        if (characterAnimator)
            characterAnimator.speed = 1;
    }

    void ForceGroundContact()
    {
        // Appliquer un petit mouvement vers le bas pour établir le contact avec le sol
        if (charController && charController.enabled)
        {
            charController.Move(new Vector3(0, -0.1f, 0));
        }
    }
}