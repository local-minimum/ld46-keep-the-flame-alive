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

    public TileEdge Left(TileEdge heading) {
        for (int i=0; i<exits.Length; i++)
        {
            if (exits[i] == heading)
            {
                return exits[i == 0 ? exits.Length - 1 : i - 1];
            }
        }
        throw new MissingComponentException();
    }

    public TileEdge Right(TileEdge heading)
    {
        for (int i = 0; i < exits.Length; i++)
        {
            if (exits[i] == heading)
            {
                return exits[i == exits.Length - 1 ? 0 : i + 1];
            }
        }
        throw new MissingComponentException();
    }
}
