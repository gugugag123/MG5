using UnityEngine;

public class BirdManager : MonoBehaviour
{
    [SerializeField] private float _interactDistance;
    [SerializeField] private Transform _player;
    [SerializeField] private Bird[] _birds;

    private void Update()
    {
        for (int i = 0; i < _birds.Length; i++)
        {
            Bird bird = _birds[i];
            if (Vector3.Distance(bird.transform.position, _player.position) <= _interactDistance)
            {
                bird.RunAway(_player.position);
            }
        }
    }
}
