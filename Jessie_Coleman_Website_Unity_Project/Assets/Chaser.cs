using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chaser : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 0.05f;
    public bool isMoving;
    public Vector2 input;
    public GameObject player;


    private Animator animator;

    public LayerMask solidObjects;

    void Start()
    {
        animator = GetComponent<Animator>();
        //var targetPos = transform.position + new Vector3(3, 0, 0);

        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving == false)
        {
            Vector3 playerVector = new Vector3(player.transform.position.x, player.transform.position.y, 1);
            var targetPos = transform.position;
            if (playerVector.x - this.transform.position.x > 0.2 ) { input.x = 1f; targetPos.x += 1 / 32f; }
            else if (playerVector.x - this.transform.position.x < -0.2 ) { input.x = -1f; targetPos.x -= 1 / 32f; }
            else { input.x = 0; }

            if (playerVector.y - this.transform.position.y > 0.2 ) { input.y = 1; targetPos.y += 1 / 32f; }
            else if (playerVector.y - this.transform.position.y < -0.2 ) { input.y = -1; targetPos.y -= 1 / 32f; }
            else { input.y = 0; }



            animator.SetFloat("moveX", input.x);
            animator.SetFloat("moveY", input.y);

            //Vector3 playerVector = new Vector3(player.transform.position.x, player.transform.position.y, 1);
            if (IsWalkable(targetPos))
            {

                StartCoroutine(Chase(targetPos, 1, 1));
            }
        }

    


    animator.SetBool("isMoving", isMoving);

    }

    IEnumerator Chase(Vector3 targetPos, float inputx, float inputy)
    {
       
        isMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
        isMoving = false;
    }
    private bool IsWalkable(Vector3 targetPos)
    {
        GetComponent<SpriteRenderer>().sortingOrder = (int)(-100 * transform.position.y);
        if (Physics2D.OverlapCircle(targetPos - new Vector3(0, 0, 0), 0.1f, solidObjects) != null)
        {
            return false;
        }

        else
        {
            return true;
        }
    }

}