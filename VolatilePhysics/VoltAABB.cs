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
  public struct VoltAABB
  {
    #region Static Methods
    public static VoltAABB CreateExpanded(VoltAABB aabb, FP expansionAmount)
    {
      return new VoltAABB(
        aabb.top + expansionAmount,
        aabb.bottom - expansionAmount,
        aabb.left - expansionAmount,
        aabb.right + expansionAmount);
    }

    public static VoltAABB CreateMerged(VoltAABB aabb1, VoltAABB aabb2)
    {
      return new VoltAABB(
        TSMath.Max(aabb1.top, aabb2.top),
        TSMath.Min(aabb1.bottom, aabb2.bottom),
        TSMath.Min(aabb1.left, aabb2.left),
        TSMath.Max(aabb1.right, aabb2.right));
    }

    public static VoltAABB CreateSwept(VoltAABB source, TSVector2 vector)
    {
      FP top = source.top;
      FP bottom = source.bottom;
      FP left = source.left;
      FP right = source.right;

      if (vector.x < 0.0f)
        left += vector.x;
      else
        right += vector.x;

      if (vector.y < 0.0f)
        bottom += vector.y;
      else
        top += vector.y;

      return new VoltAABB(top, bottom, left, right);
    }

    /// <summary>
    /// A cheap ray test that requires some precomputed information.
    /// Adapted from: http://www.cs.utah.edu/~awilliam/box/box.pdf
    /// </summary>
    private static bool RayCast(
      ref VoltRayCast ray,
      FP top,
      FP bottom,
      FP left,
      FP right)
    {
      FP txmin =
        ((ray.signX ? right : left) - ray.origin.x) *
        ray.invDirection.x;
      FP txmax =
        ((ray.signX ? left : right) - ray.origin.x) *
        ray.invDirection.x;

      FP tymin =
        ((ray.signY ? top : bottom) - ray.origin.y) *
        ray.invDirection.y;
      FP tymax =
        ((ray.signY ? bottom : top) - ray.origin.y) *
        ray.invDirection.y;

      if ((txmin > tymax) || (tymin > txmax))
        return false;
      if (tymin > txmin)
        txmin = tymin;
      if (tymax < txmax)
        txmax = tymax;
      return (txmax > 0.0f) && (txmin < ray.distance);
    }
    #endregion

    public TSVector2 TopLeft 
    { 
      get { return new TSVector2(this.left, this.top); } 
    }

    public TSVector2 TopRight 
    { 
      get { return new TSVector2(this.right, this.top); } 
    }

    public TSVector2 BottomLeft 
    { 
      get { return new TSVector2(this.left, this.bottom); } 
    }

    public TSVector2 BottomRight 
    { 
      get { return new TSVector2(this.right, this.bottom); } 
    }

    public FP Top { get { return this.top; } }
    public FP Bottom { get { return this.bottom; } }
    public FP Left { get { return this.left; } }
    public FP Right { get { return this.right; } }

    public FP Width { get { return this.Right - this.Left; } }
    public FP Height { get { return this.Top - this.Bottom; } }

    public FP Area { get { return this.Width * this.Height; } }
    public FP Perimeter 
    { 
      get { return 2.0f * (this.Width + this.Height); } 
    }

    public TSVector2 Center { get { return this.ComputeCenter(); } }
    public TSVector2 Extent 
    { 
      get { return new TSVector2(this.Width * 0.5f, this.Height * 0.5f); } 
    }

    private readonly FP top;
    private readonly FP bottom;
    private readonly FP left;
    private readonly FP right;

    #region Tests
    /// <summary>
    /// Performs a point test on the AABB.
    /// </summary>
    public bool QueryPoint(TSVector2 point)
    {
      return 
        this.left <= point.x && 
        this.right >= point.x &&
        this.bottom <= point.y &&
        this.top >= point.y;
    }

    /// <summary>
    /// Note: This doesn't take rounded edges into account.
    /// </summary>
    public bool QueryCircleApprox(TSVector2 origin, FP radius)
    {
      return
        (this.left - radius) <= origin.x &&
        (this.right + radius) >= origin.x &&
        (this.bottom - radius) <= origin.y &&
        (this.top + radius) >= origin.y;
    }

    public bool RayCast(ref VoltRayCast ray)
    {
      return VoltAABB.RayCast(
        ref ray, 
        this.top, 
        this.bottom, 
        this.left, 
        this.right);
    }

    /// <summary>
    /// Note: This doesn't take rounded edges into account.
    /// </summary>
    public bool CircleCastApprox(ref VoltRayCast ray, FP radius)
    {
      return VoltAABB.RayCast(
        ref ray,
        this.top + radius,
        this.bottom - radius,
        this.left - radius,
        this.right + radius);
    }

    public bool Intersect(VoltAABB other)
    {
      bool outside =
        this.right <= other.left ||
        this.left >= other.right ||
        this.bottom >= other.top ||
        this.top <= other.bottom;
      return (outside == false);
    }

    public bool Contains(VoltAABB other)
    {
      return
        this.top >= other.Top &&
        this.bottom <= other.Bottom &&
        this.right >= other.right &&
        this.left <= other.left;
    }
    #endregion

    public VoltAABB(FP top, FP bottom, FP left, FP right)
    {
      this.top = top;
      this.bottom = bottom;
      this.left = left;
      this.right = right;
    }

    public VoltAABB(TSVector2 center, TSVector2 extents)
    {
      TSVector2 topRight = center + extents;
      TSVector2 bottomLeft = center - extents;

      this.top = topRight.y;
      this.right = topRight.x;
      this.bottom = bottomLeft.y;
      this.left = bottomLeft.x;
    }

    public VoltAABB(TSVector2 center, FP radius)
      : this (center, new TSVector2(radius, radius))
    {
    }

    public VoltAABB ComputeTopLeft(TSVector2 center)
    {
      return new VoltAABB(this.top, center.y, this.left, center.x);
    }

    public VoltAABB ComputeTopRight(TSVector2 center)
    {
      return new VoltAABB(this.top, center.y, center.x, this.right);
    }

    public VoltAABB ComputeBottomLeft(TSVector2 center)
    {
      return new VoltAABB(center.y, this.bottom, this.left, center.x);
    }

    public VoltAABB ComputeBottomRight(TSVector2 center)
    {
      return new VoltAABB(center.y, this.bottom, center.x, this.right);
    }

    private TSVector2 ComputeCenter()
    {
      return new TSVector2(
        (this.Width * 0.5f) + this.left, 
        (this.Height * 0.5f) + this.bottom);
    }

    #region Debug
#if UNITY && DEBUG
    public void GizmoDraw(Color aabbColor)
    {
      Color current = Gizmos.color;

      Vector2 A = new TSVector2(this.Left, this.Top).ToVector();
      Vector2 B = new TSVector2(this.Right, this.Top).ToVector();
      Vector2 C = new TSVector2(this.Right, this.Bottom).ToVector();
      Vector2 D = new TSVector2(this.Left, this.Bottom).ToVector();

      Gizmos.color = aabbColor;
      Gizmos.DrawLine(A, B);
      Gizmos.DrawLine(B, C);
      Gizmos.DrawLine(C, D);
      Gizmos.DrawLine(D, A);

      Gizmos.color = current;
    }
#endif
    #endregion
  }
}
