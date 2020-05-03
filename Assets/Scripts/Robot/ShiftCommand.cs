using UnityEngine;

public struct ShiftCommand
{
    private static int HELD = -2;
    private static int OUTSIDE_FEED = -1;
    public readonly RobotCommand command;
    public readonly int position;
    public readonly int targetPosition;

    public bool Held { get { return targetPosition == HELD; } }
    public bool BeingPlayed { get { return targetPosition == OUTSIDE_FEED; } }
    public bool Drawn { get { return position == OUTSIDE_FEED; } }
    public bool Empty { get { return command == RobotCommand.NONE;  } }

    public bool LeftOrOn(int position)
    {
        return targetPosition != HELD && position <= targetPosition;
    }

    public bool SameAs(ShiftCommand other)
    {
        return command == other.command
            && position == other.position
            && targetPosition == other.targetPosition;
    }

    public bool SameButShiftedLeft(ShiftCommand other)
    {
        return command == other.command
            && targetPosition == other.position;
    }

    public bool SameButShiftedRight(ShiftCommand other)
    {
        return command == other.command
            && other.targetPosition == position;
    }

    public bool SameButPickedUp(ShiftCommand other)
    {
        return command == other.command
            && other.targetPosition == HELD
            && position == other.position;
    }

    private ShiftCommand(RobotCommand command, int position, int targetPosition)
    {
        this.command = command;
        this.position = position;
        this.targetPosition = targetPosition;
    }

    public static ShiftCommand MoveLeftFrom(RobotCommand command, int position)
    {
        return new ShiftCommand(command, position, Mathf.Max(OUTSIDE_FEED, position - 1));
    }

    public static ShiftCommand JumpRight(ShiftCommand previous)
    {
        return new ShiftCommand(previous.command, previous.position + 1, previous.targetPosition + 1);
    }

    public static ShiftCommand Pickup(ShiftCommand previous)
    {
        return new ShiftCommand(previous.command, previous.position, HELD);
    }

    public static ShiftCommand Insert(ShiftCommand previousHolder, RobotCommand command)
    {
        return new ShiftCommand(command, previousHolder.position, previousHolder.targetPosition);
    }

    public static ShiftCommand Remove(ShiftCommand previous)
    {
        return new ShiftCommand(previous.command, previous.targetPosition, OUTSIDE_FEED);
    }

    public static ShiftCommand NothingHeld
    {
        get
        {
            return new ShiftCommand(RobotCommand.NONE, OUTSIDE_FEED, HELD);
        }
    }

    public static ShiftCommand NotInPlay
    {
        get
        {
            return new ShiftCommand(RobotCommand.NONE, OUTSIDE_FEED, OUTSIDE_FEED);
        }
    }
}
