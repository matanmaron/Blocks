using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class ChunkLoadAnimation : MonoBehaviour
{
    float speed;
    Vector3 targetPos;
    float waitTimer;
    float timer;

    void Start()
    {
        speed = Random.Range(1f, 4f);
        waitTimer = Random.Range(0, 1f);
        targetPos = transform.position;
        transform.position = new Vector3(transform.position.x, -VoxelData.ChunkHeight, transform.position.z);
    }

    void Update()
    {
        if (timer < waitTimer)
        {
            timer += Time.deltaTime;
            return;
        }
        if ((targetPos.y - transform.position.y) < 1f)
        {
            speed++;
        }
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * speed);
        if ((targetPos.y - transform.position.y) < 0.05f)
        {
            transform.position = targetPos;
            Destroy(this);
        }
    }
}
