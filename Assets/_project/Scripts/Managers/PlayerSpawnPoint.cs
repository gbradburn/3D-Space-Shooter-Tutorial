using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [SerializeField] int _spawnIndex;
    [SerializeField] bool _isOccupied;
    
    public int SpawnIndex => _spawnIndex;
    public bool IsOccupied => _isOccupied;
    public Vector3 Position => transform.position;
    public Quaternion Rotation => transform.rotation;
    
    public void SetOccupied(bool occupied) => _isOccupied = occupied;
    
    void OnDrawGizmos()
    {
        Gizmos.color = _isOccupied ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, 10f);
        Gizmos.DrawRay(transform.position, transform.forward * 50f);
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 20f, 
            $"Spawn {_spawnIndex}\n{(_isOccupied ? "Occupied" : "Available")}"
        );
        #endif
    }
}
