using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.PlayerSettings;

public class CharacterController : MonoBehaviour
{
    public float Durability;
    public float Speed;
    public float Agility;
    private float stamina;

    public NavMeshAgent _agent;
    public Camera _camera;
    private Vector3 Destination = Vector3.zero;
    public bool CanWalk;
    public bool isLeader;

    Vector3[] path;
    int targetIndex;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _camera = Camera.main;
        CanWalk = true;
    }

    private void Start()
    {
        LoadParameters(Durability, Speed, Agility);
    }

    void Update()
    {
        if (isLeader)
        {
            SetPosition();
            //MoveToPosition(Destination);
        }
       else
            MoveToPosition(CharactersManager.Leader.transform.position);
    }

    public void LoadParameters(float speed, float agility, float durability)
    {
        CharacterController leader = CharactersManager.Leader.GetComponent<CharacterController>();
        Speed = (leader.Speed < speed) ? leader.Speed : speed;

        Durability = durability;
        Agility = agility;
        stamina = Durability;
        _agent.speed = Speed;
    }

    public void MoveToPosition(Vector3 pos)
    {
        _agent.SetDestination(pos);
        StopCharacter();
    }

    void SetPosition()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit)) Destination = hit.point;
            if (Destination == null) return;
            PathRequestManager.RequestPath(transform.position, Destination, OnPathFound);
        }
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessfull)
    {
        if (pathSuccessfull)
        {
            path = newPath;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
            Debug.Log("Path lenght" + path.Length);
        }
    }

    IEnumerator FollowPath()
    {
        Vector3 currentWaypoint = path[0];
        
        while(true)
        {
            //if(transform.position == currentWaypoint)
            var distance = Vector2.Distance(transform.position, currentWaypoint);
            Debug.Log("Distance: " + distance);
            if(distance < 1.1f)
            {
                targetIndex++;
                if (targetIndex >= path.Length)
                {
                    yield break;
                }
                currentWaypoint = path[targetIndex];
            }
            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, Speed * Time.deltaTime);
            yield return null;
        }
    }

    void StopCharacter()
    {
        if (!CanWalk)
        {
            _agent.isStopped = true;
            StartCoroutine(CharacterRest());
        }
        else
        {
            _agent.isStopped = false;
        }

        if(_agent.velocity.x != 0 && _agent.velocity.z != 0)
            DurabilitySystem();
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
            for (int i = targetIndex; i < path.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(path[i], Vector3.one);

                if (i == targetIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i]);
                }
                else
                {
                    Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
        }
    }

    }
