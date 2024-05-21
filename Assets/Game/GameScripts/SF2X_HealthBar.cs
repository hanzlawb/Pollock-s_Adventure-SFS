using UnityEngine;

/**
 * This script is attached to Health Bar for remote Characters
 * It allows the automatic rotation towards the Player Camera
 */
public class SF2X_HealthBar : MonoBehaviour
{
    public GameObject[] remoteHealthStars;
    private Camera cam;
    private void Start()
    {
        cam = Camera.main;
    }
    /**
     * This is executed on a LateUpdate to sync with the transform movements
     */
    private void LateUpdate()
    {
        this.transform.LookAt(transform.position + cam.transform.rotation * Vector3.back, cam.transform.rotation * Vector3.up);
    }
}
