using UnityEngine;

public class LevelsManager : MonoBehaviour
{
    [Header("Defaults")]
    [SerializeField] private LevelCreator levelPrefab;
    [SerializeField] private LevelAutoStart levelStarter;
    [SerializeField] private Transform player;
    [SerializeField] private Transform scrollBar;
    //[SerializeField] private Transform[] borders;
    [Header("\nShow assembled puzzle in start of game")]
    [SerializeField] private bool showAssembled;
    [Header("Colors used or just form-factor")]
    [SerializeField] private bool[] useColor;
    [Header("Rows for each level")]
    [SerializeField] private int[] rows;
    [Header("Columns for each level")]
    [SerializeField] private int[] columns;

    [Header("> > > OR < < <\n\nUse simple level progression")]
    [SerializeField] private bool useProgression = false;

    [Header("Debug set level")]
    [SerializeField] private int level;

    private int levelId, mainCounter, currentCounter;

    private void Awake()
    {
        GlobalEventManager.OnPuzzlesCountChanged.AddListener(SetCounter);
        GlobalEventManager.OnWin.AddListener(Win);
    }
    private void Start()
    {
        //Debug.Log("Start!");
        //DontDestroyOnLoad(gameObject);
        StartLevel();
    }
    void StartLevel()
    {
        levelId = PlayerPrefs.GetInt("Level", 0);

#if UNITY_EDITOR
        if(level != 0) levelId = level;
#endif
        var curLevel = Instantiate(levelPrefab);

        if(useProgression || columns == null || rows == null || useColor == null)
        {
            var col = 2;
            var row = 2;

            for (int i = 0; i < levelId; i++)
            {
                if (i % 2 == 0) col++;
                else row++;
            }
            
            curLevel.SetupLevel(Mathf.Clamp(col, 2, 8), Mathf.Clamp(row, 2, 6), true, showAssembled, player, scrollBar); // , borders
        }
        else
        {
            curLevel.SetupLevel(columns[Mathf.Clamp(levelId, 0, columns.Length)], rows[Mathf.Clamp(levelId, 0, rows.Length)],
                useColor[Mathf.Clamp(levelId, 0, useColor.Length)], showAssembled, player, scrollBar); // , borders
        }
    }
    void SetCounter(int count)
    {
        if (mainCounter == 0) mainCounter = (count + 1);
        else currentCounter++;

        if (currentCounter < mainCounter) return;

        GlobalEventManager.Win();

    }
    void Win()
    {
        PlayerPrefs.SetInt("Level", levelId + 1);
        Instantiate(levelStarter);
    }
}
