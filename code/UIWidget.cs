public class UIWidget : UIRect
{
#if OPTIMISE_NGUI_GC_ALLOC
    void OnDestroy () 
    { 
        if (geometry.verts.Capacity > 0)
        {
            CachedGeometries.PushToCachedGeometries(geometry); 
        }
        RemoveFromPanel(); 
    }
#else
	void OnDestroy () { RemoveFromPanel(); }
#endif
}
