using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class MonsterController : MonoBehaviour
{
    public enum State
    {
        IDLE, TRACE, ATTACK, DIE
    }
    readonly int hashTrace = Animator.StringToHash("IsTrace");
    readonly int hashAttack = Animator.StringToHash("IsAttack");
    readonly int hashHit = Animator.StringToHash("Hit");
    readonly int hashSpeed = Animator.StringToHash("Speed");
    readonly int hashPlayerDie = Animator.StringToHash("PlayerDie");
    public State state = State.IDLE;
    public float traceDist = 10;
    public float attackDist = 2;
    public bool isDie = false;


    Transform tr;
    //TODO 타겟 관리 방법 바꾸기
    [SerializeField] Transform target;
    [SerializeField] GameObject bloodEffect;
    NavMeshAgent agent;
    Animator anim;

    void Awake()
    {
        tr = GetComponent<Transform>();
        if (target == null) UnityEngine.Debug.Log($"플레이어 넣기");
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CheckMonsterState());
        StartCoroutine(MonsterAction());
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.CompareTag("Bullet"))
        {
            Destroy(other.gameObject);
            anim.SetTrigger(hashHit);
            Vector3 pos = other.GetContact(0).point;
            Quaternion rot = Quaternion.LookRotation(-other.GetContact(0).normal);
            GameObject blood = Instantiate(bloodEffect, pos, rot, transform);
            Destroy(blood, 1);
        }
    }
    IEnumerator MonsterAction()
    {
        while (!isDie)
        {
            switch (state)
            {
                case State.IDLE:
                    agent.isStopped = true;
                    anim.SetBool(hashTrace, false);
                    break;

                case State.TRACE:
                    agent.SetDestination(target.position);
                    agent.isStopped = false;
                    anim.SetBool(hashTrace, true);
                    anim.SetBool(hashAttack, false);
                    break;

                case State.ATTACK:
                    anim.SetBool(hashAttack, true);
                    break;

                case State.DIE:
                    break;
            }
            
            yield return new WaitForSeconds(0.3f);
        }
    }
    IEnumerator CheckMonsterState()
    {
        while (!isDie)
        {
            float dist = (tr.position - target.position).sqrMagnitude;
            if (dist < attackDist * attackDist)
            {
                state = State.ATTACK;
            }
            else if (dist < traceDist * traceDist)
            {
                state = State.TRACE;
            }
            else
            {
                state = State.IDLE;
            }

            yield return new WaitForSeconds(0.3f);
        }
    }
    void OnPlayerDie()
    {
        
        StopAllCoroutines();
        agent.isStopped = true;
        anim.SetFloat(hashSpeed, Random.Range(0.9f, 1.1f));
        anim.SetTrigger(hashPlayerDie);
    }
    void OnEnable()
    {
        PlayerController.OnPlayerDie += this.OnPlayerDie;
    }
    void OnDisable()
    {
        PlayerController.OnPlayerDie -= this.OnPlayerDie;
    }
    void OnDrawGizmos()
    {
        if (state == State.TRACE)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(tr.position, traceDist);
        }
        else if (state == State.ATTACK)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(tr.position, attackDist);
        }
        else if (state == State.IDLE)
        {
            Gizmos.color = Color.green;
        }
    }
}
