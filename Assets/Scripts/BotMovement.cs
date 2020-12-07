using System.Collections;
using System.Collections.Generic;
using BaseAI;
using UnityEngine;

public class BotMovement : MonoBehaviour
{
    public List<BaseAI.PathNode> PlannedPath;
    public static float Speed;
    
    private List<BaseAI.PathNode> currentPath;
    private BaseAI.PathNode currentTarget;
    
    [SerializeField] private BaseAI.MovementProperties movementProperties;

    public int steps;
    private float leftLegAngle = 3f;
    [SerializeField] private bool walking = false;

    [SerializeField] private GameObject leftLeg;
    [SerializeField] private GameObject rightLeg;
    [SerializeField] private GameObject leftLegJoint;
    [SerializeField] private GameObject rightLegJoint;

    [SerializeField] private bool needToJump;

    private Rigidbody rigidbody;
    private float gravity = 5.0f;
    private float jumpHeight = 1.5f;
    private LocalPathPlaner _localPathPlaner;
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        Speed = movementProperties.maxSpeed;
    }

    public void SetLocalPath(List<PathNode> pathList, LocalPathPlaner planer)
    {
        currentPath = new List<PathNode>(pathList);
        _localPathPlaner = planer;
    }
    
    void MoveLegs()
    {
        //  Движение ножек сделать
        if (steps >= 20)
        {
            leftLegAngle = -leftLegAngle;
            steps = -20;
        }
        steps++;

        leftLeg.transform.RotateAround(leftLegJoint.transform.position, transform.right, leftLegAngle);
        rightLeg.transform.RotateAround(rightLegJoint.transform.position, transform.right, -leftLegAngle);
    }
    /// <summary>
    /// Обновление текущей целевой точки - куда вообще двигаться
    /// </summary>
    private bool UpdateCurrentTargetPoint()
    {
        //  Если есть текущая целевая точка
        if(currentTarget != null)
        {
            float distanceToTarget = currentTarget.Distance(transform.position);
            //  Если до текущей целевой точки ещё далеко, то выходим
            if (distanceToTarget >= movementProperties.epsilon || currentTarget.TimeMoment - Time.fixedTime > movementProperties.epsilon) return true;
            //  Иначе удаляем её из маршрута и берём следующую
            //Debug.Log("Point reached : " + Time.fixedTime.ToString());
            currentPath.RemoveAt(0);
            if (currentPath.Count > 0) 
            {
                //  Берём очередную точку и на выход
                currentTarget = currentPath[0];
                return true;
            }
            else
            {
                currentTarget = null;
                currentPath = null;
                //  А вот тут надо будет проверять, есть ли уже построенный маршрут
            }
        }
        else 
        if(currentPath != null)
        {
            if(currentPath.Count > 0 )
            {
                currentTarget = currentPath[0];
                return true;
            }
            else
            {
                currentPath = null;
            }
        }

        //lock(plannedPath)
        {
            if(PlannedPath != null)
            {
                currentPath = PlannedPath;
                PlannedPath = null;
                if (currentPath.Count > 0)
                    currentTarget = currentPath[0];
            }
        }

        var res = currentTarget != null;
        if (walking && !res)
        {
            walking = false;
            if (_localPathPlaner)
            {
                _localPathPlaner.IsWalking = false;

            }
        }
        return res;
    }

    bool MoveBot()
    {
        if (!UpdateCurrentTargetPoint())
            return false;
        walking = true;

        Vector3 directionToTarget = currentTarget.Position - transform.position;
        float angle = Vector3.SignedAngle(transform.forward, directionToTarget, Vector3.up);
        angle = Mathf.Clamp(angle, -movementProperties.rotationAngle, movementProperties.rotationAngle);
        transform.Rotate(Vector3.up, angle);

        RaycastHit hit;
        if (!Physics.Raycast(transform.position, new Vector3(0, -1, 0), out hit, 2))
            return true;

        if (!currentTarget.IsAboveTheGround())
        {
            needToJump = true;

            return true;
        }

        if (needToJump)
        {
            Jump();
            needToJump = false;
            movementProperties.IsOnPlatform = !movementProperties.IsOnPlatform;
            return true;
        }

        if (movementProperties.IsOnPlatform)
            return true;

        if (!Physics.Raycast(transform.position, new Vector3(0, -1, 0), out hit, 2))
            return true;

        float stepLength = directionToTarget.magnitude;
        float actualStep = Mathf.Clamp(stepLength, 0.0f, movementProperties.maxSpeed * Time.deltaTime);
        
        transform.position = transform.position + actualStep * transform.forward;
        return true;
    }

    private void Jump()
    {
        rigidbody.velocity = new Vector3(rigidbody.velocity.x, CalculateJumpVerticalSpeed(), 5);
        float CalculateJumpVerticalSpeed() => Mathf.Sqrt(2 * jumpHeight * gravity);
    }

    void Update()
    {
        MoveBot();
        MoveLegs();
    }
    
}
