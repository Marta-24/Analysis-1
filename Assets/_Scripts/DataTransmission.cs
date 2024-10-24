using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class DataTransmission : MonoBehaviour
{
    uint currentUserId;
    uint currentSessionId;
    uint currentPurchaseId;

    private enum ActionType { NewPlayer, StartSession, EndSession, BuyItem }

    private void OnEnable()
    {
        Simulator.OnNewPlayer += (name, country, date) => StartCoroutine(UploadData(ActionType.NewPlayer, name, country, date.ToString("yyyy-MM-dd HH:mm:ss")));
        Simulator.OnNewSession += (date) => StartCoroutine(UploadData(ActionType.StartSession, date: date.ToString("yyyy-MM-dd HH:mm:ss")));
        Simulator.OnEndSession += (date) => StartCoroutine(UploadData(ActionType.EndSession, date: date.ToString("yyyy-MM-dd HH:mm:ss")));
        Simulator.OnBuyItem += (item, date) => StartCoroutine(UploadData(ActionType.BuyItem, date: date.ToString("yyyy-MM-dd HH:mm:ss"), item: item));
    }

    private void OnDisable()
    {
        Simulator.OnNewPlayer -= (name, country, date) => StartCoroutine(UploadData(ActionType.NewPlayer, name, country, date.ToString("yyyy-MM-dd HH:mm:ss")));
        Simulator.OnNewSession -= (date) => StartCoroutine(UploadData(ActionType.StartSession, date: date.ToString("yyyy-MM-dd HH:mm:ss")));
        Simulator.OnEndSession -= (date) => StartCoroutine(UploadData(ActionType.EndSession, date: date.ToString("yyyy-MM-dd HH:mm:ss")));
        Simulator.OnBuyItem -= (item, date) => StartCoroutine(UploadData(ActionType.BuyItem, date: date.ToString("yyyy-MM-dd HH:mm:ss"), item: item));
    }

    IEnumerator UploadData(ActionType actionType, string name = null, string country = null, string date = null, int item = 0)
    {
        WWWForm form = new WWWForm();
        string url = "";
        switch (actionType)
        {
            case ActionType.NewPlayer:
                form.AddField("Name", name);
                form.AddField("Country", country);
                form.AddField("Date", date);
                url = "https://citmalumnes.upc.es/~albertcf5/Player_Data.php";
                break;

            case ActionType.StartSession:
                form.AddField("User_ID", currentUserId.ToString());
                form.AddField("Start_Session", date);
                url = "https://citmalumnes.upc.es/~albertcf5/Session_Data.php";
                break;

            case ActionType.EndSession:
                form.AddField("User_ID", currentUserId.ToString());
                form.AddField("End_Session", date);
                form.AddField("Session_ID", currentSessionId.ToString());
                url = "https://citmalumnes.upc.es/~albertcf5/Close_Session_Data.php";
                break;

            case ActionType.BuyItem:
                form.AddField("Item", item.ToString());
                form.AddField("User_ID", currentUserId.ToString());
                form.AddField("Session_ID", currentSessionId.ToString());
                form.AddField("Buy_Date", date);
                url = "https://citmalumnes.upc.es/~albertcf5/Purchase_Data.php";
                break;
        }

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.LogError($"{actionType} data upload failed: " + www.error);
            }
            else
            {
                string answer = www.downloadHandler.text.Trim(new char[] { '\uFEFF', '\u200B', ' ', '\t', '\r', '\n' });

                if (actionType == ActionType.NewPlayer && uint.TryParse(answer, out uint parsedUserId) && parsedUserId > 0)
                {
                    currentUserId = parsedUserId;
                    CallbackEvents.OnAddPlayerCallback.Invoke(currentUserId);
                }
                else if (actionType == ActionType.StartSession && uint.TryParse(answer, out uint parsedSessionId) && parsedSessionId > 0)
                {
                    currentSessionId = parsedSessionId;
                    CallbackEvents.OnNewSessionCallback.Invoke(currentSessionId);
                }
                else if (actionType == ActionType.EndSession)
                {
                    CallbackEvents.OnEndSessionCallback.Invoke(currentSessionId);
                }
                else if (actionType == ActionType.BuyItem && uint.TryParse(answer, out uint parsedPurchaseId) && parsedPurchaseId > 0)
                {
                    currentPurchaseId = parsedPurchaseId;
                    CallbackEvents.OnItemBuyCallback.Invoke();
                }
                else
                {
                    UnityEngine.Debug.LogError($"Invalid response for {actionType}: " + answer);
                }
            }
        }
    }
}
