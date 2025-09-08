using UnityEngine;

public class GameplayTimer : MonoBehaviour
{
    public static GameplayTimer Instance { get; private set; }

    public float ElapsedSeconds { get; private set; } = 0f;
    public bool Running { get; private set; } = true;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // Si esta escena solo es de juego, no hace falta DontDestroyOnLoad();
    }

    private void Update()
    {
        // timeScale=0 (pausa) => deltaTime=0, así no cuenta tiempo pausado
        if (Running) ElapsedSeconds += Time.deltaTime;
    }

    public void ResetAndStart()
    {
        ElapsedSeconds = 0f;
        Running = true;
    }

    public void Stop()
    {
        Running = false;
    }
}
