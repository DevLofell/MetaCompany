using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Sound_Check : MonoBehaviour
{

    // 플레이어 설정
    public Transform player;

    // 플레이어 오디오 소스
    AudioSource playerAudioSource;

    // 소리 감지 임계값
    float soundeThreshold = 0.1f;

    

    // Enemy 소리 체크 여부
    bool isPlayerDetected;
    



    void Start()
    {
        playerAudioSource = player.GetComponent<AudioSource>();
    }

  
    void Update()
    {
        if (playerAudioSource.isPlaying)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            float volumde_distance = playerAudioSource.maxDistance;

            // 플레이어가 오디오 소스의 최대 거리 내에 있는지 확인 
            if(distance <= volumde_distance  && playerAudioSource.volume > soundeThreshold)
            {
                isPlayerDetected = true;
            }
            else
            {
                isPlayerDetected = false;
            }
        }

        
    }

    public bool IsPlayerDetected() 
    {
        return isPlayerDetected;
    }

}
