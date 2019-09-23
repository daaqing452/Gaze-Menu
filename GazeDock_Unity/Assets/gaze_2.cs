using System;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ViveSR {
    namespace anipal {
        namespace Eye {
            public class gaze_2 : MonoBehaviour {
                public int LengthOfRay = 25;
                [SerializeField]
                private LineRenderer GazeRayRenderer;
                private GameObject pos;
                private GameObject counter;
                private GameObject timer;
                private string filepath;
                private bool rec;
                private int angle;
                private int count = -1;
                public float breakTime = 5;
                private float timeCounter = 0;
                private float[] radius;



                private void Start() {

                    filepath = "1-1-" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".txt";
                    FileStream create = new FileStream("E:\\Gaze Dock\\rec\\" + filepath, FileMode.CreateNew);
                    create.Close();
                    pos = GameObject.Find("mark");
                    pos.transform.SetParent(GameObject.Find("can").transform);
                    counter = GameObject.Find("count");
                    timer = GameObject.Find("Timer");
                    radius = creatRadius();
                    if (!SRanipal_Eye_Framework.Instance.EnableEye) {
                        enabled = false;
                        return;
                    }
                    Assert.IsNotNull(GazeRayRenderer);

                }

                private void FixedUpdate() {
                    if (timeCounter > 0) TimerDown();
                }

                private void Update() {


                    if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
                        SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;
                    Vector3 GazeOriginCombinedLocal, GazeDirectionCombinedLocal;
                    if (SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { } else if (SRanipal_Eye.GetGazeRay(GazeIndex.LEFT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { } else if (SRanipal_Eye.GetGazeRay(GazeIndex.RIGHT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { } else return;
                    Vector3 GazeDirectionCombined = Camera.main.transform.TransformDirection(GazeDirectionCombinedLocal);
                    GazeRayRenderer.SetPosition(0, Camera.main.transform.position - Camera.main.transform.up * 0.05f);
                    GazeRayRenderer.SetPosition(1, Camera.main.transform.position + GazeDirectionCombined * LengthOfRay);
                    counter.GetComponent<Text>().text = count.ToString();
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyUp(KeyCode.Space)) rec = !rec;
                    if (rec&&count>=0) {
                        StreamWriter writer = new StreamWriter(new FileStream("E:\\Gaze Dock\\rec\\" + filepath, FileMode.Append));
                        writer.WriteLine(angle + " " + GazeDirectionCombinedLocal.x + " " + GazeDirectionCombinedLocal.y + " " + GazeDirectionCombinedLocal.z);
                        writer.Close();
                    }
                    if (Input.GetKeyUp(KeyCode.Space)) NewPos();

                }

                void NewPos() {

                    count++;
                    angle = UnityEngine.Random.Range(0, 24) + (count%15)*24;
                    float r = Mathf.Tan(radius[360 - angle] * Mathf.PI / 180);
                    pos.GetComponent<RectTransform>().transform.localPosition = new Vector3(580 * r * Mathf.Cos(angle * Mathf.PI / 180),
                        580 * r * Mathf.Sin(angle * Mathf.PI / 180), 0);                   
                    timeCounter = 5;
                    Debug.Log("Position Updated");
                }

                float[] creatRadius() {

                    float[] Ra;
                    Ra = new float[361];
                    StreamReader str = new StreamReader(new FileStream("E:\\Gaze Dock\\angles0.txt", FileMode.Open, FileAccess.Read));
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

                void TimerDown() {
                    timer.GetComponent<Text>().text = Math.Round(timeCounter, 1).ToString();
                    if (timeCounter > 0) timeCounter -= Time.deltaTime;
                    else timeCounter = 0;
                }

            }
        }
    }
}