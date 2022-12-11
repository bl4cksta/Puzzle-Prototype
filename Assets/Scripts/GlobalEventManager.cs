using UnityEngine;
using UnityEngine.Events;

public class GlobalEventManager
{
    public static UnityEvent OnWin = new UnityEvent();
    public static UnityEvent OnPuzzlePicked = new UnityEvent();
    public static UnityEvent OnLevelStarted = new UnityEvent();
    public static UnityEvent OnGameStarting = new UnityEvent();
    public static UnityEvent OnGameStarted = new UnityEvent();
    public static UnityEvent<int> OnPuzzlesCountChanged = new UnityEvent<int>();

    public static void Win()
    {
        OnWin.Invoke();
    }
    public static void StartLevel() // ��������� �������
    {
        OnLevelStarted.Invoke();
    }
    public static void StartingGame() // ������������ ����
    {
        OnGameStarting.Invoke();
    }
    public static void GameStarted() // �������� ��������
    {
        OnGameStarted.Invoke();
    }
    public static void PickPuzzle() // ���� ���� � ����
    {
        OnPuzzlePicked.Invoke();
    }
    public static void ChangePuzzleCount(int count) // ������������� ���� � ������ �����
    {
        OnPuzzlesCountChanged.Invoke(count);
    }

}
