//========= Copyright 2018, HTC Corporation. All rights reserved. ===========
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

namespace ViveSR {
    namespace anipal {
        namespace Eye {
            public class gaze_d8 : MonoBehaviour {
                public int LengthOfRay = 25;
                [SerializeField]
                private LineRenderer GazeRayRenderer;
                private GameObject gazeMenu;
                private GameObject info;
                private GameObject hint;
                private CanvasGroup cg;
                private Renderer[] ren;
                private string filepath;
                private string[] menu;
                private string selection= "";
                private string mark="null";
                private bool con = true;
                private bool isStart = false;
                private bool trigger = false;
                private bool rec = true;
                private bool isFinised = false;
                private int[] randomInt;
                public int rounds = 1;
                private int count = 0;
                private int mis = 0;
                private int area = -1;
                private int area_bf = -1;
                public int windowsize = 15;
                private int window_size_in = 24;
                private int frame_counter;
                private float[] thro_in;
                private float[] thro_out;
                private float[] border = { 10, 55, 125, 170, 180, 155, 112.5f, 67.5f, 25 };
                private float time_recorder;
                private List<Vector2> dis_in = new List<Vector2>();
                public Material s;
                public Material bf;

                private void Awake() {
                    Application.targetFrameRate = 60;
                }

                private void Start() {

                    filepath = "2-d8-" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".txt";
                    FileStream create = new FileStream("E:\\Gaze Dock\\rec\\" + filepath, FileMode.CreateNew);
                    create.Close();
                    gazeMenu = GameObject.Find("gaze_menu_8");
                    ren = gazeMenu.GetComponentsInChildren<Renderer>();
                    info = GameObject.Find("Info");
                    hint = GameObject.Find("Hint");
                    if (!SRanipal_Eye_Framework.Instance.EnableEye) {
                        enabled = false;
                        return;
                    }
                    Assert.IsNotNull(GazeRayRenderer);
                    thro_in = creatRadius("in");
                    thro_out = creatRadius("out");
                    cg = GameObject.Find("ring_text").GetComponent<CanvasGroup>();

                }

