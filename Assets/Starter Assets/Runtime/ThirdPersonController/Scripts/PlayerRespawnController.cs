using UnityEngine;

public class PlayerRespawnController : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterController controller = other.GetComponent<CharacterController>();
            
            if (controller != null)
            {
                controller.enabled = false;
                other.transform.position = respawnPoint.position;
                other.transform.rotation = respawnPoint.rotation;
                controller.enabled = true;
            }
            else
            {
                other.transform.position = respawnPoint.position;
                other.transform.rotation = respawnPoint.rotation;
            }
        }
    }
}