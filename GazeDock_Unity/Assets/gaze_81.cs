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
            public class gaze_81 : MonoBehaviour {
                public int LengthOfRay = 0;
                [SerializeField]
                private LineRenderer GazeRayRenderer;
                private GameObject gazeMenu;
                private GameObject info;
                private GameObject hint;
                private CanvasGroup cg;
                private Renderer[] ren;
                private string filepath;
                private string[] menu;
                private string selection = "";
                private bool con = true;
                private bool isStart = false;
                private bool trigger = false;
                private bool rec = false;
                private int[] randomInt;
                public int rounds = 1;
                private int count = 0;
                private int mis = 0;
                private int window_size_in = 18;
                private int window_size_out = 1;
                private int area = -1;
                private int area_bf = -1;
                private float[] thro_in;
                private float[] thro_out;
                private float[] thro_select;
                private float[] border = { 10, 55, 125, 170, 180, 155, 112.5f, 67.5f, 25 };
                private float pd = 0;
                private ArrayList dis_in = new ArrayList();
                private ArrayList dis_out = new ArrayList();
                public Material s;
                public Material bf;



                private void Start() {

                    filepath = "2-81-" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".txt";
                    FileStream create = new FileStream("E:\\Gaze Dock\\rec\\" + filepath, FileMode.CreateNew);
                    create.Close();
                    gazeMenu = GameObject.Find("gaze_menu_81");
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
                    thro_select = creatRadius("select");
                    cg = GameObject.Find("ring_text").GetComponent<CanvasGroup>();
                }



                private void Update() {

                    initialMenu();
                    if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
                        SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;
                    Vector3 GazeOriginCombinedLocal, GazeDirectionCombinedLocal;
                    if (SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { } else if (SRanipal_Eye.GetGazeRay(GazeIndex.LEFT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { } else if (SRanipal_Eye.GetGazeRay(GazeIndex.RIGHT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { } else return;
                    Vector3 GazeDirectionCombined = Camera.main.transform.TransformDirection(GazeDirectionCombinedLocal);
                    GazeRayRenderer.SetPosition(0, Camera.main.transform.position - Camera.main.transform.up * 0.05f);
                    GazeRayRenderer.SetPosition(1, Camera.main.transform.position + GazeDirectionCombined * LengthOfRay);
                    Vector2 Choice = new Vector2(GazeDirectionCombinedLocal.x, GazeDirectionCombinedLocal.y);
                    if (count == rounds * (menu.Length - 1) + 1) hint.GetComponent<TextMesh>().text = "Round Finished";
                    if (count == 0) hint.GetComponent<TextMesh>().text = nextCount();
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyUp(KeyCode.Space)) rec = !rec;
                    if (GetVal(Choice, GazeDirectionCombinedLocal, 1) && isStart) {
                        trigger = true;
                        ring_up();
                    }
                    if (trigger && (GetVal(Choice, GazeDirectionCombinedLocal, 2))) {
                        String str = GetChoice(Choice);
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
                        }
                    }
                    if (GetVal(Choice, GazeDirectionCombinedLocal, 0)) {
                        if (trigger && isStart) {
                            trigger = false;
                            info.GetComponent<TextMesh>().text = "You have selected\n" + selection;
                            if ((selection != hint.GetComponent<TextMesh>().text) && rec) mis++;
                            dis_in.Clear();
                        }
                        ring_down();
                    }
                    if (rec) {
                        StreamWriter writer = new StreamWriter(new FileStream("E:\\Gaze Dock\\rec\\" + filepath, FileMode.Append));
                        writer.WriteLine(randomInt[(count - 1) % 8] + " " + GazeDirectionCombinedLocal.x + " " + GazeDirectionCombinedLocal.y
                            + " " + GazeDirectionCombinedLocal.z + " " + mis);
                        writer.Close();
                    }
                    if (Input.GetKeyUp(KeyCode.Space)) hint.GetComponent<TextMesh>().text = nextCount();


                }

                private bool GetVal(Vector2 vec, Vector3 gaze, int select) {

                    float d;
                    bool val = false;
                    Vector2 Bar = new Vector2(1, 0);
                    int angle = (int)Vector2.Angle(Bar, vec);
                    if (vec.y > 0) angle += 180;
                    if (select == 1) {
                        d = Vector3.Angle(new Vector3(0, 0, 1), gaze) - thro_in[angle] + 4;
                        if (!trigger) {
                            if (dis_in.Count < window_size_in) {
                                dis_in.Add(d);
                            } else {
                                dis_in.RemoveAt(0);
                                dis_in.Add(d);
                            }
                            if (d > 0) val = Check(1, gaze);
                        } else val = trigger;
                    } else if (select == 0) {
                        d = Vector3.Angle(new Vector3(0, 0, 1), gaze) - thro_out[angle];
                        if (dis_out.Count < window_size_out) dis_out.Add(d);
                        else {
                            dis_out.RemoveAt(0);
                            dis_out.Add(d);
                        }
                        if (d < 0) {
                            val = Check(0, gaze);
                        }
                    } else {
                        d = Vector3.Angle(new Vector3(0, 0, 1), gaze) - thro_select[angle] + 4;
                        val = (d > 0) ? true : false;
                    }
                    Debug.Log(select + "/" + d);
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

                    for (int j = 0; j < ren.Length; j++) {
                        Material m = ren[j].material;
                        if (m.color.a < 1.00f) {
                            m.color = new Color(m.color.r, m.color.g, m.color.b, m.color.a + 0.02f);
                            cg.alpha += 0.02f;
                        }

                    }
                }


                private void ring_down() {

                    if (con) return;

                    for (int j = 0; j < ren.Length; j++) {
                        Material m = ren[j].material;
                        if (m.color.a > 0.02f) {
                            m.color = new Color(m.color.r, m.color.g, m.color.b, m.color.a - 0.02f);
                            cg.alpha -= 0.02f;
                        }
                    }
                }

                public void changeCon() {
                    con = false;
                    isStart = true;
                }


                float[] creatRadius(string status) {

                    String filepath;
                    if (status.Equals("in")) filepath = "E:\\Gaze Dock\\angles01.txt";
                    else if ((status.Equals("out"))) filepath = "E:\\Gaze Dock\\angles0.txt";
                    else filepath = "E:\\Gaze Dock\\angles01.txt";
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

                    string mes = "";
                    if (count % 8 == 0 || count == 0) randomInt = CreateRandom();
                    mes = menu[randomInt[count % 8] - 1];
                    count++;
                    return mes;
                }


                private bool Check(int select, Vector3 gaze) {

                    bool result = false;
                    int count = 0;
                    if (select == 1) {
                        var arr = dis_in.ToArray();
                        if (arr.Length != window_size_in) return false;
                        for (int i = 0; i < arr.Length / 2; i++) {
                            if ((float)arr[i + 1] > (float)arr[i] && (float)arr[i + window_size_in / 2] > 0) count++;
                        }
                        if (count >= arr.Length / 2 && Vector3.Angle(new Vector3(0, 0, 1), gaze) > 10) result = true;
                    } else if (select == 0) {
                        if (dis_out.Count != window_size_out) return false;
                        foreach (float d in dis_out) {
                            if (d < 0) count++;
                        }
                        if (count == dis_out.Count) result = true;
                    }
                    return result;
                }


            }
        }
    }
}
