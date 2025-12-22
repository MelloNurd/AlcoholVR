using System.Net.Http.Headers;
using Unity.AI.Navigation;
using UnityEngine;

public class UpdatingNavMeshSurface : MonoBehaviour
{
    public bool UpdateAtRunTime = true;
    public float UpdateInterval = 2f;

    [SerializeField] private SphereCollider _playerAvoidanceCollider;

    private NavMeshSurface navMeshSurface;

    private float _timeSinceLastUpdate = 0f;

    Collider[] _cache = new Collider[1];

    private void Awake()
    {
        navMeshSurface = gameObject.GetOrAdd<NavMeshSurface>();
        navMeshSurface.BuildNavMesh();

        if(_playerAvoidanceCollider == null)
        {
            Debug.LogError("Player avoidance collider not assigned in UpdatingNavMeshSurface script.", this);
        }
    }

    private void Update()
    {
        _timeSinceLastUpdate += Time.deltaTime;
        if (UpdateAtRunTime && Application.isPlaying && _timeSinceLastUpdate > UpdateInterval)
        {
            if(Physics.OverlapSphereNonAlloc(_playerAvoidanceCollider.transform.position, _playerAvoidanceCollider.radius * 1.1f, _cache, LayerMask.GetMask("NPC")) > 0)
            {
                return;
            }

            navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
            _timeSinceLastUpdate = 0f;
        }
    }
}
