using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public float moveSpeed = 1.0f;
    public float rotationSpeed = 3.0f;
    private GameObject targetEnemy;
    private PlayerTargeting targeting;


    void Start()
    {
        // initialize targeting
        targeting = GetComponent<PlayerTargeting>();
    }

    void Update()
    {
        // find active enemy from targeting
        GameObject targetEnemy = targeting.ActiveEnemy;
        // return out if nothing found
        if (targetEnemy == null) return;
        // navigate to target
        MoveToTarget(targetEnemy);
    }

    // move and rotate to target
    void MoveToTarget(GameObject targetEnemy)
    {
        // return out if nothing
        if (!targetEnemy) return;
        // calculate direction vector
        Vector3 direction = targetEnemy.transform.position - transform.position;
        // prevent tilting up/down
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            // rotate in target direction
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        // move forward in that direction to a certain point
        if (Vector3.Distance(transform.position, targetEnemy.transform.position) > 4)
        {
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
    }
}
