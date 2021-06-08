using Godot;
using System;
using Godot.Collections;

public class AlbumImage : TextureButton
{
	public ImageTexture Texture;
	public String FilePath;
	
	public AlbumImage(int size, String fileLoc):this()
	{		
		ImageTexture imgText = ImageFunctions.Singleton.getImageTexture(fileLoc);
//		imgText.CreateFromImage(img);
		RectMinSize = new Vector2(size, size);
		TextureNormal = imgText;
		Texture = imgText;
		FilePath = fileLoc;
	}
	
	public AlbumImage() {
//		GD.Print("Calling base constructor");
		Expand = true;
		StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered;
	}
	
//DRAG AND DROP
	
//	The data to be sent when dragged
	public override object GetDragData(Vector2 pos)
	{
//		Create drag texture preview
		TextureRect preview = new TextureRect();
		preview.Expand = true;
		preview.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		preview.Texture = TextureNormal;
		Vector2 prevSize = new Vector2(100,100);
		preview.RectMinSize = prevSize;
		
//		Generate a control node to have an offset half of the size of texture preview
		Control offset = new Control();
		offset.AddChild(preview);

		preview.RectPosition = prevSize * -0.5f;
		
		SetDragPreview(offset);
		
		return this;
	}


//	If this returns true, it will send the data
	public override bool CanDropData(Vector2 pos, object mover)
	{
		if(!(mover is AlbumImage)) return false;
		return (mover as AlbumImage).GetIndex() != GetIndex();
	}
	
	
//	If it receives data
	public override void DropData(Vector2 pos, object mover)
	{
		GetParent().MoveChild(mover as AlbumImage, GetIndex());
	} 
}
