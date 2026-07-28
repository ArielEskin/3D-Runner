using UnityEngine;

public class PowerUpItem : MonoBehaviour
{
    public PowerUpData powerUpData; 

    private void OnTriggerEnter(Collider other) // Sends the specific power-up data to the GameManager, plays a sound, and despawns the item
    {
        if (other.CompareTag("Player"))
        {
            SoundManager.instance.PlaySound3D("PowerUPSound", transform.position);

            GameManager.gameManager.ActivatePowerUp(powerUpData);
            
            ObjectPooler.Instance.ReturnToPool(gameObject);
        }
    }
}