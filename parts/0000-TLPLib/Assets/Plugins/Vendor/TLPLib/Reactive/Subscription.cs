﻿using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.dispose;

namespace com.tinylabproductions.TLPLib.Reactive {
  public interface ISubscription : IDisposable {
    bool isSubscribed { get; }
    bool unsubscribe();
    ISubscription andThen(Action action);
    ISubscription join(params ISubscription[] other);
    ISubscription joinEnum(IEnumerable<ISubscription> others);
  }

  public class Subscription : ISubscription {
    /** Already unsubscribed subscription. */
    public static readonly ISubscription empty;

    static Subscription() {
      empty = new Subscription(() => {});
      empty.unsubscribe();
    }

    public static ISubscription a(Action onUnsubscribe) => new Subscription(onUnsubscribe);

    readonly Action onUnsubscribe;

    public bool isSubscribed { get; private set; } = true;

    public Subscription(Action onUnsubscribe) {
      this.onUnsubscribe = onUnsubscribe;
    }

    public bool unsubscribe() {
      if (!isSubscribed) return false;
      isSubscribed = false;
      onUnsubscribe();
      return true;
    }

    public void Dispose() => unsubscribe();

    public ISubscription andThen(Action action) => new Subscription(() => {
      unsubscribe();
      action();
    });

    public ISubscription join(params ISubscription[] other) => joinEnum(other);

    public ISubscription joinEnum(IEnumerable<ISubscription> others) => new Subscription(() => {
      unsubscribe();
      foreach (var other in others) other.unsubscribe();
    });
  }

  public static class ISubscriptionExts {
    public static ISubscription joinSubscriptions(
      this IEnumerable<ISubscription> subscriptions
    ) => new Subscription(() => {
      foreach (var sub in subscriptions) sub.unsubscribe();
    });
  }

  [Obsolete("Use DisposableTracker")]
  public class SubscriptionTracker : DisposeTracker<ISubscription> {
    static readonly Act<ISubscription> dispose = s => s.unsubscribe();

    public SubscriptionTracker() : base(dispose) {}
  }
}
