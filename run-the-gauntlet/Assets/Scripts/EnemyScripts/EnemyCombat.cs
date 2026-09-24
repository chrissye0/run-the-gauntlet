using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    public GameManager gameManager;
    private GameObject player;
    private PlayerCombat playerCombat;
    public float enemyAttackRange = 3.5f;
    // time in between attacks
    public float attackCooldown = 3.0f;
    private float lastAttackTime = 0f;
    // how many points are deducted
    public float damage = 20f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerCombat = player.GetComponent<PlayerCombat>();
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        // attack if within range and cooldown has passed
        if (distance <= enemyAttackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
            // reset timer
            lastAttackTime = Time.time;
        }
    }

    void Attack()
    {
        Debug.Log("Attacking player!");
        if(GetComponent<Renderer>().material.color == Color.white)
        {
            GetComponent<Renderer>().material.color = Color.yellow;
        }
        // check to see if player is blocking
        if (playerCombat.blocking)
        {
            Debug.Log("Blocked!");
        }
        // to see if score won't go into negatives
        else if (gameManager.score >= damage)
        {
            gameManager.score -= 20;
        }
    }
}
