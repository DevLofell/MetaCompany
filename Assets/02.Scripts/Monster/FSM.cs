using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


public class FSM : MonoBehaviour
{
    public enum EEnemyState
    {
        WalkClam,
        Rotate_,
        Chase_,
        Attack
    }

    // 현재 상태
    public EEnemyState currentState;

    // Animator 컴포넌트를 저장할 전역 변수
    Animator animator;

    // NavMeshAgent 컴포넌트를 저장한 전역 변수
    NavMeshAgent agent;

    // NavMeshSurface 경계를 저장한 전역 변수
    Bounds navMeshBounds;

    // 플레이어 찾기
    GameObject player;

    // 플레이어 방향 구하기 전역 변수
    Vector3 dir;

    // 플레이어 방향으로 회전하기 위해  어느정도 돌아야 하는 지 구하는 전역 변수
    Quaternion lookRotation;

    // 플레이어의 최대 체력
    public PlayerMove play_health;

    // 콜라이더 컴포넌트
    SphereCollider scollider;

    // 콜라이더의 라디어스 값
    float radius;


    // walk clam의 setDestination 남은 거리 
    public float remaindistnace2;

    // player 방향 콕 알기
    Vector3 player_p;


    // 추격 중인지 아닌지
    bool isChasing;

    // 공격 중인지 아닌지
    bool isAttacking;

    public float walkSpeed = 0.2f;
    public float chaseSpeed = 1f;
    public float attackSpeed = 1.5f;


    void Start()
    {
        // 현재 게임 오브젝트에서 Animator 컴포넌트를 찾는다.
        animator = GetComponentInChildren<Animator>();

        // 현재 게임 오브젝트에서 NavMeshAgent 컴포넌트를 찾는다.
        agent = GetComponent<NavMeshAgent>();

        // NavMeshSurface 바운딩 박스를 설정한다.
        NavMeshSurface navMeshSurface = FindObjectOfType<NavMeshSurface>();

        // 플레이어 찾기
        player = GameObject.Find("Player");

        // 플레이어의 체력 관련 메서드 가져오기
        play_health = player.GetComponent<PlayerMove>();

        // 원형 콜라이더  갖고 오기
        scollider = GetComponent<SphereCollider>();

        // 원형 콜라이더의 반지름
        radius = scollider.radius;

        // NavMeshSurface 경계를 가져온다.
        if (navMeshSurface != null)
        {
            navMeshBounds = navMeshSurface.navMeshData.sourceBounds;
        }

        NavMeshHit hit;

        // 일단 start 부분에서 눈 없는 개 무작정 돌아다니게 하기
        if (NavMesh.SamplePosition(RandomPositionSetting(), out hit, navMeshBounds.size.magnitude, 1))
        {
            
            agent.SetDestination(hit.position);
            agent.speed = walkSpeed;
        }

        // 애니메이터 작동하기
        animator.SetBool("WalkClam", true);

        if (player == null)
        {
            Debug.LogError("Player not found. Make sure the Player object has the 'Player' tag.");
        }



    }


    void Update()
    {
        switch (currentState)
        {
            case EEnemyState.WalkClam:
                UpdateWalkClam();
                break;

            case EEnemyState.Rotate_:
                UpdateRotate();
                break;

            case EEnemyState.Chase_:
                UpdateChase_Check();
                break;

            case EEnemyState.Attack:
                Attack();
                break;
        }
    }

    void ChangState(EEnemyState state)
    {
        currentState = state;

        switch (currentState)
        {
            case EEnemyState.Rotate_:
                animator.SetBool("WalkClam", false);
                animator.SetBool("Rotate_", true);
                agent.isStopped = true; // 회전 중에는 정지
                isChasing = false;
                isAttacking = false;
                break;

            case EEnemyState.Chase_:
                animator.SetBool("Rotate_", false);
                animator.SetBool("Chase_", true);
                agent.isStopped = false; // 추적 시작
                agent.speed = chaseSpeed;
                isChasing = true;
                isAttacking = false;
                break;

            case EEnemyState.Attack:
                animator.SetBool("Chase_", false);
                animator.SetBool("Attack_", true);
                isChasing = false;
                agent.speed = attackSpeed;
                isAttacking = true;
                break;

            case EEnemyState.WalkClam:
                animator.SetBool("WalkClam", true);
                animator.SetBool("Chase_", false);
                animator.SetBool("Attack_", false);
                isChasing = false;
                isAttacking = false;
                break;
        }
    }
    void UpdateWalkClam()
    {
        remaindistnace2 = 2f;


        if (agent.remainingDistance < remaindistnace2)
        {

            NavMeshHit hit;

            if (NavMesh.SamplePosition(RandomPositionSetting(), out hit, navMeshBounds.size.magnitude, 1))
            {
                if(agent.pathPending)
                {
                    return;
                }
                agent.SetDestination(hit.position);
            }
        }
    }

