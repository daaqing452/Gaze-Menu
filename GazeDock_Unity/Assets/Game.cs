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
            public class Game : MonoBehaviour {
                public int LengthOfRay = 0;
                public enum Technique { Dwell, GazeDock };
                public Technique technique = Technique.Dwell;
                public GameObject mainCamera;

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
                private bool isStart = true;
                private bool trigger = false;
                private int window_size_in = 24;
                private int window_size_out = 1;
                private int area = -1;
                private int area_bf = -1;
                private float[] border;
                private float time_recorder;
                private float scale = 1.0f;
                private List<Vector2> dis_in = new List<Vector2>();
                private List<Vector2> dis_out = new List<Vector2>();
                public Material s;
                public Material bf;

                int DWELL_START_COUNTER = 80;
                int DWELL_PROGRESS_COUNTER = 40;
                int INFO_COUNTER = 120;
                float PENETRATE_GAZEDOCK = 1.0f;
                GameObject progressbarBg, progressbarFg, dwellMenu;
                string headNameLast = "", gazeNameLast = "";
                int  dwellCounter, infoCounter;
                GameObject headFocus, gazeFocus, headTarget;
                Toy toy;
                GameObject[] teleports;
                GameObject nowTeleport;
                StreamWriter writer = null;

                /*
                 *      0   nothing
                 *      1   head dwelling
                 *      2   gaze dwelling bottom
                 *      3   gaze menu start
                 */
                int state = 0;

                private void Start() {

                    gazeMenu = GameObject.Find("gaze_menu_4");
                    ren = gazeMenu.GetComponentsInChildren<Renderer>();
                    info = GameObject.Find("Info");
                    hint = GameObject.Find("Hint");
                    if (!SRanipal_Eye_Framework.Instance.EnableEye) {
                        enabled = false;
                        return;
                    }
                    Assert.IsNotNull(GazeRayRenderer);
                    cg = GameObject.Find("ring_text").GetComponent<CanvasGroup>();
                    toy = GameObject.Find("Toy").GetComponent<Toy>();

                    mainCamera = GameObject.Find("Main Camera");
                    progressbarBg = GameObject.Find("progressbar_bg");
                    progressbarFg = GameObject.Find("progressbar_fg");
                    dwellMenu = GameObject.Find("Dwell Menu");
                    progressbarBg.SetActive(false);
                    progressbarFg.SetActive(false);
                    info.SetActive(false);
                    dwellMenu.SetActive(false);

                    teleports = GameObject.FindGameObjectsWithTag("teleport");
                    nowTeleport = GameObject.Find("teleporter (0)");

                    GameObject.Find("ring_text").GetComponent<Ring_text>().enabled = false;
                   if (technique == Technique.GazeDock) {
                        GameObject.Find("Menu Trigger").SetActive(false);
                    } else {

                    }
                }
                

                private void Update() {
                    RaycastHit hit;
                    if (Physics.Raycast(new Ray(mainCamera.transform.position + mainCamera.transform.forward * PENETRATE_GAZEDOCK, mainCamera.transform.forward), out hit, 10)) {
                        GameObject g = hit.collider.gameObject;
                        if (g.name != headNameLast) {
                            progressbarBg.SetActive(false);
                            progressbarFg.SetActive(false);
                            // show effect
                            if (g.tag == "loc") {
                                g.GetComponentInChildren<ParticleSystem>().Play();
                            }
                            // toy or loc
                            if (toy.activeObject.Contains(g.name.Split('-')[0]) || g.tag == "loc") {
                                headFocus = g;
                                if (technique == Technique.Dwell) {
                                    if (state == 0) {
                                        state = 1;
                                        dwellCounter = DWELL_START_COUNTER;
                                        Log("dwell start head");
                                    }
                                }
                            } else {
                                if (technique == Technique.Dwell) {
                                    if (state == 1) {
                                        state = 0;
                                        dwellCounter = 0;
                                        Log("dwell quit head");
                                    }
                                }
                            }
                        }
                        headNameLast = g.name;
                    } else {
                        progressbarBg.SetActive(false);
                        progressbarFg.SetActive(false);
                        headNameLast = "";
                        if (state == 1) {
                            state = 0;
                            dwellCounter = 0;
                            Log("dwell quit head");
                        }
                    }
                    
                }

                private void FixedUpdate() {
                    // counter
                    if (dwellCounter > 0) {
                        dwellCounter--;
                        if (dwellCounter == DWELL_PROGRESS_COUNTER) {
                            progressbarBg.SetActive(true);
                            progressbarFg.SetActive(true);
                            Debug.Log("yyy " + progressbarBg.activeSelf);
                        }
                        if (dwellCounter < DWELL_PROGRESS_COUNTER) {
                            RenewProgressbar(1.0f * (DWELL_PROGRESS_COUNTER - dwellCounter) / DWELL_PROGRESS_COUNTER);
                        }
                        if (dwellCounter == 0) {
                            progressbarBg.SetActive(false);
                            progressbarFg.SetActive(false);
                            if (state == 1) {
                                // trigger menu
                                headTarget = headFocus;
                                dwellMenu.SetActive(true);
                                state = 3;
                                Log("trigger dwell");
                            } else if (state == 2) {
                                // gaze forward
                                if (gazeFocus.name == "Trigger 0") {
                                    MoveForward();
                                    state = 0;
                                    Log("forward gaze");
                                } else if (gazeFocus.name == "Trigger 1") {
                                    headTarget = null;
                                    dwellMenu.SetActive(true);
                                    state = 3;
                                    Log("trigger gaze");
                                }
                            } else if (state == 3) {
                                // select gaze menu
                                if (gazeFocus.name == "Item 0") {
                                    int res = toy.Pick(headTarget);
                                    Log("pick " + res);
                                } else if (gazeFocus.name == "Item 1") {
                                    int res = toy.Drop(headTarget);
                                    Log("drop " + res);
                                } else if (gazeFocus.name == "Item 2") {
                                    int res = MoveForward();
                                    Log("move " + res);
                                } else if (gazeFocus.name == "Item 3") {
                                    Log("exit");
                                }
                                dwellMenu.SetActive(false);
                                state = 0;
                            }
                        }
                    }
                    if (infoCounter > 0) {
                        infoCounter--;
                        if (infoCounter == 0) {
                            info.SetActive(false);
                        }
                    }

                    // get gaze
                    initialMenu();
                    border = Oval.Border;
                    if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
                        SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;
                    Vector3 GazeOriginCombinedLocal, GazeDirectionCombinedLocal;
                    if (SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { } else if (SRanipal_Eye.GetGazeRay(GazeIndex.LEFT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { } else if (SRanipal_Eye.GetGazeRay(GazeIndex.RIGHT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { } else { };
                    Vector3 GazeDirectionCombined = Camera.main.transform.TransformDirection(GazeDirectionCombinedLocal);
                    GazeRayRenderer.SetPosition(0, Camera.main.transform.position - Camera.main.transform.up * 0.05f);
                    GazeRayRenderer.SetPosition(1, Camera.main.transform.position + GazeDirectionCombined * LengthOfRay);
                    Log("gaze " + GazeDirectionCombined.x + " " + GazeDirectionCombined.y + " " + GazeDirectionCombined.z);

                    // gaze dock
                    if (technique == Technique.GazeDock) {
                        Vector3 Gaze_oval = GazeDirectionCombinedLocal - new Vector3(Oval.X0, Oval.Y0, 0);
                        Vector2 Choice = new Vector2(Gaze_oval.x, Gaze_oval.y);

                        // (0，0，1) is blink
                        if (!GazeDirectionCombinedLocal.Equals(new Vector3(0, 0, 1))) {
                            //in
                            if (GetVal(Choice, Gaze_oval, 1) && isStart) {
                                trigger = true;
                                ring_up();
                                Log("ringup");
                            } else if (trigger) {
                                // highlight
                                if (dis_out.Count > 0 && dis_out[dis_out.Count - 1].y > 0) {
                                    string str = GetChoice(Choice);
                                    if (!selection.Equals(str)) {
                                        if (area >= 0) {
                                            if (area_bf >= 0) {
                                                ren[area_bf % 4].material = bf;
                                            }
                                            bf = ren[area % 4].material;
                                            ren[area % 4].material = s;
                                        }
                                        area_bf = area;
                                        selection = str;
                                    }
                                }
                                ring_up();
                            }
                            // out
                            if (GetVal(Choice, Gaze_oval, 0)) {
                                if (trigger && isStart) {
                                    trigger = false;
                                    //RaiseInfo(selection);
                                    if (selection == "↑") {
                                        int res = MoveForward();
                                        Log("move " + res);
                                    } else if (selection == "Pick") {
                                        int res = toy.Pick(headFocus);
                                        Log("pick " + res);
                                    } else if (selection == "Drop") {
                                        int res = toy.Drop(headFocus);
                                        Log("drop " + res);
                                    }
                                    dis_in.Clear();
                                    Log("ringdown");
                                }
                                ring_down();
                            }
                        } else {
                            dis_in.Clear();
                            dis_out.Clear();
                        }
                        time_recorder += Time.fixedDeltaTime;
                    }

                    // dwell
                    if (technique == Technique.Dwell) {
                        ring_down();
                        RaycastHit hit;
                        if (Physics.Raycast(new Ray(Camera.main.transform.position - Camera.main.transform.up * 0.05f + GazeDirectionCombined * PENETRATE_GAZEDOCK, GazeDirectionCombined), out hit)) {
                            GameObject g = hit.collider.gameObject;
                            if (g.name != gazeNameLast) {
                                progressbarBg.SetActive(false);
                                progressbarFg.SetActive(false);
                                if (gazeFocus != null && gazeFocus.name.Substring(0, 4) == "Item") {
                                    gazeFocus.GetComponent<Renderer>().material.color = Color.red;
                                }
                                if (g.name.Substring(0, 4) == "Item") {
                                    g.GetComponent<Renderer>().material.color = Color.yellow;
                                    dwellCounter = DWELL_START_COUNTER;
                                    gazeFocus = g;
                                    Log("dwell start item");
                                } else if (g.name.Substring(0, 4) == "Trig") {
                                    if (technique == Technique.Dwell) {
                                        if (state == 0) {
                                            state = 2;
                                            dwellCounter = DWELL_START_COUNTER;
                                            Log("dwell start gaze");
                                        }
                                    }
                                    gazeFocus = g;
                                } else {
                                    if (state == 2) {
                                        state = 0;
                                        dwellCounter = 0;
                                        Log("dwell quit gaze");
                                    }
                                    if (state == 3) {
                                        dwellCounter = 0;
                                        Log("dwell quit item");
                                    }
                                }
                            }
                            gazeNameLast = g.name;
                        } else {
                            progressbarBg.SetActive(false);
                            progressbarFg.SetActive(false);
                            gazeNameLast = "";
                            if (state == 2) {
                                state = 0;
                                dwellCounter = 0;
                                Log("dwell quit gaze");
                            }
                            if (state == 3) {
                                dwellCounter = 0;
                                Log("dwell quit item");
                            }
                        }
                    }


                    if (Input.GetKey(KeyCode.W)) {
                        Vector3 vc = mainCamera.transform.forward;
                        vc.y = 0;
                        GameObject.Find("Movable").transform.Translate(vc / vc.magnitude * 0.1f);
                    }
                    if (Input.GetKey(KeyCode.S)) {
                        Vector3 vc = mainCamera.transform.forward;
                        vc.y = 0;
                        GameObject.Find("Movable").transform.Translate(vc / vc.magnitude * -0.1f);
                    }
                }

                public int MoveForward() {
                    Vector3 vc = mainCamera.transform.forward;
                    vc.y = 0;
                    GameObject best = null;
                    float bestDist = 1e20f;
                    foreach (GameObject teleport in teleports) {
                        if (teleport == nowTeleport) continue;
                        Vector3 vt = teleport.transform.position - nowTeleport.transform.position;
                        double angle = Math.Acos((vc.x * vt.x + vc.z * vt.z) / vc.magnitude / vt.magnitude);
                        if (angle / Math.PI * 180 < 30 && vt.magnitude < bestDist) {
                            bestDist = vt.magnitude;
                            best = teleport;
                        }
                    }
                    if (bestDist < 15) {
                        GameObject.Find("Movable").transform.Translate(best.transform.position - nowTeleport.transform.position);
                        nowTeleport = best;
                        return 1;
                    }
                    return 0;
                }

                public void RaiseInfo(string s) {
                    info.SetActive(true);
                    info.GetComponent<Text>().text = s;
                    infoCounter = INFO_COUNTER;
                }

                void RenewProgressbar(float r) {
                    RectTransform rt = progressbarFg.GetComponent<RectTransform>();
                    float w = r * 120;
                    rt.anchoredPosition = new Vector3(-60 + w / 2, rt.anchoredPosition.y, 0);
                    rt.sizeDelta = new Vector2(w, 20);
                }

                // select: 1=in, 0=out;   1: true=in;   0: true=out;
                // dis_in: [ d2=abs angle, d1=rel angle ... ]
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
                            if (d1 < 0)  val = Check(0);
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
                    if (!isStart) return;
                    for (int j = 0; j < ren.Length; j++) {
                        Material m = ren[j].material;
                        if (m.color.a > 0) {
                            m.color = new Color(m.color.r, m.color.g, m.color.b, Math.Max(m.color.a - 0.02f, 0));
                            cg.alpha = Math.Max(cg.alpha - 0.02f, 0);
                        }
                    }
                }

                public void changeIsStart() {
                    writer = new StreamWriter(new FileStream("game-" + DateTime.Now.ToString("yyMMdd-hhmmss") + "-" + technique + ".txt", FileMode.Create));
                }

                void Log(string s) {
                    long t = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
                    if (writer != null) {
                        writer.WriteLine(t + " " + s);
                    }
                }
                
                void initialMenu() {
                    GameObject[] texts = GameObject.FindGameObjectsWithTag("text");
                    menu = new string[texts.Length + 1];
                    for (int i = 0; i < texts.Length; i++) {
                        GameObject t = texts[i];
                        if (t.name == "Text (3)") {
                            t.transform.SetParent(GameObject.Find("ring_text").transform, false);
                            t.GetComponent<Text>().text = "Pick";
                            t.GetComponent<RectTransform>().transform.localPosition = new Vector3(-489, -105);
                        }
                        if (t.name == "Text (5)") {
                            t.transform.SetParent(GameObject.Find("ring_text").transform, false);
                            t.GetComponent<Text>().text = "Drop";
                            t.GetComponent<RectTransform>().transform.localPosition = new Vector3(455, -105);
                        }
                        if (t.name == "Text (8)") {
                            t.transform.SetParent(GameObject.Find("ring_text").transform, false);
                            t.GetComponent<Text>().text = "Exit";
                            t.GetComponent<RectTransform>().transform.localPosition = new Vector3(-34, -450);
                        }
                        if (t.name == "Text (11)") {
                            t.transform.SetParent(GameObject.Find("ring_text").transform, false);
                            t.GetComponent<Text>().text = "↑";
                            t.GetComponent<RectTransform>().transform.localPosition = new Vector3(-33, 305);
                        }
                        menu[i] = t.GetComponent<Text>().text;
                    }
                    menu[texts.Length] = menu[0];
                }

                // 1=check in  0=check out
                private bool Check(int select) {

                    bool result = false;
                    if (select == 1) {
                        var arr = dis_in.ToArray();
                        if (arr.Length != window_size_in) return false;
                        if (arr[0].x < 15 && arr[11].y > 0 && arr[window_size_in - 1].x > 0) result = true;
                    } else if (select == 0) {
                        var arr = dis_out.ToArray();
                        if (arr.Length != window_size_out) return false;
                        if (arr[window_size_out - 1].y < 0) return true;
                    }

                    return result;
                }
            }
        }
    }
}
