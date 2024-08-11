using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class FSM_SoundCheck : MonoBehaviour
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

    // 플레이어 방향으로 회전하기 위해 어느정도 돌아야 하는 지 구하는 전역 변수
    Quaternion lookRotation;

    // 플레이어의 최대 체력
    HpSystem play_health;

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

    // 딱 감지되었을 때 player 처음 위치
    private Vector3 initialTargetPosition;



    Transform playerTransform;
    AudioSource playerAudioSource;
    float soundThreshold = 0.1f;
    bool isPlayerDetected;

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
        play_health = player.GetComponent<HpSystem>(); // 아 이렇게 갖고 왔구나...


        playerTransform = player.GetComponent<Transform>();

        // 원형 콜라이더 갖고 오기
        scollider = GetComponent<SphereCollider>();

        // 원형 콜라이더의 반지름
        radius = scollider.radius;

        // NavMeshSurface 경계를 가져온다.
        if (navMeshSurface != null)
        {
            navMeshBounds = navMeshSurface.navMeshData.sourceBounds;
        }

        // 애니메이터 작동하기
        animator.SetBool("WalkClam", true);

        NavMeshHit hit;

        // 일단 start 부분에서 눈 없는 개 무작정 돌아다니게 하기
        if (NavMesh.SamplePosition(RandomPositionSetting(), out hit, navMeshBounds.size.magnitude, 1))
        {
            agent.SetDestination(hit.position);
            agent.speed = walkSpeed;
        }

        playerAudioSource = player.GetComponentInChildren<AudioSource>();
    }

    void Update()
    {
        // Sound_Check 로직
        CheckPlayerSound();

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

        // 사운드 체크 로직
        if (isPlayerDetected && currentState != EEnemyState.Attack)
        {
            if (!isChasing && !isAttacking)
            {
                UpdateRotate();
            }
        }
    }

    void ChangState(EEnemyState state)
    {
        currentState = state;

        switch (currentState)
        {
            case EEnemyState.WalkClam:
                animator.SetBool("WalkClam", true);
                animator.SetBool("Attack_", false);
                isChasing = false;
                isAttacking = false;
                break;

            case EEnemyState.Rotate_:
                animator.SetBool("Rotate_", true);
                animator.SetBool("WalkClam", false);
                animator.SetBool("Attack_", false);
                agent.isStopped = true;
                isChasing = false;
                isAttacking = false;
                break;

            case EEnemyState.Chase_:
                animator.SetBool("Chase_", true);
                animator.SetBool("Rotate_", false);
                animator.SetBool("WalkClam", false);
                agent.isStopped = false;
                agent.speed = chaseSpeed;
                isChasing = true;
                isAttacking = false;
                break;

            case EEnemyState.Attack:
                animator.SetBool("Attack_", true);
                animator.SetBool("Chase_", false);
                animator.SetBool("WalkClam", false);
                isChasing = false;
                agent.speed = attackSpeed;
                isAttacking = true;
                break;
        }
    }

    void UpdateWalkClam()
    {
        remaindistnace2 = 0.5f;

        if (agent.remainingDistance < remaindistnace2)
        {
            NavMeshHit hit;

            if (NavMesh.SamplePosition(RandomPositionSetting(), out hit, navMeshBounds.size.magnitude, 1))
            {
                agent.speed = walkSpeed;
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

    // 회전에 걸리는 시간 (초)
    private float rotationDuration = 7f;
    // 회전 시작 시간을 저장하는 변수
    private float rotationStartTime;

    // 플레이어를 향해 AI가 회전하도록 하는 로직을 처리하는 메서드
    void UpdateRotate()
    {
        // 플레이어의 현재 위치를 저장
        player_p = player.transform.position;
        // 처음 플레이어의 위치를 저장 (공격 시 이 위치로 이동하기 위함)
        initialTargetPosition = player_p;

        // 플레이어와 AI 간의 방향 벡터 계산 (y 축 차이를 무시함)
        dir = player_p - transform.position;
        dir.y = 0.08f;

        // 위에서 계산한 방향 벡터를 기반으로 회전할 목표 방향을 생성
        lookRotation = Quaternion.LookRotation(dir);

        // 회전 시작 시간을 기록하고 회전 상태로 전환
        if (rotationStartTime == 0)
        {
            rotationStartTime = Time.time;
            ChangState(EEnemyState.Rotate_);
        }

        // 회전이 시작된 후 경과된 시간 계산
        float elapsedTime = Time.time - rotationStartTime;
        // 회전 진행도를 0에서 1 사이의 값으로 클램프
        float t = Mathf.Clamp01(elapsedTime / rotationDuration);

        // 현재 AI의 회전을 목표 방향으로 Slerp(구면 선형 보간)하여 회전
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, t);

        // 현재 회전각과 목표 회전각 간의 차이를 계산
        float angle = Quaternion.Angle(transform.rotation, lookRotation);

        // 만약 회전 각도가 거의 맞거나 회전 시간이 완료된 경우
        if (angle < 0.1f || t >= 1)
        {
            // 회전 시작 시간을 초기화하여 다음 회전 때 다시 초기화되도록 함
            rotationStartTime = 0;

            // 플레이어와의 거리를 기반으로 다음 상태 결정
            if ((radius / 2) <= dir.magnitude)
            {
                // 플레이어가 충분히 멀리 있으면 추격 상태로 전환
                ChangState(EEnemyState.Chase_);
            }
            else
            {
                // 플레이어가 가까이 있으면 공격 상태로 전환
                ChangState(EEnemyState.Attack);
            }
        }
    }




    private void UpdateChase_Check()
    {
        // 플레이어의 현재 위치로 AI를 이동시킴
        agent.SetDestination(player_p);

        // AI가 플레이어에게 충분히 가까워졌는지 확인
        if ((radius / 1.5) > agent.remainingDistance)
        {
            // 플레이어가 AI의 공격 범위 내에 있으면 공격 상태로 전환
            ChangState(EEnemyState.Attack);
        }
        // AI가 목적지(플레이어의 마지막 알려진 위치)에 도달했는지 확인
        else if (agent.remainingDistance < 0.1f)
        {
            // 플레이어가 현재 인식 범위에 있는지 확인
            if (!isPlayerDetected)
            {
                // 목적지에 도달했지만 플레이어가 없다면, 추적을 중단하고 평상시 상태로 돌아감
                isChasing = false;
                animator.SetBool("WalkClam", true);
                ChangState(EEnemyState.WalkClam);
            }
            else
            {
                // 플레이어가 아직 인식 범위 내에 있다면 다시 추적
                UpdateChase_Check();
            }
        }
        // 위의 조건들에 해당하지 않으면 계속 추적 상태를 유지
    }

    private float attackDuration = 1f;
    private float attackStartTime;
    private float postAttackWaitDuration = 0.5f;
    private float postAttackWaitStartTime;

    void Attack()
    {
        // 공격 시작 시간 초기화
        if (attackStartTime == 0)
        {
            attackStartTime = Time.time;
            agent.SetDestination(initialTargetPosition);
        }

        // AI가 플레이어와 충분히 가까워졌는지 확인
        if (agent.remainingDistance < 0.5f)
        {
            // 플레이어 체력을 감소시키는 로직 추가
            if (play_health != null && play_health.curHp > 0)
            {
                play_health.UpdateHp(10f);  // 공격할 때마다 플레이어 체력을 10만큼 감소시킴

                // 플레이어 체력이 0 이하가 되면 처리할 로직
                if (play_health.curHp <= 0)
                {
                    play_health.Die();  // Die 메서드를 호출하여 플레이어의 사망 로직을 처리
                                        // 필요한 경우 AI도 상태를 변경하거나 멈추게 할 수 있음
                    ChangState(EEnemyState.WalkClam);  // 예시: 다시 돌아다니게 함
                }
            }

            // 공격 후 대기 로직
            if (postAttackWaitStartTime == 0)
            {
                postAttackWaitStartTime = Time.time;
                isAttacking = false;
                animator.SetBool("Attack_", false);
                agent.isStopped = true;
            }

            float elapsedTime = Time.time - postAttackWaitStartTime;

            if (elapsedTime >= postAttackWaitDuration)
            {
                postAttackWaitStartTime = 0;
                attackStartTime = 0;

                if (isPlayerDetected)
                {
                    ChangState(EEnemyState.Rotate_);
                }
                else
                {
                    ChangState(EEnemyState.WalkClam);
                }
                agent.isStopped = false;
            }
        }
        else
        {
            agent.SetDestination(initialTargetPosition);
        }
    }


    // Sound_Check의 로직을 메서드로 분리
    void CheckPlayerSound()
    {
        if (playerAudioSource != null && playerAudioSource.isPlaying)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            float volume_distance = playerAudioSource.maxDistance;

            if (distance <= volume_distance && playerAudioSource.volume > soundThreshold)
            {
                isPlayerDetected = true;
            }
            else
            {
                isPlayerDetected = false;
            }
        }
        else
        {
            isPlayerDetected = false;
        }
    }

    // IsPlayerDetected 메서드는 그대로 유지
    public bool IsPlayerDetected()
    {
        return isPlayerDetected;
    }
}