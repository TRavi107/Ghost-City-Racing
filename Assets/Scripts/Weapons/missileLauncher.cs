using System.Collections;
using UnityEngine;

public enum launcherState
{
    inactive,
    gettingReady,
    readyTofire,
    outofAmmo,
}
public class missileLauncher : MonoBehaviour
{
    public GameObject missilePrefab;
    public Transform target;
    public Transform missileInstPos;
    public launcherState mystate = launcherState.inactive;
    public Transform firePos;
    public float speed;

    public float reloadTime = 2;

    float firedTime=0;

    void Update()
    {

        if (firedTime < 0)
            firedTime = 0;
        else
            firedTime -= Time.deltaTime;
        if (Input.GetMouseButton(0))
        {
            if (mystate == launcherState.readyTofire)
            {
                if (firedTime ==0)
                {
                    print("clicked fire button");

                    int myPos = RaceManager.instance.GetIndexOfPlayer(GetComponentInParent<playerManager>().nickName);
                    if (myPos != 0)
                    {
                        RaceManager.instance.ShotMissile(GetComponentInParent<playerManager>().nickName,
                            missileInstPos.position, RaceManager.instance.raceParticipants[myPos - 1].playerManager.nickName);
                        firedTime = reloadTime;
                    }
                    else
                    {
                        print("No one is in front of you to fire missile");
                    }
                }
            }
        }
    }

    public void GettingReady()
    {
        StartCoroutine("MoveToFirePos");
    }

    IEnumerator MoveToFirePos()
    {
        while (Vector3.Distance(transform.position, firePos.position) > 0.1)
        {
            transform.Translate(Vector3.up * Time.deltaTime * speed , Space.Self);
            yield return null;
        }

        mystate = launcherState.readyTofire;
    }
}
