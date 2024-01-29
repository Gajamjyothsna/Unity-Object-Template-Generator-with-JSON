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
    private Vector2 anchorMin = Vector2.zero;
    private Vector2 anchorMax = Vector2.zero;

    //Menu item to open the Window
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

    private bool createTextElement = false;
    private bool createImageElement = false;

    private GameObject uiElement;
    private UICustomObjectData updateData;

    #endregion

    #region Private Methods
    private void OnEnable()
    {
        if (uiCustomObjectList.Count == 0) //Loading UI objects when the window is enabled
        {
            LoadUIObjects();
        }
    }
    private void OnGUI()
    {
        GUILayout.Label("Create UI Object", EditorStyles.boldLabel); //Creating Text

        if (createTextElement)
        {
            userInputText = EditorGUILayout.TextField("User Input Text:", userInputText); //If it is Text Button toggled, then enabling the text field
        }

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Create Text Element")) //Text Button
        {
            createTextElement = true;
            createImageElement = false;
        }

        if (GUILayout.Button("Create Image Element")) //Image Button
        {
            createTextElement = false;
            createImageElement = true;
        }

        GUILayout.EndHorizontal();

        //Storing the Variables from the User
        objectName = EditorGUILayout.TextField("Element Name:", objectName);
        position = EditorGUILayout.Vector3Field("Position:", position);
        scale = EditorGUILayout.Vector3Field("Size:", scale);
        rotation = EditorGUILayout.Vector3Field("Rotation:", rotation);
        color = EditorGUILayout.ColorField("Color:", color);
        parentObjectName = EditorGUILayout.TextField("Parent", parentObjectName);

        anchorMax = EditorGUILayout.Vector2Field("AnchorMax:", anchorMax); //storing anchor positions
        anchorMin = EditorGUILayout.Vector2Field("AnchorMin:", anchorMin);


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

        GUILayout.Label("Edit UI Object", EditorStyles.boldLabel);

        if (GUILayout.Button("Edit")) // Button to edit particular UI object
        {
            showTextField = !showTextField; // Toggle the text field based on the button click
        }

        if (showTextField)
        {
            textFieldContent = EditorGUILayout.TextField("Text Field:", textFieldContent);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Update UI Data"))
            {
                UpdateUIDataFromTextField(textFieldContent); //Opening text field for Edit
            }

            GUILayout.EndHorizontal();
        }
    }

    private void CreateUIElement(string _parentName)
    {
        if (createTextElement)
        {
            // Create a new UI Text GameObject
            uiElement = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        }
        else if(createImageElement)
        {
            // Create a new UI Image GameObject
            uiElement = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        }
      
        // Set position and size
        RectTransform rectTransform = uiElement.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = scale;
        rectTransform.localRotation = Quaternion.EulerAngles(rotation);

        rectTransform.anchorMin = anchorMin; // Set the anchor positions
        rectTransform.anchorMax = anchorMax;

        // Parent the UI element to the Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
       
        if (canvas == null)
        {
            // If no Canvas exists, create one
            GameObject canvasObject = new GameObject("Canvas", typeof(Canvas));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        uiElement.transform.SetParent(canvas.transform, false); //Making canvas as the parent

        CheckForParentObject(_parentName); //checking for the parent Object

        if (createTextElement)
        {
            AddTextComponentToUIElement(uiElement); 
            AddUICustomDataToList(objectName, position, scale, rotation, color, parentObjectName, userInputText, UIType.Text); //Adding data to the list
        }
        else if (createImageElement)
        {
            AddImageComponentToUIElement(uiElement);
            AddUICustomDataToList(objectName, position, scale, rotation, color, parentObjectName, "", UIType.Image); //Adding data into the list
        }
    }
    private void UpdateUIDataFromTextField(string _uiObjectName)
    {
        updateData = uiCustomObjectList.Find(data => data.objectName == _uiObjectName);

        GameObject uiGameObject = GameObject.Find(textFieldContent); //Finding the gameObject based on the input textfieldContent

        if (updateData != null)
        {
            Debug.Log("Name" + _uiObjectName);

            // updating other properties like position, scale, color, etc.
            RectTransform objectRectTransform = uiGameObject.GetComponent<RectTransform>();
            updateData.objectName = textFieldContent;
            updateData.scale = uiElement.transform.localScale;
            updateData.position = objectRectTransform.anchoredPosition;
            updateData.scale = objectRectTransform.sizeDelta;
            updateData.textData = uiGameObject.GetComponent<Text>().text;
            UpdateData(updateData);
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
            int index = uiCustomObjectList.FindIndex(data => data.objectName == updatedData.objectName); //checking the list with object name and getting index

            if (index != -1)
            {
                uiCustomObjectList[index] = updatedData; //with the index, updating the properties values from updatedData
                Debug.Log(index + ": " + updatedData.objectName);
                SaveUIObjects(); // Save the updated data to the JSON file
                showTextField = false; // Reset the text field after updating
            }
            else
            {
                Debug.Log("Object not found in the list.");
            }
        }
        else
        {
            Debug.Log("uiCustomObjectList is null or empty.");
        }
    }
    private void AddTextComponentToUIElement(GameObject element)
    {
        // Try to get the existing Image component, or add one if it doesn't exist
        Text textComponent = element.GetComponent<Text>();

        if (textComponent == null)
        {
            textComponent = element.AddComponent<Text>();
        }

        // Assigning the properties 
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

        // Assigning the properties
        if (imageComponent != null)
        {
            imageComponent.color = color;
        }
    }
    private void AddUICustomDataToList(string _objectName, Vector3 _position, Vector3 _scale, Vector3 _rotation, Color _color, string parentObjectName, string textData, UIType uiType)
    {
        UICustomObjectData _data = new UICustomObjectData(_objectName, _position,  _scale, _rotation, _color, parentObjectName, textData, uiType);
       
        uiCustomObjectList.Add(_data);
    }
    private void CheckForParentObject(string _parentName)
    {
        if (string.IsNullOrEmpty(_parentName)) //If parent is null
        {
            return;
        }
        else
        {
            UICustomObjectData parentData = uiCustomObjectList.Find(data => data.objectName == _parentName); //parent is not null, checking the list for parent

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
           // UpdateExistedData(_uiObjectName);
        }
        else
        {
            Debug.LogWarning("UI Object not found for update: " + _uiObjectName);
        }
    }
    private void UpdateExistedData(string _uiObjectName)
    {
        if (uiCustomObjectList != null && uiCustomObjectList.Count > 0)
        {
            UICustomObjectData updateData = uiCustomObjectList.Find(data => data.objectName == _uiObjectName);

            int index = uiCustomObjectList.FindIndex(data => data.objectName == updateData.objectName);

            if (index != -1)
            {
                uiCustomObjectList[index] = updateData;
                Debug.Log("Data" + uiCustomObjectList[index].objectName);
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

    // To allow proper serialization and deserialization of the list of UICustomObjectData objects using Unity's JsonUtility.
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
        public string textData;
        public string uiType;

        public UICustomObjectData(string _name, Vector3 _position, Vector3 _scale, Vector3 _rotation, Color _color, string _parentObjectName, string _textData, UIType _uiType)
        {
            objectName = _name;
            position = _position;
            rotation = _rotation;
            scale = _scale;
            color = _color;
            parentObjectName = _parentObjectName;
            textData = _textData;
            uiType = _uiType.ToString();
        }
    }
    [System.Serializable]
    public enum UIType
    {
        Text,
        Image
    }
    #endregion
}
