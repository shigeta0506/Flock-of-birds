using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    //基本設定
    private float speed = 5f;
    private float rotationSpeed = 2f;
    private float neighborRadius = 15f;
    private float separationDistance = 6f;
    private float separationStrength = 3.0f;
    private float maxSpeed = 6f;
    private float minSpeed = 2f;

    public float treeAvoidanceRange = 10f;
    private float treeAvoidanceStrength = 10f;

    public List<Boid> allBoids; //シーン内のすべてのBoidを保持

    private Vector3 velocity;   //現在の移動速度

    private int frameCounter = 0;
    private const int calculationFrequency = 5; //計算の頻度（5フレームごとに計算）

    private float minAltitude = 15f;
    private float maxAltitude = 25f;
    private float altitudeAdjustmentStrength = 2f;

    void Start()
    {
        //初期速度
        velocity = transform.forward * speed;
    }

    void Update()
    {
        frameCounter++;

        //一定フレームごとに力を計算
        if (frameCounter % calculationFrequency == 0)
        {
            CalculateForces();
            frameCounter = 0;
        }

        //Boidを移動
        MoveBoid();
    }

    void CalculateForces()
    {
        //周囲のBoidを取得
        List<Boid> nearbyBoids = GetNearbyBoids();

        //各力の計算
        Vector3 alignment = Align(nearbyBoids) * 0.5f; //整列の力
        Vector3 cohesion = Cohere(nearbyBoids) * 1.0f; //結束の力
        Vector3 separation = Separate(nearbyBoids) * separationStrength; //分離の力
        Vector3 treeAvoidance = DetectAndAvoidTreesInPath() * 2.5f;      //障害物回避の力
        Vector3 altitudeAdjustment = MaintainAltitude(); //高度維持の力

        //全ての力を合計して速度に加算
        Vector3 force = alignment + cohesion + separation + treeAvoidance + altitudeAdjustment;
        velocity += force * Time.deltaTime;

        //速度制限
        velocity = LimitSpeed(velocity);
    }

    void MoveBoid()
    {
        //位置を更新
        Vector3 newPosition = transform.position + velocity * Time.deltaTime;
        transform.position = newPosition;

        // Boidの向きを更新
        UpdateRotation();
    }

    Vector3 LimitSpeed(Vector3 vel)
    {
        //速度を最小値と最大値の間に制限
        float speed = vel.magnitude;
        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
        return vel.normalized * speed;
    }

    Vector3 MaintainAltitude()
    {
        //高度を一定範囲内に保つ
        Vector3 altitudeForce = Vector3.zero;

        if (transform.position.y < minAltitude)
        {
            altitudeForce += Vector3.up * (minAltitude - transform.position.y) * altitudeAdjustmentStrength;
        }
        else if (transform.position.y > maxAltitude)
        {
            altitudeForce += Vector3.down * (transform.position.y - maxAltitude) * altitudeAdjustmentStrength;
        }

        return altitudeForce;
    }

    List<Boid> GetNearbyBoids()
    {
        //周囲のBoidを検索しリストに格納
        List<Boid> nearbyBoids = new List<Boid>();
        foreach (Boid otherBoid in allBoids)
        {
            if (otherBoid != this && Vector3.Distance(transform.position, otherBoid.transform.position) < neighborRadius)
            {
                nearbyBoids.Add(otherBoid);
            }
        }
        return nearbyBoids;
    }

    Vector3 Align(List<Boid> nearbyBoids)
    {
        //整列の力を計算
        Vector3 alignmentForce = Vector3.zero;
        foreach (Boid otherBoid in nearbyBoids)
        {
            alignmentForce += otherBoid.velocity;
        }

        if (nearbyBoids.Count > 0)
        {
            alignmentForce /= nearbyBoids.Count;
            alignmentForce = alignmentForce.normalized * speed;
        }

        return alignmentForce;
    }

    Vector3 Cohere(List<Boid> nearbyBoids)
    {
        //結束の力を計算
        Vector3 cohesionForce = Vector3.zero;
        foreach (Boid otherBoid in nearbyBoids)
        {
            cohesionForce += otherBoid.transform.position;
        }

        if (nearbyBoids.Count > 0)
        {
            cohesionForce /= nearbyBoids.Count;
            cohesionForce -= transform.position;
            cohesionForce = cohesionForce.normalized * speed;
        }

        return cohesionForce;
    }

    Vector3 Separate(List<Boid> nearbyBoids)
    {
        //分離の力を計算
        Vector3 separationForce = Vector3.zero;
        foreach (Boid otherBoid in nearbyBoids)
        {
            float distance = Vector3.Distance(transform.position, otherBoid.transform.position);
            if (distance < separationDistance)
            {
                separationForce += (transform.position - otherBoid.transform.position).normalized / distance;
            }
        }

        return separationForce.normalized * separationStrength;
    }

    Vector3 DetectAndAvoidTreesInPath()
    {
        //Treeタグを検出して回避
        Vector3 avoidDirection = Vector3.zero;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, treeAvoidanceRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Tree"))
            {
                Vector3 directionAway = transform.position - hitCollider.bounds.center;
                avoidDirection += directionAway.normalized;
            }
        }

        return avoidDirection.normalized * treeAvoidanceStrength;
    }

    void UpdateRotation()
    {
        //Boidの向きを現在の移動方向に合わせる
        Vector3 direction = velocity.normalized;
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}
