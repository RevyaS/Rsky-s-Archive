using Godot;
using System;
using Godot.Collections;

public class AlbumEntry : TextureButton
{
//	Components
	Label title,
		  artist,
		  lastUpdate,
		  types;
	public string tags;
	public ImageTexture cover;
	
	public Dictionary data;

	public void init(Dictionary data)
	{
		title = GetNode<Label>("Album/Info1/Title");		
		artist = GetNode<Label>("Album/Info2/Artist");
		types = GetNode<Label>("Album/Info1/Types/Type");
		lastUpdate = GetNode<Label>("Album/Info2/LastUpdate");
	
		title.Text = data["Title"].ToString();
		artist.Text = data["Artist"].ToString();
		lastUpdate.Text = data["LastUpdate"].ToString();
		types.Text = data["Types"].ToString();
		
		tags = data["Tags"].ToString();
		this.data = data;

//		Locate the first image file
		String fileDir = DataManager.imgPath + "/" + Convert.ToInt32(data["ID"]);
		
		Directory dir = new Directory();
		dir.Open(fileDir);
		dir.ListDirBegin(true);
		String coverPath = fileDir + "/" + dir.GetNext();
		dir.ListDirEnd();
		
		cover = ImageFunctions.Singleton.getImageTexture(coverPath);
	}


	// Disables the entry from being clicked
	private void disable()
	{
		Disabled = true;
	}

}
