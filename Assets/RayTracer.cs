using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTracer : MonoBehaviour
{
    public int mDepth = 15;

    private Texture2D mOutput;
    private bool mShowRay;
    private int mFrame = 0;
    private int mRow = 0;
    private List<Line> mRayLine;
    struct Line
    {
        public Vector3 a;
        public Vector3 b;
        public Line(Vector3 a_, Vector3 b_)
        {
            a = a_;
            b = b_;
        }
    }

	// Use this for initialization
	void Start ()
    {
        mOutput = new Texture2D(Screen.width, Screen.height);	
        for (int x = 0; x < mOutput.width; ++x)
            for (int y = 0; y < mOutput.height; ++y)
            {
                Color color = Color.black;
                mOutput.SetPixel(x, y, color);
            }
        mOutput.Apply();

        mRayLine = new List<Line>();
    }

    void OnGUI()
    {
        if (!mShowRay)
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), mOutput);
        GUI.Label(new Rect(new Vector2(0,0), new Vector2(200, 50)), "Frame " + mFrame + " Row " + mRow);
    }

    bool lastDown = false;
    // Update is called once per frame
    void Update ()
    {
        for (int x = 0; x < mOutput.width; ++x)
            RayTraceJob(x, mRow);
        mOutput.Apply();
        mRow += 1;
        if (mRow == mOutput.height)
        {
            mRow = 0;
            mFrame += 1;
        }
        if (Input.GetMouseButtonUp(0)) 
        {
            mRayLine.Clear();
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Color o = Color.black;
            TraceRay(ray, o, true);
            mShowRay = true;
        }
        if (Input.GetMouseButtonUp(1))
        {
            mRayLine.Clear();
            mShowRay = false;
        }
        if (mShowRay)
        {
            foreach (var line in mRayLine)
            {
                Debug.DrawLine(line.a, line.b, Color.red);
            }
        }
	}

    void RayTraceJob(int x, int y)
    {
        Color color = Color.black;
        Ray ray = GetComponent<Camera>().ScreenPointToRay(new Vector3(x, y, 0));
        var oldC = mOutput.GetPixel(x, y);
        color = TraceRay(ray, color);
        var averC = (((mFrame) * oldC + color) / (mFrame + 1));
        mOutput.SetPixel(x, y, averC);
    }

    struct IterStruct
    {
        public Ray ray;
        public int level;
        public Vector3 factor;
    };
    Color TraceRay(Ray inray, Color color, bool genPath = false)
    {
        RaycastHit hit;
        IterStruct iter;
        iter.ray = inray;
        iter.level = 0;
        iter.factor = new Vector3(1,1,1);
        List<IterStruct> raycache = new List<IterStruct>();
        raycache.Add(iter);
        do
        {
            var it = raycache[0];
            raycache.Remove(it);
            // Return if max iter
            if (it.level == mDepth)
                continue;
            if (Physics.Raycast(it.ray, out hit))
            {
                var from = it.ray.direction;
                var pos = hit.point + hit.normal * 0.000001f;
                var normal = hit.normal;

                var obj = hit.collider.gameObject.GetComponent<RayTraceObject>();
                if (obj == null)
                {
                    Debug.Log("Find non trace object " + hit.collider.gameObject.name);
                    continue;
                }

                float maxColor = obj.BaseColor.maxColorComponent;
                switch (obj.mShapeType)
                {
                    case RayTraceObject.ShapeType.Diffuse:
                        {
                            //color += new Color (obj.BaseColor.r, obj.BaseColor.g, obj.BaseColor.b);
                            IterStruct next;
                            // Prepare next ray
                            float r1 = 2 * Mathf.PI * Random.value;
                            float r2 = Random.value;
                            float sr2 = Mathf.Sqrt(r2);
                            Vector3 u = Vector3.Cross(Mathf.Abs(normal.x) > 0.1f ? new Vector3(0, 1, 0) : new Vector3(1, 0, 0), normal);
                            u.Normalize();
                            Vector3 v = Vector3.Cross(normal, u);
                            Vector3 d = Mathf.Cos(r1) * sr2 * u + v * Mathf.Sin(r1) * sr2 + normal * Mathf.Sqrt(1 - sr2);
                            d.Normalize();
                            next.ray = new Ray(pos, d);
                            // Prepare next level
                            next.level = it.level + 1;
                            // Prepare next factor
                            next.factor = new Vector3(it.factor.x * obj.BaseColor.r, it.factor.y * obj.BaseColor.g, it.factor.z * obj.BaseColor.b);
                            // Push next
                            raycache.Add(next);
                            if (genPath)
                            {
                                mRayLine.Add(new Line(it.ray.origin, next.ray.origin));
                            }
                            break;
                        }
                    case RayTraceObject.ShapeType.Specular:
                        {
                            break;
                        }
                    case RayTraceObject.ShapeType.Transparent:
                        {
                            break;
                        }
                    case RayTraceObject.ShapeType.Emission:
                        {
                            // End travel
                            color += new Color(it.factor.x * obj.BaseColor.r, it.factor.y * obj.BaseColor.g, it.factor.z * obj.BaseColor.b);
                            if (genPath)
                            {
                                mRayLine.Add(new Line(it.ray.origin, pos));
                            }
                            break;
                        }
                }

            }
            else
            {
                if (genPath)
                {
                    mRayLine.Add(new Line(it.ray.origin, it.ray.origin + 20 * it.ray.direction));
                }
            }
        } while (raycache.Count > 0);
        return color;
    }
}
