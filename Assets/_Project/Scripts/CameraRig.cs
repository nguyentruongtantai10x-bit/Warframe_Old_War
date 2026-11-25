using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRig : MonoBehaviour
{
    public Transform Target;

    public float orthoSize = 8.5f;
    float shake;

    public void SetTarget(Transform target)
    {
        Target = target;
        if (target != null)
            transform.position = new Vector3(target.position.x, target.position.y + 1.5f, -10f);
    }

    public void Shake(float amount)
    {
        shake = Mathf.Max(shake, amount);
    }

    void LateUpdate()
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null) return;
        cam.orthographic = true;
        cam.orthographicSize = orthoSize;

        if (Target == null)
        {
            transform.position = new Vector3(0f, 0f, -10f);
            return;
        }

        Vector3 focus = Target.position + Vector3.up * 1.6f;

        var mouse = Mouse.current;
        if (mouse != null && GameManager.Instance != null && GameManager.Instance.State == GameState.Playing)
        {
            Vector3 screen = mouse.position.ReadValue();
            Vector3 world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 10f));
            float lookX = Mathf.Clamp((world.x - Target.position.x) * 0.15f, -2f, 2f);
            float lookY = Mathf.Clamp((world.y - Target.position.y) * 0.10f, -1.2f, 1.2f);
            focus += new Vector3(lookX, lookY, 0f);
        }

        Vector3 desired = new Vector3(focus.x, focus.y, -10f);

        float limitX = ArenaBuilder.HalfWidth - orthoSize * cam.aspect;
        if (limitX > 0f) desired.x = Mathf.Clamp(desired.x, -limitX, limitX);
        desired.y = Mathf.Clamp(desired.y, -1.5f, 7f);

        float k = 1f - Mathf.Exp(-10f * Time.unscaledDeltaTime);
        transform.position = Vector3.Lerp(transform.position, desired, k);

        if (shake > 0.01f)
        {
            shake = Mathf.Lerp(shake, 0f, 8f * Time.unscaledDeltaTime);
            transform.position += new Vector3(Random.Range(-shake, shake), Random.Range(-shake, shake), 0f) * 0.05f;
        }
    }
}
