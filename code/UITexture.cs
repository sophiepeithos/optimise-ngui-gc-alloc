public class UITexture : UIBasicSprite
{
	public override void OnFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
	{
		Texture tex = mainTexture;
		if (tex == null) return;

		Rect outer = new Rect(mRect.x * tex.width, mRect.y * tex.height, tex.width * mRect.width, tex.height * mRect.height);
		Rect inner = outer;
		Vector4 br = border;
		inner.xMin += br.x;
		inner.yMin += br.y;
		inner.xMax -= br.z;
		inner.yMax -= br.w;

		float w = 1f / tex.width;
		float h = 1f / tex.height;

		outer.xMin *= w;
		outer.xMax *= w;
		outer.yMin *= h;
		outer.yMax *= h;

		inner.xMin *= w;
		inner.xMax *= w;
		inner.yMin *= h;
		inner.yMax *= h;

		int offset = verts.Count;
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
