using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MovementController : MonoBehaviour
{
    const float pathUpdateMoveThreshold = .5f;
    const float minPathUpdateTime = .2f;

    public float Durability;
    public float Speed;
    public float Agility;
    private float stamina;

    public Camera _camera;
    private Vector3 Destination = Vector3.zero;
    public bool CanWalk;
    public bool isLeader;
    Path path;
    private float turnDst = 1;

    public Animator _animator;
    bool followingPath = false;
    public Slider staminaBar;
    public static Transform targetPointer;

    private void Awake()
    {
        _camera = Camera.main;
        CanWalk = true;
    }

    private void Start()
    {
        LoadParameters(Durability, Speed, Agility);
        StartCoroutine(UpdatePath());
        staminaBar.maxValue = Durability;
        targetPointer = GameObject.Find("Target").transform;
    }

    void Update()
    {
        SetPosition();
        StopCharacter();
    }

    public void LoadParameters(float speed, float agility, float durability)
    {
        MovementController leader = CharactersManager.Leader.GetComponent<MovementController>();
        //Speed = (leader.Speed < speed) ? leader.Speed : speed;
        Speed = speed;
        Durability = durability;
        Agility = agility;
        stamina = Durability;
    }

    void SetPosition()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
                Destination = (isLeader) ? hit.point : CharactersManager.Leader.transform.position;
            PathRequestManager.RequestPath(new PathRequest(transform.position, Destination, OnPathFound));
        }
    }

    public void OnPathFound(Vector3[] waypoints, bool pathSuccessfull)
    {
        if (pathSuccessfull)
        {
            path = new Path(waypoints, transform.position, turnDst);
            if (waypoints.Length is 0) return;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    public IEnumerator FollowPath()
    {
        followingPath = true;
        int pathIndex = 0;
        transform.LookAt(path.lookPoints[0]);
        if (isLeader)
            targetPointer.position = path.lookPoints[path.finishLineIndex];

        while (followingPath)
        {
            Vector2 pos2D = new(transform.position.x, transform.position.z);
            while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
            {
                if (pathIndex == path.finishLineIndex)
                {
                    _animator.SetBool("Running", false);
                    followingPath = false;
                    break;
                }
                else
                    pathIndex++;

                if (pathIndex == path.finishLineIndex - 1)
                    if (Vector3.Distance(transform.position, path.lookPoints[path.finishLineIndex]) < 1.5f)
                    {
                        _animator.SetBool("Running", false);
                        followingPath = false;
                        break;
                    };
            }

            if (followingPath)
            {
                if (CanWalk)
                {
                    _animator.SetBool("Running", true);
                    Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * Agility / 5f);
                    transform.Translate(Vector3.forward * Speed * Time.deltaTime, Space.Self);
                }
                else
                    _animator.SetBool("Running", false);
            }

            yield return null;
        }
    }

    public IEnumerator UpdatePath()
    {
        var target = (isLeader) ? Destination : CharactersManager.Leader.transform.position;
        if (Time.timeSinceLevelLoad < 2f)
            yield return new WaitForSeconds(.3f);

        PathRequestManager.RequestPath(new PathRequest(transform.position, target, OnPathFound));

        float sqrMoveTreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = target;

        while(true)
        {
            yield return new WaitForSeconds(minPathUpdateTime);
            target = (isLeader) ? Destination : CharactersManager.Leader.transform.position;
            if ((target - targetPosOld).sqrMagnitude > sqrMoveTreshold)
            {
                PathRequestManager.RequestPath(new PathRequest(transform.position, target, OnPathFound));
                targetPosOld = target;
            }
        }
    }

    void StopCharacter()
    {
        if (!CanWalk)
            StartCoroutine(CharacterRest());

        if(followingPath)
            DurabilitySystem();
    }

    void DurabilitySystem()
    {
        if (stamina <= 0) 
            CanWalk = false;
        else
            stamina -= Time.deltaTime;

        staminaBar.value = stamina;
    }

    IEnumerator CharacterRest()
    {
#if UNITY_EDITOR
        Debug.Log($"Character {this.gameObject.name}, is Resting");
#endif
        yield return new WaitForSeconds(4);
        stamina = Durability;
        CanWalk = true;
    }

    public void OnDrawGizmos()
    {
        if (path != null)
        {
            path.DrawWithGizmos();
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag is "Player")
        {
            Vector3 oppositeDirection = -collision.contacts[0].normal;
            transform.Translate(oppositeDirection * Speed * Time.deltaTime);

#if UNITY_EDITOR || DEBUG
            Debug.Log($"Collision between {this.gameObject.name} and {collision.gameObject.name} in {collision.transform.position}");
#endif
        }
    }

    
}
