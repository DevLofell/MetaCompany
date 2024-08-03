using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyToPlayer : MonoBehaviour
{
    public Transform player;
    public float wanderRange = 100f;    // 적이 돌아다닐 범위
    NavMeshAgent agent;
    Sound_Check sound_check;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        sound_check = GetComponent<Sound_Check>();
    }

    
    void Update()
    {

        if (sound_check.IsPlayerDetected())
        {
            agent.SetDestination(player.position);
        }
        else
        {
            // 플레이어 소리를 듣지 못했을 때, 랜덤한 위치로 이동
            if(!agent.pathPending && agent.remainingDistance < 0.5f)

            // 1. !agent.pathPending
            // 이 조건은 현재 'NavMeshAgent'가 새로운 목적지로 이동하기 위한 경로를 계산 중이지 않다는 것을 확인 즉, 경로 계산이 완료된 상태에서만 조건이 참이 된다.

            // 만약에 이 값이 true면 경로를 아직 계산중이고, false면 계산이 완료된 상태이다.

            // !agent.pathPending는 경로 계산을 진행하지 않고, 경로 계산이 완료된 상태이다.


            // 2. agent.remainingDistance < 0.5f
            // 이 조건은 NavMeshAgent가 현재 목표 위치까지의 남은 거리가 0.5 유닛보다 작다는 것을 확인
            {

                Wander();
            }
        }

    }

    void Wander()
    {
        // NavMesh 상에서 이동한 랜덤 위치를 생성
        Vector3 randomDirection = Random.insideUnitSphere * wanderRange;
        randomDirection += transform.position;

        // Random.insideUnitSphere는 Vector3의 원점(0,0,0)을 중심으로 하는 단위 구 (반지름 1) 내부의 임의의 점을 반환한다.

        // 반환되는 벡터는 구의 표면뿐만 아니라 구의 내부의 임의의 점이 될 수 있습니다.

        // 이 벡터의 길이는 0부터 1까지 입니다.


        NavMeshHit hit; // 일단은 raycasthit 비슷하다.

        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRange, NavMesh.AllAreas))

        // NavMesh.SamplePosition 메서드는 주어진 위치(여기서는 randomDirection) 근처에서 NavMesh 상의 유효한 위치를 찾습니다.

        // 매개 변수들 설명

        // randomDirection: NavMesh 상에서 유효한 위치를 찾기 위해 샘플링할 원래의 위치 벡터입니다.

        // out hit: 유효한 NavMesh 위치가 발견되면 이 변수에 결과를 저장합니다. hit.position에는 NavMesh 상의 실제 위치가 담깁니다.

        // wanderRange: 샘플링할 범위(반경)입니다. 이 범위 내에서 유효한 NavMesh 위치를 찾습니다.

        // NavMesh.AllAreas: NavMesh 상의 모든 영역을 고려하여 샘플링을 수행합니다. 특정 영역만을 고려하려면 다른 값들을 사용할 수 있다.

        {
            agent.SetDestination(hit.position);
        }
    }
}
