using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollower : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed;
    public bool isMoving;
    private Vector2 input;
    public Queue<Vector3> FollowPositions;
    public Vector3 CurrentFollowPosition;
    public GameObject follower;

    void Start()
    {
        FollowPositions = new Queue<Vector3>();

    }

    // Update is called once per frame
    void Update()
    {
        if (!isMoving)
        {
  
            if (input != Vector2.zero)
            {


                var targetPos = transform.position;
                targetPos.x += input.x / 32;
                targetPos.y += input.y / 32;
                StartCoroutine(Move(targetPos));
            }
        }



    }
    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;
        FollowPositions.Enqueue(transform.position);
        if (FollowPositions.Count > 8)
        {
            CurrentFollowPosition = FollowPositions.Dequeue();
            follower.transform.position = CurrentFollowPosition;
        }
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
        isMoving = false;
    }
}