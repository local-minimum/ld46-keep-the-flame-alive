using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileEdge : MonoBehaviour
{
    TileEdge _connection;
    [SerializeField] TileEdgeMode entryMode = TileEdgeMode.Allow;
    Tile _tile;
    public Tile tile
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
        get { return Connection?.tile; }
    }
    public TileEdge Connection
    {
        get {
            if (_connection == null)
            {
                var edges = FindObjectsOfType<TileEdge>();
                for (int i = 0; i < edges.Length; i++)
                {
                    if (edges[i] == this || (edges[i].transform.position - transform.position).sqrMagnitude > connectToleranceSq) continue;
                    edges[i].Connection = this;
                    _connection = edges[i];
                    break;
                }
            }
            return _connection;
        }

        private set
        {
            _connection = value;
        }
    }

    public TileEdgeMode ExitMode
    {
        get
        {
            var connection = Connection;
            if (entryMode == TileEdgeMode.Block) return TileEdgeMode.Block;
            if (connection == null) return TileEdgeMode.Fall;            
            return connection.entryMode;
        }
    }

    public TileEffect BumpConnected(bool flameBurning)
    {
        var connection = Connection;
        if (connection == null) return TileEffect.NONE;
        return connection.tile.Bump(Connection, flameBurning);
    }

    public TileEdge HeadingAfterPassing
    {
        get
        {
            var connection = Connection;
            return connection?.tile.Forward(connection);
        }
    }


    [Header("Autoconnect")]
    [SerializeField]
    float connectToleranceSq = 0.05f;
    /*
    [SerializeField]
    float disconnectToleranceSq = 0.1f;
    
    
    public void AutoConnect()
    {
        var connection = Connection;
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
    */
}
