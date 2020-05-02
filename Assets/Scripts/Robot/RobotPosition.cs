using UnityEngine;

public struct RobotPosition
{
    public Tile tile { get; private set; }
    public TileEdge heading { get; private set; }

    public bool valid
    {
        get {
            return tile != null && heading != null && heading.tile == tile;
        }
    }

    public RobotPosition(Tile tile, TileEdge heading)
    {
        this.tile = tile;
        this.heading = heading;
    }

    public static RobotPosition SpawnPosition(Tile tile)
    {
        return new RobotPosition(tile, tile.SpawnHeading);
    }

    public RobotPosition Rotate(TileEdge heading)
    {
        if (heading.tile != tile)
        {
            Debug.LogError(string.Format("Trying to rotate while on other tile {0}:{1} but on {2}", heading, heading.tile, tile));
            return this;
        }
        return new RobotPosition(tile, heading);
    }
}
