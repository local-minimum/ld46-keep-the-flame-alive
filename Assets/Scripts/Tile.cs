using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] Transform restPosition;
    public Transform RestPosition
    {
        get { return restPosition; }
    }
    [SerializeField] TileEdge[] entries;
    [SerializeField] TileEdge[] exits;
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

    public void Bump(TileEdge fromDirection)
    {
        //TODO: handle burn
    }
}
