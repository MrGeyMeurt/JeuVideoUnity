using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Teleportation : MonoBehaviour
{
    public float preTeleportDelay = 0.1f;
    public float postTeleportDelay = 0.1f;

    // References aux composants
    private GrapplingRaycast grapplingRaycast;
    private CharacterController charController;
    private Animator characterAnimator;
    private MonoBehaviour playerController;

    void Start()
    {
        // Recuperer les references
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

        // S'abonner aux evenements
        if (grapplingRaycast != null)
        {
            grapplingRaycast.OnGrapplingHit += HandleGrapplingHit;
        }
    }

    void OnDestroy()
    {
        // Se desabonner des evenements
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
        // Debuter la teleportation
        grapplingRaycast.StartTeleportation();

        // Desactiver les contreles
        DisableControls();

        // Attendre avant la teleportation
        yield return new WaitForSeconds(preTeleportDelay);

        // Desactiver temporairement le CharacterController
        bool controllerWasEnabled = false;
        if (charController && charController.enabled)
        {
            controllerWasEnabled = true;
            charController.enabled = false;
        }

        // Teleporter
        grapplingRaycast.Teleport();

        // Reactiver le CharacterController
        if (controllerWasEnabled && charController)
        {
            charController.enabled = true;
        }

        // Forcer le contact avec le sol
        ForceGroundContact();

        // Attendre apres la teleportation
        yield return new WaitForSeconds(postTeleportDelay);

        // Reactiver les contreles
        EnableControls();

        // Terminer la teleportation
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
        // Appliquer un petit mouvement vers le bas pour etablir le contact avec le sol
        if (charController && charController.enabled)
        {
            charController.Move(new Vector3(0, -0.1f, 0));
        }
    }
}