using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    // Transform of the camera to shake. Grabs the gameObject's transform
    // if null.
    public Transform camTransform;

    // How long the object should shake for.
    public float shakeDuration = 0f;

    // Amplitude of the shake. A larger value shakes the camera harder.
    public float shakeAmount = 0.7f;
    public float decreaseFactor = 1.0f;

    private float timer;
    Vector3 originalPos;

    void Awake()
    {
        if (camTransform == null)
        {
            camTransform = GetComponent(typeof(Transform)) as Transform;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            shakeCamera();
        }
    }

    public void shakeCamera()
    {
        StopCoroutine(nameof(shakeCoroutine));
        StartCoroutine(nameof(shakeCoroutine));
    }

    IEnumerator shakeCoroutine()
    {
        timer = 0;
        while (enabled)
        {
            timer += Time.deltaTime;
            camTransform.localPosition = camTransform.localPosition + Random.insideUnitSphere * shakeAmount;

            if (timer >= shakeDuration)
            {
                StopCoroutine(nameof(shakeCoroutine));
            }

            yield return null;
        }
    }
}