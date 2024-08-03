using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;


public class FSM_copy_sound_check_X : MonoBehaviour
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

    PlayerMove play_health;

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
            // 플레이어 방향 찾기
            dir = player.transform.position - transform.position;

            // 플레이어 방향으로 회전
            lookRotation = Quaternion.LookRotation(dir);

            if (Quaternion.Angle(transform.rotation, lookRotation) < 5)

            {
                animator.SetBool("WalkClam", false);
                animator.SetBool("Chase_", true);
                ChangState(EEnemyState.Chase_);
            }
            else
            {
                agent.SetDestination(hit.position);
            }
        }

        animator.SetBool("WalkClam", true);
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
                /*
            case EEnemyState.Attack:
                break;
                */
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
        // 현재 상태를 state 값으로 설정

    }

    void UpdateWalkClam()
    {
        if (agent.remainingDistance < 30f)
        {
            NavMeshHit hit;

            if (NavMesh.SamplePosition(RandomPositionSetting(), out hit, navMeshBounds.size.magnitude, 1))
            {
                // 플레이어 방향 찾기
                dir = player.transform.position - transform.position;

                // 플레이어 방향으로 회전
                lookRotation = Quaternion.LookRotation(dir);

                if (Quaternion.Angle(transform.rotation, lookRotation) < 10)

                {
                    animator.SetBool("WalkClam", false);
                    animator.SetBool("Chase_", true);
                    ChangState(EEnemyState.Chase_);
                }
                else
                {
                    agent.SetDestination(hit.position);
                }

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

        // 플레이어 방향 찾기
        dir = player.transform.position - transform.position;

        // 플레이어 방향으로 회전
        lookRotation = Quaternion.LookRotation(dir);

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 20f * Time.deltaTime);

        if (Quaternion.Angle(transform.rotation, lookRotation) < 10)
        {
            if (dir.magnitude > 200)
            {
                animator.SetBool("Rotate_", false);
                animator.SetBool("Chase_", true);

                // print(dir.magnitude + "회전 거리");
                ChangState(EEnemyState.Chase_);
            }
            else
            {
                animator.SetBool("Rotate_", false);
                animator.SetBool("Attack_", true);

                // print(dir.magnitude + "회전 거리");
                ChangState(EEnemyState.Attack);


            }

        }
    }

    private void UpdateChase_Check()
    {
        agent.SetDestination(player.transform.position);
        // 플레이어 방향 찾기
        Vector3 pos = player.transform.position;
        pos.y = transform.position.y;
        dir = pos - transform.position;
        print(dir.magnitude);
        //if (agent.remainingDistance < 50)
        {
            if (dir.magnitude < 200)
            {
                animator.SetBool("Chase_", false);
                animator.SetBool("Attack_", true);
                ChangState(EEnemyState.Attack);

            }
        }
    }

    void Attack()
    {
        // 플레이어 방향 찾기
        dir = player.transform.position - transform.position;

        if (dir.magnitude < 200)
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
            animator.SetBool("Rotate_", true);
            ChangState(EEnemyState.Rotate_);
        }
    }
}