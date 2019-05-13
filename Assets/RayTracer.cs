using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTracer : MonoBehaviour
{

    private Texture2D mOutput;
    private Light[] mLights;

	// Use this for initialization
	void Start ()
    {
        mOutput = new Texture2D(Screen.width, Screen.height);	
        mLights = FindObjectsOfType(typeof(Light)) as Light[];
        RayTraceJob();
    }

    void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), mOutput);        
    }

    // Update is called once per frame
    void Update ()
    {
        		
	}

    void RayTraceJob()
    {
        for (int x = 0; x < mOutput.width; ++x)
            for (int y = 0; y < mOutput.height; ++y)
            {
                Color color = Color.black;
                Ray ray = GetComponent<Camera>().ScreenPointToRay(new Vector3(x, y, 0));
                mOutput.SetPixel(x, y, TraceRay(ray, color, 0));
            }
        mOutput.Apply();    
    }

    Color TraceRay(Ray ray, Color color, int level)
    {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var from = ray.direction;
            var pos = hit.point + hit.normal * 0.0001f;
            var normal = hit.normal;

            var obj = hit.collider.gameObject.GetComponent<RayTraceObject>();
            if (obj == null)
            {
                Debug.Log("Find non trace object " + hit.collider.gameObject.name);
                return color;
            }

            /*
            Material mat = hit.collider.GetComponent<Renderer>().material;
            if (mat.mainTexture)
            {
                color += (mat.mainTexture as Texture2D).GetPixelBilinear(hit.textureCoord.x, hit.textureCoord.y);
            }
            */
            color += obj.BaseColor;
            
        }
        return color;
    }
}
