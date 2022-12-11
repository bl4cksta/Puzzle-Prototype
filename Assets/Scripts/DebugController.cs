using UnityEngine;

public class DebugController : MonoBehaviour
{
    LevelCreator level;
    private void Awake()
    {
#if !UNITY_EDITOR
Destroy(gameObject);
#endif
        GlobalEventManager.OnGameStarting.AddListener(Hide);
    }
    private void Start()
    {
        Invoke(nameof(SearchLevel), 0.2f);
    }
    public void BtnSetRotationMode(bool mode)
    {
        if (level == null) return;
        level.SetupRotationMode(mode);
    }
    public void BtnSetUseColorMode(bool mode)
    {
        if (level == null) return;
        level.SetupColorMode(mode);
    }
    public void BtnSetPreshowMode(bool mode)
    {
        if (level == null) return;
        level.SetupPreshowMode(mode);
    }
    void SearchLevel()
    {
        level = FindObjectOfType<LevelCreator>();
    }
    void Hide()
    {
        gameObject.SetActive(false);
    }
}
