using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.PlayerSettings;
using static UnityEngine.GraphicsBuffer;

public class CharacterController : MonoBehaviour
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
    public Rigidbody _rb;

    private void Awake()
    {
        _camera = Camera.main;
        _rb = GetComponent<Rigidbody>();
        CanWalk = true;
    }

    private void Start()
    {
        LoadParameters(Durability, Speed, Agility);
        StartCoroutine(UpdatePath());
    }

    void Update()
    {
        SetPosition();
    }

    public void LoadParameters(float speed, float agility, float durability)
    {
        CharacterController leader = CharactersManager.Leader.GetComponent<CharacterController>();
        Speed = (leader.Speed < speed) ? leader.Speed : speed;

        Durability = durability;
        Agility = agility;
        stamina = Durability;
    }

    public void MoveToPosition(Vector3 pos)
    {
        StopCharacter();
    }


    void SetPosition()
    {
        if(Input.GetMouseButtonDown(0))
        {
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
            _animator.SetBool("Running", true);
            path = new Path(waypoints, transform.position, turnDst);
            if (waypoints.Length is 0) return;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    public IEnumerator FollowPath()
    {
        bool followingPath = true;
        int pathIndex = 0;
        transform.LookAt(path.lookPoints[0]);

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
                    if (Vector3.Distance(path.lookPoints[pathIndex], path.lookPoints[path.finishLineIndex]) < 1.2f)
                    {
                        _animator.SetBool("Running", false);
                        followingPath = false;
                        break;
                    };
            }

            Debug.Log(Vector3.Distance(path.lookPoints[pathIndex], path.lookPoints[path.finishLineIndex]));

            if (followingPath)
            {
                Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * Agility/5f);
                transform.Translate(Vector3.forward * Speed * Time.deltaTime, Space.Self);

                //Vector3 localVelocity = transform.forward * Speed * Time.fixedDeltaTime * 80;
                //_rb.velocity = localVelocity;
            }
            else 
                //_rb.velocity = Vector3.zero;

            Debug.Log(this.gameObject.name + "Following path: " + followingPath);

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
        {
            //_agent.isStopped = true;
            StartCoroutine(CharacterRest());
        }
        else
        {
            //_agent.isStopped = false;
        }

        //if(_agent.velocity.x != 0 && _agent.velocity.z != 0)
         //   DurabilitySystem();
    }

    void DurabilitySystem()
    {
        if (stamina <= 0) CanWalk = false;
        stamina -= Time.deltaTime;
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
}
