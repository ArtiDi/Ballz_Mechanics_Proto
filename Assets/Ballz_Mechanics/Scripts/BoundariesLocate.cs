using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class BoundariesLocate : MonoBehaviour
{
    public EdgeCollider2D LeftEdge;
    public EdgeCollider2D RightEdge;
    public EdgeCollider2D TopEdge;
    public EdgeCollider2D BottomEdge;
    public CircleCollider2D InfinityLockPreventer;
    public CircleCollider2D BallCollider;

    private float EdgePointDistanceX;
    private float EdgePointDistanceY;

    void Start()
    {
        
    }

    public void PlaceBoundaries(GameAreaData areaData)
    {
        EdgePointDistanceY = (float)Math.Round(areaData.Height_World / 2, 2);
        EdgePointDistanceX = (float)Math.Round(areaData.Width_World / 2, 2);
        var SidesEdgePoints = new Vector2[] { new Vector2(0, -EdgePointDistanceY), new Vector2(0, EdgePointDistanceY) };
        var TopBottomEdgePoints = new Vector2[] { new Vector2(-EdgePointDistanceX, 0), new Vector2(EdgePointDistanceX, 0) };

        //LEFT Border
        LeftEdge.transform.position = new Vector3(areaData.CornersWorld[0].x, (float)areaData.Middle_Y, 1);
        LeftEdge.points = SidesEdgePoints;

        //RIGHT Border
        RightEdge.transform.position = new Vector3(areaData.CornersWorld[2].x, (float)areaData.Middle_Y, 1);
        RightEdge.points = SidesEdgePoints;

        //TOP Border
        TopEdge.transform.position = new Vector3((float)areaData.Middle_X, areaData.CornersWorld[1].y, 1);
        TopEdge.points = TopBottomEdgePoints;

        //BOTTOM Border
        BottomEdge.transform.position = new Vector3((float)areaData.Middle_X, areaData.CornersWorld[0].y, 1);
        BottomEdge.points = TopBottomEdgePoints;

        //Preventer
        InfinityLockPreventer.transform.position = areaData.CornersWorld[2];
        InfinityLockPreventer.transform.localScale = BallCollider.transform.localScale;
        InfinityLockPreventer.radius = BallCollider.radius * 0.7f;
        InfinityLockPreventer.offset = BallCollider.offset;
    }

}
