using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
using UnityEngine;

public class EnemyRespawnManager : MonoBehaviour
{
    public static EnemyRespawnManager Instance { get; private set; }

    [Header("UI")]
    public BossAnnouncementUI bossUI;

    private bool finalBossAnnounced = false;

    [Header("Victory")]
    public AudioSource musicSource;      // arrástrale un AudioSource
    public AudioClip victorySong;

    private bool gameWon = false;

    [System.Serializable]
    private class EnemyEntry
    {
        public GameObject enemy;
        public Vector3 spawnPos;
        public Quaternion spawnRot;
        public int baseLife;
        public int respawnsUsed = 0;     // 0..3 (3 respawns)
        public bool finished = false;    // cuando ya no puede reaparecer más
    }

    private readonly List<EnemyEntry> entries = new List<EnemyEntry>();

    private const int MaxRespawns = 0;
    private const float RespawnDelay = 10f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [System.Obsolete]
    private void Start()
    {
        // Auto-registro: toma todos los enemigos que ya tengas en escena
        var allEnemies = FindObjectsOfType<AI>(true); // incluye inactivos por si acaso
        foreach (var ai in allEnemies)
        {
            if (ai.isBoss) continue; // 👈 ignorar jefes

            var go = ai.gameObject;
            entries.Add(new EnemyEntry
            {
                enemy = go,
                spawnPos = go.transform.position,
                spawnRot = go.transform.rotation,
                baseLife = ai.lifes
            });
        }
        if (victorySong != null) victorySong.LoadAudioData(); // precarga para cero latencia
    }

    /// Llamado por AI cuando su vida llega a 0.
    public void EnemyKilled(GameObject enemyGO)
    {
        var e = entries.FirstOrDefault(x => x.enemy == enemyGO);
        if (e == null) return;

        // Desactivar instantáneamente al morir
        enemyGO.SetActive(false);

        if (e.respawnsUsed < MaxRespawns)
        {
            e.respawnsUsed++;
            Debug.Log($"[Respawn] {enemyGO.name} muerto. Respawn {e.respawnsUsed}/{MaxRespawns}");
            StartCoroutine(RespawnAfterDelay(e, RespawnDelay));
        }
        else
        {
            // Ya no puede reaparecer más
            e.finished = true;
            Debug.Log($"[Respawn] {enemyGO.name} ya no puede reaparecer (terminado).");
        }

        CheckAllFinished();
    }

    private IEnumerator RespawnAfterDelay(EnemyEntry e, float seconds)
    {
        yield return new WaitForSeconds(seconds);

        // 1) Reactivar primero
        e.enemy.SetActive(true);

        // Recolocar transform
        e.enemy.transform.SetPositionAndRotation(e.spawnPos, e.spawnRot);

        // Resetear stats y navegación
        var ai = e.enemy.GetComponent<AI>();
        if (ai != null)
        {
            ai.lifes = e.baseLife;   // vida base de tu prefab/escena
            ai.ResetForRespawn();    // re-apuntar a su primer waypoint, etc.
        }
    }

    private void CheckAllFinished()
    {
        // Si TODOS terminaron (sin respawns restantes) y ninguno está activo → Final Boss
        bool allFinished = entries.All(x => x.finished);
        bool anyActive = entries.Any(x => x.enemy.activeSelf);

        if (allFinished && !anyActive && !finalBossAnnounced)
        {
            finalBossAnnounced = true;
            Debug.Log("Final Boss");
            if (bossUI != null)
            {
                bossUI.Show("Get ready for the boss!", 2.5f);
            }

            StartCoroutine(SpawnBossAfterDelay(2.5f));
        }
    }

    [Header("Boss Config")]
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;

    [Header("Boss Path")]
    public Transform[] bossWaypoints; // <- arrastra aquí los waypoints del Boss (en orden)

    private IEnumerator SpawnBossAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (bossPrefab == null || bossSpawnPoint == null)
        {
            Debug.LogWarning("No se asignó prefab o spawnPoint del Boss.");
            yield break;
        }

        var bossRoot = Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);

        // 👇 Coger componentes en hijos (tu AI y Agent están en FinalBoss/Enemy)
        var ai = bossRoot.GetComponentInChildren<AI>(true);
        var agent = bossRoot.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>(true);
        var shoot = bossRoot.GetComponentInChildren<EnemyShoot>(true);

        if (ai == null)
        {
            Debug.LogError("[BossSpawn] No se encontró AI en el prefab del Boss (ni en hijos).");
            yield break;
        }

        // Flag boss para que el respawn manager lo ignore
        ai.isBoss = true;

        // Pasar waypoints (filtrando nulos)
        if (bossWaypoints != null && bossWaypoints.Length > 0)
        {
            var list = new List<Transform>();
            foreach (var t in bossWaypoints) if (t != null) list.Add(t);
            ai.destinations = list.ToArray();
        }

        // Reactivar IA por si Start la desactivó
        ai.enabled = true;

        // Encajar en NavMesh y fijar primer destino
        if (agent != null)
        {
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(bossSpawnPoint.position, out hit, 2f, UnityEngine.AI.NavMesh.AllAreas))
                agent.Warp(hit.position);

            if (ai.destinations != null && ai.destinations.Length > 0)
                agent.SetDestination(ai.destinations[0].position);
        }

        // Relanzar disparos del boss si los tiene
        if (shoot != null)
        {
            shoot.CancelInvoke();
            shoot.enabled = true;
            shoot.Invoke("ShootPlayer", 2f);
        }
    }
    
    // Llamado cuando muere el jefe (desde AI.cs)
    public void OnBossDefeated()
    {
        if (gameWon) return;
        gameWon = true;

        // ⏩ SONAR YA MISMO
        if (musicSource != null && victorySong != null)
        {
            musicSource.PlayOneShot(victorySong); // arranca en el mismo frame
        }

        // parar el cronómetro para fijar la marca
        if (GameplayTimer.Instance != null) GameplayTimer.Instance.Stop();

        StartCoroutine(WinSequence());
    }

    private System.Collections.IEnumerator WinSequence()
    {
        // 1) “Good Game” + música
        if (bossUI != null) bossUI.Show("GOOD GAME", 3f);

        // Espera real 3s, aunque cambies timeScale
        yield return new WaitForSecondsRealtime(3f);

        // 2) “Your record” + tiempo formateado
        float t = (GameplayTimer.Instance != null) ? GameplayTimer.Instance.ElapsedSeconds : Time.timeSinceLevelLoad;
        string record = FormatTime(t);
        if (bossUI != null) bossUI.Show($"YOUR RECORD\n{record}", 4f);
    }

    // mm:ss.ms (ej: 07:13.42)
    private string FormatTime(float seconds)
    {
        int min = Mathf.FloorToInt(seconds / 60f);
        int sec = Mathf.FloorToInt(seconds % 60f);
        int cs  = Mathf.FloorToInt((seconds - Mathf.Floor(seconds)) * 100f); // centésimas
        return $"{min:00}:{sec:00}.{cs:00}";
    }

}
