using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleParticle : MonoBehaviour
{
    public Vector3 targetPos;

    [Header("Up - Right - Down - Left;\n0 - empty, 1 - bump, 2 - hole")]
    [SerializeField] private int[] connections;
    [SerializeField] private int[] colors;
    [SerializeField] private float magnetRange = 0.25f;

    private List<PuzzleParticle> connectedParticles;
    private bool isDragging;
    private bool isInPlace;
    private bool isAlreadyPicked;
    private Vector3 offset;
    private Collider2D touchCollider;
    private Sequence currentSequence;
    private void Awake()
    {
        GlobalEventManager.OnGameStarted.AddListener(StartGame);
        GlobalEventManager.OnWin.AddListener(Win);
    }
    private void Start()
    {
        isAlreadyPicked = false;
        isDragging = false;
        connectedParticles = new List<PuzzleParticle>();
        touchCollider = GetComponent<CircleCollider2D>();
        touchCollider.enabled = false;
    }

    private void OnMouseDown()
    {
        if (isDragging) return;

        isDragging = true;
        offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset.z = 0;

        transform.DOKill();
        transform.DOScale(1f, 0.3f);

        transform.SetParent(null);

        SetSortingOrders(2);

        foreach (var i in connectedParticles)
            i.transform.SetParent(transform);

        if (!isAlreadyPicked)
        {
            GlobalEventManager.PickPuzzle();
            isAlreadyPicked = true;
        }
    }
    private void OnMouseUp()
    {
        if(!isDragging) return;

        isDragging = false;


        var x = Mathf.Round(transform.position.x);
        var y = Mathf.Round(transform.position.y);
        var newPos = new Vector3(x, y);

        var camPos = Camera.main.transform.position;
        //// магнитимся к ближайшему тайлу
        if(Vector3.Distance(transform.position, newPos) <= magnetRange)
            transform.position = newPos;

        newPos = transform.position;
        transform.position = new Vector3(Mathf.Clamp(newPos.x, camPos.x - 2.5f, camPos.x + 2.5f), Mathf.Clamp(newPos.y, camPos.y - 3, camPos.y + 5), 0);

        foreach (var i in connectedParticles)
            i.transform.SetParent(null);

        SetSortingOrders(0);

        CheckConnections();

        if (targetPos == transform.position && Vector3.Distance(transform.rotation.eulerAngles, Vector3.zero) <= 0.5f)
        {
            SetInPlace(false);
            foreach (var i in connectedParticles) i.SetInPlace(true);

            connectedParticles.Clear();
        }
    }
    private void OnMouseDrag()
    {
        if (!isDragging) return;
        var mousePos = Input.mousePosition;
        transform.position = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10)) + offset;
    }
    
    private void CheckConnections()
    {
        //var cf = new ContactFilter2D();
        //cf.useTriggers = true;
        //cf.SetLayerMask(puzzleMask);
        //cf.useLayerMask = true;

        Collider2D[] allOverlappingColliders = new Collider2D[4];

        touchCollider.OverlapCollider(new ContactFilter2D().NoFilter(), allOverlappingColliders);

        foreach(var i in allOverlappingColliders)
        {
            if (i == null) continue;

            //var particle = i.GetComponent<PuzzleParticle>();
            if (!i.TryGetComponent<PuzzleParticle>(out var particle)) return;
            if (particle.transform.rotation != Quaternion.Euler(Vector3.zero)) continue;
            if (connectedParticles.Contains(particle)) continue;

            // для варианта без проверки targetPos
            //if (i.transform.position == transform.position + Vector3.up) CheckOneDirection(particle, 0);
            //else if (i.transform.position == transform.position + Vector3.right) CheckOneDirection(particle, 1);
            //else if (i.transform.position == transform.position - Vector3.up) CheckOneDirection(particle, 2);
            //else if (i.transform.position == transform.position - Vector3.right) CheckOneDirection(particle, 3);            
            
            if (i.transform.position == transform.position + Vector3.up || i.transform.position == transform.position + Vector3.right
                ||i.transform.position == transform.position - Vector3.up || i.transform.position == transform.position - Vector3.right) 
                CheckOneDirection(particle);

        }
    }
    private void CheckOneDirection(PuzzleParticle particle)//, int dir, int color
    {
        // вариант простого коннекта любого соответствующего пазла (без проверки targetPos)
        //var ourConnect = connections[dir];
        //var targetDir = dir switch
        //{
        //    0 => 2,
        //    1 => 3,
        //    2 => 0,
        //    3 => 1,
        //    _ => 0,
        //};
        //var targetConnect = particle.GetConnections()[targetDir];
        //if (ourConnect == 0 || targetConnect == 0 || ourConnect == targetConnect) return;

        if (transform.position - particle.transform.position != targetPos - particle.targetPos) return; // non-correct connection

        AddConnectedParticle(particle);
        particle.AddConnectedParticle(this);

        foreach(var i in particle.connectedParticles)
        {
            AddConnectedParticle(i);
            i.AddConnectedParticle(this);
        }
        
    }
    public int[] GetConnections() // optimize links
    {
        return connections;
    }
    public void SetConnections(int[] conn)
    {
        connections = new int[conn.Length];
        conn.CopyTo(connections, 0);
    }
    public int[] GetColors()
    {
        return colors;
    }
    public void SetColors(int[] col)
    {
        colors = new int[col.Length];
        col.CopyTo(colors, 0);
    }
    public void AddConnectedParticle(PuzzleParticle particle)
    {
        if(!connectedParticles.Contains(particle) && particle != this)
            connectedParticles.Add(particle);
    }

    public void SetInPlace(bool isClearMemory)
    {
        isInPlace = true;

        GlobalEventManager.ChangePuzzleCount(1);

        // вариант с Fade всех частей пазла
        //foreach (var i in GetComponentsInChildren<SpriteRenderer>())
        //{
        //    var currentSequence = DOTween.Sequence();
        //    currentSequence.Append(i.DOFade(0.5f, 0.5f));
        //    currentSequence.Append(i.DOFade(0.8f, 0.6f));
        //}

        // вариант только с центральной частью
        var spriteRenderer = GetComponent<SpriteRenderer>();
        currentSequence = DOTween.Sequence();
        currentSequence.Append(spriteRenderer.DOFade(0.35f, 0.4f));
        currentSequence.Append(spriteRenderer.DOFade(0.7f, 0.4f));

        if (touchCollider != null) touchCollider.enabled = false;
        if(isClearMemory && connectedParticles != null) connectedParticles.Clear();
        //Destroy(this);
    }

    void StartGame()
    {
        if (isInPlace) return;

        touchCollider.enabled = true;
    }
    void SetSortingOrders(int order)
    {
        if (order < 0) Debug.Log("Order Layer less then 0!");
        var renderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
        var isFirst = false;
        foreach (var i in renderers)
        {
            if (isFirst) i.sortingOrder = order + 1;
            else
            {
                isFirst = true;
                i.sortingOrder = order;
            }
        }
    }
    void Win()
    {
        // do jump or shake?
        var spriteRenderer = GetComponent<SpriteRenderer>();
        //spriteRenderer.DOKill();
        if (currentSequence != null) currentSequence.Kill();
        spriteRenderer.DOFade(1f, 0.9f);
    }

    public void Rotate() // hardcore mode rotation
    {
        transform.DOComplete();

        var rotationAngle = new Vector3(0, 0, 90);
        var min = Mathf.Infinity;
        var count = 0;
        var distances = new float[4];

        for (int i = 0; i < 4; i++) distances[i] = Vector3.Distance(transform.rotation.eulerAngles, rotationAngle * i);

        for (int i = 0; i < 4; i++)
        {
            if (distances[i] < min)
            {
                min = distances[i];
                count = i;
            }
        }        
        if (min > 0.5f)
        {
            transform.rotation = Quaternion.Euler(rotationAngle * count);
        }

        transform.DOBlendableRotateBy(-rotationAngle, 0.3f);

        // вариант без проверки targetPos
        //var oldConnections = connections;
        //connections[0] = oldConnections[3];
        //connections[1] = oldConnections[0];
        //connections[2] = oldConnections[1];
        //connections[3] = oldConnections[2];

    }
}
