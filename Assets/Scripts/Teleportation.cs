using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Teleportation : MonoBehaviour
{
    public float preTeleportDelay = 0.1f;
    public float postTeleportDelay = 0.1f;

    private GrapplingRaycast grapplingRaycast;
    private CharacterController charController;
    private Animator characterAnimator;
    private MonoBehaviour playerController;

    void Start()
    {
        // Find the grappling raycast component
        grapplingRaycast = GetComponent<GrapplingRaycast>();
        charController = GetComponent<CharacterController>();
        characterAnimator = GetComponentInChildren<Animator>();

        // Find the player controller script
        foreach (MonoBehaviour script in GetComponents<MonoBehaviour>())
        {
            if (script != this && script != grapplingRaycast && script.GetType().Name.Contains("Controller"))
            {
                playerController = script;
                break;
            }
        }

        if (grapplingRaycast != null)
        {
            grapplingRaycast.OnGrapplingHit += HandleGrapplingHit;
        }
    }

    void OnDestroy()
    {
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
        grapplingRaycast.StartTeleportation();

        DisableControls();

        yield return new WaitForSeconds(preTeleportDelay);

        bool controllerWasEnabled = false;
        if (charController && charController.enabled)
        {
            controllerWasEnabled = true;
            charController.enabled = false;
        }

        // Teleporting
        grapplingRaycast.Teleport();

        if (controllerWasEnabled && charController)
        {
            charController.enabled = true;
        }

        ForceGroundContact();

        yield return new WaitForSeconds(postTeleportDelay);

        EnableControls();

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
        // Apply a small downward force to ensure the character is grounded
        if (charController && charController.enabled)
        {
            charController.Move(new Vector3(0, -0.1f, 0));
        }
    }
}