using System.Collections.Generic;
using UnityEngine;

public class CachedGeometries
{
    static int WhichPowerOfTwo(uint input)
    {
        if (input == 0) return -1;
        else if (input <= 1) return 0;
        else if (input <= 2) return 1;
        else if (input <= 4) return 2;
        else if (input <= 8) return 3;
        else if (input <= 16) return 4;
        else if (input <= 32) return 5;
        else if (input <= 64) return 6;
        else if (input <= 128) return 7;
        else if (input <= 256) return 8;
        else if (input <= 512) return 9;
        else if (input <= 1024) return 10;
        else if (input <= 2048) return 11;
        else if (input <= 4096) return 12;
        else if (input <= 8192) return 13;
        else if (input <= 16384) return 14;
        else if (input <= 32768) return 15;
        else if (input <= 65536) return 16;
        else return -1;
    }

    const int SMALL_LIST_COUNT = 10;
    const int smallListCapacityLimit = 2 << (SMALL_LIST_COUNT - 2);

    public static Stack<List<Vector2>>[] cachedListsOfVector2List = new Stack<List<Vector2>>[SMALL_LIST_COUNT];
    public static LinkedList<List<Vector2>> cachedBigListsOfVector2List = new LinkedList<List<Vector2>>();
    public static Stack<List<Vector3>>[] cachedListsOfVector3List = new Stack<List<Vector3>>[SMALL_LIST_COUNT];
    public static LinkedList<List<Vector3>> cachedBigListsOfVector3List = new LinkedList<List<Vector3>>();
    public static Stack<List<Color>>[] cachedListsOfColorList = new Stack<List<Color>>[SMALL_LIST_COUNT];
    public static LinkedList<List<Color>> cachedBigListsOfColorList = new LinkedList<List<Color>>();

    public static void PushToCachedGeometries(UIGeometry geometry)
    {
        PushToCachedGeometries(geometry.verts, geometry.uvs, geometry.cols, geometry.mRtpVerts);
    }

    static void PushToCachedGeometries<T>(Stack<List<T>>[] cache, LinkedList<List<T>> bigCache, List<T> source)
    {
        if (source.Capacity > smallListCapacityLimit)
        {
            if(bigCache.Count == 0)
            {
                bigCache.AddFirst(source);
            }
            else
            {
                LinkedListNode<List<T>> newNode = null;
                for (var node = bigCache.First; node != null; node = node.Next)
                {
                    if(node.Value.Capacity >= source.Capacity)
                    {
                        newNode = bigCache.AddBefore(node, source);
                        break;
                    }
                }
                if(newNode == null)
                {
                    bigCache.AddLast(source);
                }
            }
        }
        else
        {
            var index = WhichPowerOfTwo((uint)source.Capacity);
            Stack<List<T>> listsOfTList = cache[index];
            if (listsOfTList == null)
            {
                cache[index] = (listsOfTList = new Stack<List<T>>());
            }
            listsOfTList.Push(source);
        }
    }

    public static void PushToCachedGeometries(List<Vector3> verts, List<Vector2> uvs, List<Color> cols, List<Vector3> mRtpVerts = null)
    {
        PushToCachedGeometries(cachedListsOfVector3List, cachedBigListsOfVector3List, verts);
        PushToCachedGeometries(cachedListsOfVector2List, cachedBigListsOfVector2List, uvs);
        PushToCachedGeometries(cachedListsOfColorList, cachedBigListsOfColorList, cols);
        if (mRtpVerts != null)
        {
            PushToCachedGeometries(cachedListsOfVector3List, cachedBigListsOfVector3List, mRtpVerts);
        }
    }

    public static void PullFromCachedGeometries(int vertexCount, UIGeometry geometry)
    {
        PullFromCachedGeometries(vertexCount, ref geometry.verts, ref geometry.uvs, ref geometry.cols, ref geometry.mRtpVerts);
    }

    static void PullFromCachedGeometries<T>(int vertexCount, Stack<List<T>>[] cache, LinkedList<List<T>> bigCache, ref List<T> source)
    {
        if (vertexCount > smallListCapacityLimit)
        {
            for (var node = bigCache.First; node != null; node = node.Next)
            {
                if(node.Value.Capacity >= vertexCount)
                {
                    bigCache.Remove(node);
                    return;
                }
            }
        }
        else
        {
            var index = WhichPowerOfTwo((uint)vertexCount);
            if (cache[index] != null && cache[index].Count > 0)
            {
                source = cache[index].Pop();
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
        PullFromCachedGeometries(vertexCount, cachedListsOfVector3List, cachedBigListsOfVector3List, ref verts);
        if (mRtpVerts != null)
        {
            PullFromCachedGeometries(vertexCount, cachedListsOfVector3List, cachedBigListsOfVector3List, ref mRtpVerts);
        }
        PullFromCachedGeometries(vertexCount, cachedListsOfVector2List, cachedBigListsOfVector2List, ref uvs);
        PullFromCachedGeometries(vertexCount, cachedListsOfColorList, cachedBigListsOfColorList, ref cols);
    }
}
