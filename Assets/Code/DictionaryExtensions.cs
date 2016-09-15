//
// DictionaryExtensions.cs
//
// Copyright 2016 MunkyFun. All rights reserved.
//

using Core.Collections;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MfUnity.Extensions
{

    public static class DictionaryExtensions
    {
        public static List<V> GetOrCreate<K, V>(this Dictionary<K, List<V>> dict, K key) where V : class
        {
            List<V> value;
            if (!dict.TryGetValue(key, out value))
            {
                value = new List<V>();
                dict[key] = value;
            }
            return value;
        }

        public static V GetOrCreate<K, V>(this Dictionary<K, V> dict, K key, Func<V> constructor) where V : class
        {
            V value;
            if (!dict.TryGetValue(key, out value))
            {
                value = constructor();
                dict[key] = value;
            }
            return value;
        }
        public static V GetOrCreate<K, V>(this Dictionary<K, V> dict, K key) where V : class, new()
        {
            V value;
            if (!dict.TryGetValue(key, out value))
            {
                value = new V();
                dict[key] = value;
            }
            return value;
        }

        public static V GetOrDefault<K, V>(this Dictionary<K, V> dict, K key, V defaultValue = default(V))
        {
            V value;
            return dict.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static V GetOrNew<K, V>(this Dictionary<K, V> dict, K key) where V : new()
        {
            V value;
            return dict.TryGetValue(key, out value) ? value : new V();
        }

        public static V GetOrNew<K, V>(this Dictionary<K, V> dict, K key, Func<V> constructor) where V : class
        {
            V value;
            return dict.TryGetValue(key, out value) ? value : constructor();
        }

        public static K GetKey<K, V>(this Dictionary<K, V> dict, V value, K defaultReturn = default(K)) where V : class
        {
            foreach (var kvp in dict)
            {
                if (kvp.Value == value)
                    return kvp.Key;
            }
            return defaultReturn;
        }

        // 2 key dictionaries
        public static V GetOrNew<K1, K2, V>(this Dictionary<K1, K2, V> dict, K1 key1, K2 key2) where V : new()
        {
            return dict.GetOrNew(new Tuple<K1, K2>(key1, key2));
        }

        public static V GetOrDefault<K1, K2, V>(this Dictionary<K1, K2, V> dict, K1 key1, K2 key2, V defaultValue = default(V))
        {
            V value;
            return dict.TryGetValue(new Tuple<K1, K2>(key1, key2), out value) ? value : defaultValue;
        }

        public static V GetOrCreate<K1, K2, V>(this Dictionary<K1, K2, V> dict, K1 key1, K2 key2) where V : class, new()
        {
            return dict.GetOrCreate(new Tuple<K1, K2>(key1, key2));
        }


        // 3 key dictionaries
        public static V GetOrCreate<K1, K2, k3, V>(this Dictionary<K1, K2, k3, V> dict, K1 key1, K2 key2, k3 key3) where V : class, new()
        {
            return dict.GetOrCreate(new Tuple<K1, K2, k3>(key1, key2, key3));
        }

        public static void RemoveWhere<K, V>(this Dictionary<K, V> dict, Func<K, bool> predicate)
        {
            if (dict.Count > 0)
            {
                var duplicates = new List<K>();
                foreach (var kvp in dict)
                {
                    if (predicate(kvp.Key))
                        duplicates.Add(kvp.Key);
                }
                foreach (var duplicate in duplicates)
                {
                    dict.Remove(duplicate);
                }
            }
        }

        public static void RemoveWhere<K, V>(this Dictionary<K, V> dict, Func<K, V, bool> predicate)
        {
            if (dict.Count > 0)
            {
                var duplicates = new List<K>();
                foreach (var kvp in dict)
                {
                    if (predicate(kvp.Key, kvp.Value))
                        duplicates.Add(kvp.Key);
                }
                foreach (var duplicate in duplicates)
                {
                    dict.Remove(duplicate);
                }
            }
        }

        public static void DebugPrint<K, V>(this Dictionary<K, V> dict)
        {
    #if UNITY_EDITOR
            if (dict != null)
            {
                var msg = new StringBuilder("Dictionary.DebugPrint(): " + dict.GetType().Name + "\n");
                foreach (KeyValuePair<K, V> kvp in dict)
                {
                    msg.Append("Key: " + kvp.Key + "  Value: " + kvp.Value + "\n");
                }
                Debug.Log(msg.ToString());
            }
    #endif
        }
    }
    
}
