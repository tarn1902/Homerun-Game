using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    //CharacterController controller;

    public static int EnemyNum = 0;
    NavMeshAgent agent;
    GameObject player;
    private Animator anim;
    public float attackRange = 1;
    public float chaseRange = 10;
    public float forceMultiplier = 10;
    public float attackImpactRange = 0.8f;
    public float attackForwardRange = 0.8f;
    public int damage = 1;
    public float attackDelayTime = 1.5f;
    bool isAttackAble = true;
    Vector3 move = Vector3.zero;
    private float upOffset = 1;
    public float attackPeakTime = 1f;
    PlayerController playerController;
    bool dead = false;
    public List<Vector3> patrolPoints;
    int patrolIndex = 0;
    public bool isPatrolling = false;
    Collider[] colliders;
    Rigidbody[] rigidbodies;
    public bool isSwarm = true;
    public bool isRandomlyDie = false;
    public float objectSpeedToDeath = 10;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        
        colliders = GetComponentsInChildren<Collider>();
        rigidbodies = GetComponentsInChildren<Rigidbody>();
    }
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
        if (patrolPoints.Count == 0)
        {
            isPatrolling = false;
        }
        agent.enabled = true;
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
        EnemyNum++;
    }

    // Update is called once per frame
    void Update()
    {
        if (Random.Range(0, 2) == 1 && isRandomlyDie)
        {
            TurnOnRagdoll(Vector3.zero);
        }
        if (!playerController.dead && !dead)
        {
            if (InChaseRange() || isSwarm)
            {
                if (InAttackRange() && isAttackAble)
                {
                    StartCoroutine(AttackSequence());
                }
                else
                {
                    //Checks if another enemy is in way
                    bool isBlocked = false;
                    Collider[] colliders = Physics.OverlapSphere(transform.position + transform.up * upOffset + transform.forward * attackForwardRange, attackImpactRange);
                    foreach (Collider collider in colliders)
                    {
                        if (collider.transform.tag == "Enemy" && collider.gameObject != gameObject)
                        {
                            isBlocked = true;
                        }
                    }
                    if (isBlocked)
                    {
                        transform.forward = player.transform.position - transform.position;
                        agent.isStopped = true;
                    }
                    else
                    {
                        agent.SetDestination(player.transform.position - (player.transform.position - transform.position).normalized * (attackRange - 0.1f));
                        agent.isStopped = false;
                    }
                }
            }
            else if (isPatrolling)
            {
                if (!agent.hasPath)
                {
                    agent.SetDestination(patrolPoints[patrolIndex]);
                    if (patrolIndex < patrolPoints.Count-1)
                        patrolIndex++;
                    else
                        patrolIndex = 0;
                }
            }
        }
        anim.SetFloat("Speed", agent.velocity.magnitude);
    }

    bool InChaseRange()
    {
        float dist = Vector3.Distance(transform.position, player.transform.position);
        if (dist <= chaseRange)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool InAttackRange()
    {
        float dist = Vector3.Distance(transform.position, player.transform.position);
        if (dist <= attackRange)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void TurnOnRagdoll(Vector3 force)
    {
        agent.enabled = false;
        anim.enabled = false;
        foreach (Collider collider in colliders)
        {
            if (collider == GetComponent<Collider>())
            {
                collider.isTrigger = true;
            }
            else
            {
                collider.isTrigger = false;
            }
        }
        foreach (Rigidbody rigidbody in rigidbodies)
        {
            rigidbody.useGravity = true;
            rigidbody.AddForce(force / rigidbodies.Length, ForceMode.Impulse);
        }
        GetComponent<Collider>().enabled = false;
        dead = true;
        EnemyNum--;
    }

    IEnumerator AttackSequence()
    {
        transform.forward = player.transform.position - transform.position;
        isAttackAble = false;
        anim.SetTrigger("isAttacking");
        yield return new WaitForSeconds(attackPeakTime);
        if (!dead)
        {
            AttackArea(attackForwardRange, attackImpactRange, -transform.right * forceMultiplier);
        }
        yield return new WaitForSeconds(attackDelayTime);
        isAttackAble = true;
    }

    //Creates absolute of each flaot in vector3
    Vector3 Vector3Abs(Vector3 vec)
    {
        return new Vector3(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
    }

    void AttackArea(float forwardRange, float impactRange, Vector3 forceDirection)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position + transform.up * upOffset + transform.forward * forwardRange, impactRange);
        if (colliders.Length > 0)
        {
            foreach (Collider col in colliders)
            {
                if (col.transform.tag == "Player")
                {
                    col.transform.GetComponent<PlayerController>().TakeHealth(damage , forceDirection);
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Floor"))
        {
            if (collision.transform.GetComponent<Rigidbody>())
            {
                if (collision.transform.GetComponent<Rigidbody>().velocity.magnitude > objectSpeedToDeath)
                {
                     TurnOnRagdoll((collision.transform.position - transform.position).normalized * forceMultiplier);
                }
            }
        }
    }
}
