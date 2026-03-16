using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private float _speed;
    private float _jumpForce;
    
    public Transform groundCheck;
    public LayerMask groundMask;
    private float _groundRadius;
    
    private Rigidbody2D _rigidbody2D;
    private bool _isGrounded;
    private bool _crouch;
    private bool _attacking;
    
    private bool _facingRight = true;
    
    private Animator _animator;
    
    // Start is called before the first frame update
    void Start()
    { 
        _speed = 5f;
        _jumpForce = 10f;
        _groundRadius = 0.2f;
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_facingRight & Input.GetKeyDown(KeyCode.A))
        {
            Flip();
            
            _facingRight = false;
        }
        
        if (!_facingRight & Input.GetKeyDown(KeyCode.D))
        {
            Flip();

            _facingRight = true;
        }
        
        Walk();

        Jump();
        
        Attack();
        
        WalkCrouching();
    }

    private void WalkCrouching()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            _speed = 3;
            _crouch = true;
        }

        if (Input.GetKeyUp(KeyCode.S))
        {
            _speed = 5;
            _crouch = false;
        }
    }

    private void Jump()
    {
        if (_isGrounded)
        {
            _animator.SetBool("IsJumping", false);
        }
        else
        {
            _animator.SetBool("IsJumping", true);
        }
        
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _rigidbody2D.linearVelocity = new Vector2(_rigidbody2D.linearVelocity.x, _jumpForce);
        }
    }

    private void Walk()
    {
        float horizontal = Input.GetAxis("Horizontal");
        
        _rigidbody2D.linearVelocity = new Vector2(horizontal * _speed, _rigidbody2D.linearVelocity.y);

        if (_crouch)
        {
            _animator.SetBool("IsCrouching", true);
        }
        else
        { 
            _animator.SetBool("IsCrouching", false);
            _animator.SetFloat("Speed", Mathf.Abs(horizontal));
        }
        
        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, _groundRadius, groundMask);
    }

    private void Attack()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            _animator.SetBool("IsAttacking", true);
        }
        else
        {
            _animator.SetBool("IsAttacking", false);
        }
    }

    private void Flip()
    {
        Vector3 scale = transform.localScale;
        
        scale.x *= -1;
        
        transform.localScale = scale;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null && _isGrounded)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(groundCheck.position, _groundRadius);
        }
    }
}
