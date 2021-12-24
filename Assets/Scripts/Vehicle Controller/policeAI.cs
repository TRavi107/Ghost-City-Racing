using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum mode
{
    chase,
    engaged,
    fallBack,
}

public class policeAI : MonoBehaviour
{
    //Public Properties
    public List<Transform> checkPoints =new List<Transform>();
    public controllerDr controller;
    public mode currentMode;
    //NonSerielized Fields


    //Private Fields
    int targetCheckPoint;
    // Start is called before the first frame update
    void Start()
    {
        controller.AIControlled = true;
        targetCheckPoint = 0;
        currentMode = mode.chase;
        
    }
    // Update is called once per frame
    void Update()
    {
        if (currentMode == mode.chase)
        {
            controller.horizontal = transform.InverseTransformPoint(checkPoints[targetCheckPoint].transform.position).x /
                transform.InverseTransformPoint(checkPoints[targetCheckPoint].transform.position).magnitude;

            if (Vector3.Distance(transform.position, checkPoints[targetCheckPoint].position) > 5)
            {
                controller.vertical = 1;
            }
            else
            {
                if (targetCheckPoint == checkPoints.Count - 1)
                    targetCheckPoint = 0;
                targetCheckPoint++;
            }

            GetComponent<AudioSource>().pitch = (GetComponent<Rigidbody>().velocity.magnitude / 150) * 2.8f;
        }
    }
}
