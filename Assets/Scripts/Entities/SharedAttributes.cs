using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SharedAttributes 
{

    public static IAttribute MOVEMENT_SPEED = new RangedAttribute("movement_speed", 7.5D, 0.0D, double.MaxValue);
    public static IAttribute DAMAGE_MULTIPLICATOR = new RangedAttribute("damage_multiplicator", 1D, 0.0D, 2.0D);
    public static IAttribute MAX_HEALTH = new RangedAttribute("max_health", 20.0D, 0.0D, double.MaxValue);
    public static IAttribute FOLLOW_RANGE = new RangedAttribute("follow_range", 32.0D, 0.0D, 2048.0D);
    public static IAttribute JUMP_STRENGTH = new RangedAttribute("jump_strength", 42.0D, 0.0D, 100.0D);
}