    Vector3 RandomPositionSetting()
    {

        Vector3 randomPosition = new Vector3
        (
            Random.Range(navMeshBounds.max.x * -0.5f, navMeshBounds.max.x * 0.5f),
            Random.Range(navMeshBounds.max.y * -0.5f, navMeshBounds.max.y * 0.5f),
            Random.Range(navMeshBounds.max.z * -0.5f, navMeshBounds.max.z * 0.5f)
        );

        return randomPosition;
    }

    private float rotationDuration = 7f; // 회전에 걸리는 시간 (초)
    private float rotationStartTime; // 회전 시작 시간

    void UpdateRotate()
    {
        // 회전 시작 시간이 설정되지 않았다면 현재 시간으로 설정
        if (rotationStartTime == 0)
        {
            rotationStartTime = Time.time;
        }

        // 경과 시간 계산
        float elapsedTime = Time.time - rotationStartTime;
        float t = Mathf.Clamp01(elapsedTime / rotationDuration);

        // Slerp를 사용하여 부드럽게 회전
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, t);

        // 현재 각도 계산
        float angle = Quaternion.Angle(transform.rotation, lookRotation);
        print("Current angle: " + angle);

        // 회전이 완료되었거나 시간이 다 되었을 때
        if (angle < 0.1f || t >= 1)
        {
            print("회전 완료!");
            rotationStartTime = 0; // 회전 시작 시간 리셋

            if ((radius / 2) <= dir.magnitude)
            {
                print("Chase 상태로 전환");
                ChangState(EEnemyState.Chase_);
            }
            else
            {
                print("Attack 상태로 전환");
                ChangState(EEnemyState.Attack);
            }
        }
    }

    private void UpdateChase_Check()
    {
        agent.SetDestination(player_p);
        // 플레이어 방향 찾기
        // Vector3 pos = player.transform.position;
        // pos.y = transform.position.y;
        // dir = pos - transform.position;

        // print(agent.remainingDistance);

        // 여기서 바로 가는 구나

        if ((radius / 1.5) > agent.remainingDistance)
        {
            ChangState(EEnemyState.Attack);
        }

        else if (agent.remainingDistance < 0.1f)
        {
            // 목표 지점에 도달했을 때
            isChasing = false;
            animator.SetBool("WalkClam", true);
            ChangState(EEnemyState.WalkClam);
        }
    }

    void Attack()
    {
        agent.SetDestination(initialTargetPosition);
        if (agent.remainingDistance > 0.1f)
        {
            agent.SetDestination(initialTargetPosition);
        }
        else
        {
            // 목표 지점에 도달했을 때
            isAttacking = false;
            animator.SetBool("WalkClam", true);
            ChangState(EEnemyState.WalkClam);
        }
    }

    private Vector3 initialTargetPosition;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("Player"))
        {
            if (!isChasing && !isAttacking)
            {
                // 추격이나 공격 중이 아닐 때만 새로운 목표 위치 설정
                player_p = player.transform.position;
                initialTargetPosition = player_p;

                dir = player_p - transform.position;
                dir.y = 0;

                lookRotation = Quaternion.LookRotation(dir);

                if (Quaternion.Angle(transform.rotation, lookRotation) >= 10)
                {
                    ChangState(EEnemyState.Rotate_);
                }
                else
                {
                    if ((radius / 2) <= dir.magnitude)
                    {
                        ChangState(EEnemyState.Chase_);
                    }
                    else
                    {
                        ChangState(EEnemyState.Attack);
                    }
                }
            }
            else
            {
                // 추격 중이나 공격 중일 때는 initialTargetPosition을 계속 사용
                agent.SetDestination(initialTargetPosition);
            }
        }
    }
}