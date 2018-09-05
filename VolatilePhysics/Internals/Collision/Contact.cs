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

#if UNITY
using UnityEngine;
#endif

namespace Volatile
{
  internal sealed class Contact 
    : IVoltPoolable<Contact>
  {
    #region Interface
    IVoltPool<Contact> IVoltPoolable<Contact>.Pool { get; set; }
    void IVoltPoolable<Contact>.Reset() { this.Reset(); }
    #endregion

    #region Static Methods
    private static FP BiasDist(FP dist)
    {
      return VoltConfig.ResolveRate * TSMath.Min(0, dist + VoltConfig.ResolveSlop);
    }
    #endregion

    private TSVector2 position;
    private TSVector2 normal;
    private FP penetration;

    private TSVector2 toA;
    private TSVector2 toB;
    private TSVector2 toALeft;
    private TSVector2 toBLeft;

    private FP nMass;
    private FP tMass;
    private FP restitution;
    private FP bias;
    private FP jBias;

    private FP cachedNormalImpulse;
    private FP cachedTangentImpulse;

    public Contact()
    {
      this.Reset();
    }

    internal Contact Assign(
      TSVector2 position,
      TSVector2 normal,
      FP penetration)
    {
      this.Reset();

      this.position = position;
      this.normal = normal;
      this.penetration = penetration;

      return this;
    }

    internal void PreStep(Manifold manifold)
    {
      VoltBody bodyA = manifold.ShapeA.Body;
      VoltBody bodyB = manifold.ShapeB.Body;

      this.toA = this.position - bodyA.Position;
      this.toB = this.position - bodyB.Position;
      this.toALeft = this.toA.Left();
      this.toBLeft = this.toB.Left();

      this.nMass = 1.0f / this.KScalar(bodyA, bodyB, this.normal);
      this.tMass = 1.0f / this.KScalar(bodyA, bodyB, this.normal.Left());

      this.bias = Contact.BiasDist(penetration);
      this.jBias = 0;
      this.restitution =
        manifold.Restitution *
        TSVector2.Dot(
          this.normal,
          this.RelativeVelocity(bodyA, bodyB));
    }

    internal void SolveCached(Manifold manifold)
    {
      this.ApplyContactImpulse(
        manifold.ShapeA.Body,
        manifold.ShapeB.Body,
        this.cachedNormalImpulse,
        this.cachedTangentImpulse);
    }

    internal void Solve(Manifold manifold)
    {
      VoltBody bodyA = manifold.ShapeA.Body;
      VoltBody bodyB = manifold.ShapeB.Body;
      FP elasticity = bodyA.World.Elasticity;

      // Calculate relative bias velocity
      TSVector2 vb1 = bodyA.BiasVelocity + (bodyA.BiasRotation * this.toALeft);
      TSVector2 vb2 = bodyB.BiasVelocity + (bodyB.BiasRotation * this.toBLeft);
      FP vbn = TSVector2.Dot((vb1 - vb2), this.normal);

      // Calculate and clamp the bias impulse
      FP jbn = this.nMass * (vbn - this.bias);
      jbn = TSMath.Max(-this.jBias, jbn);
      this.jBias += jbn;

      // Apply the bias impulse
      this.ApplyNormalBiasImpulse(bodyA, bodyB, jbn);

      // Calculate relative velocity
      TSVector2 vr = this.RelativeVelocity(bodyA, bodyB);
      FP vrn = TSVector2.Dot(vr, this.normal);

      // Calculate and clamp the normal impulse
      FP jn = nMass * (vrn + (this.restitution * elasticity));
      jn = TSMath.Max(-this.cachedNormalImpulse, jn);
      this.cachedNormalImpulse += jn;

      // Calculate the relative tangent velocity
      FP vrt = TSVector2.Dot(vr, this.normal.Left());

      // Calculate and clamp the friction impulse
      FP jtMax = manifold.Friction * this.cachedNormalImpulse;
      FP jt = vrt * tMass;
      FP result = TSMath.Clamp(this.cachedTangentImpulse + jt, -jtMax, jtMax);
      jt = result - this.cachedTangentImpulse;
      this.cachedTangentImpulse = result;

      // Apply the normal and tangent impulse
      this.ApplyContactImpulse(bodyA, bodyB, jn, jt);
    }

    #region Internals
    private void Reset()
    {
      this.position = TSVector2.zero;
      this.normal = TSVector2.zero;
      this.penetration = 0.0f;

      this.toA = TSVector2.zero;
      this.toB = TSVector2.zero;
      this.toALeft = TSVector2.zero;
      this.toBLeft = TSVector2.zero;

      this.nMass = 0.0f;
      this.tMass = 0.0f;
      this.restitution = 0.0f;
      this.bias = 0.0f;
      this.jBias = 0.0f;

      this.cachedNormalImpulse = 0.0f;
      this.cachedTangentImpulse = 0.0f;
    }

    private FP KScalar(
      VoltBody bodyA,
      VoltBody bodyB,
      TSVector2 normal)
    {
      FP massSum = bodyA.InvMass + bodyB.InvMass;
      FP r1cnSqr = VoltMath.Square(VoltMath.Cross(this.toA, normal));
      FP r2cnSqr = VoltMath.Square(VoltMath.Cross(this.toB, normal));
      return
        massSum +
        bodyA.InvInertia * r1cnSqr +
        bodyB.InvInertia * r2cnSqr;
    }

    private TSVector2 RelativeVelocity(VoltBody bodyA, VoltBody bodyB)
    {
      return
        (bodyA.AngularVelocity * this.toALeft + bodyA.LinearVelocity) -
        (bodyB.AngularVelocity * this.toBLeft + bodyB.LinearVelocity);
    }

    private void ApplyNormalBiasImpulse(
      VoltBody bodyA,
      VoltBody bodyB,
      FP normalBiasImpulse)
    {
      TSVector2 impulse = normalBiasImpulse * this.normal;
      bodyA.ApplyBias(-impulse, this.toA);
      bodyB.ApplyBias(impulse, this.toB);
    }

    private void ApplyContactImpulse(
      VoltBody bodyA,
      VoltBody bodyB,
      FP normalImpulseMagnitude,
      FP tangentImpulseMagnitude)
    {
      TSVector2 impulseWorld =
        new TSVector2(normalImpulseMagnitude, tangentImpulseMagnitude);
      TSVector2 impulse = impulseWorld.Rotate(this.normal);

      bodyA.ApplyImpulse(-impulse, this.toA);
      bodyB.ApplyImpulse(impulse, this.toB);
    }
    #endregion
  }
}