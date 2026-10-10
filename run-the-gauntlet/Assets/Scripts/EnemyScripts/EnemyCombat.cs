using UnityEngine;
using TMPro;
public class EnemyCombat : MonoBehaviour
{
    public GameManager gameManager;
    private GameObject player;
    private PlayerCombat playerCombat;
    public float enemyAttackRange = 3.5f;
    // time in between attacks
    public float attackCooldown = 6.0f;
    private float lastAttackTime = 0f;
    // how many points are deducted
    public float damage = 5f;
    private TMP_Text attackText;
    private float attackTextEndTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerCombat = player.GetComponent<PlayerCombat>();

        GameObject uiObject = GameObject.Find("AttackText");
        attackText = uiObject.GetComponent<TMP_Text>();
        attackText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        // attack if within range and cooldown has passed
        if (attackText == null) return;
        if (distance <= enemyAttackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
            // reset timer
            lastAttackTime = Time.time;
            attackText.text = "ATTACKING!";
            attackText.gameObject.SetActive(true);
            attackTextEndTime = Time.time + 0.5f;
        }
        if (attackText.gameObject.activeSelf && Time.time >= attackTextEndTime)
        {
            attackText.gameObject.SetActive(false);
        }
    }

    void Attack()
    {
        //Debug.Log("Attacking player!");
        if (GetComponent<Renderer>().material.color == Color.white)
        {
            GetComponent<Renderer>().material.color = Color.yellow;
        }
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
