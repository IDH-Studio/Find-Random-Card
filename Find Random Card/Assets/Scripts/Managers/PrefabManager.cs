using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    [Header("¡å Prefabs")]
    [SerializeField] private GameObject _scorePrefab;

    [Space(10)]
    [Header("¡å Pool Counts")]
    [SerializeField] private int        _scorePoolCount = 100;
    private List<GameObject>            _scorePool;

    private void Awake()
    {
        _scorePool = new List<GameObject>();

        for (int index = 0; index < _scorePoolCount; ++index)
        {
            GameObject scoreObject = Instantiate(_scorePrefab, transform);
            scoreObject.SetActive(false);
            _scorePool.Add(scoreObject);
        }
    }

    public GameObject GetScoreObj()
    {
        GameObject scoreObject = null;

        foreach(GameObject score in _scorePool)
        {
            if (!score.activeSelf)
            {
                scoreObject = score;
                scoreObject.SetActive(true);
                break;
            }
        }

        if (!scoreObject)
        {
            scoreObject = Instantiate(_scorePrefab, transform);
            _scorePool.Add(scoreObject);
        }

        return scoreObject;
    }

    public void PutBackScoreObj(GameObject scoreObj)
    {
        scoreObj.SetActive(false);
        scoreObj.transform.SetParent(transform);
    }
}
