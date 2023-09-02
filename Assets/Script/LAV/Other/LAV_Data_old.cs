using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LAV_Data_old : MonoBehaviour
{
    //private LAV_InteractionManager InteractionManager;
    private LAV_FaultMeasurement faultMeasurement;

    public string scene_Name;
    private float start_time, final_time, SphereDiameter_max, points;
    private string userID, DominantHand, Height, Width, Depth;

    private string BASE_URL = "https://docs.google.com/forms/u/0/d/e/1FAIpQLSeTBp3fXOXyZgRgJGhfjq_1moRgFZF-qh7D5NN9UGeAF6Gvsg/formResponse";

    void Start()
    {
        //InteractionManager = GameObject.Find("FaultInteraction").GetComponent<LAV_InteractionManager>();
        faultMeasurement = GameObject.Find("Fault").GetComponent<LAV_FaultMeasurement>();

        SphereDiameter_max = 0.010f;
        DominantHand = "Right";
    }
    void Update()
    {
    }
    public void StartTest()
    {
        Debug.Log(scene_Name);
        Debug.Log("StartTimer");
        start_time = Time.time;
    }
    public void FinishTest()
    {
        string path = Path.Combine(Application.persistentDataPath, "DATA" + ".txt");
        //string path = "DATA" + ".txt";

        final_time = (Time.time - start_time) * 1000;
        Debug.Log("Completion time in ms: " + final_time);

        userID = "0";

        Height = GameObject.Find("FaultCollection/Fault/TextObjects/HeightText").GetComponent<TextMeshPro>().text;
        Width = GameObject.Find("FaultCollection/Fault/TextObjects/WidthText").GetComponent<TextMeshPro>().text;
        Depth = GameObject.Find("FaultCollection/Fault/TextObjects/DepthText").GetComponent<TextMeshPro>().text;

        for (int i = 0; i < faultMeasurement.faultPoints.Count; i++) //Count parte da 1 e non da 0
        {
            if (SphereDiameter_max < faultMeasurement.faultPoints[i].diameter)
            {
                SphereDiameter_max = faultMeasurement.faultPoints[i].diameter;
            }
            //Debug.Log("Fault Point " + i.ToString() + " diameter " + faultMeasurement.faultPoints[i].diameter.ToString("F3"));
        }
        //Debug.Log("Fault Point diameter_max " + SphereDiameter_max.ToString("F3"));
        points = faultMeasurement.faultPoints.Count;

        string orarioinizio = "Day: " + System.DateTime.Now.ToString("dd-MM-yyyy") + " - Time: " + System.DateTime.Now.ToString("HH-mm-ss") +"\n";
        string userIDbk = "User: " + userID + "\n";
        string scenebk = "Scenario: " + scene_Name + " \n";
        string DominantHandbk = "DominantHand: " + DominantHand + "\n";
        string CompletionTimebk = "CompletionTime [ms]: " + final_time.ToString("0") + "\n";
        string Heightbk = "Height: " + Height + "\n";
        string Widthbk = "Width: " + Width + "\n";
        string Depthbk = "Depth: " + Depth + "\n";
        string SphereDiameter_maxString = "SphereDiameter_max: " + SphereDiameter_max.ToString("F3") + " m \n";
        string n_points = "N. points: " + points.ToString("0") + " \n\n";
        File.AppendAllText(path, orarioinizio);
        File.AppendAllText(path, userIDbk);
        File.AppendAllText(path, scenebk);
        File.AppendAllText(path, DominantHandbk);
        File.AppendAllText(path, CompletionTimebk);
        File.AppendAllText(path, Heightbk);
        File.AppendAllText(path, Widthbk);
        File.AppendAllText(path, Depthbk);
        File.AppendAllText(path, SphereDiameter_maxString);
        File.AppendAllText(path, n_points);

        StartCoroutine(Post(userID, scene_Name, DominantHand, final_time.ToString("0"), Height, Width, Depth, SphereDiameter_max.ToString("F3"), points.ToString("0")));
    }
    IEnumerator Post(string userID, string scenario, string DominantHand, string CompletionTime, string Height, string Width, string Depth, string SphereDiameterMax, string points)
    {   //cercare sul sito modulo google FB_(PUBLIC)_LOAD_DATA_ e poi il nome della stringa a cui aggiungere entry.
        WWWForm form = new WWWForm();
        form.AddField("entry.1365740091", userID);
        form.AddField("entry.1020569243", scenario);
        form.AddField("entry.409113231", DominantHand);
        form.AddField("entry.1507905106", CompletionTime);
        form.AddField("entry.1128882753", Height);
        form.AddField("entry.918044996", Width);
        form.AddField("entry.592462508", Depth);
        form.AddField("entry.889894526", SphereDiameterMax);
        form.AddField("entry.603769062", points);
        byte[] rawData = form.data;
        WWW www = new WWW(BASE_URL, rawData);
        yield return www;
    }
    public void Right_Hand()
    {
       DominantHand = "Right";
    }
    public void Left_Hand()
    {
      DominantHand = "Left";
    }
}
