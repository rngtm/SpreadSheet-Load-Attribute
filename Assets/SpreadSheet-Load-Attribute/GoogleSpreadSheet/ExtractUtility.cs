namespace GoogleSpreadSheet
{
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;

    public class ExtractUtility : MonoBehaviour
    {
        /// <summary>
        /// 変数・プロパティの一括取得
        /// </summary>
        public static IEnumerable<MemberData> ExtractMembers(object obj)
        {
            var type = obj.GetType();
            var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

            foreach (var member in members)
            {
                if (member.MemberType == MemberTypes.Field)
                {
                    var field = (FieldInfo)member;
                    yield return new MemberData
                    {
                        Name = field.Name,
                        Value = field.GetValue(obj),
                    };
                    continue;
                }

                if (member.MemberType == MemberTypes.Property)
                {
                    var property = (PropertyInfo)member;
                    yield return new MemberData
                    {
                        Name = property.Name,
                        Value = property.GetValue(obj, null),
                    };
                    continue;
                }
            }
        }
        
        /// <summary>
        /// Propertyかどうか
        /// </summary>
        private static bool IsProperty(MethodInfo methodInfo)
        {
            switch (methodInfo.Name.Split('_')[0])
            {
                case "get":
                case "set":
                    return true;
                default:
                    return false;
            }
        }
    }
}
