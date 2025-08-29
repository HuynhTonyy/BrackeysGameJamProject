 using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using DG.Tweening;
using UnityEditor.PackageManager;
public class GameManager : MonoBehaviour
{
    [SerializeField] List<GameObject> islandBases;
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
    List<(GridSO, GameObject,bool)> grids = new List<(GridSO, GameObject,bool)>();
    [SerializeField] int maxStamina;
    [SerializeField] TMP_Text turnText;
    [SerializeField] TMP_Text gridText;
    [SerializeField] int vision = 3;
    int stamina;
    [SerializeField] private float pathLength;
    [SerializeField] private float borderLimit;
    [SerializeField] private int minPerDir;
    [SerializeField] private GameObject pathObj;
    private List<GameObject> paths = new List<GameObject>();
    [SerializeField]private int maxTurnToSink;
     private int turnToSink;
    [SerializeField] private int sinkNum;
    private int sinkedGridIndex = 0;
    private int currentTurnToSink = 0;
    [SerializeField] private GameObject endPanel;
    public static GameManager Instance { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private GameManager()
    {

    }
    private void OnEnable() {
        if (EventManager.Instance == null) return;
        EventManager.Instance.onPlayMoveCard += MovePlayer;
    }
    private void OnDisable() {
        if (EventManager.Instance == null) return;
        EventManager.Instance.onPlayMoveCard -= MovePlayer;
    }
    private void OnDestroy() {
        if (EventManager.Instance == null) return;
        EventManager.Instance.onPlayMoveCard -= MovePlayer;
    }
    void Awake()
    {
        // Check if instance already exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Prevent duplicates
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }
    void Start()
    {
        turnToSink = maxTurnToSink;
        endPanel.SetActive(false);
        stamina = maxStamina;
        turnText.SetText("Stamina remain: " + stamina);
        gridText.SetText("Current grid: " + currentGrid);
        player.SetActive(true);
        InitGrid();

    }
    void InitGrid()
    {
        int previousGridTypeIndex = -1;
        for (int i = 0; i < gridNum; i++)
        {
            Vector3 gridPos = GetNewGridPosition(i);
            // Debug.Log(i+"-"+gridPos);
            int randomIndex = Random.Range(0, gridTypes.Count);
            if (i == 0)
            {
                GameObject gridObj = Instantiate(startGrid.gameObject, gridPos, Quaternion.identity);
                grids.Add((startGrid, gridObj,true));
            }
            else if (i == gridNum - 1)
            {
                GameObject gridObj = Instantiate(endGrid.gameObject, gridPos, Quaternion.identity);
                grids.Add((endGrid, gridObj,true));
            }
            else
            {
                if (previousGridTypeIndex == randomIndex)
                {
                    GameObject gridObj = Instantiate(emptyGrid.gameObject, gridPos, Quaternion.identity);
                    ;
                    GameObject islandBaseObj = Instantiate(islandBases[Random.Range(0, islandBases.Count)], Vector3.zero, Quaternion.identity,gridObj.transform);
                    islandBaseObj.transform.localPosition = Vector3.zero;
                    islandBaseObj.transform.localRotation = Quaternion.identity;
                    islandBaseObj.transform.localScale = Vector3.one;
                    grids.Add((emptyGrid, gridObj,true));
                    previousGridTypeIndex = -1;
                }
                else
                {
                    GameObject gridObj = Instantiate(gridTypes[randomIndex].gameObject, gridPos, Quaternion.identity);
                    GameObject islandBaseObj = Instantiate(islandBases[Random.Range(0, islandBases.Count)], Vector3.zero, Quaternion.identity,gridObj.transform);
                    islandBaseObj.transform.localPosition = Vector3.zero;
                    islandBaseObj.transform.localRotation = Quaternion.identity;
                    islandBaseObj.transform.localScale = Vector3.one;
                    grids.Add((gridTypes[randomIndex], gridObj,true));
                    previousGridTypeIndex = randomIndex;

                }
                GameObject cloudObj = Instantiate(cloud, gridPos, Quaternion.identity);
                clouds.Add(cloudObj);
                if (i > vision)
                {
                    clouds[i - 1].SetActive(true);
                }
                else
                {
                    clouds[i - 1].GetComponent<Cloud>().Disappear();
                }
            }
        }
        InitPath();
    }
    void InitPath()
    {
        for (int i = 0; i < gridNum - 1; i++)
        {
            Vector3 pathPos = grids[i].Item2.transform.position + (grids[i + 1].Item2.transform.position - grids[i].Item2.transform.position) * 0.5f;

            GameObject newPathObj = Instantiate(pathObj, pathPos, Quaternion.identity);
            paths.Add(newPathObj);
            Vector3 distance = grids[i + 1].Item2.transform.position - grids[i].Item2.transform.position;
            float angle = Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg;
            newPathObj.transform.DORotate(new Vector3(0, 0, angle-360), 0.25f);
        }
    }
    Vector3 GetNewGridPosition(int index)
    {
        if(index == 0) return Vector3.zero;
        return grids[index - 1].Item2.transform.position + Vector3.right * pathLength;
    }
    bool CheckReachMinPerDir(int index)
    {
        if (index <= minPerDir) return false;
        int numSameDir = 2;
        Vector3 lastestDir = (grids[index - 1].Item2.transform.position - grids[index - 2].Item2.transform.position).normalized;
        for (int i = 0; i < minPerDir - 1; i++)
        {
            Vector3 thisDir = (grids[grids.Count - 2 - i].Item2.transform.position - grids[grids.Count - 3 - i].Item2.transform.position).normalized;
            if (lastestDir == thisDir)
            {
                numSameDir++;
            }
            else
            {
                break;
            }
        }
        if (numSameDir >= minPerDir) return true;
        else return false;
    }
    // Vector3 InitGridPosition(int index)
    // {
    //     Vector3 direction = Vector3.zero;
    //     if (index == 0)
    //     {
    //         return direction;
    //     }
    //     else if (index == 1)
    //     {
    //         Vector3 xAxis = Vector3.zero;
    //         Vector3 yAxis = Vector3.zero;
    //         int xMaxOption = 2;
    //         int yRandomNum = Random.Range(0, 2);
    //         if (yRandomNum == 0)
    //         {
    //             yAxis = Vector3.up;
    //             xMaxOption = 2;
    //         }
    //         int xRandomNum = Random.Range(0, xMaxOption);
    //         if (xRandomNum == 0)
    //         {
    //             xAxis = Vector3.left;
    //         }
    //         else if (xRandomNum == 1)
    //         {
    //             xAxis = Vector3.right;
    //         }
    //         direction = (xAxis + yAxis).normalized;
    //     }
    //     else
    //     {
    //         if (grids[index - 1].Item2.transform.position.x <= -borderLimit)
    //         {
    //             direction = ((Random.Range(0, 2) == 0 ? Vector3.left : Vector3.zero) + Vector3.up).normalized;
    //         }
    //         else if (grids[index - 1].Item2.transform.position.x >= borderLimit)
    //         {
    //             direction = ((Random.Range(0, 2) == 0 ? Vector3.right : Vector3.zero) + Vector3.up).normalized;
    //         }
    //         else if (CheckReachMinPerDir(index))
    //         {
    //             Vector3 xAxis = Vector3.zero;

    //             int xRandomNum = Random.Range(0, 3);
    //             if (xRandomNum == 0)
    //             {
    //                 xAxis = Vector3.left;
    //             }
    //             else if (xRandomNum == 1)
    //             {
    //                 xAxis = Vector3.right;
    //             }
    //             direction = (xAxis + Vector3.up).normalized;
    //             // Debug.Log("can chang dir:" + direction);
    //         }
    //         else
    //         {

    //             direction = (grids[index - 1].Item2.transform.position - grids[index - 2].Item2.transform.position).normalized;
    //             // Debug.Log(grids[index - 1].Item2.transform.position +"-"+ grids[index - 2].Item2.transform.position);
    //             // Debug.Log("same dir:" + direction);
    //         }
    //     }
    //     Vector3 gridPos = grids[index - 1].Item2.transform.position + direction * pathLength;
    //     return gridPos;
    // }
    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && stamina > 0)
        {
            int moveStep = Random.Range(1, 4);
            MovePlayer(moveStep, 1);
        }
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
    public void MovePlayer(int step, int staminaUsed)
    {
        currentGrid += step;
        if (currentGrid >= gridNum)
        {
            currentGrid = gridNum - 1;
        }
        if (currentGrid <= sinkedGridIndex)
        {
            Debug.Log("Da bi pha");
            endPanel.SetActive(true);
            // return;
        }
        player.transform.position = grids[currentGrid].Item2.transform.position;
        if (staminaUsed > 0)
        {
            stamina -= staminaUsed;
            turnText.SetText("Stamina remain: " + stamina);
            Sink();
        }
        gridText.SetText("Current grid: " + currentGrid);
        ClearCloudsInDistance(currentGrid, vision);
        GridAction();
    }
    public void ApplyTradeOff(TradeOffType tradeOffType)
    {
        if (tradeOffType == TradeOffType.Repeat)
        {
            
        }
    }
    void ClearCloudsInDistance(int startGrid, int distance)
    {
        if (startGrid == 0) return;
        for (int i = 0; i <= distance; i++)
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
    }
    void GridAction()
    {
        
        if (currentGrid == gridNum - 1)
        {
                    endPanel.SetActive(true);

        }
        else if (stamina == 0 || currentGrid <= sinkedGridIndex)
        {
                    endPanel.SetActive(true);

        }
        else
        {
            if (!grids[currentGrid].Item3) return;
            GridSO.GridType currentGridType = grids[currentGrid].Item1.gridType;
            switch (currentGridType)
            {
                case GridSO.GridType.Empty:
                    break;
                case GridSO.GridType.MoveForward:
                    StartCoroutine(MoveCoroutine(1));
                    break;
                case GridSO.GridType.MoveBackward:
                    StartCoroutine(MoveCoroutine(-1));
                    break;
                case GridSO.GridType.AddCard:
                    EventManager.Instance.EnterAddCardGrid();
                    break;
                case GridSO.GridType.DropCard:
                    EventManager.Instance.EnterDropCardGrid();
                    break;
                case GridSO.GridType.CursedFrog:
                    stamina--;
                    turnText.SetText("Stamina remain: " + stamina);
                    if (stamina == 0)
                    {
                                endPanel.SetActive(true);

                    }
                    break;
                case GridSO.GridType.Scout:
                    ClearCloudsInDistance(currentGrid, 6);
                    break;
                case GridSO.GridType.Swamp:
                    if (stamina > 2)
                    {
                        stamina -= 2;
                        turnText.SetText("Stamina remain: " + stamina);

                    }
                    else
                    {
                        stamina = 0;
                        endPanel.SetActive(true);

                    }
                    break;
                case GridSO.GridType.Teleport:
                    Sink();
                    StartCoroutine(TeleportCoroutine());
                    break;
            }
            grids[currentGrid] = (grids[currentGrid].Item1, grids[currentGrid].Item2, false);

        }
    }
    void Sink()
    {
        if(turnToSink <=0 || sinkNum <=0) return;
        currentTurnToSink++;
        if(currentTurnToSink >= turnToSink)
        {
            currentTurnToSink = 0;
            int sinkToIndex = sinkedGridIndex + sinkNum;
            if(sinkToIndex >= gridNum -1)
            {
                sinkToIndex = gridNum -2;
            }
            for (int i = sinkedGridIndex; i < sinkToIndex; i++)
            {
                grids[i].Item2.SetActive(false);
                if(i < paths.Count)
                {
                    paths[i].SetActive(false);
                }
                
            }
            sinkedGridIndex = sinkToIndex-1;
            if(currentGrid <= sinkedGridIndex)
            {
                Debug.Log("Da bi pha");
                endPanel.SetActive(true);
            }
        }
    }
    IEnumerator MoveCoroutine(int step)
    {
        yield return new WaitForSeconds(1f);
        MovePlayer(step,0);
        GridAction();
    }
    IEnumerator TeleportCoroutine()
    {
        yield return new WaitForSeconds(1f);
        int min = currentGrid - 6;
        if (currentGrid - 6 <= sinkedGridIndex)
        {
            min = currentGrid - sinkedGridIndex;
        }
        int max = gridNum - 2;
        if (currentGrid + 6 < gridNum - 2)
        {
            max = currentGrid + 6;
        }
        int randomGrid;
        while (true)
        {
            randomGrid = Random.Range(min, max + 1);
            if (randomGrid != currentGrid && grids[randomGrid].Item1.gridType != GridSO.GridType.Teleport)
            {
                break;
            }
        }
        MovePlayer(randomGrid-currentGrid,0);
        GridAction();
    }
    void EndGame()
    {
        player.SetActive(false);
    }

}
