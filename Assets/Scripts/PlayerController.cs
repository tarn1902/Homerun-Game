using System.Collections;
using UnityEngine;

//Controls Player
[RequireComponent (typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Animator anim;
    private GameObject canvas;
    private float currentSpeed = 10;
    private DashState dashState = DashState.DASHABLE;
    private AttackState attackState = AttackState.ATTACKABLE;
    
    private Vector3 move = Vector3.zero;
    private float gravity = 20f;
    public bool dead = false;
    Collider[] colliders;
    Rigidbody[] rigidbodies;
    public float mana;

    [Header("Player Settings")]
    public float playerSpeed = 10;
    public float maxMana = 100;
    public int health = 3;
    public float normalForceMultiplier = 5;
    public float strongForceMultiplier = 10;
    public ParticleSystem powerUpVFX;
    public ParticleSystem hitVFX;

    [Header("Dash Settings")]
    public KeyCode dashKey = KeyCode.Space;
    public float dashTime = 0.1f;
    public float dashSpeed = 250;
    public float dashCoolDown = 1f;
    public float dashManaCost = 0;
    public float dashImpactRange = 1.5f;
    public float dashForwardRange = 1.5f;

    [Header("Attack Settings")]
    public KeyCode attackKey = KeyCode.LeftControl;
    public float attackPeakTime = 0.1f;
    public float attackCoolDown = 1f;
    public float attackManaCost = 0;
    public float attackImpactRange = 1.5f;
    public float attackForwardRange = 1.5f;
    public float killManaAmount = 10;

    [Header("Invunerable Settings")]
    public KeyCode invunerableKey = KeyCode.F;
    public float invunerableManaCost = 5;
    public bool isInvunerable = false;

    [Header("Mouse Rotation")]
    public GameObject bodyAim = null;
    public float bodyAimHeight = 100;
    public float rotationSpeed = 10;
    public Vector3 facingDirection;

    public KeyCode rangedKey = KeyCode.LeftShift;
    public GameObject projectile = null;
    public float projectileCost = 10;

    //Holds states of dashing
    enum DashState
    {
        DASHABLE,
        DASHING,
        UNDASHABLE
    }

    //Holds states of dashing
    enum AttackState
    {
        ATTACKABLE,
        ATTACKPEAK,
        UNATTACKABLE
    }

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        canvas = GameObject.FindGameObjectWithTag("UI");
        colliders = GetComponentsInChildren<Collider>();
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        controller.enabled = true;
        anim.enabled = true;
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject != gameObject)
            {
                collider.isTrigger = true;
            }
        }
        foreach (Rigidbody rigidbody in rigidbodies)
        {
            rigidbody.useGravity = false;
        }
        dead = false;
        mana = 0;
        canvas.SendMessage("UpdateMana", mana);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!dead)
        {
            Movement();
            Invunerable();
            Dash();
            MeleeAttack();
            RangedAttack();
            MouseInput();
        }
        if (isInvunerable && !powerUpVFX.isPlaying)
        {
            powerUpVFX.Play();
        }
        else if (!isInvunerable && powerUpVFX.isPlaying)
        {
            powerUpVFX.Stop();
        }
        
    }

    void MouseInput()
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.farClipPlane));
        mouseWorldPosition.y = bodyAimHeight;
        facingDirection = (mouseWorldPosition - transform.position).normalized;
        mouseWorldPosition = facingDirection * rotationSpeed;
        bodyAim.transform.position = mouseWorldPosition;
    }
    //Runs this during dash
    IEnumerator DashSequence()
    {
        dashState = DashState.DASHING;
        yield return new WaitForSeconds(dashTime);
        dashState = DashState.UNDASHABLE;
        yield return new WaitForSeconds(dashCoolDown);
        dashState = DashState.DASHABLE;
    }

    IEnumerator AttackSequence(bool isProjectile)
    {
        attackState = AttackState.UNATTACKABLE;
        anim.SetTrigger("isAttacking");
        yield return new WaitForSeconds(attackPeakTime);
        hitVFX.transform.forward = facingDirection;
        hitVFX.Play();
        if (isProjectile)
        {
            GameObject temp = Instantiate(projectile, transform.position + facingDirection * attackForwardRange + transform.up * bodyAimHeight, Quaternion.identity);
            temp.transform.forward = facingDirection;
            temp.GetComponent<Rigidbody>().AddForce(facingDirection * normalForceMultiplier, ForceMode.Impulse);
        }
        else
        {
            AttackArea(attackForwardRange, attackImpactRange, facingDirection * normalForceMultiplier);
        }
        
        yield return new WaitForSeconds(attackCoolDown);
        attackState = AttackState.ATTACKABLE;
    }

    //Controls dash ability
    void Dash()
    {
        if (dashState == DashState.DASHABLE && Input.GetKeyDown(dashKey) && controller.collisionFlags != CollisionFlags.CollidedSides)
        {
            if (TakeMana(dashManaCost))
            {
                if (isInvunerable)
                {
                    AttackArea(dashForwardRange, dashImpactRange, transform.forward);
                }
                StartCoroutine(DashSequence());
            }
        }
        else if (dashState == DashState.DASHING)
        {
            currentSpeed = dashSpeed;
        }
        else
        {
            currentSpeed = playerSpeed;
        }
    }

    //Controls movement
    void Movement()
    {
        if (controller.isGrounded)
        {
            move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            move = Vector3.Scale(move, Vector3Abs(move.normalized) * currentSpeed);
            if (move.normalized != Vector3.zero)
            {
                transform.forward = move.normalized;
            }
            
        }
        move.y -= gravity; 
        controller.Move(move * Time.deltaTime);
        anim.SetFloat("Speed", controller.velocity.magnitude);

    }

    //Controls Invunerable ability
    void Invunerable()
    {
        if (Input.GetKeyDown(invunerableKey) && mana >= maxMana)
        {
            isInvunerable = true;
        }

        if (isInvunerable)
        {
            isInvunerable = TakeMana(invunerableManaCost * Time.deltaTime);
        }
    }

    //Removes mana
    public bool TakeMana(float drain)
    {
        if (mana - drain >= dashManaCost)
        {
            mana -= drain;
            canvas.SendMessage("UpdateMana", mana);
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool AddMana(float give)
    {
        if (mana + give <= 100)
        {
            mana += give;
            canvas.SendMessage("UpdateMana", mana);
            return true;
        }
        else if (mana + give > 100)
        {
            mana = maxMana;
            return true;
        }
        else
        {
            return false;
        }
    }

    //Removes health
    public void TakeHealth(int drain, Vector3 forceDirection)
    {
        if (!isInvunerable)
        {
            health -= drain;
            canvas.GetComponentInChildren<HealthController>().SendMessage("ReduceHealth");
            if (health <= 0)
            { 
                GoRagdoll(forceDirection);
            }
        }
        
    }

    //Creates absolute of each flaot in vector3
    Vector3 Vector3Abs(Vector3 vec)
    {
        return new Vector3(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
    }

    void MeleeAttack()
    {
        if (attackState == AttackState.ATTACKABLE && Input.GetKeyDown(attackKey))
        {
            StartCoroutine(AttackSequence(false));
        }
    }
    void RangedAttack()
    {
        if (attackState == AttackState.ATTACKABLE && Input.GetKeyDown(rangedKey))
        {
            if (TakeMana(projectileCost))
            {
                StartCoroutine(AttackSequence(true));
            }
            
        }
    }


    void AttackArea(float forwardRange, float impactRange, Vector3 forceDirection)
    {
        
        Collider[] colliders = Physics.OverlapSphere(transform.position + facingDirection * forwardRange + transform.up * bodyAimHeight, impactRange, (1 << 10) + (1 << 11) + (1 << 13));
        if (colliders.Length > 0)
        {
            foreach (Collider col in colliders)
            {
                if (col.transform.tag == "Enemy")
                {
                    col.transform.GetComponent<AIController>().TurnOnRagdoll(forceDirection * (isInvunerable ? strongForceMultiplier : normalForceMultiplier));
                    AddMana(killManaAmount);
                }
                else if (col.transform.GetComponent<Rigidbody>())
                {
                    col.transform.GetComponent<Rigidbody>().AddForce(forceDirection * (isInvunerable ? strongForceMultiplier : normalForceMultiplier ), ForceMode.Impulse);
                }
            }
        }
    }

    public void GoRagdoll(Vector3 force)
    {
        controller.enabled = false;
        anim.enabled = false;
        foreach (Collider collider in colliders)
        {
            collider.isTrigger = false;
        }
        foreach (Rigidbody rigidbody in rigidbodies)
        {
            rigidbody.useGravity = true;
            rigidbody.AddForce(force, ForceMode.Impulse);
        }
        dead = true;
    }
}
