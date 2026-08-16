using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public GameObject player;
    public GameObject player1;
    public GameObject player2;

    // Start is called before the first frame update
    void Start()
    {
       
        GetComponent<SpriteRenderer>().sortingOrder = (int)(-100 * transform.position.y);
    }

    // Update is called once per frame
    void Update()
    {
        
    
    }
}
