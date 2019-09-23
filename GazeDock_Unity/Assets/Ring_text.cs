using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Ring_text : MonoBehaviour {

    static int number = 8;
    public static int Number {
        get { return number; }
        set { number = value; }
    }
    private GameObject[] texts;
    private string[] mark = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P" };
    private float[] border;
    private int bia;

    void Start () {
        bia = UnityEngine.Random.Range(0, 8);
    }
	
	// Update is called once per frame
	void Update () {

        if (Oval.BorderCreated ) {
            texts = GameObject.FindGameObjectsWithTag("text");
            border = Oval.Border;

            for (int i = 0; i < texts.Length; i++) {
                if (i >= number) texts[i].SetActive(false);
                else {
                    float angle = 0;
                    if (i == 0) angle = (0.5f * (border[(number + i - 1) % number] + border[(number + i) % number] + 360));
                    else angle = (0.5f * (border[(number + i - 1) % number] + border[(number + i) % number]));
                    texts[i].transform.SetParent(GameObject.Find("ring_text").transform, false);
                    texts[i].GetComponent<RectTransform>().transform.localPosition = new Vector3(650 * (Oval.A * Mathf.Cos(angle * Mathf.PI / 180) + Oval.X0),
                           650 * (Oval.B * Mathf.Sin(angle * Mathf.PI / 180) + Oval.Y0), 0);
                    texts[i].GetComponent<Text>().text = mark[(i + bia) % number];
                    if (i % 2 == 1) texts[i].GetComponent<Text>().color = new Color(255, 255, 255);
                }
            }
        }
        //Debug.Log("Text:"+Oval.A + "/" + Oval.B + "/" + Oval.X0 + "/" + Oval.Y0);
    }

}
