using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class EventData
{
    public string type;
    public string data;
}
[System.Serializable]
public class EventList
{
    public List<EventData> events;
}
public class EventService : MonoBehaviour
{
    [SerializeField] private string serverUrl = "https://localhost:7228/";
    [SerializeField] private float cooldownBeforeSend = 2f;

    public static EventService Instance { get; private set; }

    private readonly List<EventData> eventQueue = new();
    private bool isCooldownActive = false;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("EventData"))
        {
            StartCoroutine(SendJson(PlayerPrefs.GetString("EventData")));
        }
    }

    public void TrackEvent(string type, string data)
    {
        // Add event to queue
        eventQueue.Add(new EventData { type = type, data = data });

        if (!isCooldownActive)
        {
            StartCoroutine(SendEventsWithCooldown());
        }

    }

    private IEnumerator SendEventsWithCooldown()
    {
        isCooldownActive = true;

        // Wait for cooldown before sending
        yield return new WaitForSeconds(cooldownBeforeSend);

        if (eventQueue.Count > 0)
        {
            SendEvents();
        }

        isCooldownActive = false;
    }

    private void SendEvents()
    {
        // Generate JSON with events
        EventList eventList = new EventList { events = eventQueue };

        string json = JsonUtility.ToJson(eventList);
        StartCoroutine(SendJson(json));

    }

    private IEnumerator SendJson(string json)
    {
        UnityWebRequest request = null;
        Debug.Log("JSON sent: " + json);

        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogError("SendJson input data is empty");
            yield return null;
        }

        try
        {
            request = new(serverUrl, "POST");

            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
        }
        catch
        {
            PlayerPrefs.SetString("EventData", json);
        }

        // Sending a request
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // Clear the event queue and delete the saves
            Debug.Log("Events sent successfully!");
            PlayerPrefs.DeleteKey("EventData");
            eventQueue.Clear(); 
        }
        else
        {
            Debug.LogError("Error sending events: " + request.error);
            PlayerPrefs.SetString("EventData", json);
            PlayerPrefs.Save();

        }
    }
}
