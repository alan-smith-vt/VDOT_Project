using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuSystemHandler : MonoBehaviour
{
    //public PredictiveMeshHandler_V7 predictivemeshhandlerref;
    public GameObject mainMenu;
    public GameObject visabilityOptionsMenu;
    public GameObject depthOptionsMenu;
    public GameObject spatialMeshMenu;
    public GameObject debugMenu;
    private MenuUtilities menuUtilitiesReference;
    public enum UIStates
    {
        none = 0,
        main_menu = 1,
        vis_options_menu = 2,
        depth_options_menu = 3,
        spatial_mesh_menu = 4,
        debug_options_menu = 5
    }
    private readonly Dictionary<UIStates, List<GameObject>> stateObjects = new Dictionary<UIStates, List<GameObject>>
    {
        {UIStates.main_menu, new List<GameObject>{} },
        {UIStates.vis_options_menu, new List<GameObject>{} },
        {UIStates.depth_options_menu, new List<GameObject>{} },
        {UIStates.spatial_mesh_menu, new List<GameObject>{} },
        {UIStates.debug_options_menu, new List<GameObject>{} }
    };

    private void Awake()
    {
        this.transform.Find("Main_Menu/Button_Collection/Visulization_Options_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.vis_options_menu); });
        this.transform.Find("Main_Menu/Button_Collection/Depth_Calc_Options_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.depth_options_menu); });
        this.transform.Find("Main_Menu/Button_Collection/Spatial_Mesh_Options_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.spatial_mesh_menu); });
        this.transform.Find("Main_Menu/Button_Collection/Debug_Options_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.debug_options_menu); });
        //Visabilty Options Menu
        this.transform.Find("Visability_Options_Menu/Back_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.main_menu); });
        //this.transform.Find("Visability_Options_Menu/Button_Collection/Spatial_Mesh_Toggle").GetComponent<Interactable>().OnClick.AddListener(delegate () { menuUtilitiesReference.ToggleSpatialMesh(this.transform.Find("Visability_Options_Menu/Button_Collection/Spatial_Mesh_Toggle").GetComponent<Interactable>().IsToggled); });
        //this.transform.Find("Visability_Options_Menu/Button_Collection/Hand_Menu_Toggle").GetComponent<Interactable>().OnClick.AddListener(delegate () { menuUtilitiesReference.ToggleHandPlane(this.transform.Find("Visability_Options_Menu/Button_Collection/Hand_Menu_Toggle").GetComponent<Interactable>().IsToggled); });
        //this.transform.Find("Visability_Options_Menu/Button_Collection/Raycast_Lines_Toggle").GetComponent<Interactable>().OnClick.AddListener(delegate () { menuUtilitiesReference.ToggleRaycastLines(this.transform.Find("Visability_Options_Menu/Button_Collection/Raycast_Lines_Toggle").GetComponent<Interactable>().IsToggled); });
        //this.transform.Find("Visulization_Options_Buttons").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.vis_options_menu); });
        //Depth Options Menu
        this.transform.Find("Depth_Options_Menu/Back_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.main_menu); });
        //this.transform.Find("Depth_Options_Menu/Button_Collection/Depth_Measure_Method/PureAverage").GetComponent<Interactable>().OnClick.AddListener(delegate () { predictivemeshhandlerref.currentDepthType = PredictiveMeshHandler_V7.DepthCalcType.PureAverage;});
        //this.transform.Find("Depth_Options_Menu/Button_Collection/Depth_Measure_Method/WeightedAverage").GetComponent<Interactable>().OnClick.AddListener(delegate () { predictivemeshhandlerref.currentDepthType = PredictiveMeshHandler_V7.DepthCalcType.WeightedAverage; });
        //this.transform.Find("Depth_Options_Menu/Button_Collection/Depth_Measure_Method/GazeWeightedAverage").GetComponent<Interactable>().OnClick.AddListener(delegate () { predictivemeshhandlerref.currentDepthType = PredictiveMeshHandler_V7.DepthCalcType.EyeWeightedAverage;});
        //Normal Options Menu
        //this.transform.Find("Depth_Options_Menu/Button_Collection/Normal_Measure_Method/PureAverage").GetComponent<Interactable>().OnClick.AddListener(delegate () { predictivemeshhandlerref.currentNormalType = PredictiveMeshHandler_V7.NormalCalcType.PureAverage; ; });
        //this.transform.Find("Depth_Options_Menu/Button_Collection/Normal_Measure_Method/GazeWeightedAverage").GetComponent<Interactable>().OnClick.AddListener(delegate () { predictivemeshhandlerref.currentNormalType = PredictiveMeshHandler_V7.NormalCalcType.WeightedAverage; });
        //this.transform.Find("Depth_Options_Menu/Button_Collection/Normal_Measure_Method/WeightedAverage").GetComponent<Interactable>().OnClick.AddListener(delegate () { predictivemeshhandlerref.currentNormalType = PredictiveMeshHandler_V7.NormalCalcType.EyeWeightedAverage;});
        //Outlier Options Menu
        //this.transform.Find("Depth_Options_Menu/Button_Collection/Oulier_Rejection/Yes").GetComponent<Interactable>().OnClick.AddListener(delegate () { predictivemeshhandlerref.isOutlierEnabled = true;});
        //this.transform.Find("Depth_Options_Menu/Button_Collection/Oulier_Rejection/No").GetComponent<Interactable>().OnClick.AddListener(delegate () { predictivemeshhandlerref.isOutlierEnabled = false;});
        //Mode Options Menu
        //this.transform.Find("Depth_Options_Menu/Button_Collection/Placement_Mode/Plane").GetComponent<Interactable>().OnClick.AddListener(delegate () { predictivemeshhandlerref.isDrawing = false;});
        //this.transform.Find("Depth_Options_Menu/Button_Collection/Placement_Mode/Paint").GetComponent<Interactable>().OnClick.AddListener(delegate () { predictivemeshhandlerref.isDrawing = true;});


        this.transform.Find("Spatial_Mesh_Options_Menu/Back_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.main_menu); });
        //this.transform.Find("Spatial_Mesh_Options_Menu/Button_Collection/Stop_Spatial_Mesh").GetComponent<Interactable>().OnClick.AddListener(delegate () { menuUtilitiesReference.ToggleSpatialMappingActive(false); });
        //this.transform.Find("Spatial_Mesh_Options_Menu/Button_Collection/Resume_Spatial_Mesh").GetComponent<Interactable>().OnClick.AddListener(delegate () { menuUtilitiesReference.ToggleSpatialMappingActive(false); });

        this.transform.Find("Debug_Options_Menu/Back_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { ChangeUIState(UIStates.main_menu); });
        //this.transform.Find("Debug_Options_Menu/Button_Collection/Debug_Console_Toggle").GetComponent<Interactable>().OnClick.AddListener(delegate () { menuUtilitiesReference.ToggleDebugConsole(this.transform.Find("Debug_Options_Menu/Button_Collection/Debug_Console_Toggle").GetComponent<Interactable>().IsToggled); });
        //this.transform.Find("Debug_Options_Menu/Button_Collection/Clear_Paint_Button").GetComponent<Interactable>().OnClick.AddListener(delegate () { predictivemeshhandlerref.DeleteAllPaint(); predictivemeshhandlerref.DeleteAllNormalObjects(); });

        menuUtilitiesReference = this.GetComponent<MenuUtilities>();

    }
    // Start is called before the first frame update
    void Start()
    {
        //Set up audio settings
        this.transform.Find("Visability_Options_Menu/Button_Collection/Spatial_Mesh_Toggle").GetComponent<Interactable>().IsToggled = false;
        this.transform.Find("Visability_Options_Menu/Button_Collection/Hand_Menu_Toggle").GetComponent<Interactable>().IsToggled = false;
        this.transform.Find("Visability_Options_Menu/Button_Collection/Raycast_Lines_Toggle").GetComponent<Interactable>().IsToggled = false;
        //menuUtilitiesReference.ToggleHandPlane(false);
        //menuUtilitiesReference.ToggleSpatialMesh(false);
        //menuUtilitiesReference.ToggleRaycastLines(false);
        //Initilize UI element dict
        stateObjects[UIStates.main_menu].Add(mainMenu);
        stateObjects[UIStates.vis_options_menu].Add(visabilityOptionsMenu);
        stateObjects[UIStates.depth_options_menu].Add(depthOptionsMenu);
        stateObjects[UIStates.spatial_mesh_menu].Add(spatialMeshMenu);
        stateObjects[UIStates.debug_options_menu].Add(debugMenu);
        //stateObjects[UIStates.All].AddRange(stateObjects[UIStates.main_menu].Union(stateObjects[UIStates.vis_options_menu]).Union(stateObjects[UIStates.depth_options_menu]).Union(stateObjects[UIStates.spatial_mesh_menu]).Union(stateObjects[UIStates.debug_options_menu]));
        ChangeUIState(UIStates.main_menu);


    }

    // Update is called once per frame
    void Update()
    {
        
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
}
