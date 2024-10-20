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

    // Manejar nuevos jugadores
    private void HandleNewPlayer(string name, string country, int age, float gender, DateTime date)
    {
        Debug.Log($"HandleNewPlayer: Name={name}, Country={country}, Age={age}, Gender={gender}, Date={date}");
        StartCoroutine(UploadPlayer(name, country, age, gender, date));
    }

    // Manejar el inicio de sesión
    private void HandleNewSession(DateTime date, uint playerID)
    {
        Debug.Log($"Evento HandleNewSession disparado: Date={date}, PlayerID={playerID}");
        StartCoroutine(UploadStartSession(date, playerID));
    }

    // Manejar el cierre de sesión
    private void HandleEndSession(DateTime date, uint sessionID)
    {
        Debug.Log($"HandleEndSession: Date={date}, SessionID={sessionID}");
        StartCoroutine(UploadEndSession(date, sessionID));
    }

    // Manejar la compra de un artículo
    private void HandleBuyItem(int itemID, DateTime date, uint sessionID)
    {
        Debug.Log($"Evento HandleBuyItem disparado: ItemID={itemID}, Date={date}, SessionID={sessionID}");
        StartCoroutine(UploadItem(itemID, date, sessionID));
    }

    // Subir datos de un nuevo jugador
    IEnumerator UploadPlayer(string name, string country, int age, float gender, DateTime dateOfCreation)
    {
        WWWForm form = new WWWForm();
        form.AddField("Name", name);
        form.AddField("Country", country);
        form.AddField("Age", age.ToString());
        form.AddField("Gender", gender.ToString());
        form.AddField("DateOfCreation", dateOfCreation.ToString("yyyy-MM-dd HH:mm:ss"));

        Debug.Log($"Enviando datos del jugador: Name={name}, Country={country}, Age={age}, Gender={gender}, DateOfCreation={dateOfCreation}");

        using (UnityWebRequest www = UnityWebRequest.Post("https://citmalumnes.upc.es/~albertcf5/Player_Data.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Player data upload failed: " + www.error);
            }
            else
            {
                Debug.Log("Player data uploaded successfully: " + www.downloadHandler.text);
            }
        }
    }

    // Subir datos de inicio de sesión
    IEnumerator UploadStartSession(DateTime date, uint playerID)
    {
        WWWForm form = new WWWForm();
        form.AddField("User_ID", playerID.ToString());
        form.AddField("Start_Session", date.ToString("yyyy-MM-dd HH:mm:ss"));

        Debug.Log($"Enviando datos de nueva sesión: UserID={playerID}, StartSession={date}");

        UnityWebRequest www = UnityWebRequest.Post("https://citmalumnes.upc.es/~albertcf5/Session_Data.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Fallo en la subida de datos de sesión: " + www.error);
        }
        else
        {
            Debug.Log("Datos de sesión subidos correctamente: " + www.downloadHandler.text);
        }
    }


    // Subir datos de cierre de sesión
    IEnumerator UploadEndSession(DateTime date, uint sessionID)
    {
        WWWForm form = new WWWForm();
        form.AddField("Session_ID", sessionID.ToString());
        form.AddField("End_Session", date.ToString("yyyy-MM-dd HH:mm:ss"));

        Debug.Log($"Enviando datos de cierre de sesión: Session_ID={sessionID}, End_Session={date}");

        UnityWebRequest www = UnityWebRequest.Post("https://citmalumnes.upc.es/~albertcf5/Close_Session_Data.php", form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Session end data uploaded successfully: " + www.downloadHandler.text);
            CallbackEvents.OnEndSessionCallback?.Invoke(sessionID);
        }
        else
        {
            Debug.LogError("Session end data upload failed: " + www.error);
        }
    }

    // Subir datos de compra de un artículo
    IEnumerator UploadItem(int itemID, DateTime date, uint sessionID)
    {
        WWWForm form = new WWWForm();
        form.AddField("Item", itemID.ToString());
        form.AddField("Session_ID", sessionID.ToString());
        form.AddField("User_ID", currentUserId.ToString());
        form.AddField("Buy_Date", date.ToString("yyyy-MM-dd HH:mm:ss"));

        Debug.Log($"Enviando datos de compra: ItemID={itemID}, SessionID={sessionID}, UserID={currentUserId}, BuyDate={date}");

        UnityWebRequest www = UnityWebRequest.Post("https://citmalumnes.upc.es/~albertcf5/Purchase_Data.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Fallo en la subida de datos de compra: " + www.error);
        }
        else
        {
            Debug.Log("Datos de compra subidos correctamente: " + www.downloadHandler.text);
        }
    }

}
