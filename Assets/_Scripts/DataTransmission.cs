using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class DataTransmission : MonoBehaviour
{
    uint currentUserId;
    uint currentSessionId;
    uint currentPurchaseId;

    private void OnEnable()
    {
        Simulator.OnNewPlayer += HandleNewPlayer;
        Simulator.OnNewSession += HandleNewSession;
        Simulator.OnEndSession += HandleEndSession;
        Simulator.OnBuyItem += HandleBuyItem;
    }

    private void OnDisable()
    {
        Simulator.OnNewPlayer -= HandleNewPlayer;
        Simulator.OnNewSession -= HandleNewSession;
        Simulator.OnEndSession -= HandleEndSession;
        Simulator.OnBuyItem -= HandleBuyItem;
    }

    private void HandleNewPlayer(string name, string country, DateTime date)
    {
        StartCoroutine(UploadPlayer(name, country, date));
    }

    private void HandleNewSession(DateTime date)
    {
        StartCoroutine(UploadStartSession(date));
    }

    private void HandleEndSession(DateTime date)
    {
        StartCoroutine(UploadEndSession(date));
    }

    private void HandleBuyItem(int item, DateTime date)
    {
        StartCoroutine(UploadItem(item, date));
    }

    IEnumerator UploadPlayer(string name, string country, DateTime date)
    {
        WWWForm form = new WWWForm();
        form.AddField("Name", name);
        form.AddField("Country", country);
        form.AddField("Date", date.ToString("yyyy-MM-dd HH:mm:ss"));

        using (UnityWebRequest www = UnityWebRequest.Post("https://citmalumnes.upc.es/~albertcf5/Player_Data.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.LogError("Player data upload failed: " + www.error);
            }
            else
            {
                string answer = www.downloadHandler.text.Trim(new char[] { '\uFEFF', '\u200B', ' ', '\t', '\r', '\n' });
                if (uint.TryParse(answer, out uint parsedId) && parsedId > 0)
                {
                    currentUserId = parsedId;
                    CallbackEvents.OnAddPlayerCallback.Invoke(currentUserId);
                }
                else
                {
                    UnityEngine.Debug.LogError("Invalid user ID received: " + answer);
                }
            }
        }
    }

    IEnumerator UploadStartSession(DateTime date)
    {
        WWWForm form = new WWWForm();
        form.AddField("User_ID", currentUserId.ToString());
        form.AddField("Start_Session", date.ToString("yyyy-MM-dd HH:mm:ss"));

        string url = "https://citmalumnes.upc.es/~albertcf5/Session_Data.php";
        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string answer = www.downloadHandler.text.Trim(new char[] { '\uFEFF', '\u200B', ' ', '\t', '\r', '\n' });
            if (uint.TryParse(answer, out uint parsedId) && parsedId > 0)
            {
                currentSessionId = parsedId;
                CallbackEvents.OnNewSessionCallback.Invoke(currentSessionId);
            }
            else
            {
                UnityEngine.Debug.LogError("Invalid session ID received: " + answer);
            }
        }
        else
        {
            UnityEngine.Debug.LogError("Session start data upload failed: " + www.error);
        }
    }


    IEnumerator UploadEndSession(DateTime date)
    {
        WWWForm form = new WWWForm();
        form.AddField("User_ID", currentUserId.ToString());
        form.AddField("End_Session", date.ToString("yyyy-MM-dd HH:mm:ss"));
        form.AddField("Session_ID", currentSessionId.ToString());

        string url = "https://citmalumnes.upc.es/~albertcf5/Close_Session_Data.php";
        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            CallbackEvents.OnEndSessionCallback.Invoke(currentSessionId);
        }
        else
        {
            UnityEngine.Debug.LogError("Session end data upload failed: " + www.error);
        }
        
    }


    IEnumerator UploadItem(int item, DateTime date)
    {
        WWWForm form = new WWWForm();
        form.AddField("Item", item.ToString());
        form.AddField("User_ID", currentUserId.ToString());
        form.AddField("Session_ID", currentSessionId.ToString());
        form.AddField("Buy_Date", date.ToString("yyyy-MM-dd HH:mm:ss"));

        UnityWebRequest www = UnityWebRequest.Post("https://citmalumnes.upc.es/~albertcf5/Purchase_Data.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            UnityEngine.Debug.LogError("Purchase data upload failed: " + www.error);
        }
        else
        {
            string answer = www.downloadHandler.text.Trim(new char[] { '\uFEFF', '\u200B', ' ', '\t', '\r', '\n' });
            if (uint.TryParse(answer, out uint parsedId) && parsedId > 0)
            {
                currentPurchaseId = parsedId;
                CallbackEvents.OnItemBuyCallback.Invoke();
            }
            else
            {
                UnityEngine.Debug.LogError("Invalid purchase ID received: " + www.downloadHandler.text);
            }
        }
    }
}