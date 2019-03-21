using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using UnityEngine.EventSystems;
using System.Linq;

public enum GameState { Pause = 0, Idle = 1, Aiming = 2, Shooting = 3, CustingSpell = 4 }

[ExecuteAlways]
public class MainManagerScript : MonoBehaviour
{
    public static MainManagerScript Instance { get; set; }
    public Camera ActiveCamera;
    [SerializeField]
    private RectTransform GameArea;
    public GameObject ActiveBall;
    [Range(1, 25)]
    public int BlockInRawCount=1;

    public Tilemap tilemap;
    GameAreaData gameAreaData;
    BoundariesLocate boundariesLocate;

  
    public GameState CurrentState;
    public BallLauncherScript BallLauncher;

    public GameObject LevelCompletedUI;
    public GameObject LevelFailedUI;

    public TriggerScript BottomTrigger;

    Vector2 MousePosition;
    Vector2 StartPoint;
    Vector2 Direction;

    private void Awake()
    {
        Instance = this;
        CurrentState = GameState.Idle;
    }
    private void Start()
    {
        InitialGameAreaCalculations();
    }
    private void Update()
    {
        MousePosition = ActiveCamera.ScreenToWorldPoint(Input.mousePosition);
        StartPoint = BallLauncher.transform.position;
         
        ProccessInput();
    }

