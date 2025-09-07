using UnityEngine;
using UnityEngine.Audio;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    public void FullScreen(bool pantallaCompleta)
    {
        Screen.fullScreen = pantallaCompleta;
    }

    public void ChangeVolume(float volumen)
    {
        audioMixer.SetFloat("Volume", volumen);
    }
}
