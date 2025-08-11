using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    public Camera thirdPersonCamera;
    public Camera firstPersonCamera;
    private bool firstPersonEnabled = true;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            firstPersonEnabled = !firstPersonEnabled;
            ChangeCamera();
        }
    }

    public void ChangeCamera()
    {
        if (firstPersonEnabled)
        {
            firstPersonCamera.enabled = true;
            thirdPersonCamera.enabled = false;
        }
        else
        {
            firstPersonCamera.enabled = false;
            thirdPersonCamera.enabled = true;
        }
    }
}
