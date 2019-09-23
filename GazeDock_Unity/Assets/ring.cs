using System.IO;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class ring : MonoBehaviour {

    public float Radius = 1.0f;
    public int Segments = 360;
    public float StartAngle = 0;
    public float EndAngle = 360;

    private MeshFilter _meshFilter;

    void Awake() {
        _meshFilter = GetComponent<MeshFilter>();
    }

    void Start() {
        _meshFilter.mesh = CreateMesh();
    }

    Mesh CreateMesh() {


        int vertCount = 2 * (Segments * 1 + 1);
        Vector3[] vertices = new Vector3[vertCount];

        StartAngle = StartAngle % 360;
        EndAngle = EndAngle % 360;
        float realAngle = (EndAngle - StartAngle + 360) % 360;
        if (realAngle < 1e-7) realAngle = 360;
        float[] InnerRadius = creatRadius();
        float deltaAngle = realAngle / Segments;
        float currAngle = EndAngle;
        for (int i = 0; i < vertCount; i += 2, currAngle -= deltaAngle) {
            float cosA = Mathf.Cos(currAngle / 180 * Mathf.PI);
            float sinA = Mathf.Sin(currAngle / 180 * Mathf.PI);
            vertices[i] = new Vector3(cosA * Mathf.Tan(InnerRadius[360 - Mathf.RoundToInt((currAngle + 360)%360)] * Mathf.PI /180),
                sinA * Mathf.Tan(InnerRadius[360 - Mathf.RoundToInt((currAngle + 360) % 360)] * Mathf.PI / 180), 0);
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


    float[] creatRadius() {

        float[] Ra;
        Ra = new float[361];
        StreamReader str = new StreamReader(new FileStream("E:\\Gaze Dock\\angles01.txt", FileMode.Open, FileAccess.Read));
        int i = 0;
        while (true) {
            string angle = str.ReadLine();
            if (angle != null) {
                Ra[i] = float.Parse(angle);
                i++;
            } else
                break;
        }
        return Ra;

    }



}

