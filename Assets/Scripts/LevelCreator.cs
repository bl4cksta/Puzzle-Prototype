using DG.Tweening;
using UnityEngine;

public class LevelCreator : MonoBehaviour
{
    [SerializeField] private Transform player;

    [Header("Level Generation\nM - Columns, N - Rows")]
    [SerializeField] private Vector3 startPos;
    [SerializeField] private int M = 4, N = 6;
    [SerializeField] private bool useColor = true;
    [SerializeField] private bool showAssembled = true;
    [SerializeField] private bool isRotationMode = false;

    [Header("Puzzle particles settings\nSprites: 0 - flat, 1 - bump, 2 - default (puzzle with holes), 3 - color")]
    [SerializeField] private float colliderRadius = 0.6f;
    [SerializeField] private float minimizedScale = 0.7f;
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float preshowTimer = 2.5f;

    [Header("Defaults")]
    [SerializeField] private LayerMask puzzleMask;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private Color[] puzzleColors;

    [Header("Scroll bar settings")]
    [SerializeField] private Transform scrollBar;

    [Header("Debug")]
    [SerializeField] private int generatorCount = 81;

    private PuzzleParticle[] puzzles;
    private Vector3[] borders;

    private void Awake()
    {
        GlobalEventManager.OnGameStarting.AddListener(StartLevel);
    }
    private void Init()
    {
        // подгон камеры под картинку
        //if (N == 6) Camera.main.orthographicSize = 5.7f;
        //else if (N >= 7) Camera.main.orthographicSize = 6.4f;

        SpawnPrefabParticles();

        // загрузка уровня сразу 
        var levelStarter = FindObjectOfType<LevelAutoStart>();
        if (levelStarter)
        {
            GlobalEventManager.StartingGame();
            Destroy(levelStarter.gameObject);
        }
    }
    private void SpawnPrefabParticles() // создаём "префабы" из которых будем собирать уровень
    {
        puzzles = new PuzzleParticle[generatorCount];
        int[] newConnections = new int[4];

        for (int i = 0; i < generatorCount; i++)
        {
            var go = new GameObject
            {
                layer = (int)Mathf.Log(puzzleMask.value, 2)
            };
            go.AddComponent<CircleCollider2D>().radius = colliderRadius;
            var puzzle = go.AddComponent<PuzzleParticle>();
            puzzle.SetConnections(newConnections);
            puzzles[i] = puzzle;
            for (int j = 0; j < 4; j++)
            {
                newConnections[j]++;
                if (newConnections[j] < 3) break;
                newConnections[j] = 0;
            }
        }
    }
    private void StartLevel() // создаём уровень
    {
        var spawnVec = startPos;
        var mainCounter = 0;
        int[] up = new int[N];
        int[] right = new int[N];

        int[] upColors = new int[N];
        int[] rightColors = new int[N];
               
        // спавним картину
        for (int i = 0; i < M; i++)
        {
            for (int j = 0; j < N; j++)
            {
                PuzzleParticle particle;
                if (i == 0) // первый ряд 
                {
                    if (j == 0) particle = GetParticleByForm(Random.Range(0, 3), Random.Range(1, 3), Random.Range(0, 3), Random.Range(0, 3));
                    else particle = GetParticleByForm(Random.Range(1, 3), Random.Range(0, 3), 0, right[j - 1]);
                }
                else // остальные
                {
                    if (j == 0) particle = GetParticleByForm(Random.Range(1, 3), Random.Range(0, 3), up[j], Random.Range(0, 3));
                    //else if (j == N - 1) particle = GetParticle(Random.Range(0, 3), 0, up[j], Random.Range(0, 3)); // если крайний в ряду делаем справа flat
                    else particle = GetParticleByForm(Random.Range(1, 3), Random.Range(0, 3), up[j], right[j - 1]);
                }

                if (particle == null)
                {
                    Debug.LogError("ERROR! Can't build level.");
                    continue;
                }

                var particleConnections = particle.GetConnections();

                particle.targetPos = spawnVec;

                up[j] = particleConnections[0];
                right[j] = particleConnections[1];

                if (useColor && puzzleColors != null)
                {
                    if (i == 0)
                    {
                        if (j == 0) SetParticleColor(particle, RandomColor(), RandomColor(), RandomColor(), RandomColor());
                        else SetParticleColor(particle, RandomColor(), RandomColor(), RandomColor(), rightColors[j - 1]);
                    }
                    else
                    {
                        if (j == 0) SetParticleColor(particle, RandomColor(), RandomColor(), upColors[j], RandomColor());
                        else SetParticleColor(particle, RandomColor(), RandomColor(), upColors[j], rightColors[j - 1]);
                    }

                    var particleColors = particle.GetColors();

                    upColors[j] = particleColors[0];
                    rightColors[j] = particleColors[1];

                    SpawnParticle(particle, spawnVec, particleConnections, particleColors);
                }
                else SpawnParticle(particle, spawnVec, particleConnections, null);

                spawnVec += Vector3.right;
                mainCounter++;
            }
            spawnVec.x = startPos.x;
            spawnVec += Vector3.up;
        }

        GlobalEventManager.ChangePuzzleCount(mainCounter - 1);

        // очищаем "префабы"
        foreach (var i in puzzles)
            Destroy(i.gameObject);
        puzzles = null;

        // центруем камеру в середину картины
        var middleCount = N * (M / 2) + (N / 2);
        var middleCountPos = transform.GetChild(middleCount).position;
        if (N % 2 == 0) middleCountPos += (Vector3.left * 0.5f);
        player.position = new Vector3(middleCountPos.x, middleCountPos.y - 1);

        // randomization
        var childCount = transform.childCount;        
        for (int i = 0; i < childCount; i++)
        {
            transform.GetChild(i).SetSiblingIndex(Random.Range(0, childCount));
        }

        // опускаем пазлы в Scroll Bar
        var puzzlesAppeared = 0;
        var counter = 0;
        for (int i = 0; i < childCount; i++)
        {
            var child = transform.GetChild(i);
            child.localScale = Vector3.zero;

            if (i == childCount - 1)
            {
                puzzlesAppeared++;
                child.DOScale(normalScale, 0.5f);
                child.GetComponent<PuzzleParticle>().SetInPlace(true);
                continue;
            }

            if (showAssembled)
            {
                var duration = 1.15f;
                child.SetParent(scrollBar);
                var s = DOTween.Sequence();
                s.Append(child.DOScale(1f, 1f));
                s.Insert(preshowTimer, child.DOLocalMove(new Vector3(0, 0, -0.22f) + (2 * counter * (minimizedScale - 0.15f) * Vector3.right), duration));
                s.Insert(preshowTimer, child.DOScale(minimizedScale, duration));
                if (isRotationMode) s.Insert(preshowTimer, child.DOBlendableRotateBy(new Vector3(0, 0, -90) * Random.Range(0, 5), duration));
            }
            else
            {
                child.SetParent(scrollBar);
                child.localPosition = new Vector3(0, 0, -0.22f) + (2 * counter * (minimizedScale - 0.15f) * Vector3.right);
                child.DOScale(minimizedScale, 0.5f); 
                if (isRotationMode) child.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -90) * Random.Range(0, 5));
            }
            i--;
            childCount--;
            counter++;
        }

        // отправляем разрешение на сбор пазла
        if (showAssembled) Invoke(nameof(GameStarted), preshowTimer);
        else Invoke(nameof(GameStarted), 0.1f);
    }
    private PuzzleParticle GetParticleByForm(int up, int right, int down, int left) // подбираем пазл по форме
    {
        foreach(var i in puzzles)
        {
            var connections = i.GetConnections();

            if (up == 0 && connections[0] != 0) continue;
            if (right == 0 && connections[1] != 0) continue;
            if (down == 0 && connections[2] != 0) continue;
            if (left == 0 && connections[3] != 0) continue;

            if (up == 1 && connections[0] != 2 || up == 2 && connections[0] != 1) continue;
            if (right == 1 && connections[1] != 2 || right == 2 && connections[1] != 1) continue;
            if (down == 1 && connections[2] != 2 || down == 2 && connections[2] != 1) continue;
            if (left == 1 && connections[3] != 2 || left == 2 && connections[3] != 1) continue;

            return i;
        }

        return null;
    }
    private void SetParticleColor(PuzzleParticle particle, int up, int right, int down, int left) // устанавливаем пазлу цвет
    {
        var colors = new int[4];
        colors[0] = up;
        colors[1] = right;
        colors[2] = down;
        colors[3] = left;

        particle.SetColors(colors);
    }

    void SpawnParticle(PuzzleParticle particle, Vector3 spawnVec, int[] connections, int[] colors) // спавним пазл
    {
        // spawn main GameObject
        var go = Instantiate(particle, spawnVec, Quaternion.identity, transform);
        go.gameObject.AddComponent<SpriteRenderer>().sprite = sprites[2];

        for (int i = 0; i < 4; i++)
        {
            if (connections[i] != 2)
            {
                var spriteGO = new GameObject();
                spriteGO.transform.SetParent(go.transform);
                spriteGO.transform.localPosition = Vector3.zero;
                spriteGO.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -90) * i);

                var sprite = spriteGO.AddComponent<SpriteRenderer>();
                sprite.sprite = sprites[connections[i]];
                sprite.sortingOrder = 1;

                if (colors != null && connections[i] == 1) sprite.color = puzzleColors[colors[i]];
            }

            if (colors == null) continue;
            if (connections[i] == 0) continue;

            var colorGO = new GameObject();
            colorGO.transform.SetParent(go.transform);
            colorGO.transform.localPosition = Vector3.zero;
            colorGO.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -90) * i);

            var colorSprite = colorGO.AddComponent<SpriteRenderer>();
            colorSprite.sprite = sprites[3];
            colorSprite.sortingOrder = 1;
            colorSprite.color = puzzleColors[colors[i]];

        }
    }
    private int RandomColor()
    {
        return Random.Range(0, puzzleColors.Length);
    }
    private void GameStarted()
    {
        GlobalEventManager.GameStarted();
    }

    public void SetupLevel(int m, int n, bool color, bool show, bool hardcore, Transform pl, Transform bar) // устанавливаем defaults 
    {
        M = m;
        N = n;
        useColor = color;
        showAssembled = show;
        isRotationMode = hardcore;
        player = pl;
        scrollBar = bar;
        borders = new Vector3[2];
        Init();
    }
    // debug
    public void SetupRotationMode(bool mode)
    {
        isRotationMode = mode;
    }
    public void SetupPreshowMode(bool mode)
    {
        showAssembled = mode;
    }
    public void SetupColorMode(bool mode)
    {
        useColor = mode;
    }
}
