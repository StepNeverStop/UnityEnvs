using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterParticle {

    public float initTime;
    public float timeToLive;

    public GameObject go;
    public Transform t;
    public MeshRenderer mr;
    public Material mat;

    public Vector3 initialVelocity;
    public Vector3 initialScale;
    public Color initialColor;
    public float initialAlpha;
}
