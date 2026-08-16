using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed;
    public bool isMoving;
    private Vector2 input;
    public Queue<Vector3> FollowPositions1;
    public Queue<float> FollowAnimations1;
    public Queue<float> FollowAnimations2;//follows the directional booleans from the first player
    public Queue<Vector3> FollowPositions2;
    public Vector3 CurrentFollowPosition;
    public GameObject follower1;
    public GameObject follower2;
    private Animator animator;
    public Animator follower1animations; //the actual animations for follower1
    public Animator follower2animations;
    public LayerMask solidObjects;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        if (input != Vector2.zero)
        {
            animator.SetBool("isMoving", true);
            animator.SetFloat("moveX", input.x);
            animator.SetFloat("moveY", input.y);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
    }
}
