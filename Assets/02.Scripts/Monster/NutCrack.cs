using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using UnityEngine.XR;

public class NutCrack : MonoBehaviour
{
    public enum EEnemyState
    {
        Patroll,
        Rotate,
        Chase,
        ShootAttack,
        Loading,
        Die,
        Drop
    }

    // 현재 상태
    EEnemyState currentState;

    // Animator  컴포넌트를 저장할 전역 변수
    Animator animator;

    // NavMeshAgent 컴포넌트를 저장할 전역 변수
    NavMeshAgent agent;

    // NavMeshSurface 경계를 저장할 전역 번수
    Bounds navMeshBounds;

    // player 찾기
    GameObject player;

    void Start()
    {
        // 현재 게임 오브젝트에서 Animator 컴포넌트를 찾는다.
        animator = GetComponentInChildren<Animator>();

        // 현재 게임 오브젝트에서 NavMeshAgent 컴포넌트를 찾는다.
        agent = GetComponent<NavMeshAgent>();

        // Player라는 오브젝트 찾기
        player = GameObject.Find("Player");

        // NavMeshSurface 바운딩 박스를 설정한다.
        NavMeshSurface navMeshSurface = FindObjectOfType<NavMeshSurface>();

        // NavMeshSurface 경계를 가져온다.
        if (navMeshSurface != null)
        {
            navMeshBounds = navMeshSurface.navMeshData.sourceBounds;
        }

        NavMeshHit hit;

        if (NavMesh.SamplePosition(RandomPositionSetting(), out hit, navMeshBounds.size.magnitude, 1))
        {
            agent.SetDestination(hit.position);
        }

        else
        {
            agent.enabled = false; // SamplePosition 실패 시 NavMeshAgent 비활성화
            // Debug.LogError("NavMesh에 에이전트를 배치할 수 없습니다.");
        }

        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true; // 콜라이더를 트리거로 설정
        }
    }

    void Update()
    {

        switch (currentState)
        {
            case EEnemyState.Patroll:
                Patroll();
                break;
            
            case EEnemyState.Chase:
                Chase();
                break;

            case EEnemyState.Loading:
                Loading();
                break;
        }
    }

    // 장애물 감지 및 상태 변경 함수

    void ChangState(EEnemyState state)
    {
        currentState = state;

        switch (currentState)
        {
            case EEnemyState.Patroll:
                agent.enabled = true;
                animator.SetBool("Patroll", true);
                animator.SetBool("Attack", false);
                animator.SetBool("Rotate", false);
                animator.SetBool("Chase", false);

                NavMeshHit hit;

                // 이전 방향과 90도 이상 다른 방향으로 목적지 설정
                Vector3 previousDirection = transform.forward;
                Vector3 newDirection;
                do
                {
                    Vector3 randomPoint = RandomPositionSetting();
                    newDirection = (randomPoint - transform.position).normalized;
                } while (Vector3.Angle(previousDirection, newDirection) < 90.0f);

                Vector3 finalDestination = transform.position + newDirection * 10.0f; // 이동할 거리를 곱해서 목적지 설정

                if (NavMesh.SamplePosition(finalDestination, out hit, navMeshBounds.size.magnitude, 1))
                {
                    agent.SetDestination(hit.position);
                }
                break;

            case EEnemyState.Rotate:
                agent.enabled = false;
                animator.SetBool("Rotate", true);
                animator.SetBool("Patroll", false);

                StartCoroutine(Rotate()); // 이걸 해야 코루틴을 실행 할 수 있다.
                break;

            case EEnemyState.Chase:
                agent.enabled = true;
                animator.SetBool("Chase", true);
                animator.SetBool("loading", false);
                animator.SetBool("Attack", false);
                animator.SetBool("Rotate", false);

                Chase();
                break;

            case EEnemyState.ShootAttack:
                animator.SetBool("Attack", true);
                animator.SetBool("loading", false);
                animator.SetBool("Rotate", false);
                animator.SetBool("Chase", false);

                StartCoroutine(ShootAttack());
                break;


            case EEnemyState.Loading:
                animator.SetBool("loading", true);
                animator.SetBool("Attack", false);

                // Loading();
                break;
        }
    }


    Vector3 RandomPositionSetting()
    {
        Vector3 randomPosition = new Vector3
        (
            Random.Range(-5.53f, 18.46f),  // x 범위
            0.04f,                        // y 고정
            Random.Range(33.05f, 48.63f)  // z 범위
        );
        return randomPosition;
    }

    void Patroll()
    {
        // 목적지에 도착했는지 확인
        if (agent.remainingDistance <= 0.5f)
        {
            ChangState(EEnemyState.Rotate);
            return; // 상태를 변경했으므로 함수 종료
        }
        else
        {
            // 현재 목적지의 방향을 계산
            Vector3 directionToDestination = (agent.destination - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToDestination);

            // 회전이 완료되었는지 확인
            if (Quaternion.Angle(transform.rotation, targetRotation) > 20.0f)
            {
                // 아직 회전 중이라면 회전 먼저 수행
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5.0f);
                agent.isStopped = true; // 회전 중에는 이동하지 않도록 정지
            }
            else
            {
                // 회전이 완료되었으면 이동 시작
                agent.isStopped = false;
            }

        }
    }


    IEnumerator Rotate()
    {
        LayerMask obstacleLayerMask = LayerMask.GetMask("Default"); // 벽이나 장애물에 사용되는 레이어
        LayerMask playerLayerMask = LayerMask.GetMask("Player");
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, transform.forward);

        RaycastHit hitinfo;

        for (int i = 0; i < 4; i++)
        {
            if (Physics.Raycast(ray, out hitinfo, 0.5f, playerLayerMask))
            {
                // 플레이어를 감지한 후, 플레이어가 장애물 뒤에 있는지 확인
                if (!Physics.Raycast(transform.position + Vector3.up, (hitinfo.point - transform.position).normalized, out RaycastHit obstacleHit, hitinfo.distance, obstacleLayerMask))
                {
                    Debug.Log("Player 감지 및 장애물 없음");

                    ChangState(EEnemyState.ShootAttack);
                    
                    yield break;
                }
                else
                {
                    Debug.Log("Player가 장애물 뒤에 있습니다.");
                }
            }

            Quaternion targetRotation = Quaternion.Euler(0, -90, 0) * transform.rotation; // 앞으로 돌 포지션

            while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5.0f);

                ray = new Ray(transform.position + Vector3.up, transform.forward);
                if (Physics.Raycast(ray, out hitinfo, float.MaxValue, playerLayerMask))
                {
                    // 플레이어를 감지한 후, 장애물 확인
                    if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, (hitinfo.point - transform.position).normalized, out RaycastHit obstacleHit, hitinfo.distance, obstacleLayerMask))
                    {
                        Debug.Log("Player 감지 및 장애물 없음");

                        ChangState(EEnemyState.ShootAttack);
                        
                        yield break;
                    }
                    else
                    {
                        Debug.Log("Player가 장애물 뒤에 있습니다.");
                    }
                }

                yield return null;
            }

            transform.rotation = targetRotation;
            ray.direction = transform.forward;
        }

        ChangState(EEnemyState.Patroll);
    }



    // 플레이어를 일정 시간 동안 발견하지 못했을 때 상태 전환을 위한 타이머 변수
    float lostPlayerTime = 0.0f;
    float lostPlayerThreshold = 5.0f; // 플레이어를 일정 시간 동안 발견하지 못했을 때 상태 전환 시간

    void Chase()
    {
        // 시야각 설정
        float fieldOfView = 60.0f; // 시야각 (각도)
        float viewDistance = 10.0f; // 시야 거리
        float attackDistance = 2.0f; // 공격 시작 거리
        float stopDistance = 2.0f; // 추적을 멈추는 거리

        Vector3 directionToPlayer = player.transform.position - transform.position;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        float distanceToPlayer = directionToPlayer.magnitude;

        if (angleToPlayer < fieldOfView * 0.5f && distanceToPlayer < viewDistance)
        {
            // 플레이어가 시야각과 시야 거리 안에 있는 경우
            if (distanceToPlayer > stopDistance)
            {
                // 플레이어와의 거리가 stopDistance보다 크면 계속 추적
                agent.isStopped = false;
                agent.SetDestination(player.transform.position);
            }
            else
            {
                // stopDistance 이내로 접근하면 정지
                agent.isStopped = true;
            }

            // attackDistance 이내에 들어오면 공격 상태로 전환
            if (distanceToPlayer <= attackDistance)
            {
                ChangState(EEnemyState.ShootAttack);
            }
        }
        else
        {
            // 플레이어가 시야각 밖에 있는 경우 Patroll 상태로 전환
            ChangState(EEnemyState.Patroll);
        }
    }


    public GameObject shootParticle; // 발사 파티클 프리팹
    float shootDamege = 10.0f; // 발사 데미지
    float shootInterval = 2.0f; // 발사 간격
    public int shotsPerAttac = 2; // 한 번에 발사할 횟수


    // float lastShootTime = 0.0f;

    // PlayerHealth playerHealth;

    float reloadTime = 2.0f;
    float reloadStartTime;
    IEnumerator ShootAttack()
    {

        // 파티클 생성, 플레이어 체력 감소, 총알 감소
        // GameObject particle = Instantiate(shootParticle, transform.position + transform.forward, transform.rotation);
        // Destroy(particle, 0.1f); // 0.1초 후 파티클 제거


        // 총알이 소모되었으면 Loading 상태로 변경
        if (shotsPerAttac <= 0)
        {
            reloadStartTime = Time.time;
            // Loading 상태로 전환
            ChangState(EEnemyState.Loading);
            yield break;
        }

        player.GetComponent<PlayerMove>().TakeDamage(shootDamege);
        shotsPerAttac--;

        yield return new WaitForSeconds(1.2f); // "1.2초 동안 잠시 멈춘 후에 다음 동작을 수행해라
        // update는 계속 수행

        // 현재 플레이어와의 거리 계산
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // 거리에 따른 애니메이션 재생

        
        if (distanceToPlayer <= 1.0f)
        {
            // 플레이어가 가까우면 뒤로 걸어가는 애니메이션 재생
            // ChangState(EEnemyState.); // 상태를 back으로 변경
            // yield break;// return StartCoroutine(back()); // back 코루틴이 끝날 때까지 기다린다.
            ChangState(EEnemyState.ShootAttack);
        }
        else
        {
            // 플레이어가 멀면 앞으로 걸어가는 애니메이션 재생
            ChangState(EEnemyState.Chase); // 상태를 go으로 변경
            yield break;// StartCoroutine(go()); // go 코루틴이 끝날 때까지 기다린다.
        }
        
    }

    void Loading()
    {
        // 재장전 시간이 지났는지 확인
        if (Time.time - reloadStartTime >= reloadTime)
        {
            // 재장전 완료
            shotsPerAttac = 2; // 원래 탄약 수로 복구

            // 플레이어와의 거리 계산
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            // 거리에 따른 상태 전환
            if (distanceToPlayer <= 1.0f)
            {
                // 플레이어가 가까우면 ShootAttack 상태로 전환
                ChangState(EEnemyState.ShootAttack);
            }
            else
            {
                // 플레이어가 멀면 Chase 상태로 전환
                ChangState(EEnemyState.Chase);
            }
        }
    }


    void OnTriggerEnter(Collider other)
    {
        // Obstacle 레이어와 충돌했는지 확인
        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            // 현재 위치와 충돌 지점의 방향을 계산하여 새로운 목적지 설정
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Vector3 directionAroundObstacle = (transform.position - hitPoint).normalized;

            // 장애물의 측면으로 우회하기 위해 새로운 목적지 설정
            Vector3 newDestination = transform.position + directionAroundObstacle * 2.0f;
            agent.SetDestination(newDestination);

            Debug.Log("Obstacle encountered! Changing destination.");
        }
    }
}

