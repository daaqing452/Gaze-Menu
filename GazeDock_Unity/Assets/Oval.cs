using System;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;


[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class Oval : MonoBehaviour {

    public float Radius = 1.0f;
    public int Segments = 180;
    public int k = 0;

    public float StartAngle = 0;
    public float EndAngle = 360;
    public int mod = 1;
    public float resize = 0.9f;

    private int counter;
    public int num = 12;
    int i = 0;

    static float[] border;
    public static float[] Border {
        get { return border; }
    }

    static float a = 0.7f;
    public static float A {
        get { return a; }
        set { a = value; }
    }
    static float b = 0.45f;
    public static float B {
        get { return b; }
        set { b = value; }
    }

    static float x0 = 0;
    public static float X0 {
        get { return x0; }
        set { x0 = value; }
    }

    static float y0 = 0;
    public static float Y0 {
        get { return y0; }
        set { y0 = value; }
    }

    static bool isFinished = false;
    public static bool IsFinished {
        get { return isFinished; }
        set { isFinished = value; }
    }

    static bool borderCreated = false;
    public static bool BorderCreated {
        get { return borderCreated; }
    }


    private MeshFilter _meshFilter;
    private LineRenderer _lineRender;
    //private LineRenderer GazeRayRenderer;

    void Awake() {
        _meshFilter = GetComponent<MeshFilter>();
        //_lineRender = GetComponent<LineRenderer>();
    }

    void Start() {
        //border = getBroder();
        //_meshFilter.mesh = CreateMesh();
    }

    Mesh CreateMesh() {
        int vertCount = 2 * (Segments * 1 + 1);
        Vector3[] vertices = new Vector3[vertCount];
        //for (int i = 0; i < border.Length; i++) Debug.Log(border[i]);
        StartAngle = border[(num + k -1) % num] % 360;
        EndAngle = border[(num + k) % num] % 360;
        float realAngle = (EndAngle - StartAngle + 360) % 360;
        if (realAngle < 1e-7) realAngle = 360;
        float deltaAngle = realAngle / Segments;
        float currAngle = EndAngle;
        for (int i = 0; i < vertCount; i += 2, currAngle -= deltaAngle) {
            float cosA = Mathf.Cos(currAngle / 180 * Mathf.PI);
            float sinA = Mathf.Sin(currAngle / 180 * Mathf.PI);
            float thro = resize * Mathf.Atan(Mathf.Sqrt(Mathf.Pow(cosA, 2) + Mathf.Pow(sinA, 2))) * 180 / Mathf.PI;
            vertices[i] = new Vector3(a * cosA + x0, b * sinA + y0, 0);
            vertices[i + 1] = new Vector3(cosA * Radius, sinA * Radius, 0);
        }

        int[] triangles = new int[3 * (vertCount - 2)];
        for (int i = 0, j = 0; i < triangles.Length; i += 6, j += 2) {
            triangles[i] = j + 1;
            triangles[i + 1] = j + 2;
            triangles[i + 2] = j + 0;
            triangles[i + 3] = j + 1;
            triangles[i + 4] = j + 3;
            triangles[i + 5] = j + 2;
        }

        Vector2[] uvs = new Vector2[vertCount];
        for (int i = 0; i < vertCount; ++i) {
            uvs[i] = new Vector2(vertices[i].x / Radius / 2 + 0.5f, vertices[i].y / Radius / 2 + 0.5f);
        }

        Mesh mesh = new Mesh {
            vertices = vertices,
            triangles = triangles,
            uv = uvs
        };

        return mesh;
    }

    void FixedUpdate() {
        if (isFinished) {
            if (i == 0) {
                border = getBroder();
                borderCreated = true;
                i++;
                _meshFilter.mesh = CreateMesh();
            } 
        } 
    }

    float[] getBroder() {
        float[] arr = new float[num + 1];
        float x1 = a, y1 = 0, angle = 0, d = 0;
        while (angle < 360) {
            float x = a * Mathf.Cos(angle * Mathf.PI / 180);
            float y = b * Mathf.Sin(angle * Mathf.PI / 180);
            d += new Vector2(x1 - x, y1 - y).magnitude;
            x1 = x;
            y1 = y;
            angle += 0.25f;
        }
        float one_arc = d / (num * 2);
        angle = 0;
        x1 = a;
        y1 = 0;
        float angle0 = 0;
        for (int i = 0; i < 2 * num; i++) {
            float dist = 0;
            while (dist < one_arc) {
                angle += 0.05f;
                float x = a * Mathf.Cos(angle * Mathf.PI / 180);
                float y = b * Mathf.Sin(angle * Mathf.PI / 180);
                dist += new Vector2(x1 - x, y1 - y).magnitude;
                x1 = x;
                y1 = y;
            }
            if (i % 2 == mod) arr[i / 2] = angle;
            angle0 = angle;
        }
        arr[num] = arr[0];
        return arr; 

    }

    

}

