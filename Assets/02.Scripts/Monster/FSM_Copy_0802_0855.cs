using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;


public class FSM_Copy_0802_0855 : MonoBehaviour
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

    // 플레이어 방향으로 회전하는 전역 변수
    Quaternion lookRotation;

    public PlayerMove play_health;

    // 콜라이더 컴포넌트
    // SphereCollider scollider;

    // 라디어스 값
    // public float radius;

    void Start()
    {
        // 현재 게임 오브젝트에서 Animator 컴포넌트를 찾는다.
        animator = GetComponentInChildren<Animator>();

        // 현재 게임 오브젝트에서 NavMeshAgent 컴포넌트를 찾는다.
        agent = GetComponent<NavMeshAgent>();

        // NavMeshSurface 바운딩 박스를 설정한다.
        NavMeshSurface navMeshSurface = FindObjectOfType<NavMeshSurface>();
        // 외부에서 갖고 온건 전역 변수 선언해서 start에서 초기화 하는 것 보다 Find 관련 함수를 통해서 갖고 온다.

        // 플레이어 찾기
        player = GameObject.Find("player");

        play_health = player.GetComponent<PlayerMove>();

        // NavMeshSurface 경계를 가져온다.
        if (navMeshSurface != null)
        {
            navMeshBounds = navMeshSurface.navMeshData.sourceBounds;
        }

        NavMeshHit hit;

        if (NavMesh.SamplePosition(RandomPositionSetting(), out hit, navMeshBounds.size.magnitude, 1))
        {
            animator.SetBool("WalkClam", true);
            agent.SetDestination(hit.position);

        }

        // scollider = GetComponent<SphereCollider>();
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
        }
    }

    void ChangState(EEnemyState state)
    {
        currentState = state;

        switch (currentState)
        {
            case EEnemyState.Chase_:
                agent.enabled = true;
                break;
            case EEnemyState.Attack:

                Attack();
                break;
        }
    }



    void UpdateWalkClam()
    {
        // 사실 여기도 절대적인 수치라 맵의 크기에 따라 설정을 다르게 해야 한다. 아래 수치보다 좀 더 좋은 코드가 있을 것 같은데;;
        // bake 영역을 실시간으로 바꿀까? 동적  bake
        if (agent.remainingDistance < (agent.remainingDistance) / 5)
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
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 20f * Time.deltaTime);
        // 근데 여기는 꼭 update에서 사용해야 하나?

        // 다 돌고 나면 추격 or 공격하기
        if (true) // 여기서 거리를 상대적인 아닌 절대적으로 추적해야 하는데;;
        {
            // 추격
            animator.SetBool("Rotate_", false);
            animator.SetBool("Chase_", true);
            ChangState(EEnemyState.Chase_);
        }
        else
        {
            // 공격
            animator.SetBool("Rotate_", false);
            animator.SetBool("Attack_", true);
            ChangState(EEnemyState.Attack);
        }
    }

    private void UpdateChase_Check()
    {
        agent.SetDestination(player.transform.position);
        // 플레이어 방향 찾기
        // Vector3 pos = player.transform.position;
        // pos.y = transform.position.y;
        // dir = pos - transform.position;

        if (true) // 절대적인 거리라 맵의 크기랑 오브젝트의 크기에 따라 달라진다.
        {
            animator.SetBool("Chase_", false);
            animator.SetBool("Attack_", true);
            ChangState(EEnemyState.Attack);
        }

    }

    void Attack()
    {
        // 플레이어 방향 찾기
        dir = player.transform.position - transform.position;

        if (true) // 절대적인 거리라 맵의 크기랑 오브젝트의 크기에 따라 달라진다.
                  // 이건 지금까지 절대적인 거리 값()
        {
            play_health.TakeDamage(play_health.health);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("player"))
        {
            agent.enabled = false;
            animator.SetBool("WalkClam", false);

            // 여기서 player 방향을 콕 찍어서 알기

            // 플레이어 방향 찾기
            dir = player.transform.position - transform.position;

            // 플레이어 방향까지 구하기
            lookRotation = Quaternion.LookRotation(dir);

            // 여기서 두 가지를 생각해야 한다. 하나는 거의 정면에서 마주쳤을 경우
            // 다른 하나는 측면에서 마주쳤을 경우


            if (Quaternion.Angle(transform.rotation, lookRotation) > 10)
            {
                // 측면에서 봤을 경우
                animator.SetBool("Rotate_", true);
                ChangState(EEnemyState.Rotate_);
            }
            else
            {
                // 거의 정면에서 봤을 경우
                if (true) // 여기서의 거리를 상대적인 수치가 아니라 "절대적인" 수치로 정해야 하는데 
                {
                    // 만약에 콜라이더 안에서도 일정 거리가 있을 경우 추격
                    animator.SetBool("Chase_", true);
                    ChangState(EEnemyState.Chase_);

                }
                else
                {
                    // 콜라이더 안에서도 가까울 경우 바로 공격
                    animator.SetBool("Attack", true);
                    ChangState(EEnemyState.Attack);
                }

            }
        }
    }
}