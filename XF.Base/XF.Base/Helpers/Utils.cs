﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace XF.Base.Helpers
{
    public static class Utils
    {

        public static T GetValue<T>(this IDictionary dict, string key)
        {
            if (key == null || dict == null) return default;
            if (!dict.Contains(key)) return default;
            var value = dict[key];
            return value is T variable ? variable : default;
        }

        public static bool TryGetValue<T>(this IDictionary dict, string key, out T value)
        {
            bool Return(out T outValue)
            {
                outValue = default;
                return false;
            }

            if (key == null || dict == null) return Return(out value);
            if (!dict.Contains(key)) return Return(out value);
            var valueObject = dict[key];
            if (valueObject is T variable)
            {
                value = variable;
                return true;
            }
            return Return(out value);
        }

        public static bool IsNullOrEmpty<T>(this IList<T> list) where T : class
        {
            return list == null || !list.Any();
        }
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> list) where T : class
        {
            return list == null || !list.Any();
        }
        public static bool IsNullOrEmpty(this IEnumerable<char> list)
        {
            return list == null || !list.Any();
        }
        public static bool IsNullOrEmpty(this IEnumerable list)
        {
            return list == null || !list.GetEnumerator().MoveNext();
        }
    }
}
