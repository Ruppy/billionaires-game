using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    private Rigidbody2D body = null;
    public float moveSpeed = 4.5f;
    public float jumpForce = 6.5f;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float movex = Input.GetAxisRaw("Horizontal");

        //body.AddForce(new Vector2(movex * moveSpeed, 0));
        Vector2 velocity = body.velocity;
        velocity.x = movex * moveSpeed;
        body.velocity = velocity;

        if (Input.GetKeyDown("space")) {
            body.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        }
    }

    void FixedUpdate(){
        float movex = Input.GetAxisRaw("Horizontal");

        //body.AddForce(new Vector2(movex * moveSpeed, 0));
        //Vector2 velocity = body.velocity;
        //velocity.x = movex * moveSpeed;
        //body.velocity = velocity;

        if (Mathf.Abs(movex) > 0.1) { return; }
    }
}
