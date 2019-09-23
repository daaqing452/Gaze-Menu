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
            public class gaze_16 : MonoBehaviour {
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
                private string mark = "null";
                private bool con = true;
                private bool isStart = false;
                private bool trigger = false;
                private bool rec = true;
                private int[] randomInt;
                public float rounds = 1;
                private int count = 0;
                private int mis = 0;
                private int window_size_in = 24;
                private int window_size_out = 20;
                private int area = -1;
                private int area_bf = -1;
                private float[] border;
                private float time_recorder;
                private float scale = 0.9f;
                private List<Vector2> dis_in = new List<Vector2>();
                private List<Vector2> dis_out = new List<Vector2>();
                public Material s;
                public Material bf;

                private void Start() {

                    filepath = "2-16-" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".txt";
                    FileStream create = new FileStream("E:\\Gaze Dock\\rec\\" + filepath, FileMode.CreateNew);
                    create.Close();
                    gazeMenu = GameObject.Find("gaze_menu_16");
                    ren = gazeMenu.GetComponentsInChildren<Renderer>();
                    info = GameObject.Find("Info");
                    hint = GameObject.Find("Hint");
                    if (!SRanipal_Eye_Framework.Instance.EnableEye) {
                        enabled = false;
                        return;
                    }
                    Assert.IsNotNull(GazeRayRenderer);
                    cg = GameObject.Find("ring_text").GetComponent<CanvasGroup>();

                }

                private void FixedUpdate() {

                    initialMenu();
                    border = Oval.Border;
                    if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
                        SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;
                    Vector3 GazeOriginCombinedLocal, GazeDirectionCombinedLocal;
                    if (SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { } else if (SRanipal_Eye.GetGazeRay(GazeIndex.LEFT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { } else if (SRanipal_Eye.GetGazeRay(GazeIndex.RIGHT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { } else { };
                    Vector3 GazeDirectionCombined = Camera.main.transform.TransformDirection(GazeDirectionCombinedLocal);
                    GazeRayRenderer.SetPosition(0, Camera.main.transform.position - Camera.main.transform.up * 0.05f);
                    GazeRayRenderer.SetPosition(1, Camera.main.transform.position + GazeDirectionCombined * LengthOfRay);
                    Vector3 Gaze_oval = GazeDirectionCombinedLocal - new Vector3(Oval.X0, Oval.Y0, 0);
                    Vector2 Choice = new Vector2(Gaze_oval.x, Gaze_oval.y);
                    if (count == rounds * (menu.Length - 1) + 1) {
                        hint.GetComponent<TextMesh>().text = "Round Finished";
                        mark = "finished";
                    }
                    if (isStart && count == 0) {
                        hint.GetComponent<TextMesh>().text = nextCount();
                        StreamWriter writer = new StreamWriter(new FileStream("E:\\Gaze Dock\\rec\\" + filepath, FileMode.Append));
                        writer.WriteLine(mark + " " + time_recorder + " " + randomInt[(count - 1) % 16] + " " + GazeDirectionCombinedLocal.x + " " + GazeDirectionCombinedLocal.y
                            + " " + GazeDirectionCombinedLocal.z + " " + mis);
                        writer.Close();
                    }
                    if (!GazeDirectionCombinedLocal.Equals(new Vector3(0, 0, 1))) {
                        if (GetVal(Choice, Gaze_oval, 1) && isStart) {
                            trigger = true;
                            ring_up();
                        } else if (trigger) {
                            if (dis_out.Count > 0 && dis_out[dis_out.Count - 1].y > 0) {
                                string str = GetChoice(Choice);
                                if (!selection.Equals(str)) {
                                    if (area >= 0) {
                                        if (area_bf >= 0) {
                                            ren[area_bf % 16].material = bf;
                                        }
                                        bf = ren[area % 16].material;
                                        ren[area % 16].material = s;
                                    }
                                    area_bf = area;
                                    selection = str;
                                }
                            }
                            ring_up();
                        } 
                        if (GetVal(Choice, Gaze_oval, 0)) {
                            if (trigger && isStart) {
                                trigger = false;
                                info.GetComponent<TextMesh>().text = "You have selected\n         " + selection;
                                if ((selection != hint.GetComponent<TextMesh>().text) && rec) mis++;
                                dis_in.Clear();
                                dis_out.Clear();
                            }
                            ring_down();
                        }
                    } else {
                        dis_in.Clear();
                        dis_out.Clear();
                    }
                    time_recorder += Time.fixedDeltaTime;
                    if (Input.GetKeyDown(KeyCode.Space)) {
                        info.GetComponent<TextMesh>().text = "";
                        hint.GetComponent<TextMesh>().text = nextCount();
                    }
                    if (rec && isStart) {
                        StreamWriter writer = new StreamWriter(new FileStream("E:\\Gaze Dock\\rec\\" + filepath, FileMode.Append));
                        writer.WriteLine(mark + " " + time_recorder + " " + randomInt[(count - 1) % 16] + " " + GazeDirectionCombinedLocal.x + " " + GazeDirectionCombinedLocal.y
                            + " " + GazeDirectionCombinedLocal.z + " " + mis);
                        writer.Close();
                    }

                }

                private bool GetVal(Vector2 vec, Vector3 gaze, int select) {

                    float d1;
                    bool val = false;
                    Vector2 Bar = new Vector2(1, 0);
                    int angle = (int)Vector2.Angle(Bar, vec);
                    float thro = scale * Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(Mathf.Cos(angle * Mathf.PI / 180) * Oval.A, 2) + Mathf.Pow(Mathf.Sin(angle * Mathf.PI / 180) * Oval.B, 2)), 1) * 180 / Mathf.PI;
                    if (vec.y > 0) angle += 180;
                    d1 = Vector3.Angle(new Vector3(0, 0, 1), gaze) - thro;
                    if (select == 1) {
                        float d2 = Vector3.Angle(gaze + new Vector3(Oval.X0, Oval.Y0, 0), new Vector3(0, 0, 1));
                        if (!trigger) {
                            if (dis_in.Count < window_size_in) {
                                dis_in.Add(new Vector2(d2, d1));
                            } else {
                                dis_in.RemoveAt(0);
                                dis_in.Add(new Vector2(d2, d1));
                            }
                            if (d1 > 0) val = Check(1);
                        } else val = false;
                    } else if (select == 0) {
                        float d2 = Vector3.Angle(gaze + new Vector3(Oval.X0, Oval.Y0, 0), new Vector3(0, 0, 1));
                        if (trigger) {
                            if (dis_out.Count < window_size_out) {
                                dis_out.Add(new Vector2(d2, d1));
                            } else {
                                dis_out.RemoveAt(0);
                                dis_out.Add(new Vector2(d2, d1));
                            }
                            if (d1 < 0) val = Check(0);
                        } else val = true;
                    }
                    return val;
                }


                private string GetChoice(Vector2 vec) {

                    float angle = 0;
                    string choi = "None";

                    angle = Mathf.Atan2(vec.y, vec.x) * 180 / Mathf.PI;
                    if (angle < 0) angle += 360;
                    for (int i = 0; i < border.Length; i++) {
                        area = i;
                        if (angle < border[area]) {
                            choi = menu[area];
                            Debug.Log("area： " + area);
                            Debug.Log(angle);
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

                public void changeCon() {
                    con = false;
                    isStart = true;
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
                    if (count % 16 == 0 || count == 0) randomInt = CreateRandom();
                    mes = menu[randomInt[count % 16] - 1];
                    count++;
                    return mes;
                }

                private bool Check(int select) {

                    bool result = false;
                    int count = 0;
                    if (select == 1) {
                        var arr = dis_in.ToArray();
                        if (arr.Length != window_size_in) return false;
                        if (arr[0].x < 10 && arr[11].y > 0 && arr[window_size_in - 1].x > 0) result = true;
                    } else if (select == 0) {
                        var arr = dis_out.ToArray();
                        if (arr.Length != window_size_out) return false;
                        if (arr[0].y > 0 && arr[10].x < 10 && arr[window_size_out - 1].x < 10) return true;
                        if (arr[0].x < 10 && arr[10].x < 10 && arr[window_size_out - 1].x < 10) return true;
                        //if (arr[window_size_out - 1].y < -10) return true;
                    }

                    return result;
                }

            }
        }
    }
}
