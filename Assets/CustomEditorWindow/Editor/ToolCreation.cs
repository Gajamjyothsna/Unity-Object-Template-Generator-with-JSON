using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;


public class ToolCreation : EditorWindow
{
    // Variables to store UI data
    private string objectName = "GameObject";
    private Vector3 position = Vector3.zero;
    private Vector3 scale = Vector3.one;
    private Vector3 rotation = Vector3.zero;
    private Color color = Color.white;
    private string parentObjectName = string.Empty;


    private bool createTextElement = false;
    private bool createImageElement = false;


    [MenuItem("Tools/Object Generation Tool")]
    public static void ShowWindow()
    {
        LoadUIObjects();
        GetWindow(typeof(ToolCreation));
    }
    #region Private Variables
    private static string jsonFilePath = "Assets/Resources/UICustomdata.json"; // Adjust the path as needed
   

    private static List<UICustomObjectData> uiCustomObjectList = new List<UICustomObjectData>();

    private GameObject _setParentObject;

    private bool showTextField = false;
    private string textFieldContent = "";
    private string userInputText = "DefaultText";
    private GUIStyle centeredLabelStyle;
    #endregion

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
        // Initialize GUIStyle for centered label if not already initialized
        if (centeredLabelStyle == null)
        {
            centeredLabelStyle = new GUIStyle(EditorStyles.boldLabel);
            centeredLabelStyle.alignment = TextAnchor.MiddleCenter;
            centeredLabelStyle.fixedHeight = 50; // Set the height as needed
        }

        GUILayout.Label("Create UI Object", EditorStyles.boldLabel);

        if (createTextElement)
        {
            userInputText = EditorGUILayout.TextField("User Input Text:", userInputText);
        }

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Create Text Element"))
        {
            createTextElement = true;
            createImageElement = false;
        }

        if (GUILayout.Button("Create Image Element"))
        {
            createTextElement = false;
            createImageElement = true;
        }

        GUILayout.EndHorizontal();


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
        if(GUILayout.Button("Edit Particular UI Object")) // Button to edit particular UI object
        {
            showTextField = !showTextField; //Based on Variable
        }
        if (showTextField)
        {
            textFieldContent = EditorGUILayout.TextField("Text Field:", textFieldContent);
            UpdateUIData(textFieldContent);
        }
    }

    private GameObject uiElement;
    private void CreateUIElement(string _parentName)
    {
       // uiElement = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer));
        if (createTextElement)
        {
            // Create a new UI GameObject
            uiElement = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        }
        else if(createImageElement)
        {
            // Create a new UI GameObject
            uiElement = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        }
      
        // Set position and size
        RectTransform rectTransform = uiElement.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = scale;
        rectTransform.localRotation = Quaternion.EulerAngles(rotation);

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

        CheckForParentObject(_parentName);

        if (createTextElement)
        {
            AddTextComponentToUIElement(uiElement);
        }
        else if (createImageElement)
        {
            Debug.Log("color " + color);
            AddImageComponentToUIElement(uiElement);
        }


        // Check if the specified parent object exists
        AddUICustomDataToList(objectName, position, scale, rotation, color, parentObjectName);
    }
    private void AddTextComponentToUIElement(GameObject element)
    {
        // Try to get the existing Image component, or add one if it doesn't exist
        Text textComponent = element.GetComponent<Text>();

        if (textComponent == null)
        {
            textComponent = element.AddComponent<Text>();
        }

        // Now safely assign the color
        if (textComponent != null)
        {
            textComponent.text = userInputText;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = 14;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.color = color;
        }
       
    }

    private void AddImageComponentToUIElement(GameObject element)
    {
        // Try to get the existing Image component, or add one if it doesn't exist
        Image imageComponent = element.GetComponent<Image>();

        if (imageComponent == null)
        {
            imageComponent = element.AddComponent<Image>();
        }

        // Now safely assign the color
        if (imageComponent != null)
        {
            imageComponent.color = color;
        }
    }
    private void AddUICustomDataToList(string _objectName, Vector3 _position, Vector3 _scale, Vector3 _rotation, Color _color, string parentObjectName)
    {
        UICustomObjectData _data = new UICustomObjectData(_objectName, _position,_scale, _rotation, _color, parentObjectName);
       
        uiCustomObjectList.Add(_data);
    }
    private void CheckForParentObject(string _parentName)
    {
        if (string.IsNullOrEmpty(_parentName))
        {
            return;
        }
        else
        {
            UICustomObjectData parentData = uiCustomObjectList.Find(data => data.objectName == _parentName);

            if (parentData != null)
            {
                Debug.Log("Parent is found");
                _setParentObject = GameObject.Find(_parentName);

                if (_setParentObject != null)
                {
                    Debug.Log("Parent Object: " + _setParentObject.name);
                    uiElement.transform.SetParent(_setParentObject.transform, true);
                }
                else
                {
                    Debug.Log("Parent GameObject not found.");
                }
            }
            else
            {
                Debug.Log("Parent Object not found in the uiCustomObjectList.");
            }
        }
    }
    private void UpdateUIData(string _uiObjectName) //based on the Object Name, updating the Data
    {
        UICustomObjectData updateData = uiCustomObjectList.Find(data => data.objectName == _uiObjectName);

        if (updateData != null)
        {
            showTextField = true;
            textFieldContent = _uiObjectName;
        }
        else
        {
            Debug.LogWarning("UI Object not found for update: " + _uiObjectName);
        }
    }
    private void UpdateData(UICustomObjectData updatedData)
    {
        if (uiCustomObjectList != null && uiCustomObjectList.Count > 0)
        {
            int index = uiCustomObjectList.FindIndex(data => data.objectName == updatedData.objectName);

            if (index != -1)
            {
                uiCustomObjectList[index] = updatedData;
                SaveUIObjects(); // Save the updated data to the JSON file
                showTextField = false; // Reset the text field after updating
            }
            else
            {
                Debug.LogError("Object not found in the list.");
            }
        }
        else
        {
            Debug.LogError("uiCustomObjectList is null or empty.");
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
