using UnityEngine;
using System.Collections;

public class CameraFollow2D : MonoBehaviour
{
    public float followDistance;
    public GameObject target;
    public Vector3 offset;
    void LateUpdate()
    {
        if (target)
        {
            Vector3 targetPosition = new Vector3(target.transform.position.x, target.transform.position.y, transform.position.z);
            transform.position = targetPosition + offset;
        }
    }
}
