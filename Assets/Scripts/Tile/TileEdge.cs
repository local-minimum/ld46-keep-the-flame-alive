using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileEdge : MonoBehaviour
{
    [SerializeField] TileEdge connection;
    [SerializeField] TileEdgeMode entryMode = TileEdgeMode.Allow;
    Tile _tile;
    Tile tile
    {
        get
        {
            if (_tile == null)
            {
                _tile = GetComponentInParent<Tile>();
            }
            return _tile;
        }
    }

    public Tile ConnectedTile
    {
        get { return connection?.tile; }
    }
    public TileEdge Connection
    {
        get { return connection; }
    }

    public TileEdgeMode ExitMode
    {
        get
        {
            if (connection == null) return TileEdgeMode.Fall;
            if (entryMode == TileEdgeMode.Block) return TileEdgeMode.Block;
            return connection.entryMode;
        }
    }

    public TileEffect BumpConnected(bool flameBurning)
    {
        if (connection == null) return TileEffect.NONE;
        return connection.tile.Bump(connection, flameBurning);
    }

    public TileEdge HeadingAfterPassing
    {
        get
        {
            return connection?.tile.Forward(connection);
        }
    }

#if UNITY_EDITOR
    [Header("Autoconnect")]
    [SerializeField]
    float connectToleranceSq = 0.05f;
    [SerializeField]
    float disconnectToleranceSq = 0.1f;
    public void AutoConnect()
    {
        if (connection != null)
        {
            if ((transform.position - connection.transform.position).sqrMagnitude > disconnectToleranceSq)
            {
                connection = null;
            }
        } else
        {
            var edges = FindObjectsOfType<TileEdge>();
            for (int i = 0; i< edges.Length; i++)
            {
                if (edges[i] == this || (edges[i].transform.position - transform.position).sqrMagnitude > connectToleranceSq) continue;
                edges[i].connection = this;
                connection = edges[i];
                break;
            }
        }
    }
#endif
}
