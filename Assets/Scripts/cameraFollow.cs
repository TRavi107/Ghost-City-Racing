using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraFollow : MonoBehaviour
{
    public GameObject player;
    public Transform lookAt;
    public float speed;
    // Start is called before the first frame update
    void Awake()
    {
        this.transform.parent = null;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, lookAt.position, Time.deltaTime * speed);
        Quaternion targetAngle = Quaternion.Euler(transform.rotation.eulerAngles.x,
                                                   lookAt.rotation.eulerAngles.y,
                                                   transform.rotation.eulerAngles.z);
        // Dampen towards the target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetAngle, Time.deltaTime * speed);
    }
}
