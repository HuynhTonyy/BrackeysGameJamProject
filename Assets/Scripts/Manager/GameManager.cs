using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject playerObj;
    [SerializeField] int gridNum;
    [SerializeField] GameObject startGridObj;
    [SerializeField] GameObject endGridObj;
    [SerializeField] List<GameObject> gridObjs;
    [SerializeField] int gridPerRow;
    int currentGrid = 0;
    List<GameObject> grids;
    [SerializeField] int maxTurn;
    [SerializeField] TMP_Text turnText;
    [SerializeField] TMP_Text gridText;
    int turnRemain;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        turnRemain = maxTurn;
        turnText.SetText("Turn left: " + turnRemain);
        gridText.SetText("Current grid: " + currentGrid);
        playerObj.SetActive(true);
        InitGrid();
    }
    void InitGrid()
    {
        for (int i = 0; i < gridNum; i++)
        {
            int row = i / gridPerRow;
            int collum = i % gridPerRow;
            if (row % 2 == 1)
            {
                collum = gridPerRow - collum -1;
            }
            Vector3 gridPos = new Vector3(collum, row, 0);
            // int randomIndex = Random.Range(0, gridObjs.Count);
            if (i == 0)
            {
                Instantiate(startGridObj, gridPos, Quaternion.identity);
            }
            else if (i == gridNum - 1)
            {
                Instantiate(endGridObj, gridPos, Quaternion.identity);
            }
            else
            {
                Instantiate(gridObjs[i%2], gridPos, Quaternion.identity);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
