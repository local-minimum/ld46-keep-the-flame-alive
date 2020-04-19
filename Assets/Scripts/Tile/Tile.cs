using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Tile : MonoBehaviour
{
    [SerializeField] Transform restPosition;
    public Transform RestPosition
    {
        get { return restPosition; }
    }
    [SerializeField] TileEffect tileEffect = TileEffect.NONE;
    public TileEffect TileEffect
    {
        get { return tileEffect; }
    }

    [Header("Edges")]
    [SerializeField] TileEdge[] entries;
    [SerializeField] TileEdge[] exits;
    [SerializeField] bool FixEdges;
    [Header("Only relevant for spawning")]
    [SerializeField] TileEdge spawnHeading;
    public TileEdge SpawnHeading
    {
        get { return spawnHeading;  }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (entries.Length != exits.Length) Debug.LogWarning(string.Format("{0} has entry exit missmatch", name));
    }

    public TileEdge Left(TileEdge heading, int steps=1) {
        for (int i=0; i<exits.Length; i++)
        {
            if (exits[i] == heading)
            {
                return exits[i - steps < 0 ? exits.Length + (i - steps)  : i - steps];
            }
        }
        Debug.LogError(string.Format("Looking for Left from {0} on {1}", heading.name, name));
        throw new MissingComponentException();
    }

    public TileEdge Right(TileEdge heading, int steps=1)
    {
        for (int i = 0; i < exits.Length; i++)
        {
            if (exits[i] == heading)
            {
                return exits[i + steps >= exits.Length ? i + steps - exits.Length : i + steps];
            }
        }
        Debug.LogError(string.Format("Looking for Right from {0} on {1}", heading.name, name));
        throw new MissingComponentException();
    }

    public TileEdge Forward(TileEdge entry)
    {
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i] == entry)
            {
                return exits[i];
            }
        }
        Debug.LogError(string.Format("Looking for Forward from {0} on {1}", entry.name, name));
        throw new MissingComponentException();
    }

    public TileEdge Backward(TileEdge heading)
    {
        return Forward(heading);
    }

    public TileEffect Bump(TileEdge fromDirection, bool flameBurning)
    {
        if (flameBurning)
        {
            var tileFire = GetComponentInChildren<TileFire>();
            tileFire.StartFire();
            tileEffect = TileEffect.Burning;
        }
        
        return TileEffect;
    }

    float gizmoCubeSize = 0.15f;
    private void OnDrawGizmosSelected()
    {
        bool emptyExit = false;
        for (int i = 0;  i < exits.Length; i++)
        {
            if (exits[i] == null)
            {
                emptyExit = true;
            }
            else
            {                
                if (exits[i].ExitMode == TileEdgeMode.Fall)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(restPosition.position, exits[i].transform.position);
                    Gizmos.DrawCube(exits[i].transform.position, Vector3.one * gizmoCubeSize);
                } else if (exits[i].ExitMode == TileEdgeMode.Block)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(transform.position, exits[i].transform.position);
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(exits[i].transform.position, Vector3.one * gizmoCubeSize);
                } else if (exits[i].ExitMode == TileEdgeMode.Allow)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(transform.position, exits[i].transform.position);
                    Gizmos.DrawCube(exits[i].transform.position, Vector3.one * gizmoCubeSize);
                    Gizmos.DrawCube(exits[i].Connection.transform.position, Vector3.one * gizmoCubeSize);
                    Gizmos.DrawLine(exits[i].Connection.transform.position, exits[i].ConnectedTile.restPosition.transform.position);
                    Gizmos.DrawCube(exits[i].ConnectedTile.RestPosition.transform.position, Vector3.one * gizmoCubeSize);
                }
                
            }
        }
        if (emptyExit)
        {
            Gizmos.color = Color.red;
        } else
        {
            Gizmos.color = Color.black;
        }
        Gizmos.DrawCube(RestPosition.position, Vector3.one * gizmoCubeSize);
    }
#if UNITY_EDITOR
    private void FixEdgeConnections()
    {
        for (int i=0, l=entries.Length; i<l; i++)
        {
            entries[i].AutoConnect();
        }

        for (int i = 0, l = exits.Length; i < l; i++) {
            exits[i].AutoConnect();
        }
    }

    private void Update()
    {
        if (FixEdges)
        {
            FixEdges = false;
            FixEdgeConnections();            
        }
    }
#endif
}
