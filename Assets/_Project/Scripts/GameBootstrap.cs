using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    void Awake()
    {
        var gm = gameObject.GetComponent<GameManager>();
        if (gm == null) gm = gameObject.AddComponent<GameManager>();
        gm.Bootstrap();
    }
}
