//========= Copyright 2018, HTC Corporation. All rights reserved. ===========
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

namespace ViveSR
{
    namespace anipal
    {
        namespace Eye
        {
            public class SRanipal_GazeRaySample : MonoBehaviour
            {
                public int LengthOfRay = 25;
                [SerializeField] private LineRenderer GazeRayRenderer;
                private GameObject gazeMenu;            
                private GameObject info;
                private GameObject hint;
                private CanvasGroup cg; 
                private Renderer[] ren;
                private string filepath;
                private string[] menu;
                private  bool con = true;
                private bool isStart = false;
                private bool trigger = false;
                private bool rec = false;                
                private int[] randomInt;
                public int rounds = 1;
                private int count = 0;
                private float[] thro_in;
                private float[] thro_out;
                private float[] border = { 10, 55, 125, 170, 180, 155, 112.5f, 67.5f, 25 };
                private void Start()
                {

                    filepath = "2-8-" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".txt";
                    FileStream create = new FileStream("E:\\Gaze Dock\\rec\\" + filepath, FileMode.CreateNew);
                    create.Close();
                    gazeMenu = GameObject.Find("gaze_menu_8");
                    ren = gazeMenu.GetComponentsInChildren<Renderer>();             
                    info = GameObject.Find("Info");
                    hint = GameObject.Find("Hint");
                    if (!SRanipal_Eye_Framework.Instance.EnableEye)
                    {
                        enabled = false;
                        return;
                    }
                    Assert.IsNotNull(GazeRayRenderer);
                    thro_in = creatRadius("in");
                    thro_out = creatRadius("out");
                    cg  = GameObject.Find("ring_text").GetComponent<CanvasGroup>();

                }

                private void Update()
                {

                    initialMenu();
                    if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
                        SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;
                    Vector3 GazeOriginCombinedLocal, GazeDirectionCombinedLocal;
                    if (SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { }
                    else if (SRanipal_Eye.GetGazeRay(GazeIndex.LEFT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { }
                    else if (SRanipal_Eye.GetGazeRay(GazeIndex.RIGHT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { }
                    else return;
                    Vector3 GazeDirectionCombined = Camera.main.transform.TransformDirection(GazeDirectionCombinedLocal);
                    GazeRayRenderer.SetPosition(0, Camera.main.transform.position - Camera.main.transform.up * 0.05f);
                    GazeRayRenderer.SetPosition(1, Camera.main.transform.position + GazeDirectionCombined * LengthOfRay);                  
                    Vector2 Choice = new Vector2(GazeDirectionCombinedLocal.x,GazeDirectionCombinedLocal.y);
                    if (count == rounds * (menu.Length -1) +1) hint.GetComponent<TextMesh>().text = "Round Finished";
                    if(count == 0) hint.GetComponent<TextMesh>().text = nextCount();
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyUp(KeyCode.Space)) rec = !rec;
                    if (GetVal(Choice,GazeDirectionCombinedLocal,1) && isStart ) {
                        trigger = true;
                        ring_up();
                    } 
                    else if(GetVal(Choice, GazeDirectionCombinedLocal,0)) {
                        if (trigger&&isStart) {
                            trigger = false;
                            info.GetComponent<TextMesh>().text="You have selected\n"+GetChoice(Choice);                                                
                        }
                          ring_down();                   
                    }          
                    if (rec) {
                        StreamWriter writer = new StreamWriter(new FileStream("E:\\Gaze Dock\\rec\\" + filepath, FileMode.Append));
                        writer.WriteLine(randomInt[(count -1) % 8] +" " +GazeDirectionCombinedLocal.x + " " + GazeDirectionCombinedLocal.y 
                            + " " + GazeDirectionCombinedLocal.z);
                        writer.Close();                       
                    }
                    if(Input.GetKeyUp(KeyCode.Space)) hint.GetComponent<TextMesh>().text = nextCount();


                }

                private bool GetVal(Vector2 vec, Vector3 gaze,int select) {

                    float d;
                    Vector2 Bar = new Vector2(1, 0);
                    int angle =(int) Vector2.Angle(Bar, vec);
                    if (vec.y > 0) angle += 180;
                    if(select == 1)  d = Vector3.Angle(new Vector3(0, 0, 1), gaze) - thro_in[angle];
                    else  d = thro_out[angle] - Vector3.Angle(new Vector3(0, 0, 1), gaze);
                    return (d > 0) ? true : false; 

                }


                private string GetChoice(Vector2 vec) {

                    float angle = 0;
                    string choi = "None";
                    

                    Vector2 Bar = new Vector2(1, 0);
                    angle = Vector2.Angle(Bar, vec);
                    for (int i = 0; i <border.Length; i++) {
                        if (angle < border[4 + (int)Mathf.Sign(vec.y) * (i - 4)]) {
                            choi = menu[4 + (int)Mathf.Sign(vec.y) * (i - 4)];
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

                public  void changeCon() {
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
                    for(int i = 0; i < texts.Length; i++) {
                        menu[i] = texts[i].GetComponent<Text>().text;
                    }
                    menu[texts.Length] = menu[0];
                }

                private int[] CreateRandom() {

                    int[] arr = Enumerable.Range(1, menu.Length -1 ).ToArray();
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
                    if (count  % 8 == 0 || count == 0 )  randomInt = CreateRandom();
                    mes = menu[randomInt[count % 8] - 1 ];
                    count++;
                    return mes;
                }




            }
        } 
    }
}
