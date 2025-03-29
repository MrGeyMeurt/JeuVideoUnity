using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Teleportation : MonoBehaviour
{
    public float preTeleportDelay = 0.1f;
    public float postTeleportDelay = 0.1f;

    // R�f�rences aux composants
    private GrapplingRaycast grapplingRaycast;
    private CharacterController charController;
    private Animator characterAnimator;
    private MonoBehaviour playerController;

    void Start()
    {
        // R�cup�rer les r�f�rences
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

        // S'abonner aux �v�nements
        if (grapplingRaycast != null)
        {
            grapplingRaycast.OnGrapplingHit += HandleGrapplingHit;
        }
    }

    void OnDestroy()
    {
        // Se d�sabonner des �v�nements
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
        // D�buter la t�l�portation
        grapplingRaycast.StartTeleportation();

        // D�sactiver les contr�les
        DisableControls();

        // Attendre avant la t�l�portation
        yield return new WaitForSeconds(preTeleportDelay);

        // D�sactiver temporairement le CharacterController
        bool controllerWasEnabled = false;
        if (charController && charController.enabled)
        {
            controllerWasEnabled = true;
            charController.enabled = false;
        }

        // T�l�porter
        grapplingRaycast.Teleport();

        // R�activer le CharacterController
        if (controllerWasEnabled && charController)
        {
            charController.enabled = true;
        }

        // Forcer le contact avec le sol
        ForceGroundContact();

        // Attendre apr�s la t�l�portation
        yield return new WaitForSeconds(postTeleportDelay);

        // R�activer les contr�les
        EnableControls();

        // Terminer la t�l�portation
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
        // Appliquer un petit mouvement vers le bas pour �tablir le contact avec le sol
        if (charController && charController.enabled)
        {
            charController.Move(new Vector3(0, -0.1f, 0));
        }
    }
}