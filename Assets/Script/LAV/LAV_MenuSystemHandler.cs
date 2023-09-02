using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vuforia;

public class LAV_MenuSystemHandler : MonoBehaviour
{
    [Header("UI Menu")]
    public GameObject Interaction_Menu;
    public GameObject Crack_Menu;
    public GameObject Manual_Test_Menu;
    public GameObject CV_Test_Menu;
    public GameObject Tutorial_Test_Menu;
    public GameObject Mesh_Mover_Menu;
    public GameObject Line_Alignment_Menu;

    [Header("Crack ImageTarget")]
    public GameObject Crack_1;
    public GameObject Crack_2;
    public GameObject Crack_3;
    public GameObject Crack_4;
    public GameObject Crack_5;

    //[Header("UI Manager Reference")]
    //public UIManager ui_manager;

    [Header("Sound")]
    public AudioSource sound_Start_Finish;
    public AudioSource sound_camera_shutter;
    public AudioSource sound_Photo;
    public GameObject UILoading;

    [HideInInspector] public string interactionType, crackType, Hand = "Right";
    private GameObject activeCrackMover;
    private float ti = 1f / 100f, ri = 5f;

    private LAV_Data LAV_Data;
    private UIManager UIManager;
    private CameraControl CameraControl;
    public enum UIStates
    {
        none = 0,
        Interaction_Menu = 1,
        Crack_Menu = 2,
        Manual_Test_Menu = 3,
        CV_Test_Menu = 4,
        Tutorial_Test_Menu = 5,
        Mesh_Mover_Menu = 6,
        Line_Alignment_Menu = 7,
    }
    private readonly Dictionary<UIStates, List<GameObject>> stateObjects = new Dictionary<UIStates, List<GameObject>>
    {
        {UIStates.Interaction_Menu, new List<GameObject>{} },
        {UIStates.Crack_Menu, new List<GameObject>{} },
        {UIStates.Manual_Test_Menu, new List<GameObject>{} },
        {UIStates.CV_Test_Menu, new List<GameObject>{} },
        {UIStates.Tutorial_Test_Menu, new List<GameObject>{} },
        {UIStates.Mesh_Mover_Menu, new List<GameObject>{} },
        {UIStates.Line_Alignment_Menu, new List<GameObject>{} },
    };

