using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    // reference to the player's Transform
    public Transform player;
    // reference to the NavMeshAgent component for pathfinding
    private NavMeshAgent navMeshAgent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get and store the NavMeshAgent component attached to this object
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        // ff there's a reference to the player
        if (player != null)
        {
            // set the enemy's destination to the player's current position
            navMeshAgent.SetDestination(player.position);
        }
    }
}
