﻿using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class ComponentExts {
    public static A clone<A>(
      this A self, Vector3? position=null, Quaternion? rotation=null, Transform parent=null, 
      int? siblingIndex=null, bool? setActive=null
    ) where A : Component {
      var cloned = Object.Instantiate(self);
      // We need to parent first and only then set the position/rotation and other properties.
      if (parent != null) cloned.transform.SetParent(parent, false);
      if (position != null) cloned.transform.position = (Vector3) position;
      if (rotation != null) cloned.transform.rotation = (Quaternion) rotation;
      if (siblingIndex != null) cloned.transform.SetSiblingIndex((int) siblingIndex);
      if (setActive != null) cloned.gameObject.SetActive((bool) setActive);
      return cloned;
    }
  }
}