    private void Awake()
    {
        LAV_Data = GameObject.Find("FaultInteraction").GetComponent<LAV_Data>();
        UIManager = GameObject.Find("MixedRealityPlayspace/Main Canvas").GetComponent<UIManager>();
        CameraControl = GameObject.Find("MixedRealityPlayspace/Main Canvas").GetComponent<CameraControl>();
        Scene scene = SceneManager.GetActiveScene();

        //UIStates from Interaction_Menu
        this.transform.Find("Interaction_Menu/Button_Collection/Tutorial_Interaction_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.Crack_Menu); interactionType = "Tutorial"; });
        this.transform.Find("Interaction_Menu/Button_Collection/Manual_Interaction_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.Crack_Menu); interactionType = "Manual"; });
        this.transform.Find("Interaction_Menu/Button_Collection/CV_Interaction_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.Crack_Menu); interactionType = "CV"; });
        this.transform.Find("Interaction_Menu/Button_Collection/CV_Human_Interaction_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.Crack_Menu); interactionType = "CV+Manual"; });
        this.transform.Find("Interaction_Menu/Button_Collection/Human_CV_Interaction_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.Crack_Menu); interactionType = "Manual+CV"; });
        this.transform.Find("Interaction_Menu/Button_Collection/Mesh_Mover_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.Crack_Menu); interactionType = "Mesh-Mover"; });

        //UI States from Manual_Test_Menu
        this.transform.Find("Manual_Test_Menu/Back_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.Interaction_Menu); SceneManager.LoadScene(scene.name); });
        this.transform.Find("Manual_Test_Menu/Button_Collection/Hand_Method/Hand_Left_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { Hand = "Left"; });
        this.transform.Find("Manual_Test_Menu/Button_Collection/Hand_Method/Hand_Right_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { Hand = "Right"; });
        this.transform.Find("Manual_Test_Menu/Button_Collection/Flag_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { LAV_Data.flag = !LAV_Data.flag; Debug.Log($"Flag toggled. New state: {LAV_Data.flag}."); });
        this.transform.Find("Manual_Test_Menu/Button_Collection/Start_Button").GetComponent<Interactable>().OnClick.AddListener(
            delegate ()
            {
                this.transform.Find("Manual_Test_Menu/Button_Collection/Start_Button").gameObject.SetActive(false);
                this.transform.Find("Manual_Test_Menu/Button_Collection/Hand_Method/Hand_Left_Button").gameObject.SetActive(false);
                this.transform.Find("Manual_Test_Menu/Button_Collection/Hand_Method/Hand_Right_Button").gameObject.SetActive(false);
                this.transform.Find("Manual_Test_Menu/Button_Collection/Finish_Button").gameObject.SetActive(true);
                LAV_Data.StartTest();
                if (interactionType == "Manual")
                {
                    sound_Start_Finish.Play();
                    this.transform.Find("Manual_Test_Menu/Button_Collection/Flag_Button").gameObject.SetActive(false);
                    UIManager.Manual_Interface();
                }
                if (interactionType == "CV+Manual")
                {
                    CameraControl.Initialize();
                    this.transform.Find("Manual_Test_Menu/Button_Collection/Flag_Button").gameObject.SetActive(true);
                    UIManager.CV_Then_Human_Interface();
                    CameraControl.TakeDelayedPhoto(); StartCoroutine(SoundWaitPhoto()); 

                }
                if (interactionType == "Manual+CV")
                {
                    CameraControl.Initialize();
                    this.transform.Find("Manual_Test_Menu/Button_Collection/Flag_Button").gameObject.SetActive(true);
                    UIManager.Human_Then_CV_Interface();
                    CameraControl.TakeDelayedPhoto(); StartCoroutine(SoundWaitPhoto());
                }

            });
        this.transform.Find("Manual_Test_Menu/Button_Collection/Finish_Button").GetComponent<Interactable>().OnClick.AddListener(
            delegate () {
                LAV_Data.FinishTest();
                sound_Start_Finish.Play();
                if (!(interactionType == "Manual"))
                {
                    CameraControl.ReleaseCamera();
                    this.transform.Find("Manual_Test_Menu/Button_Collection/Flag_Button").gameObject.SetActive(false);
                    this.transform.Find("Manual_Test_Menu/Button_Collection/Finish_Button").gameObject.SetActive(false);
                }
                this.transform.Find("Manual_Test_Menu/Button_Collection/Finish_Button").gameObject.SetActive(false);
                /*SceneManager.LoadScene(scene.name);  */
            });

        //UI States from CV_Test_Menu
        this.transform.Find("CV_Test_Menu/Back_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.Interaction_Menu); SceneManager.LoadScene(scene.name); });
        this.transform.Find("CV_Test_Menu/Button_Collection/Hand_Method/Hand_Left_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { Hand = "Left"; });
        this.transform.Find("CV_Test_Menu/Button_Collection/Hand_Method/Hand_Right_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { Hand = "Right"; });
        this.transform.Find("CV_Test_Menu/Button_Collection/Flag_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { LAV_Data.flag = !LAV_Data.flag; Debug.Log($"Flag toggled. New state: {LAV_Data.flag}."); });
        this.transform.Find("CV_Test_Menu/Button_Collection/Start_Button").GetComponent<Interactable>().OnClick.AddListener(
            delegate ()
            {
                CameraControl.Initialize();
                this.transform.Find("CV_Test_Menu/Button_Collection/Start_Button").gameObject.SetActive(false);
                this.transform.Find("CV_Test_Menu/Button_Collection/Hand_Method/Hand_Left_Button").gameObject.SetActive(false);
                this.transform.Find("CV_Test_Menu/Button_Collection/Hand_Method/Hand_Right_Button").gameObject.SetActive(false);
                this.transform.Find("CV_Test_Menu/Button_Collection/Flag_Button").gameObject.SetActive(true);
                this.transform.Find("CV_Test_Menu/Button_Collection/Photo_Button").gameObject.SetActive(true);
                this.transform.Find("CV_Test_Menu/Button_Collection/Finish_Button").gameObject.SetActive(true);
                LAV_Data.StartTest();
                UIManager.CV_Only_Interface();
                CameraControl.TakeDelayedPhoto(); StartCoroutine(SoundWaitPhoto());
            });
        this.transform.Find("CV_Test_Menu/Button_Collection/Finish_Button").GetComponent<Interactable>().OnClick.AddListener(
            delegate () {
                LAV_Data.FinishTest();
                sound_Start_Finish.Play(); 
                CameraControl.ReleaseCamera(); 
                this.transform.Find("CV_Test_Menu/Button_Collection/Finish_Button").gameObject.SetActive(false);
                this.transform.Find("CV_Test_Menu/Button_Collection/Flag_Button").gameObject.SetActive(false);
                this.transform.Find("CV_Test_Menu/Button_Collection/Photo_Button").gameObject.SetActive(false);

            });
        this.transform.Find("CV_Test_Menu/Button_Collection/Flag_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { });
        this.transform.Find("CV_Test_Menu/Button_Collection/Photo_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { CameraControl.TakeDelayedPhoto(); StartCoroutine(SoundWaitPhoto()); });

        //UI States from Tutorial_Test_Menu
        this.transform.Find("Tutorial_Test_Menu/Back_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.Interaction_Menu); SceneManager.LoadScene(scene.name); });
        this.transform.Find("Tutorial_Test_Menu/Button_Collection/Hand_Method/Hand_Left_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { Hand = "Left"; });
        this.transform.Find("Tutorial_Test_Menu/Button_Collection/Hand_Method/Hand_Right_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { Hand = "Right"; });
        this.transform.Find("Tutorial_Test_Menu/Button_Collection/Start_Button").GetComponent<Interactable>().OnClick.AddListener(
            delegate ()
            {
                CameraControl.Initialize();
                this.transform.Find("Tutorial_Test_Menu/Button_Collection/Start_Button").gameObject.SetActive(false);
                this.transform.Find("Tutorial_Test_Menu/Button_Collection/Hand_Method/Hand_Left_Button").gameObject.SetActive(false);
                this.transform.Find("Tutorial_Test_Menu/Button_Collection/Hand_Method/Hand_Right_Button").gameObject.SetActive(false);
                this.transform.Find("Tutorial_Test_Menu/Button_Collection/Flag_Button").gameObject.SetActive(true);
                this.transform.Find("Tutorial_Test_Menu/Button_Collection/Photo_Button").gameObject.SetActive(true);
                this.transform.Find("Tutorial_Test_Menu/Button_Collection/Finish_Button").gameObject.SetActive(true);
                UIManager.Tutorial_Interface();
                CameraControl.TakeDelayedPhoto(); StartCoroutine(SoundWaitPhoto());
            });
        this.transform.Find("Tutorial_Test_Menu/Button_Collection/Finish_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { sound_Start_Finish.Play(); CameraControl.ReleaseCamera(); /*SceneManager.LoadScene(scene.name);  */this.transform.Find("Manual_Test_Menu/Button_Collection/Finish_Button").gameObject.SetActive(false); });
        this.transform.Find("Tutorial_Test_Menu/Button_Collection/Flag_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { });//TODO do something with the flag button
        this.transform.Find("Tutorial_Test_Menu/Button_Collection/Photo_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { CameraControl.TakeDelayedPhoto(); StartCoroutine(SoundWaitPhoto()); });


        //UI States from Mesh Mover Menu
        this.transform.Find("Mesh_Mover_Menu/Back_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.Interaction_Menu); });

        this.transform.Find("Mesh_Mover_Menu/Button_Collection/tx+").GetComponent<Interactable>().OnClick.AddListener(delegate () { Vector3 pos = activeCrackMover.transform.localPosition; pos.x += ti; activeCrackMover.transform.localPosition = pos; updateText(); });
        this.transform.Find("Mesh_Mover_Menu/Button_Collection/ty+").GetComponent<Interactable>().OnClick.AddListener(delegate () { Vector3 pos = activeCrackMover.transform.localPosition; pos.y += ti; activeCrackMover.transform.localPosition = pos; updateText(); });
        this.transform.Find("Mesh_Mover_Menu/Button_Collection/tz+").GetComponent<Interactable>().OnClick.AddListener(delegate () { Vector3 pos = activeCrackMover.transform.localPosition; pos.z += ti; activeCrackMover.transform.localPosition = pos; updateText(); });

        this.transform.Find("Mesh_Mover_Menu/Button_Collection/tx-").GetComponent<Interactable>().OnClick.AddListener(delegate () { Vector3 pos = activeCrackMover.transform.localPosition; pos.x -= ti; activeCrackMover.transform.localPosition = pos; updateText(); });
        this.transform.Find("Mesh_Mover_Menu/Button_Collection/ty-").GetComponent<Interactable>().OnClick.AddListener(delegate () { Vector3 pos = activeCrackMover.transform.localPosition; pos.y -= ti; activeCrackMover.transform.localPosition = pos; updateText(); });
        this.transform.Find("Mesh_Mover_Menu/Button_Collection/tz-").GetComponent<Interactable>().OnClick.AddListener(delegate () { Vector3 pos = activeCrackMover.transform.localPosition; pos.z -= ti; activeCrackMover.transform.localPosition = pos; updateText(); });

        this.transform.Find("Mesh_Mover_Menu/Button_Collection/rx+").GetComponent<Interactable>().OnClick.AddListener(delegate () { Vector3 rot = activeCrackMover.transform.localEulerAngles; rot.x += ri; activeCrackMover.transform.localEulerAngles = rot; updateText(); });
        this.transform.Find("Mesh_Mover_Menu/Button_Collection/ry+").GetComponent<Interactable>().OnClick.AddListener(delegate () { Vector3 rot = activeCrackMover.transform.localEulerAngles; rot.y += ri; activeCrackMover.transform.localEulerAngles = rot; updateText(); });
        this.transform.Find("Mesh_Mover_Menu/Button_Collection/rz+").GetComponent<Interactable>().OnClick.AddListener(delegate () { Vector3 rot = activeCrackMover.transform.localEulerAngles; rot.z += ri; activeCrackMover.transform.localEulerAngles = rot; updateText(); });

        this.transform.Find("Mesh_Mover_Menu/Button_Collection/rx-").GetComponent<Interactable>().OnClick.AddListener(delegate () { Vector3 rot = activeCrackMover.transform.localEulerAngles; rot.x -= ri; activeCrackMover.transform.localEulerAngles = rot; updateText(); });
        this.transform.Find("Mesh_Mover_Menu/Button_Collection/ry-").GetComponent<Interactable>().OnClick.AddListener(delegate () { Vector3 rot = activeCrackMover.transform.localEulerAngles; rot.y -= ri; activeCrackMover.transform.localEulerAngles = rot; updateText(); });
        this.transform.Find("Mesh_Mover_Menu/Button_Collection/rz-").GetComponent<Interactable>().OnClick.AddListener(delegate () { Vector3 rot = activeCrackMover.transform.localEulerAngles; rot.z -= ri; activeCrackMover.transform.localEulerAngles = rot; updateText(); });

        this.transform.Find("Mesh_Mover_Menu/Button_Collection/t+").GetComponent<Interactable>().OnClick.AddListener(delegate () { ti = ti * 1.5f; updateText(); });
        this.transform.Find("Mesh_Mover_Menu/Button_Collection/t-").GetComponent<Interactable>().OnClick.AddListener(delegate () { ti = ti / 1.5f; updateText(); });

        this.transform.Find("Mesh_Mover_Menu/Button_Collection/r+").GetComponent<Interactable>().OnClick.AddListener(delegate () { ri = ri * 1.5f; updateText(); });
        this.transform.Find("Mesh_Mover_Menu/Button_Collection/r-").GetComponent<Interactable>().OnClick.AddListener(delegate () { ri = ri / 1.5f; updateText(); });

        //UI States for the line alignment menu (y axis is the forwards/backwards for some stupid reason. Calling it z axis because that's what it should be)
        this.transform.Find("Line_Alignment_Menu/Button_Collection/tz-").GetComponent<Interactable>().OnClick.AddListener(delegate () { Vector3 pos = activeCrackMover.transform.localPosition; pos.y -= ti; activeCrackMover.transform.localPosition = pos; updateText(); });
        this.transform.Find("Line_Alignment_Menu/Button_Collection/tz+").GetComponent<Interactable>().OnClick.AddListener(delegate () { Vector3 pos = activeCrackMover.transform.localPosition; pos.y += ti; activeCrackMover.transform.localPosition = pos; updateText(); });

        void updateText()
        {
            this.transform.Find("Mesh_Mover_Menu/Transform Position").GetComponent<TextMesh>().text = $"({activeCrackMover.transform.localPosition.x:F6} m\n  ,{activeCrackMover.transform.localPosition.y:F6} m\n    ,{activeCrackMover.transform.localPosition.z:F6} m)";
            this.transform.Find("Mesh_Mover_Menu/Transform Rotation").GetComponent<TextMesh>().text = $"({activeCrackMover.transform.localEulerAngles.x:F6} deg\n  ,{activeCrackMover.transform.localEulerAngles.y:F6} deg\n    ,{activeCrackMover.transform.localEulerAngles.z:F6} deg)";
            this.transform.Find("Mesh_Mover_Menu/Position Increment").GetComponent<TextMesh>().text = $"{ti:F6} m";
            this.transform.Find("Mesh_Mover_Menu/Rotation Increment").GetComponent<TextMesh>().text = $"{ri:F3} deg";
        }

        
    }

    void Start()
    {
        //Initilize UI element dict
        stateObjects[UIStates.Interaction_Menu].Add(Interaction_Menu);
        stateObjects[UIStates.Crack_Menu].Add(Crack_Menu);
        stateObjects[UIStates.Manual_Test_Menu].Add(Manual_Test_Menu);
        stateObjects[UIStates.CV_Test_Menu].Add(CV_Test_Menu);
        stateObjects[UIStates.Tutorial_Test_Menu].Add(Tutorial_Test_Menu);
        stateObjects[UIStates.Mesh_Mover_Menu].Add(Mesh_Mover_Menu);
        stateObjects[UIStates.Line_Alignment_Menu].Add(Line_Alignment_Menu);

        //stateObjects[UIStates.All].AddRange(stateObjects[UIStates.main_menu].Union(stateObjects[UIStates.vis_options_menu]).Union(stateObjects[UIStates.depth_options_menu]).Union(stateObjects[UIStates.spatial_mesh_menu]).Union(stateObjects[UIStates.debug_options_menu]));
        ChangeUIState(UIStates.Interaction_Menu);
    }
    void Update()
    {
        //UI States from Crack_Menu
        this.transform.Find("Crack_Menu/Back_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.Interaction_Menu); });
        if (interactionType == "Tutorial")
        {
            this.transform.Find("Line_Alignment_Menu/Button_Collection/Finish_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.Tutorial_Test_Menu); hideLines(); });
        }
        if (interactionType == "Manual")
        {
            this.transform.Find("Line_Alignment_Menu/Button_Collection/Finish_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.Manual_Test_Menu); hideLines(); });
        }
        if (interactionType == "CV")
        {
            this.transform.Find("Line_Alignment_Menu/Button_Collection/Finish_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.CV_Test_Menu); hideLines(); });
        }
        if (interactionType == "CV+Manual")
        {
            this.transform.Find("Line_Alignment_Menu/Button_Collection/Finish_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.Manual_Test_Menu); hideLines(); });
        }
        if (interactionType == "Manual+CV")
        {
            this.transform.Find("Line_Alignment_Menu/Button_Collection/Finish_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.Manual_Test_Menu); hideLines(); });
        }

        if (interactionType == "Mesh-Mover")
        {
            this.transform.Find("Crack_Menu/Button_Collection/Crack 1_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.Mesh_Mover_Menu); crackType = "1"; turnOffCracks(); Crack_1.SetActive(true); activeCrackMover = Crack_1.transform.parent.gameObject; });
            this.transform.Find("Crack_Menu/Button_Collection/Crack 2_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.Mesh_Mover_Menu); crackType = "2"; turnOffCracks(); Crack_2.SetActive(true); activeCrackMover = Crack_2.transform.parent.gameObject; });
            this.transform.Find("Crack_Menu/Button_Collection/Crack 3_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.Mesh_Mover_Menu); crackType = "3"; turnOffCracks(); Crack_3.SetActive(true); activeCrackMover = Crack_3.transform.parent.gameObject; });
            this.transform.Find("Crack_Menu/Button_Collection/Crack 4_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.Mesh_Mover_Menu); crackType = "4"; turnOffCracks(); Crack_4.SetActive(true); activeCrackMover = Crack_4.transform.parent.gameObject; });
            this.transform.Find("Crack_Menu/Button_Collection/Crack 5_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.Mesh_Mover_Menu); crackType = "5"; turnOffCracks(); Crack_5.SetActive(true); activeCrackMover = Crack_5.transform.parent.gameObject; });
        }

        if (interactionType != "Mesh-Mover")
        {
            //UI States for the crack selection menu
            this.transform.Find("Crack_Menu/Button_Collection/Crack 1_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.Line_Alignment_Menu); crackType = "1"; turnOffCracks(); Crack_1.SetActive(true); activeCrackMover = Crack_1.transform.parent.gameObject; });
            this.transform.Find("Crack_Menu/Button_Collection/Crack 2_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.Line_Alignment_Menu); crackType = "2"; turnOffCracks(); Crack_2.SetActive(true); activeCrackMover = Crack_2.transform.parent.gameObject; });
            this.transform.Find("Crack_Menu/Button_Collection/Crack 3_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.Line_Alignment_Menu); crackType = "3"; turnOffCracks(); Crack_3.SetActive(true); activeCrackMover = Crack_3.transform.parent.gameObject; });
            this.transform.Find("Crack_Menu/Button_Collection/Crack 4_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.Line_Alignment_Menu); crackType = "4"; turnOffCracks(); Crack_4.SetActive(true); activeCrackMover = Crack_4.transform.parent.gameObject; });
            this.transform.Find("Crack_Menu/Button_Collection/Crack 5_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.Line_Alignment_Menu); crackType = "5"; turnOffCracks(); Crack_5.SetActive(true); activeCrackMover = Crack_5.transform.parent.gameObject; });
        }

        void hideLines()
        {
            activeCrackMover.transform.Find("AlignmentLine_1").gameObject.SetActive(false);
            activeCrackMover.transform.Find("AlignmentLine_2").gameObject.SetActive(false);
            if (VuforiaBehaviour.Instance != null)
            {
                UIManager.Singleton.AddToLog("Disabling Vuforia instance.");
                VuforiaBehaviour.Instance.enabled = false;
            }
        }

        void turnOffCracks()
        {
            Crack_1.SetActive(false);
            Crack_2.SetActive(false);
            Crack_3.SetActive(false);
            Crack_4.SetActive(false);
            Crack_5.SetActive(false);
        }

        UILoading.transform.Find("Canvas/Image").Rotate(0, 0, 50f * Time.deltaTime);
    }
    public void ChangeUIState(UIStates state)
    {
        ClearAllUI();
        List<GameObject> TempList = new List<GameObject>();//Create a List to hold return of dict call
        TempList = stateObjects[state]; //assign list values from the reference passed UI state into temp list
        foreach (GameObject listgameobject in TempList)
        {
            listgameobject.SetActive(true);//set the specific gameobjects active
        }

    }
    private void ClearAllUI()
    {
        foreach (KeyValuePair<UIStates, List<GameObject>> entry in stateObjects)
        {
            foreach (GameObject go in entry.Value)
            {
                go.SetActive(false);
            }
        }
    }
    IEnumerator SoundWaitPhoto()
    {
        UILoading.transform.Find("PictureFrame").gameObject.SetActive(true);
        sound_Photo.Play(); 
        yield return new WaitForSeconds(3);
        UILoading.transform.Find("PictureFrame").gameObject.SetActive(false);
        sound_Photo.Stop();
        sound_camera_shutter.Play();
        UILoading.transform.Find("Canvas").gameObject.SetActive(true);
    }
}
