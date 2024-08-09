using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class LookAtUpDown : MonoBehaviour
{
    public float minY = 1.0f;
    public float maxY = 1.66f;
    public float minX = -0.4f;
    public float maxX = 2.0f;
    public float sensitivity = 0.01f;
    [SerializeField]
    private float newY;
    bool originTransition = false;
    int cnt = 0;
    private void Update()
    {
        // 마우스의 Y축 움직임 감지
        float mouseY = Input.GetAxis("Mouse Y");
        // 현재 오브젝트의 로컬 위치 가져오기
        Vector3 currentPosition = transform.localPosition;
        // 새로운 Y 위치 계산
        newY = currentPosition.y + mouseY * sensitivity;
        //float newX = currentPosition.x + mouseY * sensitivity;
        //-0.4 ~ 2
        if (originTransition != InputManager.instance.inputCrouch)
        {
            cnt = 0;
        }
        if (InputManager.instance.inputCrouch == true && cnt == 0)
        {
            if (originTransition != InputManager.instance.inputCrouch)
            {
                newY = -2 + (newY * 3.2f);
            }
            cnt = 1;
            minY = -2.0f;
            maxY = 1.2f;

        }
        if (InputManager.instance.inputCrouch == false && cnt == 0)
        {
            if (originTransition != InputManager.instance.inputCrouch)
            {
                newY = 0.25f + (newY * 1.45f);
            }
            cnt = 1;
            minY = 0.25f;
            maxY = 1.7f;

        }
        originTransition = InputManager.instance.inputCrouch;
        // Y 위치를 제한값 내로 고정
        newY = Mathf.Clamp(newY, minY, maxY);
        //newX = Mathf.Clamp(newX, minX, maxX);
        // 새로운 위치 적용
        transform.localPosition = new Vector3(currentPosition.x, newY, currentPosition.z);
        //transform.localPosition = new Vector3(newX, currentPosition.y, currentPosition.z);
    }
}