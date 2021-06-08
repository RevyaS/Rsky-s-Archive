using Godot;
using System;
using System.Diagnostics;

public enum PageEnum{Admin, Images};

public class MainPage : ColorRect
{
//	Const locations
	String adminPage = "res://Pages/SubPages/Admin/Admin.tscn";
	String imagesPage = "res://Pages/SubPages/Images/Images.tscn";

//	Components
	VBoxContainer mainBody,
				  searchArea;
				
	Button btnImages;

//	The current page
	Node currPage = null;
	
	
	private void loadPage(PageEnum page)
	{	
//		Switch to Admin if noDB
		bool dbNull = DataManager.Singleton.isDBNull();
		
		toggleNav(dbNull);
		if(dbNull) page = PageEnum.Admin;
		
		GD.Print("Loading Page" + page);
		
		if(currPage != null) 
		{
			mainBody.RemoveChild(currPage);
			currPage.QueueFree();
			currPage = null;
		}
		
		Node newPage = null;

		switch(page)
		{
			case PageEnum.Admin:
				newPage = SceneManager.getSceneInstance(adminPage) as Node;
				break;
			case PageEnum.Images:
				newPage = SceneManager.getSceneInstance(imagesPage) as Node;
				break;
		}

		mainBody.AddChild(newPage);
		currPage = newPage;
	}


//GUI FUNCTIONS
	private void toggleNav(bool noDB)
	{
//		Only move to Admin page if DBisNull
		btnImages.Disabled = noDB;
	}
	
	
	private void showSearch()
	{
		TextureButton btn = GetNode<TextureButton>("VBoxContainer/HBoxContainer/Control/TextureButton");
		if(searchArea.Visible)
		{
			searchArea.Hide();	
			btn.RectRotation = 180;
		} else
		{
			searchArea.Show();
			btn.RectRotation = 0;
		}
	}


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		mainBody = GetNode<VBoxContainer>("VBoxContainer/HBoxContainer/MainBody");
		searchArea = GetNode<VBoxContainer>("VBoxContainer/HBoxContainer/SearchArea");
		
//		Nav buttons
		btnImages = GetNode<Button>("VBoxContainer/Control/Header/Images");
		
		DataManager.Singleton.Connect("DBUpdated", this, nameof(toggleNav));
		loadPage(PageEnum.Images);
	}
}
