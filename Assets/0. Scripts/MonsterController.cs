using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.AI;

public class MonsterController : MonoBehaviour
{
    public enum State
    {
        IDLE, TRACE, ATTACK, DIE
    }
    public State state = State.IDLE;
    public float traceDist = 10;
    public float attackDist = 2;
    public bool isDie = false;

    Transform tr;
    //TODO 타겟 관리 방법 바꾸기
    [SerializeField] Transform target;
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

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator MonsterAction()
    {
        while (!isDie)
        {
            switch (state)
            {
                case State.IDLE:
                    agent.isStopped = true;
                    anim.SetBool("IsTrace", false);
                    break;

                case State.TRACE:
                    agent.SetDestination(target.position);
                    agent.isStopped = false;
                    anim.SetBool("IsTrace", true);
                    break;

                case State.ATTACK:
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
