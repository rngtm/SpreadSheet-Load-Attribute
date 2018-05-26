namespace GoogleSpreadSheet
{
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// </summary>
    /// シーンのオブジェクトへ更新をかける
    /// <summary>
    public static class SceneObjectUpdater
    {
        /// <summary>
        /// Typeに応じた変数のタイプ変換ロジック
        /// </summary>
        public static Dictionary<Type, Func<string, object>> ConvertFuncDict { get; private set; }

        private static readonly System.Type _targetAttType = typeof(GetSheetAttribute);

        [RuntimeInitializeOnLoadMethod]
        static void Initailize()
        {
            ConvertFuncDict = new Dictionary<Type, Func<string, object>>();

            // Register default actions
            RegisterInspectAction(typeof(Int32), s => int.Parse(s));
            RegisterInspectAction(typeof(String), s => s);
            RegisterInspectAction(typeof(Boolean), s => bool.Parse(s));
            RegisterInspectAction(typeof(Single), s => float.Parse(s));
            RegisterInspectAction(typeof(Vector2), s => new Vector2()); // 未実装
            RegisterInspectAction(typeof(Vector3), s => new Vector3());
            RegisterInspectAction(typeof(Vector4), s => new Vector4());
            RegisterInspectAction(typeof(Color), s => new Color(0f, 0f, 0f, 0f));
        }

        /// <summary>
        /// 変換ロジックの登録
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="action">Action.</param>
        public static void RegisterInspectAction(Type type, Func<string, object> func)
        {
            ConvertFuncDict.Add(type, func);
        }

        // ウィンドウの描画処理
        public static void UpdateObjectValues()
        {
            foreach (var component in GetAllTargetComponents())
            {
                SetValue(component);
            }
        }

        /// <summary>
        /// Registers array inspect action.
        /// </summary>
        static void ArrayTargetAction(string name, object obj)
        {
            if (obj == null)
            {
                return;
            }

            var array = (Array) obj;

            switch (array.Rank)
            {
                case 1:
                    for (int i = 0; i < array.Length; i++)
                    {
                        var value = array.GetValue(i);

                        UpdateObjectValue("Element " + i, value.GetType(), value);
                    }

                    break;
                case 2:
                    for (int i = 0; i < array.GetLength(0); i++)
                    {
                        for (int j = 0; j < array.GetLength(1); j++)
                        {
                            var value = array.GetValue(i, j);
                            UpdateObjectValue("Element " + j, value.GetType(), value);
                        }
                    }

                    break;
            }
        }


        /// <summary>
        /// ロード済みSceneのルートTransfromを返す
        /// </summary>
        /// <returns>The root transforms.</returns>
        static IEnumerable<Transform> LoadedScenesRootTransforms()
        {
            int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;
            for (int i = 0; i < sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (!scene.isLoaded)
                {
                    continue;
                }

                foreach (var go in scene.GetRootGameObjects())
                {
                    yield return go.transform;
                }
            }
        }

        /// <summary>
        /// 更新対象のコンポーネントを取得
        /// </summary>
        /// <returns>The inspectable components.</returns>
        /// <param name="transform">Transform.</param>
        static IEnumerable<Component> GetTargetComponents(Transform transform)
        {
            foreach (var co in transform.GetComponentsInChildren(typeof(Component)))
            {
                if (IsTargetComponent(co))
                {
                    yield return co;
                }
            }
        }

        /// <summary>
        /// 更新対象のコンポーネントかどうか
        /// </summary>
        /// <returns><c>true</c> if is inspectable component the specified component; otherwise, <c>false</c>.</returns>
        /// <param name="component">Component.</param>
        static bool IsTargetComponent(Component component)
        {
            if (component == null)
            {
                return false;
            }

            var type = component.GetType();
            var attrs = type.GetCustomAttributes(false);
            return attrs.Any(attr => attr.GetType() == _targetAttType);
        }

        /// <summary>
        /// 更新対象の全てのコンポーネントを取得
        /// </summary>
        /// <returns>The all inspectable components.</returns>
        static IEnumerable<Component> GetAllTargetComponents()
        {
            foreach (var transform in LoadedScenesRootTransforms())
            {
                foreach (var co in GetTargetComponents(transform))
                {
                    yield return co;
                }
            }
        }

        /// <summary>
        /// 指定したobjectの値をGoogleスプレッドシートで更新
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="type">メンバの変数型</param>
        /// <param name="_object">Object.</param>
        static void UpdateObjectValue(String name, Type type, object _object)
        {
            Debug.LogFormat("UpdateObjectValue: name = {0}  type = {1} _object = {2}", name, type, _object);

            // Unity object
            var unityObject = _object as UnityEngine.Object;
            if (unityObject != null)
            {
                return;
            }

            // Enum
            if (type.IsEnum)
            {
                return;
            }

            // null check
            if (_object == null)
            {
                return;
            }

            // object has Inspectable attribute
            if (_object.GetType().GetCustomAttributes(true).Any(attr => attr.GetType() == _targetAttType))
            {
                SetValue(_object);
                return;
            }

            // Unregistered Types
        }

        /// <summary>
        /// Extracts the inspectables.
        /// </summary>
        /// <returns>The inspectables.</returns>
        /// <param name="type">Type.</param>
        private static IEnumerable<MemberWithAttr> ExtractTargetMembers(IReflect type)
        {
            var members = type.GetMembers(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
                                          BindingFlags.Instance);

            foreach (var member in members)
            {
                var attrs = member.GetCustomAttributes(true);

                foreach (var attr in attrs)
                {
                    var a = attr as GetSheetAttribute;
                    if (a == null)
                    {
                        continue;
                    }

                    yield return new MemberWithAttr
                    {
                        member = member,
                        attr = a,
                    };
                }
            }
        }

        public static void SetValue(object obj)
        {
            var objType = obj.GetType();

            foreach (var member in ExtractTargetMembers(objType))
            {
                object value;
                string strValue = SpreadSheetLoader.GetValue(member.attr.Key);
                if (member.member.MemberType == MemberTypes.Field)
                {
                    var field = (FieldInfo) member.member;
                    if (!ConvertFuncDict.ContainsKey(field.FieldType))
                    {
                        continue;
                    }

                    value = ConvertFuncDict[field.FieldType].Invoke(strValue); // 変数の型を正しいものへ変換
                    field.SetValue(obj, value);
                    continue;
                }
                else if (member.member.MemberType == MemberTypes.Property)
                {
                    var prop = (PropertyInfo) member.member;
                    if (!ConvertFuncDict.ContainsKey(prop.PropertyType))
                    {
                        continue;
                    }

                    value = ConvertFuncDict[prop.PropertyType].Invoke(strValue); // 変数の型を正しいものへ変換
                    Debug.Log(prop.PropertyType);
                    prop.SetValue(obj, value, null);
                    continue;
                }
            }
        }
    }
    
    public class MemberWithAttr
    {
        public MemberInfo member;
        public GetSheetAttribute attr;
    }
}