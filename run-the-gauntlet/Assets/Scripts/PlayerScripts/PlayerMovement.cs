using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerMovement : MonoBehaviour
{

    public float moveSpeed = 2.0f;
    public float rotationSpeed = 5.0f;

    // Update is called once per frame
    void Update()
    {
        // return out if no enemy detected
        if (ClosestEnemy() == null) return;
        // calculate direction vector
        Vector3 direction = ClosestEnemy().transform.position - transform.position;
        // prevent tilting up/down
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            // rotate in target direction
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        // move forward in that direction to a certain point
        if(Vector3.Distance(transform.position, ClosestEnemy().transform.position) > 4)
        {
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
    }

    // returns the nearest enemy (out of all enemies)
    GameObject ClosestEnemy()
    {
        // finds all existing game objects with tag "Enemy"
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        // for storing closest enemy and shortest distance
        GameObject closestEnemy = null;
        float shortestDistance = 50;
        // loop through all found enemies
        foreach (GameObject enemy in enemies)
        {
            // calculate distance
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            // see if it is the shortest distance away
            if (distance < shortestDistance)
            {
                // if so, set it to the closest enemy
                shortestDistance = distance;
                closestEnemy = enemy;
            }
        }
        return closestEnemy;
    }
}
