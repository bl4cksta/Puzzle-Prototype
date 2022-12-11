using UnityEngine;

public class PuzzleRotator : MonoBehaviour
{
    [Header("Max distance between MouseUp and MouseDown to perform rotate")]
    [SerializeField] private float maxDist = 4f;
    [SerializeField] private LayerMask puzzleMask;

    private Vector3 mousePos;
    private bool isActing;
    private void Awake()
    {
        GlobalEventManager.OnGameStarted.AddListener(StartGame);
        GlobalEventManager.OnWin.AddListener(Win);
        isActing = false;
    }
    void Update()
    {
        if (!isActing) return;

        if (Input.GetMouseButtonDown(0))
        {
            mousePos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            var newMousePos = Input.mousePosition;
            var dist = Vector3.Distance(mousePos, newMousePos);
            if (dist > maxDist) return; // если увели мышь больше заданного расстояния

            newMousePos = Camera.main.ScreenToWorldPoint(newMousePos);
            var hitInfo = Physics2D.Raycast(newMousePos, Vector2.zero, 20f, puzzleMask);

            if (hitInfo.transform != null)
            {
                if(hitInfo.transform.TryGetComponent<PuzzleParticle>(out var puzzle))
                {
                    puzzle.Rotate();
                }
            }
        }
    }
    void StartGame()
    {
        isActing = true;
    }
    void Win()
    {
        isActing = false;
    }
}
