using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 0.7f;
    public bool isMoving;
    private Vector2 input;
    public float frequency;
   
   
    private Animator animator;
   
    public LayerMask solidObjects;

    void Start()
    {
        animator = GetComponent<Animator>();
        var targetPos = transform.position+new Vector3(3,0,0);
      
        StartCoroutine(MoveRight(targetPos, 1, 1));
    }

    // Update is called once per frame
    void Update()
    {
        if (!isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
            

              
                
              //  targetPos.x += 1;
              //  targetPos.y += 1;
               // if (IsWalkable(targetPos))
               // {
                    

                    
              //  }
            
        }


        animator.SetBool("isMoving", isMoving);
      
    }
    IEnumerator MoveLeft(Vector3 targetPos, float inputx, float inputy)
    {
        

        animator.SetFloat("moveX", -1);
        animator.SetFloat("moveY", 0);
        isMoving = true;
      
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            if (IsWalkable(Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime))){
                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                isMoving = true;
            }
            
            yield return null;  
        }
        transform.position = targetPos;
        isMoving = false;
        yield return new WaitForSeconds(Random.Range(frequency /2, frequency * 2));
        StartCoroutine(MoveRight(transform.position+new Vector3(3,0,0), 1, 1));
    }

    IEnumerator MoveRight(Vector3 targetPos, float inputx, float inputy)
    {     
        var startingposition = transform.position;
        animator.SetFloat("moveX", 1);
        animator.SetFloat("moveY", 0);
        isMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            if (IsWalkable(Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime)))
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                isMoving = true;
            }

            yield return null;
        }
        transform.position = targetPos;
        isMoving = false;
        yield return new WaitForSeconds(Random.Range(frequency-1,frequency*2));
        StartCoroutine(MoveLeft(startingposition, 1, 1));
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos - new Vector3(0, 0.25f, 0), 0.2f, solidObjects) != null)
        {
            isMoving = false;
            return false;
          
        }

        else
        {
            isMoving = true;
            return true;
        }
    }
}
