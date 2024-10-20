using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Simulator : MonoBehaviour
{

    public static Action<string, string, DateTime> OnNewPlayer; //Name, Country and date
    public static Action<DateTime> OnNewSession;
    public static Action<DateTime> OnEndSession;
    public static Action<int, DateTime> OnBuyItem; //Item id and date

    private DateTime _currentDate;

    public int MaxPlayers;
    public float ReplayChance => _currentDate.Month < 6 ? 0.7f : 0.95f;
    public float BuyProbability = 0.1f;


    private int _nPlayers;

    private List<string> Countries;

    [SerializeField]
    private Lexic.NameGenerator namegen;

    #region Subscribe
    private void OnEnable()
    {
        CallbackEvents.OnAddPlayerCallback += OnPlayerAdded;
        CallbackEvents.OnNewSessionCallback += OnNewSessionAdded;
        CallbackEvents.OnEndSessionCallback += OnEndSessionAdded;
        CallbackEvents.OnItemBuyCallback += OnItemBought;
    }

    private void OnDisable()
    {
        CallbackEvents.OnAddPlayerCallback -= OnPlayerAdded;
        CallbackEvents.OnNewSessionCallback -= OnNewSessionAdded;
        CallbackEvents.OnEndSessionCallback -= OnEndSessionAdded;
    }

    #endregion

    void Start()
    {
        Countries = new List<string>();
        int nCountries = Random.Range(1, 10);

        string[] coutryNames = System.Enum.GetNames(typeof(AllCountries));
        for (int i = 0; i < nCountries; i++)
        {
            int rdm = Random.Range(0, coutryNames.Length);
            Countries.Add(((AllCountries)rdm).ToString());
        }

        for (int i = 0; i < MaxPlayers; i++)
        {
            MakeOnePlayer();
        }



    }

    void MakeOnePlayer()
    {
        _nPlayers++;
        if (_nPlayers > MaxPlayers)
        {
            UnityEngine.Debug.Log("Finished");
            return;
        }


        _currentDate = GetNewPlayerDate();
        AddNewPlayer(_currentDate);

    }




    void AddNewPlayer(DateTime dateTime)
    {
        string name = namegen.GetNextRandomName();
        int rdm = Random.Range(0, Countries.Count);
        string country = Countries[rdm];


        OnNewPlayer?.Invoke(name, country, dateTime);
    }

    void AddNewSession()
    {
        DateTime dateTime = _currentDate;
        OnNewSession?.Invoke(dateTime);
    }

    void EndSession()
    {
        _currentDate = _currentDate.Add(GetSessionLength());
        DateTime dateTime = _currentDate;
        OnEndSession?.Invoke(dateTime);
    }

    void TryBuy()
    {
        _currentDate = _currentDate.Add(GetSessionLength());
        if (UserBuys())
            OnBuyItem?.Invoke(GetItem(), _currentDate);
        else
            EndSession();
    }

    private bool UserBuys()
    {
        return Random.value < BuyProbability;
    }

    private int GetItem()
    {
        float rdm = Random.value;
        if (rdm < 0.5f)
            return 1;
        if (rdm < 0.75f)
            return 2;
        if (rdm < 0.9f)
            return 3;
        if (rdm < 0.91f)
            return 4;

        return 5;

    }



    #region Probabilistic values
    DateTime GetNewPlayerDate()
    {
        int year = 2022;
        int month = Random.Range(1, 13);
        int numOfDays = DateTime.DaysInMonth(year, month);
        int day = Random.Range(1, numOfDays + 1);

        int hour = Random.Range(0, 24);
        int minut = Random.Range(0, 60);
        int second = Random.Range(0, 60);

        return new DateTime(year, month, day, hour, minut, second);

    }


    private TimeSpan GetSessionLength()
    {
        float lengthInSeconds = Random.Range(30, 500);
        return TimeSpan.FromSeconds(lengthInSeconds);
    }

    private TimeSpan GetBuyTime()
    {
        float lengthInSeconds = Random.Range(10, 120);
        return TimeSpan.FromSeconds(lengthInSeconds);
    }

    private TimeSpan TimeTillNextSession()
    {
        float lengthInSeconds = Random.Range(3000, 30000);
        return TimeSpan.FromSeconds(lengthInSeconds);
    }
    #endregion

    #region callback subscribers
    private void OnPlayerAdded(uint obj)
    {
        AddNewSession();
    }
    private void OnNewSessionAdded(uint obj)
    {
        TryBuy();
    }

    private void OnItemBought()
    {
        EndSession();
    }

    private void OnEndSessionAdded(uint obj)
    {

        if (Random.value > ReplayChance)
        {
            MakeOnePlayer();
            return;
        }
        TimeSpan timeSpan = TimeTillNextSession();

        _currentDate = _currentDate.Add(timeSpan);
        // Debug.Log(_currentDate.ToLongDateString());

        if (_currentDate.Year == 2022)
            AddNewSession();
        else
            MakeOnePlayer();
    }


    #endregion
}

public class CallbackEvents
{
    public static Action<uint> OnEndSessionCallback;
    public static Action<uint> OnNewSessionCallback;
    public static Action<uint> OnAddPlayerCallback;
    public static Action OnItemBuyCallback;
}
