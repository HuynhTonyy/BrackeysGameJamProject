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
    private bool isOperating = false;
    int currentGrid = 0;
    List<(GridSO, GameObject,bool)> grids = new List<(GridSO, GameObject,bool)>();
    [SerializeField] int maxStamina;
    [SerializeField] TMP_Text staminaText;
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
    [SerializeField] private AudioClip powerUpCip;
    [SerializeField] private AudioClip thunderStrikeCip;
    [SerializeField] private AudioClip scountCip;
    [SerializeField] private AudioClip loseClip;
    [SerializeField] private AudioClip victoryClip; 
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject inGamePanel;
    [SerializeField] private TMP_Text endText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private GameManager()
    {

    }
    private void OnEnable()
    {
        if (EventManager.Instance == null) return;
        EventManager.Instance.onPlayerMoveEnd += GridAction;
        EventManager.Instance.onPlayMoveCard += MovePlayer;
        EventManager.Instance.onIslandSinkEnd += CheckSinkedGrid;
        EventManager.Instance.onClickDrawCard += ConsumeStamina;
    }
    private void OnDisable() {
        if (EventManager.Instance == null) return;
        EventManager.Instance.onPlayerMoveEnd -= GridAction;
        EventManager.Instance.onIslandSinkEnd -= CheckSinkedGrid;
        EventManager.Instance.onClickDrawCard -= ConsumeStamina;
        EventManager.Instance.onPlayMoveCard -= MovePlayer;
    }
    private void OnDestroy() {
        if (EventManager.Instance == null) return;
        EventManager.Instance.onIslandSinkEnd -= CheckSinkedGrid;
        EventManager.Instance.onClickDrawCard -= ConsumeStamina;
        EventManager.Instance.onPlayerMoveEnd -= GridAction;
        EventManager.Instance.onPlayMoveCard -= MovePlayer;
    }
    private void ConsumeStamina()
    {
        stamina--;
        if (stamina <= 0)
        {
            endText.SetText("Better luck next time!");
            endPanel.SetActive(true);
            return;
        }
        staminaText.SetText(stamina.ToString());
        EventManager.Instance.StaminaChange(-1);
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
        InitGrid();
    }
    void Start()
    {
        turnToSink = maxTurnToSink;
        endPanel.SetActive(false);
        stamina = maxStamina;
        staminaText.SetText(stamina.ToString());
        gridText.SetText(currentGrid.ToString());
        player.SetActive(true);
        inGamePanel.SetActive(false);   
        startPanel.SetActive(true);
    }
    public void StartGame()
    {
        startPanel.SetActive(false);
        inGamePanel.SetActive(true);
    }
    public void Restart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    void InitGrid()
    {
        int previousGridTypeIndex = -1;
        for (int i = 0; i < gridNum; i++)
        {
            Vector3 gridPos = GetNewGridPosition(i);
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
    public void MovePlayer(int step, int staminaUsed)
    {
        int stepToGo = step;
        currentGrid += step;
        if (currentGrid >= gridNum)
        {
            currentGrid = gridNum - 1;
            stepToGo = gridNum - 1 - currentGrid;
        }
        if (staminaUsed > 0)
        {
            stamina -= staminaUsed;
            EventManager.Instance.StaminaChange(-staminaUsed);
            staminaText.SetText(stamina.ToString());
            currentTurnToSink += staminaUsed;
        }
        gridText.SetText(currentGrid.ToString());
        ClearCloudsInDistance(currentGrid, vision);
        EventManager.Instance.PlayMoveAnimation(stepToGo,0);
    }
    void ClearCloudsInDistance(int startGrid, int distance)
    {
        if (startGrid == 0) return;
        for (int i = 0; i < startGrid+distance; i++)
        {
            if (i < clouds.Count)
            {
                clouds[i].GetComponent<Cloud>().Disappear();

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
            endText.SetText("Victory");
            endPanel.SetActive(true);

        }
        else if (stamina == 0 )
        {
            endText.SetText("Better luck next time!");
            endPanel.SetActive(true);

        }
        else
        {
            if (!grids[currentGrid].Item3)
            {
                isOperating = false;
            }
            else
            {
                GridSO.GridType currentGridType = grids[currentGrid].Item1.gridType;
                switch (currentGridType)
                {
                    case GridSO.GridType.Empty:
                        isOperating = false;
                        break;
                    case GridSO.GridType.MoveForward:
                        isOperating = true;
                        EventManager.Instance.PlaySFX(powerUpCip);
                        StartCoroutine(MoveCoroutine(1));
                        break;
                    case GridSO.GridType.MoveBackward:
                        isOperating = true;
                        StartCoroutine(MoveCoroutine(-1));
                        break;
                    case GridSO.GridType.AddCard:
                        isOperating = false;
                        EventManager.Instance.PlaySFX(powerUpCip);
                        EventManager.Instance.EnterAddCardGrid();
                        break;
                    case GridSO.GridType.DropCard:
                        isOperating = false;
                        EventManager.Instance.PlaySFX(powerUpCip);
                        EventManager.Instance.EnterDropCardGrid();
                        break;
                    case GridSO.GridType.CursedFrog:
                        EventManager.Instance.PlaySFX(thunderStrikeCip);
                        stamina--;
                        currentTurnToSink++;
                        EventManager.Instance.StaminaChange(-1);
                        isOperating = false;
                        staminaText.SetText(stamina.ToString());
                        if (stamina == 0)
                        {
                            endPanel.SetActive(true);

                        }
                        break;
                    case GridSO.GridType.Scout:
                        isOperating = false;
                        EventManager.Instance.PlaySFX(scountCip);
                        ClearCloudsInDistance(currentGrid, 6);
                        break;
                    case GridSO.GridType.Swamp:
                        isOperating = false;
                        EventManager.Instance.PlaySFX(thunderStrikeCip);
                        if (stamina > 2)
                        {
                            stamina -= 2;
                            EventManager.Instance.StaminaChange(-2);
                            staminaText.SetText(stamina.ToString());
                            currentTurnToSink += 2;
                        }
                        else
                        {
                            stamina = 0;
                            endPanel.SetActive(true);

                        }
                        break;
                    case GridSO.GridType.Teleport:
                        isOperating = true;
                        currentTurnToSink++;
                        StartCoroutine(TeleportCoroutine());
                        break;
                    default:
                        break;
                }
                grids[currentGrid] = (grids[currentGrid].Item1, grids[currentGrid].Item2, false);
                foreach (Transform child in grids[currentGrid].Item2.transform)
                {
                    if (child.gameObject.CompareTag("IslandOverlay"))
                    {
                        child.gameObject.SetActive(false);
                        break;
                    }
                }
            }

        }
        if (!isOperating)
        {
            EventManager.Instance.CompleteAction();
            Sink();
            isOperating = false;
        }
    }
    void Sink()
    {
        if (turnToSink <= 0 || sinkNum <= 0) return;
        int sinkMultiply = currentTurnToSink/turnToSink;

        if (sinkMultiply > 0)
        {
            currentTurnToSink %= turnToSink;
            int sinkToIndex = sinkedGridIndex + sinkNum * sinkMultiply;
            if (sinkToIndex >= gridNum - 1)
            {
                sinkToIndex = gridNum - 2;
            }
            for (int i = sinkedGridIndex; i < sinkToIndex; i++)
            {
                grids[i].Item2.GetComponent<Island>().PlayIslandSinkEffect(sinkToIndex);
                if (i < paths.Count)
                {
                    paths[i].SetActive(false);
                }

            }
            if (currentGrid <= sinkedGridIndex)
            {
                endPanel.SetActive(true);
            }
        }
    }
    private void CheckSinkedGrid(int sinkToIndex)
    {
        sinkedGridIndex = sinkToIndex;
        if (currentGrid < sinkedGridIndex)
        {
            endText.SetText("Better luck next time!");
            endPanel.SetActive(true);
        }
    }
    IEnumerator MoveCoroutine(int step)
    {
        yield return new WaitForSeconds(0.25f);
        if (step > 0)
            EventManager.Instance.EnterMoveForwardGrid(step, 0);
        else
            grids[currentGrid].Item2.GetComponent<Bomb>().ActivateBomb();
        EventManager.Instance.DistanceChange(step);
        MovePlayer(step,0);
    }
    IEnumerator TeleportCoroutine()
    {
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
        grids[currentGrid].Item2.GetComponent<Portal>().Active(true);
        yield return new WaitForSeconds(1.5f);
        EventManager.Instance.DistanceChange(randomGrid - currentGrid);
        grids[currentGrid].Item2.GetComponent<Portal>().Active(false);
        EventManager.Instance.EnterPortalGrid(grids[randomGrid].Item2.transform.position);
        MovePlayer(randomGrid - currentGrid, 0);
        GridAction();
    }
    void EndGame()
    {
        player.SetActive(false);
    }

}
