using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Volatile;

public static class VolatileUtils
{
    public static TSVector2 ToTSVector2(this Vector2 vec)
    {
        return new TSVector2(vec.x, vec.y);
    }

    public static TSVector2 ToTSVector2(this Vector3 vec)
    {
        return new TSVector2(vec.x, vec.y);
    }
    
}
