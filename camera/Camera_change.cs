using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_change : MonoBehaviour
{
    // Start is called before the first frame update
    private Camera targetCamera;
    public Camera previousCamera;

    private void Start()
    {
        targetCamera = GetComponent<Camera>();
        targetCamera.enabled = false;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("playerTag"))
        {
            if (previousCamera != null)
                previousCamera.enabled = false;

            targetCamera.enabled = true;
        }
    }
}
