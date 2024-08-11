using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TraceTest : MonoBehaviour
{
    public Transform player;
    public LayerMask layermask;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        RaycastHit hit;
        if (Physics.Raycast(player.position, -directionToPlayer, out hit, 10f, layermask))
        {
            if (hit.collider.gameObject == player.gameObject)
            {
                Debug.Log(1);
            }
        }
    }
}
