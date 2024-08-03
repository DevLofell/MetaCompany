using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.AI.Navigation;
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

    void Start()
    {
        // 현재 게임 오브젝트에서 Animator 컴포넌트를 찾는다.
        animator = GetComponentInChildren<Animator>();

        // 현재 게임 오브젝트에서 NavMeshAgent 컴포넌트를 찾는다.
        agent = GetComponent<NavMeshAgent>();

        // NavMeshSurface 바운딩 박스를 설정한다.
        NavMeshSurface navMeshSurface = FindObjectOfType<NavMeshSurface>();

        // 플레이어 찾기
        player = GameObject.Find("Capsule");

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

        // 애니메이터 작동하기
        animator.SetBool("WalkClam", true);


        NavMeshHit hit;

        // 일단 start 부분에서 눈 없는 개 무작정 돌아다니게 하기
        if (NavMesh.SamplePosition(RandomPositionSetting(), out hit, navMeshBounds.size.magnitude, 1))
        {
            agent.SetDestination(hit.position);
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
        Debug.Log(currentState + ">>" +state.ToString());
        currentState = state;

        switch (currentState)
        {
            case EEnemyState.Rotate_:
                animator.SetBool("WalkClam", false);
                animator.SetBool("Rotate_", true);
                agent.isStopped = true; // 회전 중에는 정지
                break;

            case EEnemyState.Chase_:
                animator.SetBool("Rotate_", false);
                animator.SetBool("Chase_", true);
                agent.isStopped = false; // 추적 시작
                break;

            case EEnemyState.Attack:
                animator.SetBool("Chase_", false);
                animator.SetBool("Attack_", true);
                agent.isStopped = true; // 공격 중 정지
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
                agent.SetDestination(hit.position);
            }
        }
    }

    Vector3 RandomPositionSetting()
    {
        Vector3 randomPosition = new Vector3
        (
            Random.Range(navMeshBounds.min.x, navMeshBounds.max.x),
            Random.Range(navMeshBounds.min.y, navMeshBounds.max.y),
            Random.Range(navMeshBounds.min.z, navMeshBounds.max.z)
        );

        return randomPosition;
    }

    void UpdateRotate()
    {
        // Ensure we have calculated the lookRotation once at the start
        if (Quaternion.Angle(transform.rotation, lookRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 50);
        }

        float angle = Quaternion.Angle(transform.rotation, lookRotation);
        //print("Current angle: " + angle);  // 현재 각도 출력

        // If the angle is small enough, consider the rotation complete
        if (angle < 0.1f)
        {
            print("회전 완료!");  // 회전 완료 시 출력
            if ((radius / 2) <= dir.magnitude)
            {
                print("Chase 상태로 전환");  // Chase 상태로 전환 시 출력
                ChangState(EEnemyState.Chase_);
            }
            else
            {
                print("Attack 상태로 전환");  // Attack 상태로 전환 시 출력
                ChangState(EEnemyState.Attack);
            }
        }


        /*

        // 여기서 player 방향을 콕 찍어서 알기
        player_p = player.transform.position;

        // 플레이어 방향 찾기
        dir = player.transform.position - transform.position;

        // 플레이어 방향까지 구하기
        lookRotation = Quaternion.LookRotation(dir);

        print(Quaternion.Angle(transform.rotation, lookRotation));

        // 실시간으로 플레이어를 체크 하지 말고 딱 처음 봤을 때만 하기

        // 정면에서 봤을 경우
        if (Quaternion.Angle(transform.rotation, lookRotation) > 10)
        {
            // 만약에 콜라이더 안에서도 일정 거리가 있을 경우 추격
            if ((radius / 2) < dir.magnitude)
            {
                animator.SetBool("Chase_", true);
                ChangState(EEnemyState.Chase_);
            }
            else // 콜라이더 안에서도 가까울 경우 바로 공격
            {
                
                animator.SetBool("Attack", true);
                ChangState(EEnemyState.Attack);
            }
        }*/
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

        if ((radius / 3) > agent.remainingDistance)
        {
            ChangState(EEnemyState.Attack);
        }
    }

    void Attack()
    {

        animator.SetBool("Attack_", false);
        agent.isStopped = false;

        // 일단 start 부분에서 눈 없는 개 무작정 돌아다니게 하기

        animator.SetBool("WalkClam", true);
        ChangState(EEnemyState.WalkClam);

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("Capsule"))
        {
            // 여기서 player 방향을 콕 찍어서 알기
            player_p = player.transform.position;

            // 플레이어 방향 찾기
            dir = player_p - transform.position;

            dir.y = 0;

            // 플레이어 방향까지 구하기
            lookRotation = Quaternion.LookRotation(dir);

            // 측면에서 봤을 경우
            if (Quaternion.Angle(transform.rotation, lookRotation) >= 10)
            {
                ChangState(EEnemyState.Rotate_);
            }
            // 정면에서 봤을 경우 
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
    }
}