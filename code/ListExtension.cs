using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtension {
    public static void GrowIfMust<T>(this List<T> list, int newCount)
    {
        int minimumSize = list.Count + newCount;
        if (minimumSize > list.Capacity)
            list.Capacity = Math.Max(Math.Max(list.Capacity * 2, 4), minimumSize);
    }
}
