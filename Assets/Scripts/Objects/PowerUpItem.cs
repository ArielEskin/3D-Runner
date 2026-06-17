using UnityEngine;

public class PowerUpItem : MonoBehaviour
{
    public PowerUpData powerUpData; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SoundManager.instance.PlaySound3D("PickUpCoin", transform.position); // add a powerup sound!

            GameManager.gameManager.ActivatePowerUp(powerUpData);
            
            ObjectPooler.Instance.ReturnToPool(gameObject);
        }
    }
}