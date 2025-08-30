using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<AI>().LooseLife(1);
        }
        else if (collision.gameObject.CompareTag("Diana"))
        {
            Destroy(collision.gameObject); // Destruye la diana
        }
    }
}
