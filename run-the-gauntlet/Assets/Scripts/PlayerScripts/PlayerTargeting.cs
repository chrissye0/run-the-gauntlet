using UnityEngine;

public class PlayerTargeting : MonoBehaviour
{
    // distance to be targeted
    public float targetRange = 20f;
    // distance to attack
    public float attackRange = 4f;
    // store activeEnemy (make a public version for other scripts)
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
        if (activeEnemy == null)
        {
            FindNewTarget();
        }
        // check whether the current enemy has entered attack range
        if (activeEnemy != null)
        {
            CheckAttackRange();
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
        // set active enemy to the closest enemy
        SetActiveEnemy(closestEnemy);
    }

    // see if an enemy is within attack range
    void CheckAttackRange()
    {
        float distance = Vector3.Distance(transform.position, activeEnemy.transform.position);
        // if enemy is close enough to be attacked
        if (distance <= attackRange)
        {
            // mark as red
            activeEnemy.GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            // enemy is not close enough
            // mark as white
            activeEnemy.GetComponent<Renderer>().material.color = Color.white;
        }
    }

    // set the active enemy
    void SetActiveEnemy(GameObject newEnemy)
    {
        // if the target hasn't changed, return out
        if (activeEnemy == newEnemy) return;
        // reset old enemy's color
        if (activeEnemy != null)
        {
            activeEnemy.GetComponent<Renderer>().material.color = Color.white;
        }
        // set active enemy to new one
        activeEnemy = newEnemy;
    }
}