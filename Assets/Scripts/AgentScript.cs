using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
public class AgentScript : MonoBehaviour
{
    NavMeshAgent agent;
    [SerializeField] Transform targetTR;
    [SerializeField] Animator anim;
    [SerializeField] float velocity;
    [SerializeField] Transform[] wayPoints;
    int currentWayPoint = 0;

    Transform playerTransform;
    public bool isChasing;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    // Start is called before the first frame update
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        targetTR = wayPoints[0];
        agent.destination = targetTR.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (isChasing)
        {
            ChasePlayer();
            return;
        }
        if ((transform.position-targetTR.position).magnitude<0.7f)
        {
            currentWayPoint++;
            if (currentWayPoint > wayPoints.Length - 1) currentWayPoint = 0;
            targetTR = wayPoints[currentWayPoint];
        }
        agent.destination = targetTR.position;
        velocity = agent.velocity.magnitude;
        anim.SetFloat("Speed",velocity);
    }

    void ChasePlayer()
    {
        agent.destination = playerTransform.position;
        velocity = agent.velocity.magnitude;
        anim.SetFloat("Speed", velocity);
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            Debug.Log("Game Over"); 
            SceneManager.LoadScene("GameOver");
        }
    }

}
