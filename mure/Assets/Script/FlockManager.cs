using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour
{
    public Transform spawnLocation;
    public GameObject boidPrefab;
    public int boidCount = 20; //鳥の数
    public List<Boid> allBoids = new List<Boid>();

    void Start()
    {
        SpawnBoids();
    }

    void SpawnBoids()
    {
        //鳥のスポーン&Boidへ入力
        for (int i = 0; i < boidCount; i++)
        {
            Vector3 spawnPosition = spawnLocation.position + Random.insideUnitSphere * 2f;
            GameObject boidObject = Instantiate(boidPrefab, spawnPosition, Quaternion.identity);

            Boid boid = boidObject.GetComponent<Boid>();
            boid.allBoids = allBoids;
            allBoids.Add(boid);
        }
    }
}
