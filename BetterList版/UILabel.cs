public class UILabel : UIWidget
{
#if OPTIMISE_NGUI_GC_ALLOC
    public static BetterList<Vector3> vertsForOnFill = new BetterList<Vector3>();
    public static BetterList<Vector2> uvsForOnFill = new BetterList<Vector2>();
    public static BetterList<Color32> colsForOnFill = new BetterList<Color32>();
#endif

	public override void OnFill (BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols)
	{
		if (!isValid) return;

		int offset = verts.size;
		Color col = color;
		col.a = finalAlpha;
		
		if (mFont != null && mFont.premultipliedAlphaShader) col = NGUITools.ApplyPMA(col);

		if (QualitySettings.activeColorSpace == ColorSpace.Linear)
		{
			col.r = Mathf.Pow(col.r, 2.2f);
			col.g = Mathf.Pow(col.g, 2.2f);
			col.b = Mathf.Pow(col.b, 2.2f);
		}

		string text = processedText;
		int start = verts.size;

		UpdateNGUIText();

		NGUIText.tint = col;
#if OPTIMISE_NGUI_GC_ALLOC
        vertsForOnFill.Clear();
        uvsForOnFill.Clear();
        colsForOnFill.Clear();
        NGUIText.Print(text, vertsForOnFill, uvsForOnFill, colsForOnFill);
        var diff = effectStyle == Effect.None ? 0 : vertsForOnFill.size - offset;
        if (effectStyle == Effect.Outline)
        {
            diff += 3 * diff;
        }
        diff += vertsForOnFill.size;
        if (geometry.verts.buffer == null || geometry.verts.buffer.Length < diff)
        {
            CachedGeometries.PullFromCachedGeometries(diff, geometry);
            verts = geometry.verts;
            uvs = geometry.uvs;
            cols = geometry.cols;
        }
        verts.AddRange(vertsForOnFill);
        uvs.AddRange(uvsForOnFill);
        cols.AddRange(colsForOnFill);
#else
        NGUIText.Print(text, verts, uvs, cols);
#endif
        NGUIText.bitmapFont = null;
#if DYNAMIC_FONT
		NGUIText.dynamicFont = null;
#endif
		// Center the content within the label vertically
		Vector2 pos = ApplyOffset(verts, start);

		// Effects don't work with packed fonts
		if (mFont != null && mFont.packedFontShader) return;

		// Apply an effect if one was requested
		if (effectStyle != Effect.None)
		{
			int end = verts.size;
			pos.x = mEffectDistance.x;
			pos.y = mEffectDistance.y;

			ApplyShadow(verts, uvs, cols, offset, end, pos.x, -pos.y);

			if (effectStyle == Effect.Outline)
			{
				offset = end;
				end = verts.size;

				ApplyShadow(verts, uvs, cols, offset, end, -pos.x, pos.y);

				offset = end;
				end = verts.size;

				ApplyShadow(verts, uvs, cols, offset, end, pos.x, pos.y);

				offset = end;
				end = verts.size;

				ApplyShadow(verts, uvs, cols, offset, end, -pos.x, -pos.y);
			}
		}

		if (onPostFill != null)
			onPostFill(this, offset, verts, uvs, cols);
	}
}