    void ProccessInput()
    {
        if (CurrentState == GameState.Pause)
            return;
        else if (CurrentState == GameState.Idle)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                    return;

                CurrentState = GameState.Aiming;
                BallLauncher.lineRenederer.enabled = true;
            }
        }
        else if (CurrentState == GameState.Aiming)
        {
            if (Input.GetMouseButton(0))
            {
                try
                {
                    if (EventSystem.current.IsPointerOverGameObject())
                        return;
                    MousePosition = ActiveCamera.ScreenToWorldPoint(Input.mousePosition);
                    StartPoint = BallLauncher.transform.position;

                    if (MousePosition.y <= StartPoint.y)
                        MousePosition.y = StartPoint.y + StartPoint.y * 0.05f;
                    Direction = (MousePosition - StartPoint).normalized;

                    BallLauncher.DrawPath(StartPoint, Direction, 2);
                }
                catch (Exception e)
                {
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                BallLauncher.lineRenederer.enabled = false;
                CurrentState = GameState.Shooting;
                var ballsCount = 50;
                BallLauncher.shootedBallsCount = ballsCount;
                BallLauncher.Shoot(BallLauncher.transform.position, Direction, ballsCount, 0.05f);
            }
        }
        else if (CurrentState == GameState.Shooting)
            return;
    }

    public void RowStepDown()
    {
        for (int i = 0; i < tilemap.transform.childCount; i++)
        {
            var child = tilemap.transform.GetChild(i);
            var cell = tilemap.layoutGrid.WorldToCell(child.transform.position);
            var newCell = new Vector3Int(cell.x, cell.y - 1, cell.z);
            child.transform.position = tilemap.GetCellCenterWorld(newCell);
        }

        CheckFail();
    }
    public void RowStepUp()
    {
        for (int i = 0; i < tilemap.transform.childCount; i++)
        {
            var child = tilemap.transform.GetChild(i);
            var cell = tilemap.layoutGrid.WorldToCell(child.transform.position);
            var newCell = new Vector3Int(cell.x, cell.y + 1, cell.z);
            child.transform.position = tilemap.GetCellCenterWorld(newCell);
        }
    }
    public void ClearAll()
    {
        try
        {
            while (tilemap.transform.childCount >= 1)
            {
                var block = tilemap.transform.GetChild(0);
#if UNITY_EDITOR
                DestroyImmediate(block.gameObject);
#else
                Destroy(block.gameObject);
#endif
            }
        }
        catch (Exception e)
        {

        }
    }
    public void InitialGameAreaCalculations()
    {
        boundariesLocate = GetComponent<BoundariesLocate>();

        Canvas.ForceUpdateCanvases();
        gameAreaData = new GameAreaData(GameArea);

        var gameAreaWidth = gameAreaData.Width_World;
        var totalBlockSpaceInRaw = gameAreaWidth;
        var singleBlockSpace = (float)totalBlockSpaceInRaw / BlockInRawCount;
        tilemap.layoutGrid.transform.position = gameAreaData.CornersWorld[1];
        tilemap.transform.position = gameAreaData.CornersWorld[1];

        var blocks = new List<GameObject>();
        var prevState = new Dictionary<GameObject, Vector3Int>();
        for (int i = 0; i < tilemap.transform.childCount; i++)
        {
            var child = tilemap.transform.GetChild(i);
            blocks.Add(child.gameObject);
            var cell = tilemap.layoutGrid.WorldToCell(child.transform.position);
            prevState.Add(child.gameObject, cell);
        }

        tilemap.layoutGrid.cellSize = new Vector3(singleBlockSpace, singleBlockSpace, 0);

        foreach (var block in blocks)
        {
            block.transform.localScale = new Vector3(singleBlockSpace, singleBlockSpace, 0);
            block.transform.position = tilemap.GetCellCenterWorld(prevState[block]);
        }
        ActiveBall.transform.localScale = new Vector3(singleBlockSpace, singleBlockSpace, 0);
        BallLauncher.CreateBalls();
        boundariesLocate.PlaceBoundaries(gameAreaData);
    }
    public void CheckFail()
    {
        var blocks = new List<GameObject>();
        for (int i = 0; i < tilemap.transform.childCount; i++)
        {
            var child = tilemap.transform.GetChild(i);
            if (child.transform.tag.Equals("Block"))
                blocks.Add(child.gameObject);
        }
        var cells = new List<Vector3Int>();
        foreach (var block in blocks)
        {
            cells.Add(tilemap.WorldToCell(block.transform.position)); 
        }

        var bottomY = tilemap.WorldToCell(BottomTrigger.gameObject.transform.position).y;

        var bottomCells = cells.Where(cell => cell.y == bottomY).ToList();
        if (bottomCells.Count > 0)
        {
            CurrentState = GameState.Pause;
            if(!LevelFailedUI.activeInHierarchy)
                LevelFailedUI.SetActive(true);
        }
    }
    public void CheckWin()
    {
        bool won = true;
        for (int i = 0; i < tilemap.transform.childCount; i++)
        {
            var child = tilemap.transform.GetChild(i);
            if (child.tag.Equals("Block"))
            {
                won = false;
                break;
            }
        }
        if (won)
        {
            CurrentState = GameState.Pause;
            if (!LevelCompletedUI.activeInHierarchy)
            {
                LevelCompletedUI.SetActive(true);
            }
        }
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(MainManagerScript))]
public class MainManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var script = (MainManagerScript)target;
        if (GUILayout.Button("Recalculate Game Area"))
        {
            script.InitialGameAreaCalculations();
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Row Step Down"))
        {
            script.RowStepDown();
        }
        if (GUILayout.Button("Row Step Up"))
        {
            script.RowStepUp();
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Clear All"))
        {
            script.ClearAll();
        }
     
    }
}
#endif

public class GameAreaData
{
    public GameAreaData(RectTransform rectTransform)
    {
        CornersWorld = new Vector3[4];

        rectTransform.GetWorldCorners(CornersWorld);

        Height_World = (CornersWorld[1] - CornersWorld[0]).y;
        Width_World = (CornersWorld[3] - CornersWorld[0]).x;
        Middle_X = Width_World / 2 + CornersWorld[0].x;
        Middle_Y = Height_World / 2 + CornersWorld[0].y;

    }
    public Vector3[] CornersWorld; 
    public double Height_World
    { get; private set; }
    public double Width_World
    { get; private set; }
    public double Middle_X
    { get; private set; }
    public double Middle_Y
    { get; private set; }
}
