using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using static LAV_InteractionManager;

public class LAV_Data : MonoBehaviour
{
    private LAV_FaultMeasurement faultMeasurement;
    private LAV_InteractionFault LAV_InteractionFault;
    private LAV_InteractionScale LAV_InteractionScale;
    private LAV_MenuSystemHandler LAV_MenuSystemHandler;
    private CameraControl CameraControl;
    private AGS_Interaction_CV AGS_Interaction_CV;
    private float start_time, CompletionTime, SphereDiameter_max, points, existing_point_interactions, no_created_points, interactions_tot;
    private string userID, Height, Width, Depth;
    private int CV_points_tot, CV_points_final, pictures;
    public bool flag;
    private string CV_rawData;
    private string Final_rawData;

    string path, path_excel;

    void Start()
    {
        faultMeasurement = GameObject.Find("Fault").GetComponent<LAV_FaultMeasurement>();
        LAV_InteractionFault = GameObject.Find("FaultInteraction").GetComponent<LAV_InteractionFault>();
        LAV_InteractionScale = GameObject.Find("FaultInteraction").GetComponent<LAV_InteractionScale>();
        LAV_MenuSystemHandler = GameObject.Find("Menu_Crack").GetComponent<LAV_MenuSystemHandler>();
        CameraControl = GameObject.Find("MixedRealityPlayspace/Main Canvas").GetComponent<CameraControl>();
        AGS_Interaction_CV = GameObject.Find("FaultInteraction").GetComponent<AGS_Interaction_CV>();

        SphereDiameter_max = 0.0015875f;
        flag = false;
    }
    void Update()
    {
    }
    public void StartTest()
    {
        Debug.Log(LAV_MenuSystemHandler.interactionType);
        Debug.Log(LAV_MenuSystemHandler.crackType);
        Debug.Log(LAV_MenuSystemHandler.Hand);
        Debug.Log("StartTimer");
        start_time = Time.time;
    }
    public void FinishTest()
    {
        Debug.Log($"Finish clicked. Flag state: {flag}");
        if (!Application.isEditor)
        {
            path = Path.Combine(Application.persistentDataPath + "/FolderData", "DATA" + ".txt");
            path_excel = Path.Combine(Application.persistentDataPath + "/FolderData", "DATA" + ".CSV");
            if (!Directory.Exists(Application.persistentDataPath + "/FolderData"))
            { Directory.CreateDirectory(Application.persistentDataPath + "/FolderData"); }
            Debug.Log(Application.persistentDataPath + "/FolderData");
        }
        else
        {
            string FolderData = "FolderData/";
            path = "FolderData/DATA" + ".txt";
            path_excel = "FolderData/DATA" + ".CSV";
            if (!Directory.Exists(FolderData))
            { Directory.CreateDirectory(FolderData); }
        }

        CompletionTime = (Time.time - start_time) * 1000;
        Debug.Log("Completion time in ms: " + CompletionTime);

        userID = "";

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
            Vector3 Pos = faultMeasurement.faultPoints[i].position;
            float T = faultMeasurement.faultPoints[i].diameter;
            Vector3 Pos_i = faultMeasurement.faultPoints[i].position + new Vector3(T / 2, 0, 0);
            Vector3 Pos_j = faultMeasurement.faultPoints[i].position - new Vector3(T / 2, 0, 0);
            Vector2 pos = new Vector2();
            float t = 0;
            if (LAV_MenuSystemHandler.interactionType != "Manual")
            {
                Vector2 pos_i = AGS_Interaction_CV.get_xy_from_XYZ(Pos_i);
                Vector2 pos_j = AGS_Interaction_CV.get_xy_from_XYZ(Pos_j);
                pos = AGS_Interaction_CV.get_xy_from_XYZ(Pos);
                t = Vector2.Distance(pos_i, pos_j);
            }           
            
            Final_rawData += $"(x = {pos.x} y = {pos.y} t = {t} X = {Pos.x} Y = {Pos.y} Z = {Pos.z} T = {T}) ";
        }
        //Debug.Log("Fault Point diameter_max " + SphereDiameter_max.ToString("F3"));
        points = faultMeasurement.faultPoints.Count - AGS_Interaction_CV.currentPoints;
        existing_point_interactions = LAV_InteractionScale.existingPoint_interactions;
        no_created_points = LAV_InteractionFault.created_points_trial - points - existing_point_interactions;
        interactions_tot = existing_point_interactions + no_created_points + points;
        pictures = CameraControl.numberPictures;
        CV_points_tot = AGS_Interaction_CV.totalPoints;
        CV_points_final = AGS_Interaction_CV.currentPoints;
        CV_rawData = AGS_Interaction_CV.pointData;


