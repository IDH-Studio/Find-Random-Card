using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Runtime.CompilerServices;
using System;
using Unity.VisualScripting;

// 저장하는 데이터 구조체
public struct SaveData
{
    public SaveData(string nickname, string elapsedTime)
    {
        Nickname = nickname;
        ElapsedTime = elapsedTime;
    }

    public string Nickname { get; set; }
    public string ElapsedTime { get; set; }
}

public class DatabaseManager : MonoBehaviour
{
    private string[]                            _databaseTypes = { "easy", "normal", "hard" };
    private string                              _databaseType;

    [SerializeField] private Transform          _showScores;

    private DatabaseReference                   _db;
    private Queue<SaveData>                     _scores;
    private bool                                _showScore = false;
    private bool                                _reShowScore = false;

    private void Awake()
    {
        _db = FirebaseDatabase.DefaultInstance.RootReference;
        _scores = new Queue<SaveData>();
    }

    private void LateUpdate()
    {
        if (_showScore) { ShowScores(); }
        else if (_reShowScore) { ReShowScores(); }
    }

    /// <summary>
    /// Firebase에서 데이터 타입에 맞는 데이터를 가져온다.
    /// </summary>
    public void GetDatas(bool reLoad=false)
    {
        // 데이터를 가져오는 코드
        _db.Child(_databaseType).OrderByChild("elapsed_time").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
#if UNITY_EDITOR
                Debug.LogError("Database Error");
#endif
                return;
            }

            if(!task.IsCompleted)
            {
#if UNITY_EDITOR
                Debug.LogError("Fail Get");
#endif
                return;
            }

            DataSnapshot snapshot = task.Result;

            foreach(DataSnapshot child in snapshot.Children)
            {
                IDictionary data = (IDictionary)child.Value;

                _scores.Enqueue(new SaveData(data["nickname"].ToString(), data["elapsed_time"].ToString()));
            }

            if (reLoad) _reShowScore = true;
            else _showScore = true;
        });
    }

    public bool WriteData(string nickname, float elapsedTime)
    {
        try
        {
            string time = string.Format("{0:0.###}", elapsedTime);

            // 데이터를 저장하는 코드
            DatabaseReference data = _db.Child(_databaseType).Push();

            data.Child("nickname").SetValueAsync(nickname);
            data.Child("elapsed_time").SetValueAsync(time);

            return true;
        }
        catch (Exception e)
        {
#if UNITY_EDITOR
            Debug.LogException(e);
#endif
            return false;
        }
    }

    public void ShowScores()
    {
        _showScore = false;

        while (_scores.Count > 0)
        {
            // objectManager에서 스코어 프리팹을 가져온 뒤 데이터를 집어넣는다.
            GameObject scoreObj = GameManager._instance._prefabManager.GetScoreObj();
            SaveData score = _scores.Dequeue();

            // 데이터 집어넣기
            scoreObj.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = score.Nickname;
            scoreObj.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = score.ElapsedTime;

            scoreObj.transform.SetParent(_showScores);
            scoreObj.transform.localScale = Vector3.one;
            scoreObj.transform.position = Vector3.zero;
        }
    }

    public void ReShowScores()
    {
        _reShowScore = false;
        PutBackScores();
        ShowScores();
    }

    public void SetDatabase(DIFFICULTY difficulty)
    {
        _databaseType = _databaseTypes[(int)difficulty - 3];
        //GetDatas();
    }

    public void SaveCancel()
    {
        PutBackScores();
        GameManager._instance._screenManager.PrevScreen();
    }

    public void GoMain()
    {
        PutBackScores();
        GameManager._instance._screenManager.ScreenClear();
    }

    public void PutBackScores()
    {
        int childCount = _showScores.childCount;

        for (int index = 0; index < childCount; ++index)
        {
            GameManager._instance._prefabManager.PutBackScoreObj(_showScores.GetChild(index).gameObject);
        }
    }
}
