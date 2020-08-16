using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    // Start is called before the first frame update
    public float fuse = 3;
    public float blastRadiusConed = 10;
    public float blastRadiusSphere = 1;
    public float blastForce = 10;
    public float maxExplodeRangeTrigger = 10;
    public float attackAngle = 20;
    public float killManaGain = 10;
    GameObject player = null;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(player.transform.position, transform.position) >= maxExplodeRangeTrigger)
        {
            Explode();
        }

        if (fuse > 0)
        {
            fuse -= Time.deltaTime;
        }
        else
        {
            Explode();
        }
    }

    void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, blastRadiusConed, (1 << 10) + (1 << 11) + (1 << 13));
        if (colliders.Length != 0)
        {
            foreach (Collider col in colliders)
            {
                if (Mathf.Abs(Vector3.Angle(transform.forward, (col.transform.position - transform.position).normalized)) < attackAngle || Vector3.Distance(col.transform.position, transform.position) < blastRadiusSphere)
                {
                    if (col.tag == "Enemy")
                    {
                        player.GetComponent<PlayerController>().AddMana(killManaGain);
                        col.GetComponent<AIController>().TurnOnRagdoll((col.transform.position - transform.position).normalized * blastForce);
                        Debug.Log((col.transform.position - transform.position).normalized * blastForce);
                    }
                    else if (col.transform.GetComponent<Rigidbody>())
                    {
                        col.transform.GetComponent<Rigidbody>().AddForce((col.transform.position - transform.position).normalized * blastForce, ForceMode.Impulse);

                    }
                }
            }
        }
        Destroy(transform.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        Explode();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, blastRadiusConed);
    }
}
