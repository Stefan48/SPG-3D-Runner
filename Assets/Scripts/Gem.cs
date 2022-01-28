using UnityEngine;

public class Gem : MonoBehaviour
{
    private Vector3 rotation = new Vector3(0.0f, 50.0f, 0.0f);

    void Update()
    {
        transform.Rotate(rotation * Time.deltaTime);
    }
}
