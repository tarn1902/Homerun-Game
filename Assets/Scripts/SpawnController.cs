using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
    public Transform[] spawnGroups;
    Transform[] spawnPoints;
    public GameObject spawnable;
    public float spawnTime = 5;
    public int spawnCountMax = 50;
    PlayerController player;
    public int totalCount = 0;
    public int manaPercentageTrigger = 15;
    public int firstEnemyNumTrigger = 3;
    public int firstIndividualSpawnChance = 3;
    public int secondEnemyNumTrigger = 5;
    public int secondIndividualSpawnChance = 5;
    public int thirdEnemyNumTrigger = 20;
    public int thirdIndividualSpawnChance = 10;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        StartCoroutine("Spawn");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Spawn()
    {
        while (!player.dead )
        {
            spawnPoints = spawnGroups[Random.Range(0, spawnGroups.Length)].GetComponentsInChildren<Transform>();
            if (player.mana / player.maxMana * 100 < manaPercentageTrigger)
            {
                SpawnIndividual(Random.Range(1, spawnGroups.Length));
                yield return new WaitForSeconds(spawnTime);
            }
            else if (AIController.EnemyNum <= firstEnemyNumTrigger)
            {
                switch(Random.Range(0, firstIndividualSpawnChance) == 0)
                {
                    case true:
                        {
                            SpawnGroup(Random.Range(2, spawnPoints.Length - 1));
                            break;
                        }
                    case false:
                        {
                            
                            SpawnIndividual(Random.Range(1, spawnPoints.Length - 1));
                            break;
                        }
                }
                yield return new WaitForSeconds(spawnTime);
            }
            else if (AIController.EnemyNum > secondEnemyNumTrigger && AIController.EnemyNum <= thirdEnemyNumTrigger)
            {
                switch (Random.Range(0, secondIndividualSpawnChance) == 0)
                {
                    case true:
                        {
                            SpawnGroup(Random.Range(2, spawnPoints.Length - 1));
                            break;
                        }
                    case false:
                        {
                            SpawnIndividual(Random.Range(1, spawnPoints.Length - 1));
                            break;
                        }
                }
                yield return new WaitForSeconds(spawnTime);
            }
            else if (AIController.EnemyNum > thirdEnemyNumTrigger)
            {
                switch (Random.Range(0, thirdIndividualSpawnChance) == 0)
                {
                    case true:
                        {
                            SpawnGroup(Random.Range(2, spawnPoints.Length - 1));
                            break;
                        }
                    case false:
                        {
                            SpawnIndividual(Random.Range(1, spawnPoints.Length - 1));
                            break;
                        }
                }
                yield return new WaitForSeconds(spawnTime);
            }
            else
            {
                yield return null;
            }
        }
    }

    void SpawnGroup(int spawnNum)
    {
        for (int i = 1; i < spawnNum; i++)
        {
            SpawnIndividual(i);
        }
    }

    void SpawnIndividual(int SpawnPointIndex)
    {
        totalCount++;
        GameObject temp = Instantiate(spawnable, spawnPoints[SpawnPointIndex]);
        temp.transform.parent = null;
    }
}
