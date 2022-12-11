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
    public static void StartLevel() // загружаем уровень
    {
        OnLevelStarted.Invoke();
    }
    public static void StartingGame() // раскладываем пазл
    {
        OnGameStarting.Invoke();
    }
    public static void GameStarted() // начинаем собирать
    {
        OnGameStarted.Invoke();
    }
    public static void PickPuzzle() // берём пазл в руку
    {
        OnPuzzlePicked.Invoke();
    }
    public static void ChangePuzzleCount(int count) // устанавливаем пазл в нужное место
    {
        OnPuzzlesCountChanged.Invoke(count);
    }

}
