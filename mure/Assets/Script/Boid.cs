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

    public List<Boid> allBoids;

    private Vector3 velocity;
    private Vector3 collisionAvoidanceForce;
    private float currentSpeed;

    public float detectionRadius = 5f;
    public float avoidanceRange = 10f;
    private bool isAvoidingTree = false;

    private int frameCounter = 0;
    private const int calculationFrequency = 5;

    void Start()
    {
        currentSpeed = speed;
        velocity = transform.forward * currentSpeed;
        collisionAvoidanceForce = Vector3.zero;
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
        List<Boid> nearbyBoids = GetNearbyBoids();

        Vector3 alignment = Align(nearbyBoids) * 0.5f;
        Vector3 cohesion = Cohere(nearbyBoids) * 1.2f;
        Vector3 separation = Separate(nearbyBoids) * 1.0f;

        Vector3 avoidance = AvoidTaggedObjects("Bird") * 1.5f;
        Vector3 treeAvoidance = DetectAndAvoidTreesInPath() * 2.5f;

        Vector3 force = alignment + cohesion + separation + avoidance + collisionAvoidanceForce + treeAvoidance;
        velocity += force * Time.deltaTime;

        AdjustSpeedAndHeightBasedOnAltitude();

        velocity = velocity.normalized * currentSpeed;
    }

    void MoveBoid()
    {
        Vector3 newPosition = transform.position + velocity * Time.deltaTime;
        transform.position = newPosition;

        UpdateRotation();
        collisionAvoidanceForce = Vector3.zero;
    }

    void AdjustSpeedAndHeightBasedOnAltitude()
    {
        float minAltitude = 15f;
        float maxAltitude = 25f;
        float altitudeAdjustmentStrength = 0.1f;

        if (transform.position.y < minAltitude)
        {
            float altitudeDifference = minAltitude - transform.position.y;
            velocity += Vector3.up * altitudeDifference * altitudeAdjustmentStrength;
        }
        else if (transform.position.y > maxAltitude)
        {
            float altitudeDifference = transform.position.y - maxAltitude;
            velocity += Vector3.down * altitudeDifference * altitudeAdjustmentStrength;
        }
        else
        {
            velocity.y = Mathf.Lerp(velocity.y, 0, Time.deltaTime * 2f);
        }

        currentSpeed = Mathf.Max(currentSpeed, 1f);
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

    List<Boid> GetNearbyBoids()
    {
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
        Vector3 alignmentForce = Vector3.zero;
        foreach (Boid otherBoid in nearbyBoids)
        {
            alignmentForce += otherBoid.velocity;
        }

        if (nearbyBoids.Count > 0)
        {
            alignmentForce /= nearbyBoids.Count;
            alignmentForce = (alignmentForce - velocity).normalized * currentSpeed;
        }

        return alignmentForce;
    }

    Vector3 Cohere(List<Boid> nearbyBoids)
    {
        Vector3 cohesionForce = Vector3.zero;
        foreach (Boid otherBoid in nearbyBoids)
        {
            cohesionForce += otherBoid.transform.position;
        }

        if (nearbyBoids.Count > 0)
        {
            cohesionForce /= nearbyBoids.Count;
            cohesionForce -= transform.position;
            cohesionForce = cohesionForce.normalized * currentSpeed;
        }

        return cohesionForce;
    }

    Vector3 Separate(List<Boid> nearbyBoids)
    {
        Vector3 separationForce = Vector3.zero;
        foreach (Boid otherBoid in nearbyBoids)
        {
            float distance = Vector3.Distance(transform.position, otherBoid.transform.position);
            if (distance < separationDistance)
            {
                separationForce += (transform.position - otherBoid.transform.position).normalized / distance;
            }
        }

        if (nearbyBoids.Count > 0)
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
