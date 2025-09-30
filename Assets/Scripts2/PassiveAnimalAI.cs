using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class PassiveAnimalAI : MonoBehaviour
{
    public float wanderRange = 60f;
    public float wanderMoveDuration = 3f;
    public float wanderPauseDuration = 5f;
    public float fleeSpeed = 8f;
    public float fleeDuration = 4f;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;
    private Vector3 spawnCenter;

    private bool isFleeing = false;
    private float wanderTimer = 0f;
    private bool isMoving = false;

    public int health = 3;
    public GameObject dropItemPrefab;
    public float dropForce = 3f;
    private PassiveAnimalDen myDen;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player")?.transform;
        StartCoroutine(WanderRoutine());
    }

    void Update()
    {
        if (agent != null && animator != null)
        {
            float currentSpeed = agent.velocity.magnitude;
            animator.SetFloat("Speed", currentSpeed);
        }
    }

    public void SetDen(PassiveAnimalDen den)
    {
        myDen = den;
        spawnCenter = den.transform.position;
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("[PassiveAnimalAI] TakeDamage called");

        health -= damage;
        Debug.Log("Passive animal hit: HP = " + health);

        if (health <= 0)
        {
            DropItem();
            myDen?.UnregisterAnimal();
            Destroy(gameObject);
        }
        else
        {
            if (!isFleeing)
            {
                Debug.Log("[PassiveAnimalAI] Start fleeing");
                StartCoroutine(FleeFromPlayer());
            }
        }
    }

    IEnumerator FleeFromPlayer()
    {
        isFleeing = true;
        agent.speed = fleeSpeed;

        Vector2 randomDir2D = Random.insideUnitCircle.normalized;
        Vector3 randomDir = new Vector3(randomDir2D.x, 0, randomDir2D.y);
        Vector3 fleeTarget = transform.position + randomDir * 10f;

        Debug.Log($"도망 방향: {randomDir}, 목표 위치: {fleeTarget}");

        if (NavMesh.SamplePosition(fleeTarget, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            Debug.Log($"도망 경로 설정됨: {hit.position}");
        }
        else
        {
            Debug.LogWarning("도망 NavMesh 위치 찾기 실패");
            isFleeing = false;
            yield break;
        }

        yield return new WaitForSeconds(fleeDuration);

        agent.speed = 3.5f; // 걷기 속도로 복귀
        isFleeing = false;
    }

    IEnumerator WanderRoutine()
    {
        while (true)
        {
            if (!isFleeing)
            {
                isMoving = true;
                Vector3 destination = GetRandomWanderPoint();

                if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }

                yield return new WaitForSeconds(wanderMoveDuration);
                agent.ResetPath();
                isMoving = false;

                yield return new WaitForSeconds(wanderPauseDuration);
            }
            else
            {
                yield return null;
            }
        }
    }

    Vector3 GetRandomWanderPoint()
    {
        Vector2 randomOffset = Random.insideUnitCircle * wanderRange;
        return spawnCenter + new Vector3(randomOffset.x, 0, randomOffset.y);
    }

    void DropItem()
    {
        if (dropItemPrefab != null)
        {
            GameObject dropped = Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
            Rigidbody rb = dropped.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(Vector3.up * dropForce, ForceMode.Impulse);
            }
        }
    }
}
