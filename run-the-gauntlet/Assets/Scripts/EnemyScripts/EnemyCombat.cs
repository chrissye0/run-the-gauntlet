using UnityEngine;
using TMPro;
public class EnemyCombat : MonoBehaviour
{
    public GameManager gameManager;
    private GameObject player;
    private PlayerCombat playerCombat;
    [SerializeField] private float enemyAttackRange = 3.5f;
    // time in between attacks
    [SerializeField] private float attackCooldown = 6.0f;
    [SerializeField] private float lastAttackTime = 0f;
    // how many points are deducted
    [SerializeField] private float damage = 5f;
    [SerializeField] private Animator animator;

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
        //Debug.Log("Attacking player!");
        if (GetComponent<Renderer>().material.color == Color.white)
        {
            GetComponent<Renderer>().material.color = Color.yellow;
        }
        animator.SetTrigger("punch");
        // check to see if player is blocking
        if (playerCombat.blocking)
        {
            Debug.Log("Blocked!");
            gameManager.SetSpecialMeterValue(20f);
        }
        // to see if score won't go into negatives
        else if (gameManager.score >= damage)
        {
            gameManager.score -= damage;
        }
    }
}
