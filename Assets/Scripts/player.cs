using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Unity.VisualScripting;
using TMPro;

[System.Serializable]
public class Condition
{
    public float curValue;
    public float maxValue;
    public float startValue;
    public float regenRate;
    public float decayRate;
    public Image uiBar;
    public TextMeshProUGUI textBar;

    public void Add(float amount)
    {
        curValue = Mathf.Min(curValue + amount, maxValue);
    }

    public void Subtract(float amount)
    {
        curValue = Mathf.Max(curValue - amount, 0.0f);
    }

    public float GetPercentage()
    {
        return curValue / maxValue;
    }
}

public class player : MonoBehaviour
{
    public Condition health;
    public Condition hunger;
    public Condition temperature;

    float vAxis;
    float hAxis;
    public float moveSpeed = 10f;
    public float aroundSpeed = 20f;
    Rigidbody rig;
    Animator anim;
    Vector3 move = Vector3.zero;
    bool isAttacking = false;

    public int attackDamage = 1;
    public int woodAttack = 1;
    public int stoneAttack = 1;
    public int basicAttackDamage = 1;
    public TextMeshProUGUI animalAttackUI;
    public TextMeshProUGUI woodAttackUI;
    public TextMeshProUGUI stoneAttackUI;
    public float attackRange = 2f;
    public LayerMask targetLayer;
    public GameObject keyoptionUI;

    public PlayerInventory inventory;
    // 시간을 알기위한 변수
    public Light directionalLight;
    private DayNightCycle dayNightCycle;
    [Header("Sound")]
    public AudioClip walkingSound;
    public AudioClip treeHitSound;
    public AudioClip stoneHitSound;
    public AudioClip getSound;
    public AudioClip equipSound;
    public AudioClip makeSound;
    public AudioClip buildingSound;
    private bool isWalkingSound = false;

    void Start()
    {
        rig = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        inventory = GetComponent<PlayerInventory>();
        dayNightCycle = directionalLight.GetComponent<DayNightCycle>();

        attackDamage = basicAttackDamage;

        // 초기 Condition 세팅
        health.startValue = 100f;
        hunger.startValue = 100f;
        temperature.startValue = 100f;

        health.curValue = health.startValue;
        hunger.curValue = hunger.startValue;
        temperature.curValue = temperature.startValue;
        keyoptionUI.gameObject.SetActive(false);
    }

    void Update()
    {
        // 입력 처리
        vAxis = Input.GetAxisRaw("Vertical");
        hAxis = Input.GetAxisRaw("Horizontal");
        move = new Vector3(hAxis, 0, vAxis).normalized;
        bool isMoving = move.magnitude > 0.01f && !isAttacking && !inventory.isInventoryOpen;
        if (isMoving)
        {
            if (!isWalkingSound)
            {
                AudioManager.Instance.sfxSource.clip = walkingSound;
                AudioManager.Instance.sfxSource.loop = true;
                AudioManager.Instance.sfxSource.Play();
                isWalkingSound = true;
            }
            Quaternion targetRot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * aroundSpeed);
        }
        else
        {
            if (isWalkingSound)
            {
                AudioManager.Instance.sfxSource.Stop();
                isWalkingSound = false;
            }
        }
        anim.SetBool("isMove", isMoving);
        // 공격 입력
        if (Input.GetMouseButtonDown(0) && !isAttacking && !inventory.isInventoryOpen && !inventory.IsHoldingBuilding())
        {
            StartCoroutine(Attack());
        }

