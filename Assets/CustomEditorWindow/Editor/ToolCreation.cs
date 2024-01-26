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

    [MenuItem("Tools/Object Generation Tool")]
    public static void ShowWindow()
    {
        GetWindow(typeof(ToolCreation));
    }
    private string jsonFilePath = "Assets/Resources/UICustomdata.json"; // Adjust the path as needed

    private List<UICustomObjectData> uiCustomObjectList = new List<UICustomObjectData>();

    private void OnEnable()
    {
       LoadUIObjects();
    }
    private void OnGUI()
    {
        GUILayout.Label("Create UI Object", EditorStyles.boldLabel);

        objectName = EditorGUILayout.TextField("Element Name:", objectName);
        position = EditorGUILayout.Vector3Field("Position:", position);
        scale = EditorGUILayout.Vector3Field("Size:", scale);
        rotation = EditorGUILayout.Vector3Field("Rotation:", rotation);
        color = EditorGUILayout.ColorField("Color:", color);

        if (GUILayout.Button("Create UI Element"))
        {
            CreateUIElement(); // Calling a method to instantiate the UI element
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
    private void CreateUIElement()
    {
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

        AddUICustomDataToList(objectName, position, scale, rotation, color);
    }
    private void AddUICustomDataToList(string _objectName, Vector3 _position, Vector3 _scale, Vector3 _rotation, Color _color)
    {
        UICustomObjectData _data = new UICustomObjectData(_objectName, _position, _scale, _rotation, _color);
       
        uiCustomObjectList.Add(_data);
    }

    private void SaveUIObjects()
    {
        string json = JsonUtility.ToJson(new UICustomObjectListWrapper(uiCustomObjectList), true);
        System.IO.File.WriteAllText(jsonFilePath, json);
        Debug.Log("Data saved to JSON.");
        //string json = JsonUtility.ToJson(uiCustomObjectList, true);
        //System.IO.File.WriteAllText(jsonFilePath, json);
        //Debug.Log("Data saved to JSON.");
    }
    private void LoadUIObjects()
    {
        //if (System.IO.File.Exists(jsonFilePath))
        //{
        //    string json = System.IO.File.ReadAllText(jsonFilePath);
        //    uiCustomObjectList = JsonUtility.FromJson<List<UICustomObjectData>>(json);

        //    if (uiCustomObjectList.Count > 0)
        //    {
        //        UICustomObjectData data = uiCustomObjectList[0];

        //        objectName = data.objectName;
        //        position = data.position;
        //        rotation = data.rotation;
        //        scale = data.scale;
        //        color = data.color;

        //        Debug.Log("Data loaded from JSON.");
        //    }
        //    else
        //    {
        //        Debug.LogError("No UI objects found in JSON.");
        //    }
        //}
        //else
        //{
        //    Debug.LogError("JSON file not found.");
        //}

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

    [System.Serializable]
    private class UICustomObjectListWrapper
    {
        public List<UICustomObjectData> uiCustomObjectList;

        public UICustomObjectListWrapper(List<UICustomObjectData> list)
        {
            uiCustomObjectList = list;
        }
    }

    [System.Serializable]
    private class UICustomObjectData
    {
        public string objectName;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public Color color;

        public UICustomObjectData(string _name, Vector3 _position, Vector3 _rotation, Vector3 _scale, Color _color)
        {
            objectName = _name;
            position = _position;
            rotation = _rotation;
            scale = _scale;
            color = _color;
        }
    }
}
