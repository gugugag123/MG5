using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Player : MonoBehaviour
{
    private Animator _animator;
    private NavMeshAgent _agent;
    private Camera _mainCamera;
    private Vector3 _lastPosition;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _mainCamera = Camera.main;
        _lastPosition = transform.position;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 500f, LayerMask.GetMask("Ground")) &&
                NavMesh.SamplePosition(hit.point, out NavMeshHit destination, 2f, _agent.areaMask))
            {
                Vector3 clickStart = transform.position;
                if (_agent.SetDestination(destination.position))
                {
                    _lastPosition = clickStart;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && _agent.Warp(_lastPosition))
        {
            _agent.ResetPath();
        }

        _animator.SetBool("isMoving", _agent.velocity.sqrMagnitude > 0.01f);
    }
}
