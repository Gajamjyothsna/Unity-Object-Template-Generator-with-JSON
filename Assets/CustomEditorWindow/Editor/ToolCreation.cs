using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class ToolCreation : EditorWindow
{
    // Variables to store UI data
    private string objectName = "GameObject";
    private Vector3 position = Vector3.zero;
    private Vector3 scale = Vector3.one;
    private Vector3 rotation = Vector3.zero;
    private Color color = Color.white;
    private string parentObjectName = string.Empty;

    [MenuItem("Tools/Object Generation Tool")]
    public static void ShowWindow()
    {
        LoadUIObjects();
        GetWindow(typeof(ToolCreation));
    }
    private static string jsonFilePath = "Assets/Resources/UICustomdata.json"; // Adjust the path as needed
   

    private static List<UICustomObjectData> uiCustomObjectList = new List<UICustomObjectData>();


    #region Private Methods
    private void OnEnable()
    {
        //jsonFilePath = "Assets/Resources/UICustomdata_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".json";
        if (uiCustomObjectList.Count == 0)
        {
            LoadUIObjects();
        }
    }
    private void OnGUI()
    {
        GUILayout.Label("Create UI Object", EditorStyles.boldLabel);

        objectName = EditorGUILayout.TextField("Element Name:", objectName);
        position = EditorGUILayout.Vector3Field("Position:", position);
        scale = EditorGUILayout.Vector3Field("Size:", scale);
        rotation = EditorGUILayout.Vector3Field("Rotation:", rotation);
        color = EditorGUILayout.ColorField("Color:", color);
        parentObjectName = EditorGUILayout.TextField("Parent", parentObjectName);

        if (GUILayout.Button("Create UI Element"))
        {
            CreateUIElement(parentObjectName); // Calling a method to instantiate the UI element
        }
        if (GUILayout.Button("Save to JSON"))
        {
            SaveUIObjects(); //Calling method to Save Data in JSON
        }

        if (GUILayout.Button("Load from JSON"))
        {
            LoadUIObjects(); //Calling method to Save Data in JSON
        }
    }
    
    private void CreateUIElement(string _parentName)
    {
        CheckForParentObject(_parentName);
        // Create a new UI GameObject
        GameObject uiElement = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

        // Set position and size
        RectTransform rectTransform = uiElement.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = scale;
        rectTransform.localRotation = Quaternion.EulerAngles(rotation);

        uiElement.GetComponent<Image>().color = color;  

        // Parent the UI element to the Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
       
        if (canvas == null)
        {
            // If no Canvas exists, create one
            GameObject canvasObject = new GameObject("Canvas", typeof(Canvas));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        uiElement.transform.SetParent(canvas.transform, false);

        // Check if the specified parent object exists
       

        AddUICustomDataToList(objectName, position, scale, rotation, color, parentObjectName);
    }
    private void AddUICustomDataToList(string _objectName, Vector3 _position, Vector3 _scale, Vector3 _rotation, Color _color, string parentObjectName)
    {
        UICustomObjectData _data = new UICustomObjectData(_objectName, _position,_scale, _rotation, _color, parentObjectName);
       
        uiCustomObjectList.Add(_data);
    }

    private void CheckForParentObject(string _parentName)
    {
        if (uiCustomObjectList.Count == 0)
        {
            return; 
        }
        else
        {
            string parentName = uiCustomObjectList.Find(x => x.objectName == _parentName).objectName;
            if (parentName == null)
            {
                return;
            }
            else
            {
                Debug.Log("Parent is found");
                GameObject parentObject = new GameObject(parentName);
                Debug.Log(parentObject.name);
            }
            
        }
    }
    #endregion


    #region JSON Serialization and Deserialization

    private void SaveUIObjects()
    {
        string json = JsonUtility.ToJson(new UICustomObjectListWrapper(uiCustomObjectList), true);
        System.IO.File.WriteAllText(jsonFilePath, json);
        Debug.Log("Data saved to JSON.");
        
    }
    private static void LoadUIObjects()
    {
        if (System.IO.File.Exists(jsonFilePath))
        {
            string json = System.IO.File.ReadAllText(jsonFilePath);
            UICustomObjectListWrapper dataWrapper = JsonUtility.FromJson<UICustomObjectListWrapper>(json);

            uiCustomObjectList = dataWrapper.uiCustomObjectList;

            Debug.Log("Data loaded from JSON.");
        }
        else
        {
            Debug.LogError("JSON file not found.");
        }


    }
    #endregion

    #region Wrapper Class

    [System.Serializable]
    private class UICustomObjectListWrapper
    {
        public List<UICustomObjectData> uiCustomObjectList;

        public UICustomObjectListWrapper(List<UICustomObjectData> list)
        {
            uiCustomObjectList = list;
        }
    }
    #endregion

    #region Data classs

    [System.Serializable]
    private class UICustomObjectData
    {
        public string objectName;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public Color color;
        public string parentObjectName;

        public UICustomObjectData(string _name, Vector3 _position, Vector3 _rotation, Vector3 _scale, Color _color, string _parentObjectName)
        {
            objectName = _name;
            position = _position;
            rotation = _rotation;
            scale = _scale;
            color = _color;
            parentObjectName = _parentObjectName;
        }
    }
    #endregion
}