        string StartTime_string = "Day: " + System.DateTime.Now.ToString("dd-MM-yyyy") + " - Time: " + System.DateTime.Now.ToString("HH-mm-ss") +"\n";
        string userID_String = "User: " + userID + "\n";
        string interaction_String = "Interaction type: " + LAV_MenuSystemHandler.interactionType + " \n";
        string crack_String = "Crack type: " + LAV_MenuSystemHandler.crackType + " \n";
        string Hand_String = "Hand: " + LAV_MenuSystemHandler.Hand + "\n";
        string CompletionTime_String = "CompletionTime [ms]: " + CompletionTime.ToString("0") + "\n";
        string Height_String = "Height: " + Height + "\n";
        string Width_String = "Width: " + Width + "\n";
        string Depth_String = "Depth: " + Depth + "\n";
        string SphereDiameter_maxString = "SphereDiameter_max: " + SphereDiameter_max.ToString("F3") + " m \n";
        string n_points = "N. points: " + points.ToString("0") + " \n";
        string n_point_interactions = "N. point interactions: " + existing_point_interactions.ToString("0") + " \n";
        string n_no_created_points = "N. no created points: " + no_created_points.ToString("0") + " \n";
        string n_interactions_tot = "N. interactions tot: " + interactions_tot.ToString("0") + " \n";
        string n_pictures = "N. pictures: " + pictures.ToString("0") + " \n";
        string n_CV_points_tot = "N. CV points tot: " + CV_points_tot.ToString("0") + " \n";
        string n_CV_points_final = "N. CV points final: " + CV_points_final.ToString("0") + " \n";
        string n_CV_rawData = "CV raw data: " + CV_rawData + "\n";
        string n_Final_rawData = "Final raw data: " + Final_rawData + "\n";
        string flag_state = "Flag active: " + flag.ToString() + " \n\n";


        string Day_string_excel = System.DateTime.Now.ToString("dd-MM-yyyy") + ";";
        string StartTime_string_excel = System.DateTime.Now.ToString("HH.mm.ss") +";";
        string userID_String_excel = userID + ";";
        string interaction_String_excel = LAV_MenuSystemHandler.interactionType + " ;";
        string crack_String_excel = LAV_MenuSystemHandler.crackType + ";";
        string Hand_String_excel = LAV_MenuSystemHandler.Hand + ";";
        string CompletionTime_String_excel = CompletionTime.ToString("0") + ";";
        string Height_String_excel = Height + ";";
        string Width_String_excel = Width + ";";
        string Depth_String_excel = Depth + ";";
        string SphereDiameter_maxString_excel = SphereDiameter_max.ToString("F3") + ";";
        string n_points_excel = points.ToString("0") + ";";
        string n_point_interactions_excel = existing_point_interactions.ToString("0") + ";";
        string n_no_created_points_excel = no_created_points.ToString("0") + ";";
        string n_interactions_tot_excel = interactions_tot.ToString("0") + " ;";
        string n_pictures_excel = pictures.ToString("0") + " ;";
        string n_CV_points_tot_excel = CV_points_tot.ToString("0") + " ;";
        string n_CV_points_final_excel = CV_points_final.ToString("0") + " ;";
        string n_CV_rawData_excel = CV_rawData + ";";
        string n_Final_rawData_excel = Final_rawData + ";";
        string flag_state_excel = flag.ToString() + " \n";

        if (!System.IO.File.Exists(path_excel))
        {
            Debug.Log("File Created");
            File.AppendAllText(path_excel, "Day;" + "StartTime;" + "UserID;" + "Interaction Type;" + 
                "Crack Type;" + "Hand;" + "Completion Time [ms];" + "Height [m];" + "Width [m];" + 
                "Depth [m];" + "Dmax [m];" + "Points;" + "Release interactions;" + "No created points;" + 
                "Total interactions;" + "N. pictures; " + "N. CV points tot; " + "N. CV points final; " + "CV Raw Data; " + "Final Raw Data; " + "Flag active; " + "\n");
        }
        File.AppendAllText(path_excel, Day_string_excel);
        File.AppendAllText(path_excel, StartTime_string_excel);
        File.AppendAllText(path_excel, userID_String_excel);
        File.AppendAllText(path_excel, interaction_String_excel);
        File.AppendAllText(path_excel, crack_String_excel);
        File.AppendAllText(path_excel, Hand_String_excel);
        File.AppendAllText(path_excel, CompletionTime_String_excel);
        File.AppendAllText(path_excel, Height_String_excel);
        File.AppendAllText(path_excel, Width_String_excel);
        File.AppendAllText(path_excel, Depth_String_excel);
        File.AppendAllText(path_excel, SphereDiameter_maxString_excel);
        File.AppendAllText(path_excel, n_points_excel);
        File.AppendAllText(path_excel, n_point_interactions_excel);
        File.AppendAllText(path_excel, n_no_created_points_excel);
        File.AppendAllText(path_excel, n_interactions_tot_excel);
        File.AppendAllText(path_excel, n_pictures_excel);
        File.AppendAllText(path_excel, n_CV_points_tot_excel);
        File.AppendAllText(path_excel, n_CV_points_final_excel);
        File.AppendAllText(path_excel, n_CV_rawData_excel);
        File.AppendAllText(path_excel, n_Final_rawData_excel);
        File.AppendAllText(path_excel, flag_state_excel);

        File.AppendAllText(path, StartTime_string);
        File.AppendAllText(path, userID_String);
        File.AppendAllText(path, interaction_String);
        File.AppendAllText(path, crack_String);
        File.AppendAllText(path, Hand_String);
        File.AppendAllText(path, CompletionTime_String);
        File.AppendAllText(path, Height_String);
        File.AppendAllText(path, Width_String);
        File.AppendAllText(path, Depth_String);
        File.AppendAllText(path, SphereDiameter_maxString);
        File.AppendAllText(path, n_points);
        File.AppendAllText(path, n_point_interactions);
        File.AppendAllText(path, n_no_created_points);
        File.AppendAllText(path, n_interactions_tot);
        File.AppendAllText(path, n_pictures);
        File.AppendAllText(path, n_CV_points_tot);
        File.AppendAllText(path, n_CV_points_final);
        File.AppendAllText(path, n_CV_rawData);
        File.AppendAllText(path, n_Final_rawData);
        File.AppendAllText(path, flag_state);
    }
}
