using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // reference to enemy prefab
    public GameObject enemyPrefab;
    // use this to reference player's location
    public Transform player;
    // max enemy account (adjust as needed)
    public int maxEnemyCount = 3;
    // make a list of enemies to know how many exist at a time
     List<GameObject> enemyList = new List<GameObject>();

    // Update is called once per frame
    void Update()
    {
        // set a cap for number of alive enemies
        if (enemyList.Count < maxEnemyCount)
        {
            // instantiate an enemy at a random location in bounds
            GameObject enemy = Instantiate(enemyPrefab, new Vector3(Random.Range(-8, 8), 0, Random.Range(10, 20)), Quaternion.identity);
            // add enemy to list
            enemyList.Add(enemy);
            // add the player's location for the enemy's nav mesh
            EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
            movement.player = player;
        }
    }
}
