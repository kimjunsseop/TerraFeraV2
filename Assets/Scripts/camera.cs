using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera : MonoBehaviour
{
    Vector3 offset;
    GameObject player;
    private Vector3 initialPosition;
    public float moveSpeed = 5.0f;
    void Start()
    {
        initialPosition = transform.position;
        player = GameObject.FindGameObjectWithTag("Player");
        offset = transform.position - player.transform.position;
    }

    private void LateUpdate() 
    {
        transform.position = Vector3.Lerp(transform.position, player.transform.position + offset, moveSpeed * Time.deltaTime);    
    }
    void Update()
    {
        
    }
}
