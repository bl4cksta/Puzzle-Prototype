using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI counterText, levelText;
    [SerializeField] private GameObject gameScreen, menuScreen, winScreen;
    [SerializeField] private Vector2 winScreenHiddenPoint;

    private int mainCounter;
    private int currentCounter;
    private void Awake()
    {
        GlobalEventManager.OnPuzzlesCountChanged.AddListener(SetCounter);
        GlobalEventManager.OnWin.AddListener(Win);
        GlobalEventManager.OnGameStarting.AddListener(StartGame);

        gameScreen.SetActive(false);
        winScreen.SetActive(false);
        menuScreen.SetActive(true);
    }
    private void Start()
    {
        var curLevel = PlayerPrefs.GetInt("Level", 0) + 1;
        levelText.text = "LEVEL " + curLevel;
    }

    void SetCounter(int count) // ставим пазл в нужное место, обновляем счётчик
    {
        if (mainCounter == 0) mainCounter = (count + 1);
        else currentCounter++;

        if (currentCounter > mainCounter) return;

        counterText.text = currentCounter + "/" + mainCounter;
    }
    void Win()
    {
        gameScreen.SetActive(false);
        menuScreen.SetActive(false);

        var winEndPoint = winScreen.transform.position;
        winScreen.transform.localPosition = winScreenHiddenPoint;
        winScreen.transform.DOMove(winEndPoint, 1f);
        winScreen.SetActive(true);
    }
    void StartGame()
    {
        menuScreen.SetActive(false);
        winScreen.SetActive(false);
        gameScreen.SetActive(true);
    }

    public void BtnStartGame()
    {
        GlobalEventManager.StartingGame();
    }
    public void BtnClaimWin()
    {
        SceneManager.LoadScene(0);
    }
}
