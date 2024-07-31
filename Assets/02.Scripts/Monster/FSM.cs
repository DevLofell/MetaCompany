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

    // 플레이어 방향으로 회전하는 전역 변수
    Quaternion lookRotation;

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

        // NavMeshSurface 경계를 가져온다.
        if (navMeshSurface != null)
        {
            navMeshBounds = navMeshSurface.navMeshData.sourceBounds; 
            // sourceBounds가 정육면체?를 나타내는 거?? 맞나??
        }

        NavMeshHit hit;

        if (NavMesh.SamplePosition(RandomPositionSetting(), out hit, navMeshBounds.size.magnitude, 1))
        {
            agent.SetDestination(hit.position);
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

            case EEnemyState.Chase_: // 계속 업데이트를 하고 처음 플레이어 봤던 곳을 돌진해야 하니까 
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
        if(agent.remainingDistance < 5f)
        {
            NavMeshHit hit;

            if (NavMesh.SamplePosition(RandomPositionSetting(), out hit, navMeshBounds.size.magnitude, 1))
            {
                // 플레이어 방향 찾기
                dir = player.transform.position - transform.position;

                // 플레이어 방향으로 회전
                lookRotation = Quaternion.LookRotation(dir); // 정확히 말하면 적이 플레이어를 향하기 위한 필요한 회전을 계산

                // 부드럽게 회전, Lerp를 사용해서 회전한다
                // transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 20f * Time.deltaTime);

                // 근데 둘이 값이 같지 않나? 질문해야겠다...

                if (Quaternion.Angle(transform.rotation, lookRotation) < 5) // 각도를 중요시 여겨야 한다.

                // if문의 뜻
                // =>  Quaternion.Angle은 두 개의 쿼터니언 사이의 각도를 계산하는 유니티 내장 함수이다.
                // Quaternion.Angle(transform.rotation, lookRotaion)은 현재 객체의 회전과 목표 회전의 각도 차이를 반환한다.
                // <5도 미만인지 확인

                // 이 조건을 사용하여 객체가 거의 목표 방향을 향하고 있는지 여부를 확인한다.
                // 예를 들어, 적이 플레이어를 향하기 위해 회전하고 있을 때 목표 회전(lookRotation)과 현재 회전(transform.rotation) 사이의 각도 차이가 5도 이하가 되면 다음 상태(추적 상태)로 전환하게 됩니다.

                // 빠른 상태 전환:
                // 각도 체크 값을 증가시키면 현재 회전과 목표 회전 사이의 허용 오차가 커집니다.
                // 이는 캐릭터가 완벽하게 목표 방향을 향하지 않아도 더 빨리 다음 상태(Chase_ 상태)로 전환할 수 있게 합니다.
                {
                    animator.SetBool("WalkClam", false);
                    animator.SetBool("Chase_", true);
                    ChangState(EEnemyState.Chase_);
                }
                else
                {
                    agent.SetDestination(hit.position); // 그러면 여기서  hit.position이 navMeshSurface 공간이 아닐 수도 있다는 건데
                                                        // navMeshAgent 때문에 자체 알아서 조종이 되는 건가?   
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
        lookRotation = Quaternion.LookRotation(dir); // 정확히 말하면 적이 플레이어를 향하기 위한 필요한 회전을 계산

        // 부드럽게 회전, Lerp를 사용해서 회전한다
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 20f * Time.deltaTime);

        // 근데 둘이 값이 같지 않나? 질문해야겠다...

        if (Quaternion.Angle(transform.rotation, lookRotation) < 5) // 각도를 중요시 여겨야 한다.

        // if문의 뜻
        // =>  Quaternion.Angle은 두 개의 쿼터니언 사이의 각도를 계산하는 유니티 내장 함수이다.
        // Quaternion.Angle(transform.rotation, lookRotaion)은 현재 객체의 회전과 목표 회전의 각도 차이를 반환한다.
        // <5도 미만인지 확인

        // 이 조건을 사용하여 객체가 거의 목표 방향을 향하고 있는지 여부를 확인한다.
        // 예를 들어, 적이 플레이어를 향하기 위해 회전하고 있을 때 목표 회전(lookRotation)과 현재 회전(transform.rotation) 사이의 각도 차이가 5도 이하가 되면 다음 상태(추적 상태)로 전환하게 됩니다.

        // 빠른 상태 전환:
        // 각도 체크 값을 증가시키면 현재 회전과 목표 회전 사이의 허용 오차가 커집니다.
        // 이는 캐릭터가 완벽하게 목표 방향을 향하지 않아도 더 빨리 다음 상태(Chase_ 상태)로 전환할 수 있게 합니다.
        {
            animator.SetBool("Rotate_", false);
            animator.SetBool("Chase_", true);
            ChangState(EEnemyState.Chase_);
        }
    }

    void UpdateChase_Check()
    {
        agent.SetDestination(player.transform.position);
        // 플레이어 방향 찾기
        Vector3 pos = player.transform.position;
        pos.y = transform.position.y;
        dir = pos - transform.position;
        if (agent.remainingDistance < 50)
        {
            if(dir.magnitude < 40)
            {
                ChangState(EEnemyState.Attack);
                animator.SetBool("Attack_", true);
                animator.SetBool("Chase_", false);
            }
            else if(dir.magnitude > 3)
            {
                /*
                ChangState(EEnemyState.Rotate_);
                animator.SetBool("Chase_", false);
                animator.SetBool("Rotate_", true);
                */

                /*
                ChangState(EEnemyState.WalkClam);
                animator.SetBool("Chase_", false);
                animator.SetBool("WalkClam", true);
                */

            }
            /*
            else
            {
            }
            */
        }
    }

    void Attack()
    {

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

    /*
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.Contains("player"))
        {
            agent.isStopped = false;
            animator.SetBool("WalkClam", true);
            animator.SetBool("Rotate_", false);
            ChangState(EEnemyState.WalkClam);
        }
    }
    */
}