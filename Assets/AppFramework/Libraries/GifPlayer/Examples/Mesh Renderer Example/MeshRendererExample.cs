using UnityEngine;

// This script is just used to rotate the cube
public class MeshRendererExample : MonoBehaviour {
    private float dir = 1;

    void Update() {

        transform.Rotate(new Vector3(0, dir, 0) * 50f * Time.deltaTime);
        if (transform.rotation.y > 0.8) {
            dir = -1f;
        }
        if (transform.rotation.y < 0.3) {
            dir = 1f;
        }
    }
}
