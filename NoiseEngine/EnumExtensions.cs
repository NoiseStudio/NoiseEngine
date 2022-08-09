using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NoiseEngine;

internal static class EnumExtensions {

    public static T GetCustomAttribute<T>(this Enum value) where T : Attribute {
        return GetCustomAttributes<T>(value).First();
    }

    public static IEnumerable<T> GetCustomAttributes<T>(this Enum value) where T : Attribute {
        Type type = value.GetType();
        MemberInfo[] memberInfo = type.GetMember(value.ToString());
        object[] attributes = memberInfo[0].GetCustomAttributes(typeof(T), false);

        return attributes.Cast<T>();
    }

}
