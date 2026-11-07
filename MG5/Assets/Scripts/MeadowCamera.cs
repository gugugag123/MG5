using UnityEngine;

public class MeadowCamera : MonoBehaviour
{
    public Transform target;
    private readonly Vector3 _offset = new Vector3(0f, 17f, -14f);
    private void LateUpdate()
    {
        transform.position = target.position + _offset;
        transform.LookAt(target.position + Vector3.forward * 2f);
    }
    private void OnGUI()
    {
        GUI.Box(new Rect(16, 16, 360, 66), "MOUNTAIN MEADOW\nClick ground to walk. SPACE returns to before your last click.\nWalk near the sparrows to make them scatter.");
    }
}
