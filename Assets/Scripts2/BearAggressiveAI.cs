using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BearAggressiveAI : MonoBehaviour
{
    [Header("Bear Settings")]
    public float normalMoveRange = 50f;
    public float chaseMoveRange = 30f;
    public float wanderDelay = 4f;
    public float attackDistance = 2.5f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;
    private DayNightCycle dayNightCycle;
    private Transform denCenter;
    private float lastWanderTime;
    private bool isChasing = false;
    private bool isAttacking = false;
    private float lastAttackTime = 0f;
    private Vector3 lastPlayerPosition = Vector3.zero;

    public int health = 5;
    public GameObject[] dropItemPrefabs;
    private Vector3 dropPoint;
    public float dropForce;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player")?.transform;
        dayNightCycle = FindObjectOfType<DayNightCycle>();
        lastWanderTime = Time.time;
    }

    public void SetDenCenter(Transform center)
    {
        denCenter = center;
        SetNewDestination();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("bear hit! 남은체력: " + health);

        if (health <= 0)
        {
            DropItem();
            Destroy(gameObject);
        }
    }

    void DropItem()
    {
        if (dropItemPrefabs == null || dropItemPrefabs.Length == 0)
        {
            return;
        }
        for (int i = 0; i < dropItemPrefabs.Length; i++)
        {
            GameObject prefab = dropItemPrefabs[i];
            if (prefab == null)
            {
                continue;
            }
            Vector3 spawnPos = dropPoint + Random.insideUnitSphere * 0.5f;
            spawnPos.y = dropPoint.y;
            GameObject droppedItem = Instantiate(prefab, spawnPos, Quaternion.identity);
            Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-1f, 1f)).normalized;
                rb.AddForce(randomDirection * dropForce, ForceMode.Impulse);
            }
        }
    }

    void Update()
    {
        dropPoint = this.transform.position;

        if (player == null || dayNightCycle == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (isChasing)
        {
            if (distanceToPlayer > chaseMoveRange)
            {
                isChasing = false;
                agent.speed = 3.5f;
                SetNewDestination();
                lastWanderTime = Time.time;
                agent.isStopped = false;
            }
            else
            {
                if (distanceToPlayer <= attackDistance)
                {
                    agent.isStopped = true;

                    if ((player.position - lastPlayerPosition).sqrMagnitude > 0.01f)
                    {
                        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
                        lastPlayerPosition = player.position;
                    }

                    if (!isAttacking && Time.time - lastAttackTime >= attackCooldown)
                    {
                        isAttacking = true;
                        StartCoroutine(Attack());
                        lastAttackTime = Time.time;
                    }
                }
                else
                {
                    NavMeshPath path = new NavMeshPath();
                    bool hasPath = NavMesh.CalculatePath(transform.position, player.position, NavMesh.AllAreas, path);

                    if (!hasPath || path.status != NavMeshPathStatus.PathComplete)
                    {
                        Building blockingBuilding = TryAttackBlockingBuilding();
                        if (blockingBuilding != null)
                        {
                            agent.isStopped = true;
                            transform.LookAt(blockingBuilding.transform);
                            if (!isAttacking && Time.time - lastAttackTime >= attackCooldown)
                            {
                                isAttacking = true;
                                StartCoroutine(Attack());
                                lastAttackTime = Time.time;
                            }
                        }
                        else
                        {
                            agent.isStopped = true;
                        }
                    }
                    else
                    {
                        agent.isStopped = false;
                        agent.SetDestination(player.position);
                    }
                
                }
            }
        }
        else
        {
            if (distanceToPlayer <= chaseMoveRange)
            {
                isChasing = true;
                agent.speed = 6f;
                lastPlayerPosition = player.position;
            }
            else
            {
                if (!isAttacking && (Time.time - lastWanderTime > wanderDelay || agent.remainingDistance < 1f))
                {
                    SetNewDestination();
                    lastWanderTime = Time.time;
                }
            }
        }

        animator.SetFloat("Speed", agent.velocity.magnitude);
        animator.SetBool("IsChasing", isChasing);
    }

    Building TryAttackBlockingBuilding()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, attackDistance, LayerMask.GetMask("building"));
        foreach (Collider col in colliders)
        {
            var building = col.GetComponent<Building>();
            if (building != null)
            {
                return building;
            }
        }
        return null;
    }

    void SetNewDestination()
    {
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * normalMoveRange;
        randomPoint.y = transform.position.y;

        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    IEnumerator Attack()
    {
        animator.SetBool("IsAttacking", true);

        yield return new WaitForSeconds(0.3f); // 공격 준비 시간

        bool didAttack = false;

        // 플레이어가 공격 범위 안에 있으면 우선 공격
        if (Vector3.Distance(transform.position, player.position) <= attackDistance)
        {
            player.GetComponent<player>()?.TakeDamage(attackDamage);
            Debug.Log(" Bear attacked player for " + attackDamage + " damage.");
            didAttack = true;
        }

        // 플레이어가 사거리 밖이면 건물 공격 시도
        if (!didAttack)
        {
            Building building = TryAttackBlockingBuilding();
            Debug.Log("곰이 건물 찾는중");
            if (building != null)
            {
                transform.LookAt(building.transform);
                building.TakeDamage(attackDamage);
                Debug.Log($"🏚️ Bear attacked building: {building.name} for {attackDamage} damage.");
            }
        }

        yield return new WaitForSeconds(0.5f); // 후딜

        animator.SetBool("IsAttacking", false);
        isAttacking = false;

        float dist = Vector3.Distance(transform.position, player.position);
        agent.isStopped = dist <= attackDistance;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, normalMoveRange);

        Gizmos.color = new Color(0f, 0f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, chaseMoveRange);

        NavMeshAgent currentAgent = GetComponent<NavMeshAgent>();
        if (currentAgent != null && currentAgent.hasPath)
        {
            Gizmos.color = Color.green;
            Vector3[] pathPoints = currentAgent.path.corners;
            for (int i = 0; i < pathPoints.Length - 1; i++)
            {
                Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
            }
        }
    }
}


