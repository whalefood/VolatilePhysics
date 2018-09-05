using System;
using System.Collections.Generic;

using UnityEngine;
using Volatile;

public class SimpleExplode : MonoBehaviour
{
  [SerializeField]
  private FP radius;

  [SerializeField]
  private FP forceMax;

  [SerializeField]
  private int rayCount;

  [SerializeField]
  private VolatileBody body;

  private List<TSVector2> hits;
  private TSVector2 lastOrigin;
  private FP showDelay;

  void Awake()
  {
        this.hits = new List<TSVector2>();
  }

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.E))
    {
      this.hits.Clear();
      this.lastOrigin = this.transform.position.ToTSVector2();
      this.showDelay = Time.time + 0.2f;

      VolatileWorld.Instance.World.PerformExplosion(
        this.lastOrigin,
        this.radius,
        this.ExplosionCallback,
        (body) => (body.IsStatic == false) && (body != this.body.Body),
        VoltWorld.FilterExcept(this.body.Body));
    }
  }

  private void ExplosionCallback(
    VoltRayCast rayCast,
    VoltRayResult rayResult,
    FP rayWeight)
  {
    TSVector2 point = rayResult.ComputePoint(ref rayCast);
    this.hits.Add(point);
  }

  void OnDrawGizmos()
  {
    if (Application.isPlaying && (Time.time < showDelay))
      foreach (var hit in this.hits)
        Gizmos.DrawLine(this.lastOrigin.ToVector2(), hit.ToVector2());
  }
}
