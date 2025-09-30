using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;

public class network : MonoBehaviour
{
    private TcpClient socket; // 서버와의 TCP 연결을 관리하는 소켓
    private NetworkStream stream; // 소켓을 통해 데이터를 송수신하는 스트림
    private byte[] buffer = new byte[1024]; // 데이터를 받기 위한 버퍼

    public GameObject ballPrefab; // 내 공 프리팹 (게임 오브젝트)
    private GameObject ball; // 내 공 (동적으로 생성된 게임 오브젝트)

    public GameObject opponentBallPrefab; // 상대 공 프리팹
    private GameObject opponentBall; // 상대 공 (동적으로 생성된 게임 오브젝트)

    void Start()
    {
        Application.targetFrameRate = 60; // 게임의 목표 프레임 레이트 설정 (60 FPS)
        
        // 내 공을 처음 생성할 때, Y값이 10인 위치에 공을 생성
        Vector3 spawnPosition = new Vector3(0, 10f, 0);
        ball = Instantiate(ballPrefab, spawnPosition, Quaternion.identity); // 공 생성

        // 서버와 연결을 시도
        ConnectToServer("192.168.0.114", 3600); // 서버 IP와 포트 설정
    }

    // 서버에 연결하는 함수
    void ConnectToServer(string ip, int port)
    {
        try
        {
            // 서버에 접속 (IP와 포트 번호)
            socket = new TcpClient(ip, port);
            stream = socket.GetStream(); // 네트워크 스트림을 얻음
            new Thread(ListenForData).Start(); // 데이터 수신을 위한 별도의 스레드를 시작
        }
        catch (Exception e)
        {
            Debug.Log("Connection error: " + e.Message); // 연결 에러 처리
        }
    }

    // 서버에서 데이터를 받는 함수
    void ListenForData()
    {
        while (true)
        {
            try
            {
                // 서버에서 받은 데이터를 읽음
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string data = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim(); // 데이터 처리
                    Debug.Log("Received from server: " + data);

                    // 데이터가 좌표 형식을 포함하고 있다면
                    if (data.Contains("(") && data.Contains(")"))
                    {
                        Vector3 pos = ParsePosition(data); // 좌표 파싱
                        UpdateOpponentPosition(pos); // 상대 공 위치 업데이트
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error receiving data: " + e.Message); // 수신 에러 처리
                break; // 에러 발생 시 반복문 종료
            }
        }
    }

    // 상대방 공의 위치를 업데이트하는 함수
    void UpdateOpponentPosition(Vector3 pos)
    {
        if (opponentBall == null)
        {
            // 상대 공이 없다면 생성
            opponentBall = Instantiate(opponentBallPrefab, pos, Quaternion.identity);
        }
        else
        {
            // 공이 있다면 부드럽게 이동 (Lerp 사용)
            StartCoroutine(SmoothMove(opponentBall.transform, pos));
        }
    }

    // 공을 부드럽게 이동시키는 코루틴
    IEnumerator SmoothMove(Transform obj, Vector3 targetPos)
    {
        float elapsedTime = 0f; // 경과 시간
        float duration = 0.1f; // 이동 시간 (네트워크 속도에 맞게 조절 가능)

        Vector3 startPos = obj.position; // 시작 위치
        while (elapsedTime < duration)
        {
            // Lerp를 사용하여 부드럽게 이동
            obj.position = Vector3.Lerp(startPos, targetPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime; // 시간 갱신
            yield return null; // 다음 프레임까지 대기
        }
        obj.position = targetPos; // 정확한 목표 위치로 이동
    }

    // 서버에서 받은 위치 데이터를 파싱하는 함수
    Vector3 ParsePosition(string data)
    {
        string[] parts = data.Split('(')[1].Split(')')[0].Split(','); // 데이터에서 좌표 추출
        return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
    }

    // 매 프레임마다 공의 위치를 서버로 전송하는 함수
    void Update()
    {
        if (socket != null && stream != null && ball != null)
        {
            // 공의 위치 데이터를 서버로 전송
            string positionData = $"{ball.transform.position}";
            byte[] data = Encoding.UTF8.GetBytes(positionData + "\n"); // 데이터를 UTF-8 형식으로 변환
            stream.Write(data, 0, data.Length); // 서버로 전송
        }
    }
}
