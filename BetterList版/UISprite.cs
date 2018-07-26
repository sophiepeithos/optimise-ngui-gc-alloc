public class UISprite : UIBasicSprite
{
	public override void OnFill (BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols)
	{
		Texture tex = mainTexture;
		if (tex == null) return;

		if (mSprite == null) mSprite = atlas.GetSprite(spriteName);
		if (mSprite == null) return;

		Rect outer = new Rect(mSprite.x, mSprite.y, mSprite.width, mSprite.height);
		Rect inner = new Rect(mSprite.x + mSprite.borderLeft, mSprite.y + mSprite.borderTop,
			mSprite.width - mSprite.borderLeft - mSprite.borderRight,
			mSprite.height - mSprite.borderBottom - mSprite.borderTop);

		outer = NGUIMath.ConvertToTexCoords(outer, tex.width, tex.height);
		inner = NGUIMath.ConvertToTexCoords(inner, tex.width, tex.height);

		int offset = verts.size;
		Fill(verts, uvs, cols, outer, inner);

		if (onPostFill != null)
#if OPTIMISE_NGUI_GC_ALLOC
            //because we might change geometry's verts uvs cols value in Fill
            onPostFill(this, offset, geometry.verts, geometry.uvs, geometry.cols);
#else
            onPostFill(this, offset, verts, uvs, cols);
#endif
    }


}
