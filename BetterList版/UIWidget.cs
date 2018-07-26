public class UIWidget : UIRect
{
#if OPTIMISE_NGUI_GC_ALLOC
    void OnDestroy () 
    { 
        if (geometry.verts.buffer != null && geometry.verts.buffer.Length > 0)
        {
            CachedGeometries.PushToCachedGeometries(geometry); 
        }
        RemoveFromPanel(); 
    }
#else
    void OnDestroy() { RemoveFromPanel(); }
#endif
}
