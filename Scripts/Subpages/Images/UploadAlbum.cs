using Godot;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using GC = Godot.Collections;
using UF = UtilityFunctions;

public enum SelectionType {Tag, Type}

public class UploadAlbum : ColorRect
{
//	Components
	TextureRect imgPreview; 
	GridContainer imgGrid;
	HBoxContainer tagOptionList, //List of options presented
				  tagList, //List of options selected
				  typeList,
				  typeOptionList;
	Label dimPreview;
	LineEdit searchTags,
			 titleInput;
	
//	Data & Operations
	Thread t;
	GC.Array<String> fileList; //List of files in queue for upload
	int progVal; //Value of uploaded progress
	AlbumImage previewed = null; //Current image previewed
	
//	Consts
	String[] validExtensions = {"png", "jpg", "jpeg", "gif"};
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
//		Components
		imgPreview = GetNode<TextureRect>("VBoxContainer/Body//ImageDetails/TextureRect");
		imgGrid = GetNode<GridContainer>("VBoxContainer/Body//VBoxContainer/ImageUpload/HBoxContainer/ScrollContainer/AlbumGrid");
		dimPreview = GetNode<Label>("VBoxContainer/Body//ImageDetails/Dimensions");
		tagOptionList = GetNode<HBoxContainer>("VBoxContainer/Body//VBoxContainer/VBoxContainer/ScrollContainer/OptionList");
		tagList = GetNode<HBoxContainer>("VBoxContainer/Body//VBoxContainer/VBoxContainer/HBoxContainer/ScrollContainer/TagList");
		searchTags = GetNode<LineEdit>("VBoxContainer/Body//VBoxContainer/VBoxContainer/Genre/LineEdit");
		titleInput = GetNode<LineEdit>("VBoxContainer/Body/VBoxContainer/Title/LineEdit");
		
		typeList = GetNode<HBoxContainer>("VBoxContainer/Body//VBoxContainer/Type/Selected/ScrollContainer/TypeList");
		typeOptionList = GetNode<HBoxContainer>("VBoxContainer/Body//VBoxContainer/Type/Selection/OptionList");
		
		showTags("");
		showTypes();
//      Connect files dropped signal first
		GetTree().Connect("files_dropped", this, nameof(filesDropped));
		
		t = new Thread();
		fileList = new GC.Array<String>();
	}


//	Adds image to grid
	private void filesDropped(String[] filesArr, int screen)
	{
		GD.Print("Files Received");
		
		GC.Array<String> files = new GC.Array<String>(filesArr);
				
//		Start counting
		if(progVal == -1) 
		{
			progVal = 0;
//			Finish the thread to add another
			t.WaitToFinish();
		}

		toggleExitButton();

//		Add them to FileList
		fileList += files;
//		Activate thread if it's not active yet else it's just processing
		if(!t.IsActive()) t.Start(this, "addImages");
		GD.Print("Finished File Processing call " + fileList.Count());
	}
	
	
//	Start adding images being sent
//	Added parameter becuz threads in Godot doesn't work without params, weird
	private void addImages(int nul)
	{	
		toggleAddAlbum(titleInput.Text);
		
//		References to manipulate progBar
		TextureProgress progBar = GetNode<TextureProgress>("VBoxContainer/Body//VBoxContainer/ImageUpload/TextureProgress");
		Label progBarLabel = progBar.GetNode<Label>("Label");
		progBar.Value = 0;
		
//		Get files
		for(int i = 0; i < fileList.Count; i++)
		{
			String file = fileList[i];
//			Check if extension is valid
			if(!validExtensions.Contains(file.Extension().ToLower())) continue;
			GD.Print("Valid Extension");

	//		Compute for rect size based on imgGrid.x - 4
			int sides = (int)imgGrid.RectSize.x/imgGrid.Columns - 4;		
	//		Generate Image for GridContainer
			AlbumImage newImg = new AlbumImage(sides, file);
//			newImg.Connect("MoveRequested", this, nameof(moveImage));

			imgGrid.AddChild(newImg);
			newImg.Connect("pressed", this, nameof(showPreview), new GC.Array{newImg});
	//		Show in preview
			showPreview(newImg);
			
//			Show upload progress
			progVal++;
			progBarLabel.Text = progVal + "/" + fileList.Count;
			progBar.Value = progVal;
			progBar.MaxValue = fileList.Count; 
		}
		
		progVal = -1;
		progBarLabel.Text = imgGrid.GetChildCount() + " Uploaded";
		fileList = new GC.Array<String>();
		toggleExitButton();
		GD.Print("Upload Complete");
//		Toggle AddAlbum
		toggleAddAlbum(titleInput.Text);
	}
	
	
//	Configures drag and drop to rearrange
	private void moveImage(Node child, int index)
	{
		imgGrid.MoveChild(child, index);
	}
	
	
	private void removeImage()
	{
		if(previewed == null) return;
		imgGrid.RemoveChild(previewed);
		previewed.QueueFree();
		previewed = null;
	}
	
