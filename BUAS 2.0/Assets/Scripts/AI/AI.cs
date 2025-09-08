using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;

    public Transform[] destinations;

    public float distanceToFollowPlath = 2;

    private int i = 0;

    [Header("-------FollowPlayer?-------")]

    public bool followPlayer;

    private GameObject player;

    private float distanceToPlayer;

    public float distanceToFollowPlayer = 10;

    public int lifes = 5;

    [Header("Tipo de unidad")]
    public bool isBoss = false;   // 🔑 Marcar en el prefab del boss

    [System.Obsolete]
    void Start()
    {
        // Buscar el primer waypoint válido
        int first = -1;
        if (destinations != null)
        {
            for (int k = 0; k < destinations.Length; k++)
            {
                if (destinations[k] != null) { first = k; break; }
            }
        }

        if (first == -1)
        {
            // Sin waypoints válidos (prefab recién instanciado, etc.)
            // Desactiva IA temporalmente; el manager puede reactivarla tras asignar destinos
            GetComponent<AI>().enabled = false;
            Debug.LogWarning($"[AI] {name} no tiene waypoints válidos asignados.");
            return;
        }

        // Arrancar navegación
        i = first;
        navMeshAgent.destination = destinations[i].position;

        // Referencia al player
        player = FindObjectOfType<PlayerMovement>().gameObject;
    }


    void Update()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= distanceToFollowPlayer && followPlayer)
        {
            FollowPlayer();
        }
        else
        {
            EnemyPath();
        }
    }

    public void EnemyPath()
    {
        navMeshAgent.destination = destinations[i].position;

        if (Vector3.Distance(transform.position, destinations[i].position) <= distanceToFollowPlath)
        {
            if (destinations[i] != destinations[destinations.Length - 1])
            {
                i++;
            }
            else
            {
                i = 0;
            }
        }
    }

    public void FollowPlayer()
    {
        navMeshAgent.destination = player.transform.position;
    }

    public void GrenadeImpact()
    {
        LooseLife(3);
    }

    public void LooseLife(int lifesToLoose)
    {
        lifes = lifes - lifesToLoose;

        if (lifes <= 0)
        {
            if (isBoss)
            {
                // --- MUERTE DEL JEFE ---
                // (No avisar al respawn manager)
                // Cancela disparos si los hubiera y destruye o lanza victoria, etc.
                var shoot = GetComponent<EnemyShoot>();
                if (shoot != null) shoot.CancelInvoke();

                // 👇 Notificar fin del juego
                if (EnemyRespawnManager.Instance != null)
                    EnemyRespawnManager.Instance.OnBossDefeated();

                Destroy(gameObject);
                return;
            }
            else
            {
                // AVISAR al manager y NO destruir
                if (EnemyRespawnManager.Instance != null)
                {
                    EnemyRespawnManager.Instance.EnemyKilled(gameObject);
                }
                else
                {
                    gameObject.SetActive(false); // fallback simple
                }

                // Desactivar scripts para que no disparen ni se muevan
                GetComponent<AI>().enabled = false;
                var shoot = GetComponent<EnemyShoot>();
                if (shoot != null)
                {
                    shoot.CancelInvoke();   // cancela cualquier Invoke/InvokeRepeating
                    shoot.enabled = false;  // deshabilita el script
                }

                if (navMeshAgent != null) navMeshAgent.enabled = false;
            }
        }
    }

    // --- NUEVO: llamada desde el manager al respawnear ---
    public void ResetForRespawn()
    {
        // Reactivar este script (IA)
        this.enabled = true;

        // Reactivar disparos si tiene el componente
        var shoot = GetComponent<EnemyShoot>();
        if (shoot != null)
        {
            shoot.enabled = true;
            shoot.CancelInvoke();              // por si acaso trae invokes viejos
            shoot.Invoke("ShootPlayer", 2f);   // volver a iniciar el ciclo de disparo
        }

        // Asegurar que el NavMeshAgent existe y está activo
        if (navMeshAgent == null) navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.enabled = true;

        // Colocar correctamente al agente en el NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 2f, NavMesh.AllAreas))
        {
            navMeshAgent.Warp(hit.position);  // Encaja en el NavMesh
        }

        // Reiniciar índice de waypoint y fijar destino inicial
        i = 0;
        if (destinations != null && destinations.Length > 0)
        {
            navMeshAgent.SetDestination(destinations[0].position);
        }
    }


}
