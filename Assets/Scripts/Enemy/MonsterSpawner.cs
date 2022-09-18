using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private EnemyBehaviour prefab;
    [SerializeField] private int monstersCount = 1;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float minRange = 1f;
    [SerializeField] private float maxRange = 2f;
    [SerializeField] private LightController lightController;

    public void SpawnMonsters()
    {
        for (int i = 0; i < monstersCount; i++)
        {
            var enemy = Instantiate(prefab);
            var direction = Random.insideUnitCircle;
            direction.Normalize();
            direction *= Random.Range(minRange, maxRange);
            enemy.transform.position = playerTransform.position + (Vector3)direction;
            enemy.Initialize(lightController);
        }
    }
}
