using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AggressiveAnimalAI : MonoBehaviour
{
    [Header("Wander Settings")]
    private Transform denCenter;
    public float normalMoveRange = 30f;
    public float nightMoveRange = 50f;
    public float wanderDelay = 4f;

    [Header("Detection Settings")]
    public float normalDetectionRange = 15f;
    public float nightDetectionRange = 30f;
    public float normalChaseRange = 25f;
    public float nightChaseRange = 45f;

    [Header("Attack Settings")]
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public float attackDamage = 10f;

    private NavMeshAgent agent;
    private Transform player;
    private DayNightCycle dayNightCycle;
    private float lastWanderTime;
    private bool isChasing = false;
    private float lastAttackTime = 0f;
    private Vector3 tempWanderCenter;

    // �߰��κ�
    public int health = 5;
    public GameObject dropItemPrefab;
    private Vector3 dropPoint;
    public float dropForce;


    // �߰� �κ�
    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("tigger hit! ����ü��: " + health);

        if (health <= 0)
        {
            DropItem();
            Destroy(gameObject);
        }
    }
    // �߰� �κ�
    void DropItem()
    {
        if (dropItemPrefab != null)
        {
            // �������� ����
            GameObject droppedItem = Instantiate(dropItemPrefab, dropPoint, Quaternion.identity);

            // Rigidbody�� ������ ƨ�ܳ����� ����
            Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Ʈ������ �ణ �������� ƨ�ܳ����� ��
                Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-1f, 1f)).normalized;
                rb.AddForce(randomDirection * dropForce, ForceMode.Impulse);
            }
        }
    }
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player")?.transform;
        dayNightCycle = FindObjectOfType<DayNightCycle>();

        lastWanderTime = Time.time;
        SetNewDestination();

        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.SetBool("isGround", true);
    }

    void Update()
    {
        // �߰� �κ�
        dropPoint = this.transform.position;

        if (player == null || dayNightCycle == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        float detectionRange = dayNightCycle.isNight ? nightDetectionRange : normalDetectionRange;
        float chaseRange = dayNightCycle.isNight ? nightChaseRange : normalChaseRange;

        if (isChasing)
        {
            if (distanceToPlayer > chaseRange)
            {
                isChasing = false;
                lastWanderTime = Time.time;
                tempWanderCenter = dayNightCycle.isNight ? transform.position : Vector3.zero;
                SetNewDestination();
            }
            else
            {
                agent.SetDestination(player.position);

                if (distanceToPlayer <= attackRange && Time.time - lastAttackTime >= attackCooldown)
                {
                    AttackPlayer();
                    lastAttackTime = Time.time;
                }
            }
        }
        else
        {
            if (distanceToPlayer <= detectionRange)
            {
                isChasing = true;
            }
            else
            {
                if (Time.time - lastWanderTime > wanderDelay || agent.remainingDistance < 1f)
                {
                    SetNewDestination();
                    lastWanderTime = Time.time;
                }
            }
        }
    }

    void AttackPlayer()
    {
        player.GetComponent<player>()?.TakeDamage(attackDamage);
        Debug.Log("Animal attacked player for " + attackDamage + " damage.");
    }

    void SetNewDestination()
    {
        Vector3 center = (tempWanderCenter != Vector3.zero) ? tempWanderCenter : (denCenter != null ? denCenter.position : transform.position);
        float moveRange = dayNightCycle.isNight ? nightMoveRange : normalMoveRange;

        Vector3 dest = GetRandomPointInRange(center, moveRange);
        if (NavMesh.SamplePosition(dest, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    Vector3 GetRandomPointInRange(Vector3 center, float range)
    {
        Vector2 offset = Random.insideUnitCircle * range;
        return center + new Vector3(offset.x, 0, offset.y);
    }

    public void SetDenCenter(Transform center)
    {
        denCenter = center;
    }

    void OnDrawGizmosSelected()
    {
        if (denCenter == null) return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.8f);
        Gizmos.DrawWireSphere(denCenter.position, normalMoveRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, normalDetectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, normalChaseRange);
    }

    void OnDestroy()
    {
        if (AggressiveAnimalManager.Instance != null)
        {
            AggressiveAnimalManager.Instance.UnregisterAnimal();
        }
    }
}
