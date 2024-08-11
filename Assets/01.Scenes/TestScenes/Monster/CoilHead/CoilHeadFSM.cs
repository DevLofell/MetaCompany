using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum CoilHeadState
{
    Idle,
    Patrol,
    Trace,
    FastTrace
}

public class CoilHeadFSM : MonoBehaviour
{
    public CoilHeadState coilHeadState = CoilHeadState.Idle;
    public NavMeshAgent navMeshAgent;
    public float patrolSpeed = 3f;
    public float traceSpeed = 5f;
    public float fastTraceSpeed = 10f;
    public float detectionRange = 10f;
    public float patrolRadius = 10f;
    public Transform player;
    public LayerMask obstacleLayer;

    private Vector3 randomDestination;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        StartCoroutine(FSMRoutine());
    }

    private IEnumerator FSMRoutine()
    {
        while (true)
        {
            yield return StartCoroutine(coilHeadState.ToString() + "State");
        }
    }

    public void ChangeState(CoilHeadState state)
    {
        coilHeadState = state;
    }

    private IEnumerator IdleState()
    {
        Debug.Log("Idle State");
        yield return new WaitForSeconds(2f);
        ChangeState(CoilHeadState.Patrol);
    }

    private IEnumerator PatrolState()
    {
        Debug.Log("Patrol State");
        navMeshAgent.speed = patrolSpeed;

        if (!navMeshAgent.hasPath)
        {
            randomDestination = RandomNavMeshPoint();
            navMeshAgent.SetDestination(randomDestination);
        }

        if (Vector3.Distance(transform.position, player.position) <= detectionRange)
        {
            ChangeState(CoilHeadState.Trace);
        }

        yield return null;
    }

    private IEnumerator TraceState()
    {
        Debug.Log("Trace State");
        navMeshAgent.speed = traceSpeed;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        RaycastHit hit;

        // Ray를 발사할 때 시작 위치를 위로 1f 올립니다.
        Vector3 rayStartPosition = transform.position + Vector3.up * 1f;
        Vector3 playerPosition = player.position + Vector3.up * 1f;

        if (Physics.Raycast(playerPosition, directionToPlayer, out hit, detectionRange, obstacleLayer))
        {
            if (hit.collider.gameObject == gameObject)
            {
                ChangeState(CoilHeadState.Patrol);
                yield break;
            }
        }

        navMeshAgent.SetDestination(player.position);

        if (!IsPlayerVisible())
        {
            navMeshAgent.isStopped = false;
            ChangeState(CoilHeadState.FastTrace);
        }
        else
        {
            if (IsPlayerLookingAtEnemy())
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.velocity = Vector3.zero;
            }
            else
            {
                navMeshAgent.isStopped = false;
                ChangeState(CoilHeadState.FastTrace);
            }
        }

        yield return null;
    }

    private IEnumerator FastTraceState()
    {
        Debug.Log("Fast Trace State");
        navMeshAgent.speed = fastTraceSpeed;

        navMeshAgent.SetDestination(player.position);

        if (IsPlayerVisible() || IsPlayerLookingAtEnemy())
        {
            ChangeState(CoilHeadState.Trace);
        }

        yield return null;
    }

    private bool IsPlayerLookingAtEnemy()
    {
        Vector3 directionToEnemy = (transform.position - player.position).normalized;
        float dotProduct = Vector3.Dot(player.forward, directionToEnemy);
        return dotProduct > 0.5f; // 플레이어가 적을 바라보는 각도를 조절할 수 있습니다.
    }

    private bool IsPlayerVisible()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        RaycastHit hit;

        // Ray를 발사할 때 시작 위치를 위로 1f 올립니다.
        Vector3 rayStartPosition = transform.position + Vector3.up * 1f;

        Debug.DrawRay(rayStartPosition, directionToPlayer * detectionRange, Color.red);
        if (Physics.Raycast(rayStartPosition, directionToPlayer, out hit, detectionRange, obstacleLayer))
        {
            Debug.Log("IsPlayerVisible()" + (hit.collider.gameObject == player.gameObject));
            return hit.collider.gameObject == player.gameObject;
        }
        return false;
    }

    private Vector3 RandomNavMeshPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, 1);
        return hit.position;
    }
}
