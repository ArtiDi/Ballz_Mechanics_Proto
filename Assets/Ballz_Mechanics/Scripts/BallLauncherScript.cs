using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using System;

public class BallLauncherScript : MonoBehaviour
{
    
    public LineRenderer lineRenederer;
    public Camera ActiveCamera;
    public GameObject Ball;
    public TriggerScript OutOfBoundTrigger;
    public MainManagerScript MainManager;

    

    ReboundPathDrawer reboundPathDrawer;
    Queue<GameObject> BallPool;
    public int shootedBallsCount = 0;


    void Start()
    {
        reboundPathDrawer = new ReboundPathDrawer();
       
        OutOfBoundTrigger.TriggerEnterEvent += OutOfBoundTrigger_TriggerEnterEvent;
    }

    public void CreateBalls()
    {
        if (BallPool != null)
        {
            foreach (var ball in BallPool)
            {
#if UNITY_EDITOR
                DestroyImmediate(ball.gameObject);
#else
                Destroy(ball.gameObject);
#endif
            }
        }
        BallPool = new Queue<GameObject>();

        for (int i = 0; i < 50; i++)
        {
            var ball = CreateBall(Vector2.zero, Ball);
            ball.SetActive(false);
            BallPool.Enqueue(ball);
        }
    }
    

    void Update()
    {
        
    }

    public void DrawPath(Vector2 StartPoint, Vector2 Direction, int RayCount=1)
    {
        reboundPathDrawer.DrawPath(lineRenederer, StartPoint, Direction, RayCount);
    }
    private void OutOfBoundTrigger_TriggerEnterEvent(Collider2D obj)
    {
        if (obj.CompareTag("Ball"))
        {
            obj.gameObject.SetActive(false);
            BallPool.Enqueue(obj.gameObject);
            shootedBallsCount--;
            if (shootedBallsCount <= 0)
            {
                MainManager.CurrentState = GameState.Idle;
            }
        }
    }
    public void Shoot(Vector2 startPosition, Vector2 direction, int ballscount, float delay)
    {
        StartCoroutine(ShootCoroutine(startPosition, direction, ballscount, delay));
    }
    private IEnumerator ShootCoroutine(Vector2 startPosition, Vector2 direction, int ballscount, float delay)
    {
        for (int i = 0; i < ballscount; i++)
        {
            if (BallPool.Count > 0)
            {
                var ball = BallPool.Dequeue();
                ball.transform.position = startPosition;
                ball.SetActive(true);
                LaunchBall(ball, direction);
            }
            else
            {
                LaunchBall(CreateBall(startPosition, Ball), direction);
            }
            yield return new WaitForSecondsRealtime(delay);
        }
    }

    void LaunchBall(GameObject ball, Vector2 direction)
    {
        ball.GetComponent<Rigidbody2D>().AddForce(direction, ForceMode2D.Force);
    }
    GameObject CreateBall(Vector2 position, GameObject ballPrefab)
    {
        var newBall = Instantiate(ballPrefab, position, Quaternion.identity);
        return newBall;
    }

    public class ReboundPathDrawer
    {
        private List<Vector2> PathPoints;
        private RaycastHit2D[] RaycastHitBuff;
        public ReboundPathDrawer()
        {
            PathPoints = new List<Vector2>();
            RaycastHitBuff = new RaycastHit2D[2];
        }

        private void CalculatePath(Vector2 startPoint, Vector2 Direction, int RayCount = 1)
        {
            try
            {
                if (RayCount < 1) RayCount = 1;

                if (PathPoints == null) PathPoints = new List<Vector2>();

                if (PathPoints.Count > 0) PathPoints.Clear();

                PathPoints.Add(startPoint);

                var direction = Direction;
                Collider2D castingColliderFrom = null;
                for (int i = 0; i < RayCount; i++)
                {
                    Physics2D.RaycastNonAlloc(PathPoints[i], direction, RaycastHitBuff);
                    RaycastHit2D hit;
                    if (RaycastHitBuff[0].collider == castingColliderFrom)
                        hit = RaycastHitBuff[1];
                    else
                        hit = RaycastHitBuff[0];

                    castingColliderFrom = hit.collider;
                    direction = Vector2.Reflect(direction, hit.normal.normalized);
                    PathPoints.Add(hit.point);
                    Array.Clear(RaycastHitBuff, 0, RaycastHitBuff.Length);
                }
                
            }
            catch (Exception e)
            {

            }
        }
        public void DrawPath(LineRenderer lineRenderer, Vector2 startPoint, Vector2 Direction, int RayCount = 1)
        {
            try
            {
                if (RayCount < 1) RayCount = 1;
                PathPoints.Clear();
                CalculatePath(startPoint, Direction, RayCount);
                lineRenderer.positionCount = PathPoints.Count;
                for (int i = 0; i < PathPoints.Count; i++)
                {
                    lineRenderer.SetPosition(i, PathPoints[i]);
                }
            }
            catch (Exception e)
            {

            }
        }
    }
}
