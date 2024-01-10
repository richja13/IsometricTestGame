using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;

public class CharacterController : MonoBehaviour
{
    public float Durability;
    public float Speed;
    public float Agility;

    public NavMeshAgent _agent;
    public Camera _camera;
    private Vector3 Destination = Vector3.zero;
    public bool CanWalk;
    public bool isLeader;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _camera = Camera.main;
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
            if (CanWalk) MoveToPosition(Destination);
        }
        else
        {
            MoveToPosition(CharactersManager.Leader.transform.position);
        }
    }

    public void LoadParameters(float speed, float agility, float durability)
    {
        Durability = durability;
        Speed = speed;
        _agent.speed = Speed;
        Agility = agility;
    }

    public void MoveToPosition(Vector3 pos)
    {
        _agent.SetDestination(pos);
        CanWalk = true;
        ResetPosition();
    }

    void SetPosition()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit)) Destination = hit.point;
            CanWalk = true;
        }
    }

    void ResetPosition()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Destination = transform.position;
            CanWalk = false;
        }
    }

 
}
