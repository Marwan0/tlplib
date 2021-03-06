﻿using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Functional;
using Smooth.Pools;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public interface IHeapFuture<A> {
    bool isCompleted { get; }
    Option<A> value { get; }
    void onComplete(Act<A> action);
  }

  public static class IHeapFutureExts {
    public static Future<A> asFuture<A>(this IHeapFuture<A> f) => Future.a(f);
  }

  class FutureImpl<A> : IHeapFuture<A>, Promise<A> {
    static readonly Pool<IList<Act<A>>> pool = new Pool<IList<Act<A>>>(
      () => new List<Act<A>>(), list => list.Clear()
    );

    IList<Act<A>> listeners = pool.Borrow();

    public bool isCompleted => value.isSome;
    public Option<A> value { get; private set; } = F.none<A>();

    public override string ToString() => $"{nameof(FutureImpl<A>)}({value})";

    public void complete(A v) {
      if (! tryComplete(v)) throw new IllegalStateException(
        $"Trying to complete future with \"{v}\" but it is already completed with \"{value.get}\""
      );
    }

    public bool tryComplete(A v) {
      // Cannot use fold here because of iOS AOT.
      var ret = value.isNone;
      if (ret) {
        value = F.some(v);
        // completed should be called only once
        completed(v);
      }
      return ret;
    }

    public void onComplete(Act<A> action) {
      if (value.isSome) action(value.get);
      else listeners.Add(action);
    }

    void completed(A v) {
      foreach (var action in listeners) action(v);
      listeners.Clear();
      pool.Release(listeners);
      listeners = null;
    }
  }
}