//	Shows tags to add
	private void showTags(String expr)
	{
//		Clear the tags lineEdit first
		SceneManager.clearChildren(tagOptionList);
//		Search the array
		foreach(String tag in GlobalData.imgTags)
		{
//			Check if we can match our search expression
			Match m = Regex.Match(tag, expr, RegexOptions.IgnoreCase);
//			Move on if no success
			if(!m.Success) continue;
			
//			Check if a node with same name already selected
			if(tagList.HasNode(new NodePath(tag))) continue;
			
//			Generate button that will be added to tags
			generateSelectionButtons(tag, tagOptionList, SelectionType.Tag);
		}
	}
	
	
//	Shows types to add
	private void showTypes()
	{
//		Clear the tags lineEdit first
		SceneManager.clearChildren(typeOptionList);
//		Generate tags
		foreach(String type in GlobalData.imgTypes)
		{			
//			Check if a node with same name already selected
			if(typeList.HasNode(new NodePath(type))) continue;
			generateSelectionButtons(type, typeOptionList, SelectionType.Type);
		}
	}
	
	
//	Adds bth to dest, or removes them
	private void changeSelectionCont(Button btn, HBoxContainer dest, bool toAdd, SelectionType type)
	{	
		btn.GetParent().RemoveChild(btn);
		if(!toAdd)
		{
			btn.QueueFree();
			if(type == SelectionType.Type) showTypes();
			else showTags(searchTags.Text);
			return;
		}
//		Add to tag list
		dest.AddChild(btn);
//		Disconnect to add Tag
		GC.Array param = new GC.Array();
		param.Add(btn);
		param.Add(dest);
		param.Add(false);
		param.Add(type);
		btn.Disconnect("pressed", this, nameof(changeSelectionCont));	
//		Connect to removeTag
		btn.Connect("pressed", this, nameof(changeSelectionCont), param);
//		Toggle AddAlbum
		toggleAddAlbum(titleInput.Text);
	}
	
	
//	Generates buttons to dest
	private void generateSelectionButtons(String val, HBoxContainer dest, SelectionType btnType)
	{
//		Generate button that will be added to tags
		Button btn = new Button();
		btn.Theme = ResourceLoader.Load(GlobalData.defaultTheme) as Theme;
		btn.Set("custom_fonts/font", ResourceLoader.Load(GlobalData.sansSerif14) as DynamicFont);
		btn.Text = val;
		btn.Name = val;
		btn.RectMinSize = new Vector2(100, 25);
		
		GC.Array param = new GC.Array();
		param.Add(btn);
		param.Add( (btnType == SelectionType.Type) ? typeList : tagList );
		param.Add(true);
		param.Add(btnType);
		
		dest.AddChild(btn);
		btn.Connect("pressed", this, nameof(changeSelectionCont), param);
	}
	
	
//	Activates the Add Album button
	private void toggleAddAlbum(String title)
	{
//		Get add album button
		Button addAlbum = GetNode<Button>("VBoxContainer/Body/ImageDetails/Add");
		addAlbum.Disabled = true;
		if(title == "") return;
		if(typeList.GetChildCount() == 0) return;
		if(tagList.GetChildCount() == 0) return;
		if(imgGrid.GetChildCount() == 0) return;
		if(progVal != -1) return;
		addAlbum.Disabled = false;
	}
	
	
//	Disables the Exit button when thread is still being used
	private void toggleExitButton()
	{
		Button exit = GetNode<Button>("VBoxContainer/Top/Button");
		exit.Disabled = true;
		if(progVal != -1) return;
		exit.Disabled = false;
	}


//	Generate album
	private void addAlbum()
	{
//		Necessary references
		LineEdit artistInput = GetNode<LineEdit>("VBoxContainer/Body/VBoxContainer/Artist/LineEdit");

		GC.Dictionary data = new GC.Dictionary();
//		Compile info to dictionary
		data.Add("Title", titleInput.Text);
		data.Add("Artist", artistInput.Text);
//		List types into 1 string sorted
		String[] arr = UF.childNameToStringArr(typeList);
		
		Array.Sort(arr);
		data.Add("Types", String.Join(",", arr));
//		List tags into 1 string sorted
		arr = UF.childNameToStringArr(tagList);
		Array.Sort(arr);
		data.Add("Tags", String.Join(",", arr));
//		Generate a list of paths to upload
		arr = new String[imgGrid.GetChildCount()];
		for(int i = 0; i < arr.Length; i++)
		{
			arr[i] = (imgGrid.GetChild(i) as AlbumImage).FilePath;
		}
		data.Add("Files", arr);
		
		DataManager.Singleton.upload(CategoryEnum.Images, data);
		t.WaitToFinish();
		close();
	}

	
//	Shows the image preview with details
	private void showPreview(AlbumImage tex)
	{
		ImageTexture img = tex.Texture;
		imgPreview.Texture = img;
		dimPreview.Text = img.GetSize().x + "X" + img.GetSize().y + 
			" " + tex.FilePath.Extension().ToLower() + " File";
		previewed = tex;
	}
	
	private void close()
	{
		QueueFree();
	}
}
