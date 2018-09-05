/*
 *  VolatilePhysics - A 2D Physics Library for Networked Games
 *  Copyright (c) 2015-2016 - Alexander Shoulson - http://ashoulson.com
 *
 *  This software is provided 'as-is', without any express or implied
 *  warranty. In no event will the authors be held liable for any damages
 *  arising from the use of this software.
 *  Permission is granted to anyone to use this software for any purpose,
 *  including commercial applications, and to alter it and redistribute it
 *  freely, subject to the following restrictions:
 *  
 *  1. The origin of this software must not be misrepresented; you must not
 *     claim that you wrote the original software. If you use this software
 *     in a product, an acknowledgment in the product documentation would be
 *     appreciated but is not required.
 *  2. Altered source versions must be plainly marked as such, and must not be
 *     misrepresented as being the original software.
 *  3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Collections.Generic;

#if UNITY
using UnityEngine;
#endif

namespace Volatile
{
  public static class VoltMath
  {
    #region Transformations
    public static TSVector2 WorldToBodyPoint(
      TSVector2 bodyPosition,
      TSVector2 bodyFacing,
      TSVector2 vector)
    {
      return (vector - bodyPosition).InvRotate(bodyFacing);
    }

    public static TSVector2 WorldToBodyDirection(
      TSVector2 bodyFacing,
      TSVector2 vector)
    {
      return vector.InvRotate(bodyFacing);
    }
    #endregion

    #region Body-Space to World-Space Transformations
    public static TSVector2 BodyToWorldPoint(
      TSVector2 bodyPosition,
      TSVector2 bodyFacing,
      TSVector2 vector)
    {
      return vector.Rotate(bodyFacing) + bodyPosition;
    }

    public static TSVector2 BodyToWorldDirection(
      TSVector2 bodyFacing,
      TSVector2 vector)
    {
      return vector.Rotate(bodyFacing);
    }
    #endregion

    public static TSVector2 Right(this TSVector2 v)
    {
      return new TSVector2(v.y, -v.x);
    }

    public static TSVector2 Left(this TSVector2 v)
    {
      return new TSVector2(-v.y, v.x);
    }

    public static TSVector2 Rotate(this TSVector2 v, TSVector2 b)
    {
      return new TSVector2(v.x * b.x - v.y * b.y, v.y * b.x + v.x * b.y);
    }

    public static TSVector2 InvRotate(this TSVector2 v, TSVector2 b)
    {
      return new TSVector2(v.x * b.x + v.y * b.y, v.y * b.x - v.x * b.y);
    }

    public static FP Angle(this TSVector2 v)
    {
      return TSMath.Atan2(v.y, v.x);
    }

    public static TSVector2 Polar(FP radians)
    {
      return new TSVector2(TSMath.Cos(radians), TSMath.Sin(radians));
    }

    public static FP Cross(TSVector2 a, TSVector2 b)
    {
      return a.x * b.y - a.y * b.x;
    }

    public static FP Square(FP a)
    {
      return a * a;
    }
  }
}
