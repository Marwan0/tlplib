﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using com.tinylabproductions.TLPLib.validations;
using JetBrains.Annotations;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using ErrorType = com.tinylabproductions.TLPLib.Utilities.Editor.ObjectValidator.Error.Type;

// ReSharper disable ClassNeverInstantiated.Local, NotNullMemberIsNotInitialized, NotAccessedField.Local
#pragma warning disable 169

namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  public class ObjectValidatorTest {
    #region Test Classes

    class PublicField : MonoBehaviour {
      public GameObject field;
    }

    class NotNullPublicFieldObject : MonoBehaviour {
      [NotNull] public Object field;
    }

    class NotNullPublicField : MonoBehaviour {
      [NotNull] public GameObject field;
    }

    class NotNullSerializedField : MonoBehaviour {
      [NotNull, SerializeField] GameObject field;
      public void setField (GameObject go) { field = go; }
    }

    class PublicFieldExtended : PublicField {
      [NotNull, SerializeField] GameObject field2;
    }

    class NotNullPublicFieldExtended : NotNullPublicField { }

    class NotNullSerializedFieldExtended : NotNullSerializedField { }

    class NonSerializedField : MonoBehaviour {
      [NotNull, NonSerialized] GameObject field;
      public void setField (GameObject go) { field = go; }
    }

    class ArrayWithNulls : MonoBehaviour {
      public GameObject[] field;
    }

    class ListNotEmpty : MonoBehaviour {
      [NonEmpty] public List<InnerNotNull> field;
    }

    class NotNullArray : MonoBehaviour {
      [NotNull] public GameObject[] field;
    }

    class NullReferenceList : MonoBehaviour {
      [NotNull] public List<InnerNotNull> field;
    }

    [Serializable]
    public struct InnerNotNull {
      [NotNull] public GameObject field;  
    }

    class NullReferencePublicField : MonoBehaviour {
      public InnerNotNull field;
    }

    class NullReferenceSerializedField : MonoBehaviour {
      [SerializeField] InnerNotNull field;
      public void setField (InnerNotNull inn) { field = inn; }
    }

    class TextFieldTypeNotTag : MonoBehaviour {
#pragma warning disable 649
      [TextField(TextFieldType.Area)]
      public string field;
#pragma warning restore 649
    }

    class TextFieldTypeTag : MonoBehaviour {
      [TextField(TextFieldType.Tag)]
      public string field;
    }

    #endregion

    #region Missing References

    [Test] public void WhenMissingComponent() {
      var go = AssetDatabase.GetAllAssetPaths().find(s => 
        s.EndsWithFast("TLPLib/Editor/Test/Utilities/ObjectValidatorTestGameObject.prefab")
      ).map(AssetDatabase.LoadMainAssetAtPath).get;
      var errors = ObjectValidator.check(new ObjectValidator.CheckContext(), new [] { go });
      noErrorsOrExistsErrorOfType(errors, ErrorType.MissingComponent.some());
    }

    [Test] public void WhenMissingReference() => test<PublicField>(
      a => {
        a.field = new GameObject();
        Object.DestroyImmediate(a.field);
      },
      ErrorType.MissingReference.some()
    );

    [Test] public void WhenReferenceNotMissing() => 
      test<PublicField>(a => {
        a.field = new GameObject();
      });

    [Test] public void WhenMissingReferenceInner() => test<NullReferencePublicField>(
      a => {
        a.field.field = new GameObject();
        Object.DestroyImmediate(a.field.field);
      },
      ErrorType.MissingReference.some()
    );

    [Test] public void WhenReferenceNotMissingInner() => test<NullReferencePublicField>(
      a => {
        a.field.field = new GameObject();
      }
    );

    #endregion

    #region Public/Serialized Field

    [Test] public void WhenNotNullPublicField() => test<NotNullPublicField>(
      errorType: ErrorType.NullReference.some()
    );

    [Test] public void WhenNotNullPublicFieldSet() => 
      test<NotNullPublicField>(a => {
        a.field = new GameObject();
      });

    [Test] public void WhenNotNullSerializedField() => test<NotNullSerializedField>(
      errorType: ErrorType.NullReference.some()
    );

    [Test] public void WhenPublicFieldExtended() => test<PublicFieldExtended>(
      errorType: ErrorType.NullReference.some()
    );

    [Test] public void WhenNotNullPublicFieldExtended() => test<NotNullPublicFieldExtended>(
      errorType: ErrorType.NullReference.some()
    );

    [Test] public void WhenNotNullSerializedFieldExtended() => test<NotNullSerializedFieldExtended>(
      errorType: ErrorType.NullReference.some()
    );

    [Test] public void WhenNotNullPublicFieldObjectSet() => 
      test<NotNullPublicFieldObject>(
        a => {
          a.field = new Object();
        },
        ErrorType.NullReference.some()
      );

    [Test] public void WhenNotNullSerializedFieldSet() => 
      test<NotNullSerializedField>(a => {
        a.setField(new GameObject());
      });

    #endregion

    #region Array/List

    [Test] public void WhenArrayWithNulls() => test<ArrayWithNulls>(
      a => {
        a.field = new [] { new GameObject(), null, new GameObject() };
      }
    );

    [Test] public void WhenNotNullArray() => test<NotNullArray>(
      a => {
        a.field = new [] { new GameObject(), null, new GameObject() };
      },
      ErrorType.NullReference.some()
    );

    [Test] public void WhenReferenceListEmpty() => test<ListNotEmpty>(
      a => {
        a.field = new List<InnerNotNull>();
      },
      ErrorType.EmptyCollection.some()
    );

    [Test] public void WhenReferenceListNotEmpty() => test<ListNotEmpty>(
      a => {
        var inner = new InnerNotNull { field = new GameObject() };
        a.field = new List<InnerNotNull> { inner };
      }
    );

    [Test] public void WhenNullReferenceList() => test<NullReferenceList>(
      a => {
        a.field = new List<InnerNotNull> { new InnerNotNull() };
      },
      ErrorType.NullReference.some()
    );

    [Test] public void WhenNullReferenceListSet() => test<NullReferenceList>(
      a => {
        var inner = new InnerNotNull { field = new GameObject() };
        a.field = new List<InnerNotNull> { inner };
      }
    );

    #endregion

    [Test] public void WhenNonSerializedFieldIsNotSet() => test<NonSerializedField>();

    [Test] public void WhenNonSerializedFieldIsSet() => 
      test<NonSerializedField>(a => {
        a.setField(new GameObject());
      });

    [Test] public void WhenNullInsideMonoBehaviorPublicField() => 
      test<NullReferencePublicField>(
        errorType: ErrorType.NullReference.some()
      );

    [Test] public void WhenNullInsideMonoBehaviorPublicFieldSet() => 
      test<NullReferencePublicField>(a => {
        a.field = new InnerNotNull {field = new GameObject()};
      });

    [Test] public void WhenNullInsideMonoBehaviorSerializedField() => 
      test<NullReferenceSerializedField>(
        errorType: ErrorType.NullReference.some()
      );

    [Test] public void WhenNullInsideMonoBehaviorSerializedFieldSet() => 
      test<NullReferenceSerializedField>(a => {
        a.setField(new InnerNotNull {field = new GameObject()});
      });

    #region [TextField(TextFieldType.Tag)]

    [Test] public void WhenTextFieldTypeNotTag() => test<TextFieldTypeNotTag>();

    [Test] public void WhenBadTextFieldValue() =>
      test<TextFieldTypeTag>(a => {
          a.field = "";
        },
        ErrorType.TextFieldBadTag.some()
      );

    [Test] public void WhenGoodTextFieldValue() => test<TextFieldTypeTag>(a => {
      a.field = UnityEditorInternal.InternalEditorUtility.tags.First();
    });

    #endregion

    static void test<A>(
      Act<A> setupA = null,
      Option<ErrorType> errorType = new Option<ErrorType>()
    ) where A : Component {
      var go = new GameObject();
      var a = go.AddComponent<A>();
      setupA?.Invoke(a);
      var errors = ObjectValidator.check(new ObjectValidator.CheckContext(), new Object[] { go });
      noErrorsOrExistsErrorOfType(errors, errorType);
    }

    static void noErrorsOrExistsErrorOfType(ImmutableList<ObjectValidator.Error> errors, Option<ErrorType> errorType) {
      errorType.voidFold(
        () => errors.shouldBeEmpty(),
        type => errors.shouldMatch(t => t.Exists(x => x.type == type))
      );
    }
  }
}
