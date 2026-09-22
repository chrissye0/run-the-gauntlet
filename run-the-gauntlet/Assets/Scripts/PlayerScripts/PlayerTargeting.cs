using UnityEngine;

public class PlayerTargeting : MonoBehaviour
{
    // distance to be targeted
    public float targetRange = 20f;
    // distance to attack
    public float attackRange = 4f;
    // next enemy to target
    private GameObject targetEnemy;
    public GameObject TargetEnemy
    {
        get { return targetEnemy; }
    }
    // for targeted enemy in attack range
    private GameObject activeEnemy;
    public GameObject ActiveEnemy
    {
        get { return activeEnemy; }
    }

    // start by finding a target
    void Start()
    {
        FindNewTarget();
    }

    void Update()
    {
        // if we don't have an enemy, find one
        if (targetEnemy == null)
        {
            FindNewTarget();
        } else
        {
            // otherwise, see if the enemy is within attack range
            CheckAttackRange(targetEnemy);
        }
    }

    // find a new target
    void FindNewTarget()
    {
        // get all enemies
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closestEnemy = null;
        float shortestDistance = targetRange;
        // loop through all enemies
        foreach (GameObject enemy in enemies)
        {
            // get distance between player and enemy
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            // if shortest distance
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                // set to closest enemy
                closestEnemy = enemy;
            }
        }
        targetEnemy = closestEnemy;
    }

    // see if an enemy is within attack range
    void CheckAttackRange(GameObject enemy)
    {
        float distance = Vector3.Distance(transform.position, enemy.transform.position);
        // if enemy is close enough to be attacked
        if (distance <= attackRange)
        {
            // mark as red
            enemy.GetComponent<Renderer>().material.color = Color.red;
            // set it to the active enemy
            activeEnemy = enemy;
        }
        else
        {
            // enemy is not close enough
            // mark as white
            enemy.GetComponent<Renderer>().material.color = Color.white;
        }
    }
}