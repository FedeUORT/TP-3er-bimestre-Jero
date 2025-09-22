using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentEye : MonoBehaviour
{
    [SerializeField] float reachLength;
    AgentScript agent;
    [SerializeField] Transform eye;

    float chasingTime;
    [SerializeField] float maxChasingTime;

    private void Awake()
    {
        agent = GameObject.FindObjectOfType<AgentScript>();
    }
    void Start()
    {
        
    }

    void Update()
    {
        Ray ray = new Ray(eye.position, eye.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction, reachLength);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                agent.isChasing = true;
                chasingTime = 0;
            }
        }
        if (chasingTime > maxChasingTime) agent.isChasing = false;
        else chasingTime += Time.deltaTime;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(eye.transform.position, eye.forward);
    }
}
