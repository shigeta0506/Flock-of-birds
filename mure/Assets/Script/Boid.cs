using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    // 基本設定
    private float speed = 5f;
    private float rotationSpeed = 5f;
    private float neighborRadius = 15f;
    private float separationDistance = 6f;
    private float separationStrength = 3.0f;
    private float maxSpeed = 15f; // 最大速度を増加
    private float minSpeed = 2f;
    private float birdMaxSpeed = 6f;
    private float birdMinSpeed = 1f;
    private float treeAvoidanceRange = 10f;
    private float treeAvoidanceStrength = 10f;
    private float boundaryRadius = 100f;
    private float minAltitude = 15f;
    private float maxAltitude = 25f;
    private float altitudeAdjustmentStrength = 2f;
    private float trackingRange = 50f;
    private float trackingStrength = 15f; // 追跡の強さを増加
    private float diveHeight = 50f;
    private float diveSpeedMultiplier = 5f;

    public List<Boid> allBoids;
    public Transform centerObject;

    private Vector3 velocity;
    private Transform targetBird;
    private Transform targetTaka;
    private int frameCounter = 0;
    private const int calculationFrequency = 5;

    private enum TakaState { Normal, Ascend, Dive }
    private TakaState takaState = TakaState.Normal;

    private float boostedSpeedMultiplier = 1.5f;
    private Vector3 diveTargetPosition;

    void Start()
    {
        velocity = transform.forward * speed;
    }

    void Update()
    {
        frameCounter++;

        if (frameCounter % calculationFrequency == 0)
        {
            CalculateForces();
            frameCounter = 0;
        }

        MoveBoid();
    }

    void CalculateForces()
    {
        Vector3 force = Vector3.zero;

        // Takaの追跡動作
        if (gameObject.CompareTag("Taka"))
        {
            force += TakaBehavior();
        }

        // Birdの回避動作
        if (gameObject.CompareTag("Bird"))
        {
            force += TrackTaka();
        }

        // 他の力（境界維持、高度調整など）
        force += MaintainAltitude();
        force += StayWithinBoundary();

        // 全体の力を適用
        velocity += force * Time.deltaTime;
        velocity = LimitSpeed(velocity);
    }

    Vector3 TakaBehavior()
    {
        UpdateTargetBird();

        if (targetBird != null)
        {
            Vector3 directionToTarget;

            if (takaState == TakaState.Dive)
            {
                // Birdの予測位置を計算
                diveTargetPosition = CalculateDiveTargetPosition(targetBird);
                directionToTarget = (diveTargetPosition - transform.position).normalized;
            }
            else
            {
                directionToTarget = (targetBird.position - transform.position).normalized;
            }

            // 即座に方向を向く
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // ターゲット方向に向かう力
            return directionToTarget * (takaState == TakaState.Dive ? maxSpeed * diveSpeedMultiplier : trackingStrength);
        }

        return Vector3.zero;
    }

    void UpdateTargetBird()
    {
        if (targetBird == null || Vector3.Distance(transform.position, targetBird.position) > trackingRange)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, trackingRange);
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Bird"))
                {
                    targetBird = collider.transform;
                    break;
                }
            }
        }
    }

    Vector3 TrackTaka()
    {
        targetTaka = null;
        Collider[] colliders = Physics.OverlapSphere(transform.position, trackingRange);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Taka"))
            {
                targetTaka = collider.transform;
                break;
            }
        }

        if (targetTaka != null)
        {
            Vector3 directionAwayFromTaka = (transform.position - targetTaka.position).normalized;
            return directionAwayFromTaka * trackingStrength * 1.5f;
        }
        return Vector3.zero;
    }

    Vector3 LimitSpeed(Vector3 vel)
    {
        float speed = vel.magnitude;

        // TakaとBirdで速度制限を分ける
        if (gameObject.CompareTag("Bird"))
        {
            speed = Mathf.Clamp(speed, birdMinSpeed, birdMaxSpeed);
        }
        else
        {
            speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
        }

        return vel.normalized * speed;
    }

    Vector3 MaintainAltitude()
    {
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

    Vector3 StayWithinBoundary()
    {
        if (centerObject == null)
        {
            Debug.LogWarning("Center object is not assigned.");
            return Vector3.zero;
        }

        Vector3 directionToCenter = centerObject.position - transform.position;
        float distanceFromCenter = directionToCenter.magnitude;

        if (distanceFromCenter > boundaryRadius)
        {
            return directionToCenter.normalized * 5f;
        }

        return Vector3.zero;
    }

    void MoveBoid()
    {
        transform.position += velocity * Time.deltaTime;
        UpdateRotation();
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

    private void OnDrawGizmos()
    {
        if (centerObject != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(centerObject.position, boundaryRadius);
        }

        if (targetBird != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetBird.position);
        }
        else if (targetTaka != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, targetTaka.position);
        }
    }

    Vector3 CalculateDiveTargetPosition(Transform bird)
    {
        // Birdの速度を取得
        Rigidbody birdRigidbody = bird.GetComponent<Rigidbody>();
        Vector3 birdVelocity = birdRigidbody != null ? birdRigidbody.velocity : Vector3.zero;

        // Takaが同じ高さに到達するまでの時間を計算
        float timeToSameAltitude = Mathf.Abs(transform.position.y - bird.position.y) / (maxSpeed * diveSpeedMultiplier);

        // 予測位置を計算
        Vector3 predictedPosition = bird.position + birdVelocity * timeToSameAltitude;

        // 高度をターゲットBirdの現在高度に調整
        predictedPosition.y = bird.position.y;

        return predictedPosition;
    }
}
