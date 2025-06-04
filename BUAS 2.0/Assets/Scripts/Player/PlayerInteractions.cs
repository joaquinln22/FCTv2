using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{
    public Transform startPosition;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("GunAmmo"))
        {
            GameManager.Instance.gunAmmo += other.gameObject.GetComponent<AmmoBox>().ammo;
            Destroy(other.gameObject);
        }

        if (other.gameObject.CompareTag("DeathFloor"))
        {
            // Perder vida, respawnear a nuestro player
            GameManager.Instance.LoseHealth(20);

            GetComponent<CharacterController>().enabled = false;
            gameObject.transform.position = startPosition.position;
            GetComponent<CharacterController>().enabled = true;
        }
    }
}
