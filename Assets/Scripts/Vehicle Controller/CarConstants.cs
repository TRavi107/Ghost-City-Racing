using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Car Properties",menuName ="Scriptable Objects/Car/Car Properties")]
public class CarConstants : ScriptableObject
{
    public enum Xg        //Gear Ratio 
    {
        one,
        two,
        three,
        four,
        five,
        reverse,
    }   
    public float Xd=1;      //differential ratio
    public float n=1;       //transmission effiecint
    public float Rw=1;      //Wheel Radius
    public float Cd=1;      //Coefficient of friction
    public float Crr=1;
    public float A=1;       //Frontal Area of car
    public float rho = 1.29f; //Air density
}
