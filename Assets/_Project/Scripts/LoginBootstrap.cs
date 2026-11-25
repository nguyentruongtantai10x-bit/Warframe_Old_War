using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class LoginBootstrap : MonoBehaviour
{
    void Awake()
    {
        var ui = gameObject.GetComponent<LoginUI>();
        if (ui == null) ui = gameObject.AddComponent<LoginUI>();
        ui.Build();

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            go.transform.SetParent(transform, false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
