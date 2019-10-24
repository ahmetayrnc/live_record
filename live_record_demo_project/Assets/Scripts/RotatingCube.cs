using UnityEngine;

public class RotatingCube : MonoBehaviour
{
    private readonly Vector3 _rotateAmount = new Vector3(15f, 30f, 45f);

    private void Update()
    {
        transform.Rotate(_rotateAmount * Time.deltaTime);
    }
}