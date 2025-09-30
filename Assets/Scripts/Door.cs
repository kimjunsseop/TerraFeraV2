using UnityEngine;

public class Door : MonoBehaviour
{
    public Transform visual;                         // 문 시각 요소 (MeshRenderer)
    public float slideDistance = 1.0f;               // 옆으로 이동 거리
    public float slideSpeed = 2f;                    // 이동 속도
    public float openDuration = 2f;                  // 열린 채 유지 시간

    private Vector3 closedLocalPos;
    private Vector3 openLocalPos;
    private Vector3 slideDir = Vector3.right;        // 설치 방향 기준 슬라이딩 방향
    private bool isOpening = false;
    private bool isReturning = false;
    private float timer = 0f;

    private void Start()
    {
        if (visual == null)
        {
            Debug.LogError("[SlidingDoor] visual 오브젝트가 연결되지 않았습니다!");
            return;
        }

        //closedLocalPos = visual.localPosition;
        //openLocalPos = closedLocalPos + slideDir * slideDistance;
    }

    private void Update()
    {
        if (isOpening)
        {
            visual.localPosition = Vector3.MoveTowards(visual.localPosition, openLocalPos, Time.deltaTime * slideSpeed);
            if (Vector3.Distance(visual.localPosition, openLocalPos) < 0.01f)
            {
                visual.localPosition = openLocalPos;
                isOpening = false;
                timer = openDuration;
            }
        }
        else if (!isReturning && timer > 0f)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                isReturning = true;
            }
        }
        else if (isReturning)
        {
            visual.localPosition = Vector3.MoveTowards(visual.localPosition, closedLocalPos, Time.deltaTime * slideSpeed);
            if (Vector3.Distance(visual.localPosition, closedLocalPos) < 0.01f)
            {
                visual.localPosition = closedLocalPos;
                isReturning = false;
            }
        }
    }

    public void Interact()
    {
        if (!isOpening && !isReturning)
        {
            isOpening = true;
        }
    }

    // 설치 방향 기준 오른쪽으로 slideDir 설정
    public void SetInstallDirection(Vector3 forward)
    {
        forward.y = 0f;
        forward.Normalize();
        slideDir = Vector3.Cross(Vector3.up, forward).normalized; // 오른쪽 방향
        if (visual != null)
        {
            closedLocalPos = visual.localPosition;
            openLocalPos = closedLocalPos + slideDir * slideDistance;
        }
    }
    public Vector3 GetInstallForward()
    {
        return Vector3.Cross(slideDir, Vector3.up).normalized;
    }
}