                private void FixedUpdate() {

                    initialMenu();
                    if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
                        SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;
                    Vector3 GazeOriginCombinedLocal, GazeDirectionCombinedLocal;
                    if (SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { } else if (SRanipal_Eye.GetGazeRay(GazeIndex.LEFT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { } else if (SRanipal_Eye.GetGazeRay(GazeIndex.RIGHT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { } else { };
                    Vector3 GazeDirectionCombined = Camera.main.transform.TransformDirection(GazeDirectionCombinedLocal);
                    GazeRayRenderer.SetPosition(0, Camera.main.transform.position - Camera.main.transform.up * 0.05f);
                    GazeRayRenderer.SetPosition(1, Camera.main.transform.position + GazeDirectionCombined * LengthOfRay);
                    Vector2 Choice = new Vector2(GazeDirectionCombinedLocal.x, GazeDirectionCombinedLocal.y);
                    if (count == rounds * (menu.Length - 1) + 1) {
                        hint.GetComponent<TextMesh>().text = "Round Finished";
                        mark = "finished";
                    }
                    if (isStart && count == 0) {
                        hint.GetComponent<TextMesh>().text = nextCount();
                        StreamWriter writer = new StreamWriter(new FileStream("E:\\Gaze Dock\\rec\\" + filepath, FileMode.Append));
                        writer.WriteLine(mark + " " + time_recorder + " " + randomInt[(count - 1) % 8] + " " + GazeDirectionCombinedLocal.x + " " + GazeDirectionCombinedLocal.y
                            + " " + GazeDirectionCombinedLocal.z + " " + mis);
                        writer.Close();
                    } 
                    if (!GazeDirectionCombinedLocal.Equals(new Vector3(0, 0, 1))) {
                        if (GetVal(Choice, GazeDirectionCombinedLocal,1) && isStart) {
                            if(!isFinised)  trigger = true;
                            ring_up();
                            string str = GetChoice(Choice);
                            if (!selection.Equals(str)) {
                                if (area >= 0) {
                                    if (area_bf >= 0) {
                                        ren[area_bf % 8].material = bf;
                                    }
                                    bf = ren[area % 8].material;
                                    ren[area % 8].material = s;
                                }
                                area_bf = area;
                                selection = str;
                                frame_counter = 0;
                            } else {
                                if (frame_counter >= windowsize) {
                                    info.GetComponent<TextMesh>().text = "You have selected\n       " + selection;
                                    trigger = false;
                                    isFinised = true;
                                    frame_counter = -1;
                                    ring_off();
                                    if ((selection != hint.GetComponent<TextMesh>().text) && rec) mis++;
                                } else if (frame_counter >= 0) {
                                    frame_counter++;
                                }
                                else ring_off();
                            }
                        } else {
                            ring_down();
                            frame_counter = 0;
                            isFinised = false;
                        }
                    } else {
                        frame_counter = 0;
                    }
                    time_recorder += Time.fixedDeltaTime;
                    if (Input.GetKeyDown(KeyCode.Space)) {
                        hint.GetComponent<TextMesh>().text = nextCount();
                        info.GetComponent<TextMesh>().text = "";
                    } 
                    if (rec&&isStart) {
                        StreamWriter writer = new StreamWriter(new FileStream("E:\\Gaze Dock\\rec\\" + filepath, FileMode.Append));
                        writer.WriteLine( mark + " " + time_recorder +" " + randomInt[(count - 1) % 8] + " " + GazeDirectionCombinedLocal.x + " " + GazeDirectionCombinedLocal.y
                            + " " + GazeDirectionCombinedLocal.z + " " + mis);
                        writer.Close();
                    }
                    
                }

                private bool GetVal(Vector2 vec, Vector3 gaze, int select) {

                    float d1;
                    float err = 5;
                    bool val = false;
                    Vector2 Bar = new Vector2(1, 0);
                    int angle = (int)Vector2.Angle(Bar, vec);
                    if (vec.y > 0) angle += 180;
                    if (select == 1) {
                        d1 = Vector3.Angle(new Vector3(0, 0, 1), gaze);
                        float d2 = d1 - thro_in[angle] + err;
                        if (!trigger) {
                            if (dis_in.Count < window_size_in) {
                                dis_in.Add(new Vector2(d1, d2));
                            } else {
                                dis_in.RemoveAt(0);
                                dis_in.Add(new Vector2(d1, d2));
                            }
                            if (d2 > 0) val = Check(1);
                        } else val = true;
                    }
                    return val;
                }


                private string GetChoice(Vector2 vec) {

                    float angle = 0;
                    string choi = "None";


                    Vector2 Bar = new Vector2(1, 0);
                    angle = Vector2.Angle(Bar, vec);
                    for (int i = 0; i < border.Length; i++) {
                        area = 4 + (int)Mathf.Sign(vec.y) * (i - 4);
                        if (angle < border[area]) {
                            choi = menu[area];
                            break;
                        }
                    }

                    return choi;
                }

                private void ring_up() {

                    mark = "in";

                    for (int j = 0; j < ren.Length; j++) {
                        Material m = ren[j].material;
                        if (m.color.a < 1.00f) {
                            m.color = new Color(m.color.r, m.color.g, m.color.b, m.color.a + 0.02f);
                            cg.alpha += 0.02f;
                        }

                    }
                }


                private void ring_down() {

                    mark = "out";

                    if (con) return;

                    for (int j = 0; j < ren.Length; j++) {
                        Material m = ren[j].material;
                        if (m.color.a > 0.02f) {
                            m.color = new Color(m.color.r, m.color.g, m.color.b, m.color.a - 0.02f);
                            cg.alpha -= 0.02f;
                        }
                    }
                }

                private void ring_off() {

                    mark = "stay";

                    if (con) return;

                    for (int j = 0; j < ren.Length; j++) {
                        Material m = ren[j].material;
                        m.color = new Color(m.color.r, m.color.g, m.color.b, 0.02f);
                        cg.alpha = 0.02f;
                    }
                }

                public void changeCon() {
                    con = false;
                    isStart = true;
                }


                float[] creatRadius(string status) {

                    String filepath;
                    if (status.Equals("in")) filepath = "E:\\Gaze Dock\\angles02.txt";
                    else filepath = "E:\\Gaze Dock\\angles0.txt";
                    float[] Ra;
                    Ra = new float[361];
                    StreamReader str = new StreamReader(new FileStream(filepath, FileMode.Open, FileAccess.Read));
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

                void initialMenu() {

                    GameObject[] texts = GameObject.FindGameObjectsWithTag("text");
                    menu = new string[texts.Length + 1];
                    for (int i = 0; i < texts.Length; i++) {
                        menu[i] = texts[i].GetComponent<Text>().text;
                    }
                    menu[texts.Length] = menu[0];
                }

                private int[] CreateRandom() {

                    int[] arr = Enumerable.Range(1, menu.Length - 1).ToArray();
                    int currentIndex;
                    int tempValue;
                    for (int i = menu.Length - 2; i >= 0; i--) {
                        currentIndex = UnityEngine.Random.Range(0, i + 1);
                        tempValue = arr[currentIndex];
                        arr[currentIndex] = arr[i];
                        arr[i] = tempValue;
                    }
                    return arr;
                }

                string nextCount() {

                    mark = "next";
                    string mes = "";
                    if (count % 8 == 0 || count == 0) randomInt = CreateRandom();
                    mes = menu[randomInt[count % 8] - 1];
                    count++;
                    return mes;
                }


                private bool Check(int select) {

                    bool result = false;
                    if (select == 1) {
                        var arr = dis_in.ToArray();
                        if (arr.Length != window_size_in) return false;
                        if (arr[0].x < 11 && arr[11].y > 0 && arr[window_size_in - 1].x > 0) result = true;
                    } 
                    return result;
                }



            }
        }
    }
}
