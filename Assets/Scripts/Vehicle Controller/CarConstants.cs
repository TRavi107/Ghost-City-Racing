using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Car Properties",menuName ="Scriptable Objects/Car/Car Properties")]
public class CarConstants : ScriptableObject
{
    public int carPrice;
    public string carName;
    public float handBrakeFrictionMultiplier = 2f;
    public float maxRPM, minRPM;
    public float maxVelocity;
    public float[] gears;
    public float[] gearChangeSpeed;
    public float brakeForce;
    public AnimationCurve enginePower;

}
