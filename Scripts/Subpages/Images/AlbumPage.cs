using Godot;
using System;
using System.Diagnostics;
using GC = Godot.Collections;

public enum AlbumControlsEnum{First, Prev, Next, Last};

public class AlbumPage : Control
{
//	Components
	Label title, artist, tags, types, pageSize, lastUpdate,
		  pageCount;
	TextureRect cover;
	GridContainer imgGrid;
	
//	Data
	//Stores the id
	int id;
//	How many will be presented by the grid
	const int div = 13;
	const int albumImgSize = 205;
//	Curr Page loaded (starts with 0)
	int currPage = 0;
	int pages = 0;
	GC.Array<String> filePaths;
	
	[Signal]
	public delegate void AlbumDeleted();

	public override void _Ready()
	{
		cover = GetNode<TextureRect>("ScrollContainer/VBoxContainer/Top/Cover");
		title = GetNode<Label>("ScrollContainer/VBoxContainer/Title");
		artist = GetNode<Label>("ScrollContainer/VBoxContainer/Top/Details/Artist");
		tags = GetNode<Label>("ScrollContainer/VBoxContainer/Top/Details/Tags");
		types = GetNode<Label>("ScrollContainer/VBoxContainer/Top/Details/Types");
		pageSize = GetNode<Label>("ScrollContainer/VBoxContainer/Top/Details/PageSize");
		lastUpdate = GetNode<Label>("ScrollContainer/VBoxContainer/Top/Details/LastUpdate");
		pageCount = GetNode<Label>("ScrollContainer/VBoxContainer/Control/Controls/Label");
		imgGrid = GetNode<GridContainer>("ScrollContainer/VBoxContainer/ImgGrid");
		
		filePaths = new GC.Array<String>();
	}
	
	
//	Loads everything
	public void load(GC.Dictionary data, ImageTexture coverTex)
	{
//		Get folder path
		String albumPath = DataManager.imgPath + "/" + data["ID"];
		id = (int)data["ID"];
		GD.Print(albumPath);
		
//		Show info
		title.Text = data["Title"].ToString();
		artist.Text = "Arstist: " + data["Artist"].ToString();
		tags.Text = "Tags: " + data["Tags"].ToString();
		types.Text = "Types: " + data["Types"].ToString();
		pageSize.Text = "Pages: " + data["Size"].ToString();
		lastUpdate.Text = "Last Updated: " + data["LastUpdate"].ToString();
		
		cover.Texture = coverTex;
		
//		Load images
		Directory dir = new Directory();
		
//		Loop through the rest of images
		dir.Open(albumPath);
		dir.ListDirBegin(true);
		
		GC.Dictionary<int, String> files = new GC.Dictionary<int, String>();
		
//		Get file paths
		for(String nextFile = dir.GetNext(); nextFile != ""; nextFile = dir.GetNext())
		{
			String filePath = albumPath + "/" + nextFile;
			int fileIndex = Convert.ToInt32(nextFile.BaseName());
			files.Add(fileIndex, filePath);
		}
		
		dir.ListDirEnd();
//		sort arrays
		GC.Array<int> keyArr = new GC.Array<int>(files.Keys);

		int[] sorter = new int[keyArr.Count]; 
		keyArr.CopyTo(sorter, 0);
		Array.Sort(sorter);
		keyArr = new GC.Array<int>(sorter);
		
		foreach(int key in keyArr)
			filePaths.Add(files[key]);
		
		loadImages(currPage * div, div);
		pages = Convert.ToInt32(Math.Ceiling((float)filePaths.Count / (float)div));
		pageCount.Text = (currPage + 1) +"/"+pages;
	}
	
	
	// Opposite of load, when Delete Album is pressed
	private void deleteAlbum()
	{
		DataManager.Singleton.delete(CategoryEnum.Images, id);
		EmitSignal("AlbumDeleted");
		QueueFree();
	}

//	When album controls are clicked
	private void albumControl(AlbumControlsEnum control)
	{	
		switch(control)
		{
			case AlbumControlsEnum.Next:
				if(currPage >= pages - 1)return;
				currPage++;
				break;
			case AlbumControlsEnum.Prev:
				if(currPage <= 0)return;
				currPage--;
				break;
			case AlbumControlsEnum.First:
				currPage = 0;
				break;
			case AlbumControlsEnum.Last:
				currPage = pages - 1;
				break;
		}
//		Change buttons
//		first
		GetNode<Button>("ScrollContainer/VBoxContainer/Control/Controls/First").Disabled = currPage == 0;
//		prev
		GetNode<Button>("ScrollContainer/VBoxContainer/Control/Controls/Prev").Disabled = currPage == 0;
//		next
		GetNode<Button>("ScrollContainer/VBoxContainer/Control/Controls/Next").Disabled = currPage == pages-1;
//		last
		GetNode<Button>("ScrollContainer/VBoxContainer/Control/Controls/Last").Disabled = currPage == pages-1;
		
		pageCount.Text = (currPage + 1) +"/"+pages;
		loadImages(currPage * div, div);
	}
	
	
//	Loads the list of file to be shown from specific indexes, 
//	div refers to amount of files to show, and must not be less than 1
	private void loadImages(int indexFrom, int div)
	{
		if(div < 1) return;
//		Clear imgGrid
		SceneManager.clearChildren(imgGrid);
//		Compute for size for Album Image
		for(int i = indexFrom; i < indexFrom + div; i++)
		{
//			Exit to avoid image out of bounds
			if(i >= filePaths.Count) return;
			
			String filePath = filePaths[i];
			AlbumImage newImg = new AlbumImage(albumImgSize, filePath);
			imgGrid.AddChild(newImg);
		
//			Connect this pressed to open Image
			newImg.Connect("pressed", this, nameof(openImage), new Godot.Collections.Array{filePath});
		}
		updateGrid();
	}
	
	
//	Changes column amount to resize	
	private void updateGrid()
	{
		if(imgGrid == null) return;	
		int hSize = (int) RectSize.x;
		imgGrid.Columns = hSize / albumImgSize;
		imgGrid.RectSize = new Vector2(hSize, imgGrid.RectSize.y);
	}
	
	
//	Open Image from process
	private void openImage(String imagePath)
	{
		Process.Start(imagePath);
	}
	
	
//	Handling GUI inputs
	public override void _Input(InputEvent @event)
	{
		if(@event is InputEventMouseButton)
		{
			InputEventMouseButton evMouse = @event as InputEventMouseButton;

//			Right click
			if(evMouse.ButtonIndex == 2 && evMouse.Pressed) QueueFree();
		}
	}

}
