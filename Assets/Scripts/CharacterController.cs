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
            MoveToPosition(Destination);
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
}
