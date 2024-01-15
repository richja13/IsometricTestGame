using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float CameraSpeed = 8f;
    [SerializeField]
    private Vector3 CameraOffset;
    public static Transform target;

    void Update()
    {
        if (target is null) return; 
        FollowTarger();
    }

    void FollowTarger()
    {
        Vector3 gameObjectPosition = new Vector3(target.position.x + CameraOffset.x, target.position.y + CameraOffset.y, target.position.z - CameraOffset.z);
        transform.position = Vector3.Lerp(transform.position, gameObjectPosition, CameraSpeed * Time.deltaTime);
    }
}
