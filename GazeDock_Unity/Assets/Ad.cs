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
            public class Ad : MonoBehaviour {

                public int LengthOfRay = 25;
                [SerializeField]
                
                private LineRenderer GazeRayRenderer;
                //private LineRenderer _lineRender;
                private int s_counter = 0, bar_s = 0 , previous = 0;
                public int num = 8;
                public int serial_number = 0;
                private bool status = true;
                float x_min = 0, x_max = 0, y_min = 0, y_max = 0;
                private int counter = 0;
                List<float> l;
                Oval[] ovals;
                CanvasGroup cg;
                GameObject gazemenu;
                GameObject bars;
                Transform[] trans;
                public Material s;
                private Material bf;
                private string filepath;
                void Awake() {
                    //_lineRender = GetComponent<LineRenderer>();
                    
                }

                void Start() {
                    

                    bars = GameObject.Find("bars");
                    trans = bars.GetComponentsInChildren<Transform>();
                    gazemenu = GameObject.Find("gaze_menu_" + num);                   
                    ovals = GameObject.Find("gaze_menu_" + num).GetComponentsInChildren<Oval>();
                    gazemenu.SetActive(false);
                    foreach (Oval i in ovals) {
                        i.enabled = false;
                    }
                    Ring_text.Number = num;
                    cg = GameObject.Find("ring_text").GetComponent<CanvasGroup>();
                    cg.alpha = 0.00f;
                    if (!SRanipal_Eye_Framework.Instance.EnableEye) {
                        enabled = false;
                        return;
                    }
                    Assert.IsNotNull(GazeRayRenderer);
                    filepath = "No" + serial_number + ".txt";
                    
                    if (File.Exists("E:\\Gaze Dock\\rec\\" + filepath)) {
                        StreamReader str = new StreamReader(new FileStream("E:\\Gaze Dock\\rec\\" + filepath, FileMode.Open, FileAccess.Read));
                        float[] f = new float[4];
                        int i = 0;
                        while (true) {
                            string date = str.ReadLine();
                            if (date != null) {
                                f[i] = float.Parse(date);
                                i++;
                            } else
                                break;
                        }
                        Oval.A = f[0];
                        Oval.B = f[1];
                        Oval.X0 = f[2];
                        Oval.Y0 = f[3];
                        LengthOfRay = 0;
                        GazeRayRenderer.SetPosition(0, new Vector3(0, 0, 0));
                        GazeRayRenderer.SetPosition(1, new Vector3(0, 0, 0));
                        //GetComponent<LineRenderer>().enabled = false;
                        gazemenu.SetActive(true);
                        foreach (Oval o in ovals) {
                            o.enabled = true;
                        }
                        cg.alpha = 1.0f;
                        Oval.IsFinished = true;
                        //GameObject.Find("Info").transform.localPosition = new Vector3(20 * Oval.X0, 20 * (Oval.Y0 + 0.2f), 20);
                        //GameObject.Find("Hint").transform.localPosition = new Vector3(20 * Oval.X0, 20 * Oval.Y0, 20);
                        status = false;

                    } else {
                        FileStream create = new FileStream("E:\\Gaze Dock\\rec\\" + filepath, FileMode.CreateNew);
                        create.Close();
                    }
                    //Debug.Log(Oval.A +"/"+ Oval.B + "/" + Oval.X0 + "/" + Oval.Y0);
                }

              

                void FixedUpdate() {

                    if (status) {

                        if (Input.GetKeyDown(KeyCode.Space)) s_counter++;

                        if (s_counter == 0) {
                            AdjustY(1);
                        } else if (s_counter == 1) {
                            if (bar_s >= 10) {
                                bar_s = 0;
                                bf = GameObject.Find("bar_" + bar_s.ToString()).GetComponent<MeshRenderer>().material;
                                GameObject.Find("bar_" + bar_s.ToString()).GetComponent<MeshRenderer>().material = s;
                                previous = bar_s;
                            } 
                            AdjustY(-1);
                        } else if (s_counter == 2) {
                            Oval.B = 0.5f * (y_max - y_min);
                            Oval.Y0 = 0.5f * (y_max + y_min);
                            AdjustX(1);
                        } else if (s_counter == 3) {
                            AdjustX(-1);
                        } else {
                            Oval.A = 0.5f * (x_max - x_min);
                            Oval.X0 = 0.5f * (x_max + x_min);
                            foreach (Transform t in trans) {
                                t.localPosition = new Vector3(10, 10, -10);
                            }
                            LengthOfRay = 0;
                            GazeRayRenderer.SetPosition(0, new Vector3(0, 0, 0));
                            GazeRayRenderer.SetPosition(1, new Vector3(0, 0, 0));
                            GetComponent<LineRenderer>().enabled = false;
                            gazemenu.SetActive(true);
                            foreach (Oval i in ovals) {
                                i.enabled = true;
                            }
                            cg.alpha = 1.0f;
                            Oval.IsFinished = true;
                            StreamWriter writer = new StreamWriter(new FileStream("E:\\Gaze Dock\\rec\\" + filepath, FileMode.Append));
                            writer.WriteLine(Oval.A);
                            writer.WriteLine(Oval.B);
                            writer.WriteLine(Oval.X0);
                            writer.WriteLine(Oval.Y0);
                            writer.Close();
                            //GameObject.Find("Info").transform.localPosition = new Vector3(20 * Oval.X0, 20 * (Oval.Y0 + 0.2f), 20);
                            //GameObject.Find("Hint").transform.localPosition = new Vector3(20 *Oval.X0,20 * Oval.Y0, 20);
                            status = false;
                        } 


                    } else {
                        GetComponent<Ad>().enabled = false;
                    }
                                  
                }

                void AdjustY(int selection) {
                  
                    for (int i = 0; i < trans.Length; i++) {
                        trans[i].SetParent(Camera.main.transform, false);
                        if (selection == 1) trans[i].localPosition = new Vector3(Oval.X0, selection * Mathf.Tan((8 + i * 1.25f) * Mathf.PI / 180), 1);
                        else {
                            if(i < 10)  trans[i].localPosition = new Vector3(Oval.X0, selection * Mathf.Tan((45 - 2.5f * i) * Mathf.PI / 180), 1);
                            else  trans[i].localPosition = new Vector3(Oval.X0, selection * Mathf.Tan((45 - 2.5f * i) * Mathf.PI / 180), -1);
                        } ;
                    }
                    if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
                        SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;
                    Vector3 GazeOriginCombinedLocal, GazeDirectionCombinedLocal;
                    if (SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { } else if (SRanipal_Eye.GetGazeRay(GazeIndex.LEFT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { } else if (SRanipal_Eye.GetGazeRay(GazeIndex.RIGHT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { } else { };
                    Vector3 GazeDirectionCombined = Camera.main.transform.TransformDirection(GazeDirectionCombinedLocal);
                    GazeRayRenderer.SetPosition(0, Camera.main.transform.position - Camera.main.transform.up * 0.05f);
                    GazeRayRenderer.SetPosition(1, Camera.main.transform.position + GazeDirectionCombined * LengthOfRay);
                    //_lineRender.transform.SetParent(Camera.main.transform, false);
                    //_lineRender.positionCount = 2;
                    //_lineRender.startWidth = 0.01f;
                    //_lineRender.endWidth = 0.01f;
                    //_lineRender.SetPosition(0, new Vector3(Oval.X0, -2, 1));
                    //_lineRender.SetPosition(1, new Vector3(Oval.X0, 2, 1));
                    if (Input.GetKeyDown(KeyCode.UpArrow) && bar_s < 14 + selection * 4) bar_s ++;
                    if (Input.GetKeyDown(KeyCode.DownArrow) && bar_s >0 ) bar_s --;
                    if(bf !=null)GameObject.Find("bar_" + previous.ToString()).GetComponent<MeshRenderer>().material = bf;
                    bf = GameObject.Find("bar_" + bar_s.ToString()).GetComponent<MeshRenderer>().material;
                    GameObject.Find("bar_" + bar_s.ToString()).GetComponent<MeshRenderer>().material = s;
                    previous = bar_s;
                    if (selection == 1) y_max = selection * Mathf.Tan((15 + bar_s) * Mathf.PI / 180);
                    else y_min = selection * Mathf.Tan((45 - 2 * bar_s) * Mathf.PI / 180);
                }

                void AdjustX(int selection) {

                    
                    if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
                        SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;
                    Vector3 GazeOriginCombinedLocal, GazeDirectionCombinedLocal;
                    if (SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { } else if (SRanipal_Eye.GetGazeRay(GazeIndex.LEFT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { } else if (SRanipal_Eye.GetGazeRay(GazeIndex.RIGHT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { } else { };
                    Vector3 GazeDirectionCombined = Camera.main.transform.TransformDirection(GazeDirectionCombinedLocal);
                    GazeRayRenderer.SetPosition(0, Camera.main.transform.position - Camera.main.transform.up * 0.05f);
                    GazeRayRenderer.SetPosition(1, Camera.main.transform.position + GazeDirectionCombined * LengthOfRay);
                    //_lineRender.transform.SetParent(Camera.main.transform, false);
                    //_lineRender.positionCount = 2;
                    //_lineRender.startWidth = 0.01f;
                    //_lineRender.endWidth = 0.01f;
                    //_lineRender.SetPosition(0, new Vector3(2,Oval.Y0 , 1));
                    //_lineRender.SetPosition(1, new Vector3(-2,Oval.Y0 , 1));
                    for (int i = 0; i < trans.Length; i++) {
                        trans[i].SetParent(Camera.main.transform, false);
                        if(i < 10){
                            if (selection == 1) trans[i].localPosition = new Vector3(selection * Mathf.Tan((20 + i * 2.5f) * Mathf.PI / 180), Oval.Y0, 1);
                            else trans[i].localPosition = new Vector3(selection * Mathf.Tan((45 - i * 2.5f) * Mathf.PI / 180), Oval.Y0, 1);
                        }else  trans[i].localPosition = new Vector3(selection * Mathf.Tan((45 - i * 2.5f) * Mathf.PI / 180), Oval.Y0, -1);
                    }
                    if (Input.GetKeyDown(KeyCode.UpArrow) && bar_s < 10) bar_s ++;
                    if (Input.GetKeyDown(KeyCode.DownArrow) && bar_s > 0) bar_s --;
                    if (bf != null) GameObject.Find("bar_" + previous.ToString()).GetComponent<MeshRenderer>().material = bf;
                    bf = GameObject.Find("bar_" + bar_s.ToString()).GetComponent<MeshRenderer>().material;
                    GameObject.Find("bar_" + bar_s.ToString()).GetComponent<MeshRenderer>().material = s;
                    previous = bar_s;
                    if (selection == 1) x_max = selection * Mathf.Tan((25 + bar_s * 2) * Mathf.PI / 180);
                    else x_min = selection * Mathf.Tan((45 - bar_s * 2) * Mathf.PI / 180);
                }
            }




        }
    }
}

