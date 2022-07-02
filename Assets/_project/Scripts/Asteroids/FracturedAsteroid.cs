using UnityEngine;

public class FracturedAsteroid : MonoBehaviour
{
    [SerializeField] [Range(1f, 60f)] private float _duration = 10f;

    private void OnEnable()
    {
        Destroy(gameObject, _duration);
    }
}