        // 상태바 업데이트
        hunger.Subtract(hunger.decayRate * Time.deltaTime);
        if (hunger.curValue <= 0)
        {
            health.Subtract(health.decayRate * Time.deltaTime);
        }
        if (temperature.curValue <= 0 && dayNightCycle.isNight)
        {
            health.Subtract(health.decayRate * Time.deltaTime);
        }
        if (dayNightCycle != null)
        {
            bool nearFire = false;
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 3f, LayerMask.GetMask("Fire"));
            foreach (var col in hitColliders)
            {
                if (col.GetComponent<Fire>() != null)
                {
                    nearFire = true;
                    break;
                }
            }
            if (!nearFire && dayNightCycle.isNight)
            {
                temperature.Subtract(temperature.decayRate * Time.deltaTime);
            }
            if (nearFire)
            {
                temperature.Add(0.5f * Time.deltaTime);
            }
        }
        health.uiBar.fillAmount = health.GetPercentage();
        hunger.uiBar.fillAmount = hunger.GetPercentage();
        temperature.uiBar.fillAmount = temperature.GetPercentage();
        health.textBar.text = $"HP : {health.curValue: 0} / {health.maxValue: 0}";
        hunger.textBar.text = $"Hungry : {hunger.curValue: 0} / {hunger.maxValue: 0}";
        temperature.textBar.text = $"Temperature : {temperature.curValue: 0} / {temperature.maxValue: 0}";
        animalAttackUI.text = $" : {attackDamage}";
        stoneAttackUI.text = $" : {stoneAttack}";
        woodAttackUI.text = $" : {woodAttack}";
        if (Input.GetKey(KeyCode.P))
        {
            keyoptionUI.SetActive(true);
        }
        else
        {
            keyoptionUI.SetActive(false);
        }
    }

    void FixedUpdate()
    {
        if (!isAttacking)
        {
            Vector3 moveDelta = move * moveSpeed * Time.fixedDeltaTime;
            rig.MovePosition(rig.position + moveDelta);
        }
    }

    IEnumerator Attack()
    {
        if (isAttacking)
        {
            yield break;
        }
        isAttacking = true;
        anim.SetBool("isAttack", true);

        // 공격 애니메이션 길이만큼 대기
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

        PerformAttack();
        anim.SetBool("isAttack", false);
        isAttacking = false;
    }

    void PerformAttack()
    {
        float coneAngle = 60f;       // 시야각
        float radius = attackRange;  // 범위

        Collider[] hits = Physics.OverlapSphere(transform.position, radius, targetLayer);
        Transform closestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (var hit in hits)
        {
            Vector3 directionToTarget = (hit.transform.position - transform.position).normalized;

            // 시야각 내에 있는지 확인
            float angle = Vector3.Angle(transform.forward, directionToTarget);
            if (angle < coneAngle)
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = hit.transform;
                }
            }
        }

        if (closestTarget != null)
        {
            // 가장 가까운 대상에 대해 공격 처리
            var tree = closestTarget.GetComponent<Tree>();
            if (tree != null)
            {
                tree.TakeDamage(woodAttack);
                AudioManager.Instance.PlaySFX(treeHitSound);
                TargetHealthDisplay.Instance.ShowHealth(tree.health, 5);
                return;
            }

            var stone = closestTarget.GetComponent<Stone>();
            if (stone != null)
            {
                stone.TakeDamage(stoneAttack);
                AudioManager.Instance.PlaySFX(stoneHitSound);
                TargetHealthDisplay.Instance.ShowHealth(stone.health, 5);
                return;
            }

            var bear = closestTarget.GetComponent<BearAggressiveAI>();
            if (bear != null)
            {
                bear.TakeDamage(attackDamage);
                TargetHealthDisplay.Instance.ShowHealth(bear.health, 5);
                return;
            }

            var tigger = closestTarget.GetComponent<AggressiveAnimalAI>();
            if (tigger != null)
            {
                tigger.TakeDamage(attackDamage);
                TargetHealthDisplay.Instance.ShowHealth(tigger.health, 5);
                return;
            }

            var deer = closestTarget.GetComponent<PassiveAnimalAI>();
            if (deer != null)
            {
                deer.TakeDamage(attackDamage);
                TargetHealthDisplay.Instance.ShowHealth(deer.health, 5);
                return;
            }
        }
        else
        {
            Debug.Log("공격 범위 내 타겟 없음");
        }
    }

    public void ToggleCursor(bool toggle)
    {
        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void TakeDamage(float damage)
    {
        health.Subtract(damage);
    }
}
