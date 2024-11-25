using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    private float speed = 5f;
    private float rotationSpeed = 2f;
    private float neighborRadius = 15f;
    private float separationDistance = 6f;
    private float separationStrength = 3.0f;
    private float maxSpeed = 6f;
    private float minSpeed = 2f;

    public float treeAvoidanceRange = 10f;
    private float treeAvoidanceStrength = 10f;

    public List<Boid> allBoids;

    private Vector3 velocity;

    private int frameCounter = 0;
    private const int calculationFrequency = 5;

    private float minAltitude = 15f;
    private float maxAltitude = 25f;
    private float altitudeAdjustmentStrength = 2f;

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
        List<Boid> nearbyBoids = GetNearbyBoids();

        Vector3 alignment = Align(nearbyBoids) * 0.5f;
        Vector3 cohesion = Cohere(nearbyBoids) * 1.0f;
        Vector3 separation = Separate(nearbyBoids) * separationStrength;

        Vector3 treeAvoidance = DetectAndAvoidTreesInPath() * 2.5f;

        Vector3 altitudeAdjustment = MaintainAltitude();

        Vector3 force = alignment + cohesion + separation + treeAvoidance + altitudeAdjustment;
        velocity += force * Time.deltaTime;

        velocity = LimitSpeed(velocity);
    }

    void MoveBoid()
    {
        Vector3 newPosition = transform.position + velocity * Time.deltaTime;
        transform.position = newPosition;

        UpdateRotation();
    }

    Vector3 LimitSpeed(Vector3 vel)
    {
        float speed = vel.magnitude;
        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
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
            alignmentForce = alignmentForce.normalized * speed;
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
            cohesionForce = cohesionForce.normalized * speed;
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

        return separationForce.normalized * separationStrength;
    }

    Vector3 DetectAndAvoidTreesInPath()
    {
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
        Vector3 direction = velocity.normalized;
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}
