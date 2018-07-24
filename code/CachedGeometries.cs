using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CachedGeometries
{
    public static SortedDictionary<int, Stack<List<Vector2>>> cachedListsOfVector2List = new SortedDictionary<int, Stack<List<Vector2>>>();
    public static SortedDictionary<int, Stack<List<Vector3>>> cachedListsOfVector3List = new SortedDictionary<int, Stack<List<Vector3>>>();
    public static SortedDictionary<int, Stack<List<Color>>> cachedListsOfColorList = new SortedDictionary<int, Stack<List<Color>>>();

    public static void PushToCachedGeometries(UIGeometry geometry)
    {
        PushToCachedGeometries(geometry.verts, geometry.uvs, geometry.cols, geometry.mRtpVerts);
    }

    static void PushToCachedGeometries<T>(SortedDictionary<int, Stack<List<T>>> cache, List<T> source)
    {
        Stack<List<T>> listsOfTList;
        if (!cache.TryGetValue(source.Capacity, out listsOfTList))
        {
            cache[source.Capacity] = (listsOfTList = new Stack<List<T>>());
        }
        listsOfTList.Push(source);
    }

    public static void PushToCachedGeometries(List<Vector3> verts, List<Vector2> uvs, List<Color> cols, List<Vector3> mRtpVerts = null)
    {
        PushToCachedGeometries(cachedListsOfVector3List, verts);
        PushToCachedGeometries(cachedListsOfVector2List, uvs);
        PushToCachedGeometries(cachedListsOfColorList, cols);
        if (mRtpVerts != null)
        {
            PushToCachedGeometries(cachedListsOfVector3List, mRtpVerts);
        }
    }

    public static void PullFromCachedGeometries(int vertexCount, UIGeometry geometry)
    {
        PullFromCachedGeometries(vertexCount, ref geometry.verts, ref geometry.uvs, ref geometry.cols, ref geometry.mRtpVerts);
    }

    static void PullFromCachedGeometries<T>(int vertexCount, SortedDictionary<int, Stack<List<T>>> cache, ref List<T> source)
    {
        foreach(var pair in cache)
        {
            if (pair.Key >= vertexCount && pair.Value.Count > 0)
            {
                source = pair.Value.Pop();
                source.Clear();
                return;
            }
        }
        if (source.Capacity < vertexCount)
        {
            source.Capacity = vertexCount;
        }
    }

    public static void PullFromCachedGeometries(int vertexCount, ref List<Vector3> verts, ref List<Vector2> uvs, ref List<Color> cols, ref List<Vector3> mRtpVerts)
    {
        PullFromCachedGeometries(vertexCount, cachedListsOfVector3List, ref verts);
        if (mRtpVerts != null)
        {
            PullFromCachedGeometries(vertexCount, cachedListsOfVector3List, ref mRtpVerts);
        }
        PullFromCachedGeometries(vertexCount, cachedListsOfVector2List, ref uvs);
        PullFromCachedGeometries(vertexCount, cachedListsOfColorList, ref cols);
    }
}
