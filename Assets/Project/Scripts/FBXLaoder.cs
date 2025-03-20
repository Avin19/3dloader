using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Siccity.GLTFUtility;
using Newtonsoft.Json;// Using Siccity's GLTFUtility

public class GLTFLoader : MonoBehaviour
{

    [SerializeField] private string iD = "1kltrtb2wVw0m6admIt-F5wM7VoVFSqFP_EsVnJoSWoQ";
    [SerializeField] private string apiKey = "AIzaSyAA23WLN6TWfFj_J1VXvYPUOCIMSXGo254";
    [SerializeField] private string _sheetName = "Sheet1";

    public Button leftButton, rightButton;

    [SerializeField] private List<GameObject> loadedModels = new List<GameObject>();
    private int currentModelIndex = 0;

    void Start()
    {
        Debug.Log("Starting GLTFLoader...");
        StartCoroutine(DownloadGLTFList());

        leftButton.onClick.AddListener(ShowPreviousModel);
        rightButton.onClick.AddListener(ShowNextModel);
    }

    IEnumerator DownloadGLTFList()
    {
        string googleSheetUrl = $"https://sheets.googleapis.com/v4/spreadsheets/{iD}/values/{_sheetName}?key={apiKey}";
        Debug.Log("Fetching model list from Google Sheets...");

        using (UnityWebRequest www = UnityWebRequest.Get(googleSheetUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {

                Debug.Log("Successfully downloaded Json data.");
                string jsonText = www.downloadHandler.text;
                List<string> gltfUrls = ParseJSON(jsonText);

                Debug.Log($"Total {gltfUrls.Count} models found.");
                foreach (string url in gltfUrls)
                {
                    Debug.Log($"Downloading model: {url}");
                    yield return StartCoroutine(DownloadAndLoadGLTF(url));
                }

                ShowModel(0); // Show the first model after loading
            }
            else
            {
                Debug.LogError("Failed to download CSV: " + www.error);
            }
        }
    }

    List<string> ParseJSON(string jsonText)
    {
        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError("JSON text is null or empty!");
            return new List<string>();
        }

        try
        {
            // Deserialize JSON using Newtonsoft.Json
            GoogleSheetResponse response = JsonConvert.DeserializeObject<GoogleSheetResponse>(jsonText);

            if (response == null || response.values == null)
            {
                Debug.LogError("Failed to parse JSON: response or values is null.");
                return new List<string>();
            }

            List<string> urls = new List<string>();

            foreach (var row in response.values)
            {
                if (row.Length > 0 && !string.IsNullOrEmpty(row[0])) // First column contains URL
                {
                    urls.Add("https://drive.google.com/uc?export=download&id=" + row[0]);
                }
            }

            Debug.Log($"Extracted {urls.Count} URLs from sheet.");
            return urls;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error parsing JSON: {ex.Message}");
            return new List<string>();
        }
    }

    IEnumerator DownloadAndLoadGLTF(string gltfUrl)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(gltfUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Successfully downloaded: {gltfUrl}");
                byte[] gltfData = www.downloadHandler.data;
                string filePath = $"{Application.persistentDataPath}/tempModel.glb";
                System.IO.File.WriteAllBytes(filePath, gltfData);
                Debug.Log($"Saved GLTF model to: {filePath}");

                Debug.Log("Loading GLTF model into scene...");
                GameObject model = Importer.LoadFromFile(filePath); // Siccity Import
                if (model != null)
                {
                    model.transform.position = Vector3.zero;
                    model.SetActive(false); // Hide initially
                    loadedModels.Add(model);
                    Debug.Log("Model successfully loaded and added to the list.");
                }
                else
                {
                    Debug.LogError("Failed to import GLTF model.");
                }
            }
            else
            {
                Debug.LogError("Failed to download GLTF: " + www.error);
            }
        }
    }

    void ShowModel(int index)
    {
        if (loadedModels.Count == 0)
        {
            Debug.LogWarning("No models loaded to display.");
            return;
        }

        Debug.Log($"Showing model at index: {index}");
        for (int i = 0; i < loadedModels.Count; i++)
        {
            loadedModels[i].SetActive(i == index);
        }

        currentModelIndex = index;
    }

    public void ShowNextModel()
    {
        if (loadedModels.Count == 0)
        {
            Debug.LogWarning("No models to switch.");
            return;
        }

        int nextIndex = (currentModelIndex + 1) % loadedModels.Count;
        Debug.Log($"Switching to next model: Index {nextIndex}");
        ShowModel(nextIndex);
    }

    public void ShowPreviousModel()
    {
        if (loadedModels.Count == 0)
        {
            Debug.LogWarning("No models to switch.");
            return;
        }

        int prevIndex = (currentModelIndex - 1 + loadedModels.Count) % loadedModels.Count;
        Debug.Log($"Switching to previous model: Index {prevIndex}");
        ShowModel(prevIndex);
    }
    [System.Serializable]
    public class GoogleSheetResponse
    {
        public string range;
        public string majorDimension;
        public List<string[]> values; // JSON format uses array of arrays
    }
}
