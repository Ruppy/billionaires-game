using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class CharacterController2D : MonoBehaviour
{
  [SerializeField] private float m_JumpForce = 400f;              // Amount of force added when the player jumps.
  [SerializeField] private float m_WallSlideSpeed = 2f;
  [Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;      // Amount of maxSpeed applied to crouching movement. 1 = 100%
  [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;  // How much to smooth out the movement
  [SerializeField] private bool m_AirControl = false;             // Whether or not a player can steer while jumping;
  [SerializeField] private LayerMask m_WhatIsGround;              // A mask determining what is ground to the character
  [SerializeField] private Transform m_GroundCheck;             // A position marking where to check if the player is grounded.
  [SerializeField] private Transform m_CeilingCheck;              // A position marking where to check for ceilings
  [SerializeField] private Transform m_WallCheck;             // A position marking where to check if the player is grounded.
  [SerializeField] private Collider2D m_CrouchDisableCollider;        // A collider that will be disabled when crouching
  [SerializeField] private int m_NumberOfJumps = 1;
  [SerializeField] private float m_fallMultiplier = 2.5f;
  [SerializeField] private float m_lowJumpMultiplier = 2f;
  [SerializeField] private float m_WallCheckDistance = 0.4f;

  public bool isTouchingWall = false;
  public bool isWallSliding = false;
  public bool ignorePlayerInput = false;


  public int m_RemainingJumps = 1;
  private float m_CurrentJumpForce = 1;

  const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
  private bool m_Grounded;            // Whether or not the player is grounded.
  const float k_CeilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up
  private Rigidbody2D m_Rigidbody2D;
  private bool m_FacingRight = true;  // For determining which way the player is currently facing.
  private Vector3 m_Velocity = Vector3.zero;
  private float horizontalMoveInput = 0f;

  [Header("Events")]
  [Space]

  public UnityEvent OnLandEvent;

  [System.Serializable]
  public class BoolEvent : UnityEvent<bool> { }

  public BoolEvent OnCrouchEvent;
  private bool m_wasCrouching = false;

  private void Awake()
  {
    m_Rigidbody2D = GetComponent<Rigidbody2D>();
    m_RemainingJumps = m_NumberOfJumps;
    m_CurrentJumpForce = m_JumpForce;

    if (OnLandEvent == null)
      OnLandEvent = new UnityEvent();

    if (OnCrouchEvent == null)
      OnCrouchEvent = new BoolEvent();
  }

  private void Update() {

    ApplyJumpGravity();
    CheckWall();
  }

  private void ApplyJumpGravity() {
    if (isWallSliding) {
      m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, -m_WallSlideSpeed);
      m_RemainingJumps = 2;
    }
    else if (m_Rigidbody2D.velocity.y < 0 && isTouchingWall == false) {
      m_Rigidbody2D.velocity += Vector2.up * Physics2D.gravity.y * (m_fallMultiplier - 1) * Time.deltaTime;
    }
    else if ((m_Rigidbody2D.velocity.y > 0) && (Input.GetButton("Jump") == false)) {
      m_Rigidbody2D.velocity += Vector2.up * Physics2D.gravity.y * (m_lowJumpMultiplier - 1) * Time.deltaTime;
    }
  }

  private void CheckWall() {
    isTouchingWall = Physics2D.Raycast(m_WallCheck.position, transform.right * transform.localScale.x, m_WallCheckDistance, m_WhatIsGround);

    isWallSliding = isTouchingWall &&
                    m_Grounded == false &&
                    m_Rigidbody2D.velocity.y < 0 &&
                    horizontalMoveInput != 0;

  }

  private void OnDrawGizmos() {
    Gizmos.DrawLine(m_WallCheck.position, new Vector3(m_WallCheck.position.x + m_WallCheckDistance, m_WallCheck.position.y, m_WallCheck.position.z));
  }

  private void FixedUpdate()
  {
    bool wasGrounded = m_Grounded;
    m_Grounded = false;
    if (m_Rigidbody2D.velocity.y > 0) {
      return;
    }

    // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
    // This can be done using layers instead but Sample Assets will not overwrite your project settings.
    Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
    for (int i = 0; i < colliders.Length; i++)
    {
      if (colliders[i].gameObject != gameObject)
      {
        m_Grounded = true;
        if (!wasGrounded)
          OnLandEvent.Invoke();
      }
    }
  }


  public void Move(float move, bool crouch, bool jump)
  {
    if (ignorePlayerInput) {
      return;
    }

    horizontalMoveInput = move;
    if (m_Grounded) {
      m_RemainingJumps = m_NumberOfJumps;
      m_CurrentJumpForce = m_JumpForce;
    }

    // If crouching, check to see if the character can stand up
    if (!crouch)
    {
      // If the character has a ceiling preventing them from standing up, keep them crouching
      if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
      {
        crouch = true;
      }
    }

    //only control the player if grounded or airControl is turned on
    if (m_Grounded || m_AirControl)
    {

      // If crouching
      if (crouch)
      {
        if (!m_wasCrouching)
        {
          m_wasCrouching = true;
          OnCrouchEvent.Invoke(true);
        }

        // Reduce the speed by the crouchSpeed multiplier
        move *= m_CrouchSpeed;

        // Disable one of the colliders when crouching
        if (m_CrouchDisableCollider != null)
          m_CrouchDisableCollider.enabled = false;
      } else
      {
        // Enable the collider when not crouching
        if (m_CrouchDisableCollider != null)
          m_CrouchDisableCollider.enabled = true;

        if (m_wasCrouching)
        {
          m_wasCrouching = false;
          OnCrouchEvent.Invoke(false);
        }
      }

      // Move the character by finding the target velocity
      Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
      // And then smoothing it out and applying it to the character
      m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

      // If the input is moving the player right and the player is facing left...
      if (move > 0 && !m_FacingRight)
      {
        // ... flip the player.
        Flip();
      }
      // Otherwise if the input is moving the player left and the player is facing right...
      else if (move < 0 && m_FacingRight)
      {
        // ... flip the player.
        Flip();
      }
    }
    // If the player should jump...
    if (m_RemainingJumps > 0 && jump)
    {
      // Add a vertical force to the player.
      m_Grounded = false;

      //m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
      if (isWallSliding) {
        m_Rigidbody2D.velocity = new Vector2(-m_Rigidbody2D.velocity.x, m_JumpForce);
        isWallSliding = false;
        StartCoroutine("StopMove");
      }
      else {
        m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, m_JumpForce);
      }

      m_CurrentJumpForce = m_JumpForce * 0.6f;
      m_RemainingJumps -= 1;
    }
  }

  IEnumerator StopMove() {
    ignorePlayerInput = true;
    transform.localScale = transform.localScale.x == 1 ? new Vector2(-1, 1): Vector2.one;
    yield return new WaitForSeconds(0.3f);
    transform.localScale = transform.localScale.x == 1 ? new Vector2(-1, 1): Vector2.one;
    ignorePlayerInput = false;
  }



  private void Flip()
  {
    // Switch the way the player is labelled as facing.
    m_FacingRight = !m_FacingRight;

    // Multiply the player's x local scale by -1.
    Vector3 theScale = transform.localScale;
    theScale.x *= -1;
    transform.localScale = theScale;
  }
}