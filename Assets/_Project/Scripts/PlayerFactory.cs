using UnityEngine;

public static class PlayerFactory
{
    public static PlayerController Create(CharacterId id, Vector3 position)
    {
        var stats = CharacterStats.Get(id);
        var go = new GameObject("Player");
        go.transform.position = position;
        var pc = go.AddComponent<PlayerController>();
        pc.Init(stats);
        return pc;
    }
}
