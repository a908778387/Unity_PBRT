using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTraceObject : MonoBehaviour {
    public enum ShapeType
    {
        Diffuse,
        Specular,
        Transparent,
        Emission,
    }

    public ShapeType mShapeType;

    public Color BaseColor = Color.gray;

	// Use this for initialization
	void Start () {
        GetComponent<MeshRenderer>().material.color = BaseColor;
	}
	
	// Update is called once per frame
	void Update () {
	}
}
