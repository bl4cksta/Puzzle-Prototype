using DG.Tweening;
using UnityEngine;

public class ScrollBar : MonoBehaviour
{
    [SerializeField] private LayerMask puzzleMask;
    private bool isGameStarted;
    private bool isDragging;
    private Vector3 offset;
    private Vector2 bounds;
    private BoxCollider2D touchCollider;
    private void Awake()
    {
        GlobalEventManager.OnGameStarted.AddListener(StartGame);
        GlobalEventManager.OnPuzzlePicked.AddListener(RegroupPuzzles);
    }
    private void Start()
    {
        isGameStarted = false;
        touchCollider = GetComponent<BoxCollider2D>();
    }
    private void OnMouseDown()
    {
        if (!isGameStarted) return;
        if (isDragging) return;

        isDragging = true;
        offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset.z = 0;
    }
    private void OnMouseDrag()
    {
        if (!isGameStarted) return;
        if (!isDragging) return;

        var newPos = new Vector3((offset + Camera.main.ScreenToWorldPoint(Input.mousePosition)).x, transform.position.y);
        if (newPos.x > bounds.x) newPos.x = bounds.x;
        else if (newPos.x < bounds.y) newPos.x = bounds.y;

        transform.position = newPos;

    }
    private void OnMouseUp()
    {
        isDragging = false;
    }
    void StartGame()
    {
        isGameStarted = true;
        var childCount = transform.childCount;

        bounds = transform.position;

        SetColliderSize(childCount);
    }
    void RegroupPuzzles() // двигаем пазлы внутри бара 
    {
        var childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            var child = transform.GetChild(i);
            child.DOLocalMove(new Vector3(0, 0, -0.22f) + (2 * i * (child.localScale.x - 0.15f) * Vector3.right), 0.5f);
        }
        SetColliderSize(childCount);
        CorrectPosition();
    }
    void SetColliderSize(int childCount) // меняем размер коллайдера 
    {
        touchCollider.size = new Vector2(childCount + (childCount * 0.1f), touchCollider.size.y);
        touchCollider.offset = new Vector2(childCount * 0.5f, touchCollider.offset.y);
        bounds.y = bounds.x - touchCollider.size.x + 3f;
    }
    void CorrectPosition() // двигаем чтоб не уехали за пределы экрана
    {
        var newPos = new Vector2(Mathf.Clamp(transform.position.x + 1, bounds.y, bounds.x), transform.position.y);
        transform.DOMove(newPos, 0.3f);
    }
}
