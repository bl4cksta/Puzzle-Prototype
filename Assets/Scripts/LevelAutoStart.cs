using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelAutoStart : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
