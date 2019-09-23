using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using ViveSR.anipal.Eye;

public class Toy : MonoBehaviour {
    const int N_TOY = 6;

    Game game;
    Dictionary<string, KeyValuePair<Vector3, Quaternion>> posTrue;
    Dictionary<string, KeyValuePair<Vector3, Quaternion>> posFalse;
    GameObject[] toys;
    public HashSet<string> activeObject;
    GameObject picked = null;

    void Start() {
        game = GameObject.Find("3-game").GetComponent<Game>();
        toys = GameObject.FindGameObjectsWithTag("toy");
        posTrue = ReadConfig("pos_true.txt");
        posFalse = ReadConfig("pos_false.txt");

        activeObject = new HashSet<string>();
        StreamReader reader = new StreamReader(new FileStream("order.txt", FileMode.Open));
        for (int i = 0; i < N_TOY; i++) {
            string line = reader.ReadLine();
            activeObject.Add(line);
        }

        foreach (GameObject toy in toys) {
            if (activeObject.Contains(toy.name)) {
                //Debug.Log(toy.name);
                toy.transform.position = posFalse[toy.name].Key;
                toy.transform.rotation = posFalse[toy.name].Value;
                //Debug.Log(toy.name + " " + toy.transform.position + " " + toy.transform.rotation);
            } else {
                //Debug.Log(toy.name + "_loc");
                GameObject.Find(toy.name + "_loc").SetActive(false);
                toy.SetActive(false);
            }
        }
    }
    
    void Update () {

    }

    public int Pick(GameObject g) {
        if (g == null) {
            if (picked == null) {
                game.RaiseInfo("You have nothing picked");
            } else {
                game.RaiseInfo("You have already picked " + picked.name.Substring(0, picked.name.Length - 3));
            }
            return 0;
        }
        g = GameObject.Find(g.name.Split('-')[0]);
        if (picked == null) {
            if (g.tag == "toy") {
                game.RaiseInfo("Picked " + g.name.Substring(0, g.name.Length - 3));
                picked = g;
                g.transform.parent = game.mainCamera.transform;
                g.transform.localPosition = new Vector3(0, 0, -2);
                return 1;
            } else {
                game.RaiseInfo("This cannot be picked");
            }
        } else {
            game.RaiseInfo("You have already picked " + picked.name.Substring(0, picked.name.Length - 3));
        }
        return 0;
    }

    public int Drop(GameObject g) {
        if (g == null) {
            game.RaiseInfo("No place to drop");
            return 0;
        }
        if (g.tag == "loc") {
            if (picked == null) {
                game.RaiseInfo("You have nothing picked");
            } else {
                if (picked.name == g.name.Substring(0, g.name.Length - 4)) {
                    picked.transform.parent = GameObject.Find("Toy").transform;
                    picked.transform.position = posTrue[picked.name].Key;
                    picked.transform.rotation = posTrue[picked.name].Value;
                    g.SetActive(false);
                    activeObject.Remove(picked.name);
                    picked = null;
                    return 1;
                } else {
                    game.RaiseInfo("Your object dosen't belong here");
                }
            }
        } else {
            game.RaiseInfo("This place cannot drop");
        }
        return 0;
    }

    void WriteConfig(string filename) {
        GameObject[] toys = GameObject.FindGameObjectsWithTag("toy");
        StreamWriter writer = new StreamWriter(new FileStream(filename, FileMode.OpenOrCreate));
        foreach (GameObject toy in toys) {
            Transform t = toy.transform;
            Vector3 p = t.position;
            Quaternion r = t.rotation;
            Vector3 s = t.lossyScale;
            writer.Write(toy.name + " " + p.x + " " + p.y + " " + p.z + " ");
            writer.Write(r.w + " " + r.x + " " + r.y + " " + r.z + " ");
            writer.WriteLine(s.x + " " + s.y + " " + s.z);
        }
        writer.Close();
    }

    Dictionary<string, KeyValuePair<Vector3, Quaternion>> ReadConfig(string filename) {
        GameObject[] toys = GameObject.FindGameObjectsWithTag("toy");
        StreamReader reader = new StreamReader(new FileStream(filename, FileMode.Open));
        Dictionary<string, KeyValuePair<Vector3, Quaternion>> dict = new Dictionary<string, KeyValuePair<Vector3, Quaternion>>();
        while (true) {
            string line = reader.ReadLine();
            if (line == null) break;
            string[] arr = line.Split(' ');
            GameObject g = GameObject.Find(arr[0]);
            if (g == null) {
                Debug.Log("can't find game object");
                continue;
            }
            Vector3 p = new Vector3(float.Parse(arr[1]), float.Parse(arr[2]), float.Parse(arr[3]));
            Quaternion r = new Quaternion(float.Parse(arr[5]), float.Parse(arr[6]), float.Parse(arr[7]), float.Parse(arr[4]));
            dict.Add(arr[0], new KeyValuePair<Vector3, Quaternion>(p, r));
        }
        reader.Close();
        return dict;
    }
}
