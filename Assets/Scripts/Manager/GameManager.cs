 using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] int gridNum;
    [SerializeField] GridSO startGrid;
    [SerializeField] GridSO emptyGrid;
    [SerializeField] GridSO endGrid;
    [SerializeField] GameObject cloud;
    List<GameObject> clouds = new List<GameObject>();
    [SerializeField] List<GridSO> gridTypes;
    [SerializeField] int gridPerRow;
    int currentGrid = 0;
    List<(GridSO,GameObject)> grids = new List<(GridSO,GameObject)>();
    [SerializeField] int maxTurn;
    [SerializeField] TMP_Text turnText;
    [SerializeField] TMP_Text gridText;
    [SerializeField] int vision = 3;
    int turnRemain;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        turnRemain = maxTurn;
        turnText.SetText("Turn remain: " + turnRemain);
        gridText.SetText("Current grid: " + currentGrid);
        player.SetActive(true);
        InitGrid();
    }
    void InitGrid()
    {
        int previousGridTypeIndex = -1;
        for (int i = 0; i < gridNum; i++)
        {
            Vector3 gridPos = GetGridPosition(i);
            int randomIndex = Random.Range(0, gridTypes.Count);
            if (i == 0)
            {
                GameObject gridObj = Instantiate(startGrid.gameObject, gridPos, Quaternion.identity);
                grids.Add((startGrid,gridObj));
            }
            else if (i == gridNum - 1)
            {
                GameObject gridObj = Instantiate(endGrid.gameObject, gridPos, Quaternion.identity);
                grids.Add((endGrid,gridObj));
            }
            else
            {
                if (previousGridTypeIndex == randomIndex)
                {
                    GameObject gridObj = Instantiate(emptyGrid.gameObject, gridPos, Quaternion.identity);
                    grids.Add((emptyGrid,gridObj));
                    previousGridTypeIndex = -1;
                }
                else
                {
                    GameObject gridObj = Instantiate(gridTypes[randomIndex].gameObject, gridPos, Quaternion.identity);
                    grids.Add((gridTypes[randomIndex],gridObj));
                    previousGridTypeIndex = randomIndex;

                }
                GameObject cloudObj = Instantiate(cloud, gridPos, Quaternion.identity);
                clouds.Add(cloudObj);
                if (i > vision)
                {
                    clouds[i-1].SetActive(true);
                }
                else
                {
                    clouds[i - 1].GetComponent<Cloud>().Disappear();
                }
            }
        }
    }
    Vector3 GetGridPosition(int i)
    {
        int row = i / gridPerRow;
        int collum = i % gridPerRow;
        if (row % 2 == 1)
        {
            collum = gridPerRow - collum - 1;
        }
        Vector3 gridPos = new Vector3(collum, row, 0);
        return gridPos;
    }
    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && turnRemain > 0)
        {
            int moveStep = Random.Range(1, 4);
            // Debug.Log("Move step: " + moveStep);
            currentGrid += moveStep;
            if (currentGrid >= gridNum)
            {
                currentGrid = gridNum - 1;
            }
            PlayerMove(1);
            GridAction();


        }
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
    void PlayerMove(int turnUsed)
    {
        player.transform.position = GetGridPosition(currentGrid);
        turnRemain -= turnUsed;
        turnText.SetText("Turn remain: " + turnRemain);
        gridText.SetText("Current grid: " + currentGrid);
        CheckClouds();
    }
    void CheckClouds()
    {
        for (int i = 0; i <= vision; i++)
        {
            int cloudIndex = currentGrid - 1 + i;
            if (cloudIndex <= clouds.Count)
            {
                clouds[cloudIndex].GetComponent<Cloud>().Disappear();

            }
            else
            {
                break;
            }
            
        }
        // for (int i = 0; i < clouds.Count; i++)
        // {
        //     if (i >= currentGrid - 1 && i < currentGrid + vision)
        //     {
        //         clouds[i].SetActive(false);
        //     }
        //     else
        //     {
        //         clouds[i].SetActive(true);
        //     }
            // if (i < currentGrid - 1)
            // {
            //     int randomIndex = Random.Range(0, gridTypes.Count);
            //     GameObject gridObj = Instantiate(gridTypes[randomIndex].gameObject, GetGridPosition(i + 1), Quaternion.identity);
            //     Destroy(grids[i + 1].Item2);    
            //     grids[i + 1] =(gridTypes[randomIndex], gridObj);
            // }
        // }
    }
    void GridAction()
    {
        if (currentGrid == gridNum - 1)
        {
            turnText.SetText("You Win! Press R to restart");
            player.SetActive(false);
        }
        else if (turnRemain == 0)
        {
            turnText.SetText("You Lose! Press R to restart");
            player.SetActive(false);
        }
        else
        {
            GridSO.GridType currentGridType = grids[currentGrid].Item1.gridType;
            // Debug.Log("Current grid type: " + currentGridType);
            switch (currentGridType)
            {
                case GridSO.GridType.Empty:
                    break;
                case GridSO.GridType.MoveForward:
                    StartCoroutine(MoveForwardCoroutine());
                    break;
                case GridSO.GridType.MoveBackward:
                    StartCoroutine(MoveBackwardCoroutine());
                    break;
                case GridSO.GridType.AddCard:
                    break;
                case GridSO.GridType.DropCard:
                    break;
                case GridSO.GridType.IceLake:
                    turnRemain--;
                    turnText.SetText("Turn remain: " + turnRemain);
                    if (turnRemain == 0)
                    {
                        turnText.SetText("You Lose! Press R to restart");
                        player.SetActive(false);
                    }
                    break;
                case GridSO.GridType.Scout:
                    break;
                case GridSO.GridType.Swamp:
                    if (turnRemain > 1)
                    {
                        turnRemain -= 2;
                    }
                    else
                    {
                        turnRemain = 0;
                        turnText.SetText("You Lose! Press R to restart");
                        player.SetActive(false);
                    }
                    turnText.SetText("Turn remain: " + turnRemain);
                    break;
                case GridSO.GridType.Teleport:
                    StartCoroutine(TeleportCoroutine());
                    break;
            }
        }
    }
    IEnumerator MoveForwardCoroutine()
    {
        yield return new WaitForSeconds(1f);
        currentGrid += 1;
        PlayerMove(0);
        GridAction();
    }
    IEnumerator MoveBackwardCoroutine()
    {
        yield return new WaitForSeconds(1f);
        currentGrid -= 1;
        PlayerMove(0);
        GridAction();
    }
    IEnumerator TeleportCoroutine()
    {
        yield return new WaitForSeconds(1f);
        int min = 1;
        if (currentGrid - 6 > 1)
        {
            min = currentGrid - 6;
        }
        int max = gridNum - 2;
        if (currentGrid + 6 < gridNum - 2)
        {
            max = currentGrid + 6;
        }
        int randomGrid = currentGrid;
        bool found = true;
        while (found)
        {
            randomGrid = Random.Range(min, max + 1);
            if (randomGrid != currentGrid || grids[randomGrid].Item1.gridType != GridSO.GridType.Teleport)
            {
                found = false;
                break;
            }
        }
        currentGrid = randomGrid;
        PlayerMove(0);
        GridAction();
    }
    void EndGame()
    {
        player.SetActive(false);
    }
}
