using Godot;
using System;
using GC = Godot.Collections;

public class Images : ColorRect
{
//	Components
	VBoxContainer albumList;
	TextureRect preview;
	Label tagsList, pageAmount;
	Button btnAddAlbum;

//	Const strings
	String uploadAlbum = "res://Pages/SubPages/Images/UploadAlbum.tscn";
	String albumEntry = "res://Pages/SubPages/Images/Components/AlbumEntry.tscn";
	String albumPage = "res://Pages/SubPages/Images/AlbumPage.tscn";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
//		Exit if no db exists
		if(DataManager.Singleton.isDBNull()) return;
		
		albumList = GetNode<VBoxContainer>("VBoxContainer/Div/HBoxContainer/AlbumList");
		preview = GetNode<TextureRect>("VBoxContainer/Div/HBoxContainer/Preview/TextureRect");
		tagsList = GetNode<Label>("VBoxContainer/Div/HBoxContainer/Preview/Tags");
		pageAmount = GetNode<Label>("VBoxContainer/Div/HBoxContainer/Preview/Pages");
		btnAddAlbum = GetNode<Button>("VBoxContainer/Buttons/Button");
		loadAlbums();
		DataManager.Singleton.Connect("DBProcessStart", this, nameof(toggleButton), new GC.Array{btnAddAlbum, true});
	
		DataManager.Singleton.Connect("DBProcessDone", this, nameof(loadAlbums));
		
//		Connect this to DataManager to show db progress
		DataManager.Singleton.Connect("DBProgress", this, nameof(updateProgress));
	}


//	Opens the upload album page
	private void addAlbum()
	{
		SceneManager.Singleton.stackPage(uploadAlbum);
	}
	
	
//	Load and show the list of albums
	private async void loadAlbums()
	{
//		Wait 0.3s for stabilization
		GlobalData.timer.WaitTime = 0.3f;
		GlobalData.timer.OneShot = true;
		GlobalData.timer.Start();
		await ToSignal(GlobalData.timer, "timeout");		

		toggleButton(btnAddAlbum, false);
		GC.Array<GC.Dictionary> albums = DataManager.Singleton.getAlbums();
		SceneManager.clearChildren(albumList);
		GD.Print("Retrieved Albums");
//		GD.Print(albums);
		foreach(GC.Dictionary data in albums)
		{
			AlbumEntry entry = SceneManager.getSceneInstance(albumEntry) as AlbumEntry;
			albumList.AddChild(entry);
			albumList.MoveChild(entry,0);
			entry.init(data);
			
			entry.Connect("mouse_entered", this, nameof(showPreview), new GC.Array{entry});
//			Connect this to open album when pressed
			entry.Connect("pressed", this, nameof(openAlbum), new GC.Array{entry});
		}
	}
	
	
//	Opens the album and add it to the tags
	private void openAlbum(AlbumEntry entry)
	{
		AlbumPage newPage = SceneManager.getSceneInstance(albumPage) as AlbumPage;
		
		// Get data
		GC.Dictionary albumData = entry.data;
		ImageTexture cover = entry.cover;

		AddChild(newPage);
		GD.Print(newPage == null);
		newPage.load(albumData, cover);

		// Disable entry when album will be deleted
		newPage.Connect("AlbumDeleted", entry, "disable");
	}
	
	
//	Toggles button to disable
	private void toggleButton(Button btn, bool toDisable)
	{
		GD.Print("Toggling to" + toDisable);
		btn.Disabled = toDisable;
	}
	
	
//	Show preview of the AlbumEntry hovered
	private void showPreview(AlbumEntry hovered)
	{
		preview.Texture = hovered.cover;
		tagsList.Text = hovered.tags;
		pageAmount.Text = hovered.data["Size"].ToString() + " Pages";
	}
	
	
//	Shows the progress of DB processing
	private void updateProgress(int max, int val)
	{
		TextureProgress bar = GetNode<TextureProgress>("VBoxContainer/Buttons/TextureProgress");
		bar.MaxValue = max;
		bar.Value = val;
		Label lbl = bar.GetNode<Label>("Label");
		lbl.Text = "";
		if(val < max-1) lbl.Text = val + "/" + max;
		
		 
	}
}
