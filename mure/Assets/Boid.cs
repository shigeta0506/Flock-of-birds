using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    private float speed = 5f;
    private float boostedSpeed = 8f;
    private float reducedSpeed = 2f;
    private float treeAvoidanceSpeed = 3f;
    private float rotationSpeed = 2f;
    private float neighborRadius = 10f;
    private float separationDistance = 5f;
    private float separationStrength = 0.5f;
    private float bankingFactor = 5f;

    private float maxDistanceFromGroup = 20f;

    public List<Boid> allBoids; //群れ全体のBoidリスト

    private Vector3 velocity;
    private Vector3 collisionAvoidanceForce;
    private float currentSpeed;

    public float detectionRadius = 5f;
    public float avoidanceRange = 10f;
    private bool isAvoidingTree = false;

    void Start()
    {
        currentSpeed = speed;
        velocity = transform.forward * currentSpeed;
        collisionAvoidanceForce = Vector3.zero;
    }

    void Update()
    {
        //群れ行動（整列、結束、分離）の力を計算
        Vector3 alignment = Align() * 0.5f;
        Vector3 cohesion = Cohere() * 1.2f;
        Vector3 separation = Separate() * 1.0f;

        // 障害物回避の力を優先的に適用
        Vector3 avoidance = AvoidTaggedObjects("Bird") * 1.5f;
        Vector3 treeAvoidance = DetectAndAvoidTreesInPath() * 2.5f;

        //各力を合成してBoidの動きを決定
        Vector3 force = alignment + cohesion + separation + avoidance + collisionAvoidanceForce + treeAvoidance;
        velocity += force * Time.deltaTime;

        AdjustSpeedAndHeightBasedOnAltitude();

        velocity = velocity.normalized * currentSpeed;

        Vector3 newPosition = transform.position + velocity * Time.deltaTime;
        transform.position = newPosition;

        UpdateRotation();
        collisionAvoidanceForce = Vector3.zero;
    }

    void AdjustSpeedAndHeightBasedOnAltitude()
    {
        float minAltitude = 15f; //最低高度
        float maxAltitude = 25f; //最高高度
        float altitudeAdjustmentStrength = 0.1f; //高度調整の強さ

        if (transform.position.y < minAltitude)
        {
            //高さが最低高度未満の場合、上昇する力を追加
            float altitudeDifference = minAltitude - transform.position.y;
            velocity += Vector3.up * altitudeDifference * altitudeAdjustmentStrength;
        }
        else if (transform.position.y > maxAltitude)
        {
            //高さが最高高度を超えた場合、下降する力を追加
            float altitudeDifference = transform.position.y - maxAltitude;
            velocity += Vector3.down * altitudeDifference * altitudeAdjustmentStrength;
        }
        else
        {
            //高さが範囲内の場合、上昇・下降の力をリセット
            velocity.y = Mathf.Lerp(velocity.y, 0, Time.deltaTime * 2f);
        }
    }



    Vector3 DetectAndAvoidTreesInPath()
    {
        Vector3 avoidDirection = Vector3.zero;
        bool treeDetected = false;
        float avoidanceStrength = 10.0f;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, avoidanceRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Tree"))
            {
                treeDetected = true;
                isAvoidingTree = true;

                Vector3 colliderCenter = hitCollider.bounds.center;
                Vector3 directionAway = transform.position - colliderCenter;
                avoidDirection += directionAway.normalized;
            }
        }

        if (treeDetected)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, treeAvoidanceSpeed, Time.deltaTime * 1.0f);
            return avoidDirection.normalized * currentSpeed * avoidanceStrength;
        }

        if (!treeDetected && isAvoidingTree)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, speed, Time.deltaTime * 0.5f);
            isAvoidingTree = false;
        }

        return Vector3.zero;
    }

    Vector3 AvoidTaggedObjects(string tag)
    {
        Vector3 avoidanceForce = Vector3.zero;
        float avoidanceStrength = 5f;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(tag) && hitCollider.transform != this.transform)
            {
                Vector3 directionAway = transform.position - hitCollider.transform.position;
                avoidanceForce += directionAway.normalized * avoidanceStrength;
            }
        }

        return avoidanceForce;
    }

    Vector3 Align()
    {
        Vector3 alignmentForce = Vector3.zero;
        int count = 0;

        foreach (Boid otherBoid in allBoids)
        {
            if (otherBoid != this)
            {
                float distance = Vector3.Distance(transform.position, otherBoid.transform.position);
                if (distance < neighborRadius)
                {
                    alignmentForce += otherBoid.velocity;
                    count++;
                }
            }
        }

        if (count > 0)
        {
            alignmentForce /= count;
            alignmentForce = (alignmentForce - velocity).normalized * currentSpeed;
        }

        return alignmentForce;
    }

    Vector3 Cohere()
    {
        Vector3 cohesionForce = Vector3.zero;
        int count = 0;

        foreach (Boid otherBoid in allBoids)
        {
            if (otherBoid != this)
            {
                float distance = Vector3.Distance(transform.position, otherBoid.transform.position);
                if (distance < neighborRadius)
                {
                    cohesionForce += otherBoid.transform.position;
                    count++;
                }
            }
        }

        if (count > 0)
        {
            cohesionForce /= count;
            cohesionForce -= transform.position;
            cohesionForce = cohesionForce.normalized * currentSpeed;
        }

        return cohesionForce;
    }

    Vector3 Separate()
    {
        Vector3 separationForce = Vector3.zero;
        int count = 0;

        foreach (Boid otherBoid in allBoids)
        {
            if (otherBoid != this)
            {
                float distance = Vector3.Distance(transform.position, otherBoid.transform.position);
                if (distance < separationDistance)
                {
                    separationForce += (transform.position - otherBoid.transform.position).normalized / distance;
                    count++;
                }
            }
        }

        if (count > 0)
        {
            separationForce = separationForce.normalized * currentSpeed * separationStrength;
        }

        return separationForce;
    }

    void UpdateRotation()
    {
        Vector3 direction = velocity.normalized;
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}