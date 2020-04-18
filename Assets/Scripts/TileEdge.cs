using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileEdge : MonoBehaviour
{
    [SerializeField] TileEdge connection;
    [SerializeField] TileEdgeMode entryMode = TileEdgeMode.Allow;
    Tile tile;
    public Tile ConnectedTile
    {
        get { return connection?.tile; }
    }

    private void Start()
    {
        tile = GetComponentInParent<Tile>();
    }

    public TileEdgeMode ExitMode
    {
        get
        {
            if (connection == null) return TileEdgeMode.Fall;
            return connection.entryMode;
        }
    }

    public void BumpConnected()
    {
        connection?.tile.Bump(connection);
    }

    public TileEdge HeadingAfterPassing
    {
        get
        {
            return connection?.tile.Forward(connection);
        }
    }
}
