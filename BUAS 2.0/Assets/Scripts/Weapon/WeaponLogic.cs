using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponLogic : MonoBehaviour
{
    public Transform spawnPoint;

    public GameObject bullet;

    public float shotForce = 1500f;
    public float shotRate = 0.5f;

    private float shotRateTime = 0;
    private AudioSource audioSource;
    public AudioClip shotSound;
    public bool continueShooting = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && Time.timeScale != 0)
        {
            if (Time.time > shotRateTime && GameManager.Instance.gunAmmo > 0)
            {
                if (continueShooting)
                {
                    InvokeRepeating("Shoot", .001f, shotRate);
                }
                else
                {
                    Shoot();
                }

            }
            

        }else if (Input.GetButtonUp("Fire1") && continueShooting && Time.timeScale != 0)
        {
            CancelInvoke("Shoot");
        }
    }

    public void Shoot()
    {
        if (GameManager.Instance.gunAmmo > 0)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(shotSound);
            }

            GameManager.Instance.gunAmmo--;

            GameObject newBullet;
            newBullet = Instantiate(bullet, spawnPoint.position, spawnPoint.rotation);
            newBullet.GetComponent<Rigidbody>().AddForce(-spawnPoint.forward * shotForce);
            shotRateTime = Time.time + shotRate;
            Destroy(newBullet, 5);
        }
        else
        {
            CancelInvoke("Shoot");
        }
    }
}
