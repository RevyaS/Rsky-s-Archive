using Godot;
using System;
using Godot.Collections;
using UF = UtilityFunctions;

public class GlobalData : Node
{
//	Const paths
	public static string archiveInfoPath = "user://archive.rd";
	
	public static string defaultTheme = "res://Resources/DefaultTheme.tres";
	public static string sansSerif14 = "res://Resources/DynamicFonts/SansSerif14.tres";
	
//	Const data
//	TAGS
	public static string[] imgTags = {"2D", "3D", "Anal", "Blonde", "Creampie", "Gangbang", 
		"Loli", "Paizuri", "Rape", "Tentacles", "Vaginal"};
	
	public static string[] imgNwTags = {"Fan Art", "Manga", "Meme", "Original", "Pixel Art",
		"Sketch", "Western Comic"};
	
//	TYPES
	public static string[] imgTypes = {"Artist CG", "Doujinshi", "Game CG",
		"Image Set", "Manga", "Western"};
	
	
//	Components
	public static Timer timer;
	
	public override void _Ready()
	{
		timer = new Timer();
		AddChild(timer);
		loadArchive();
	}
	
	//	Load Functions
	public static void loadArchive()
	{
//		Attempt to retreieve archive location
		Directory dir = new Directory();
		GD.Print("Checking archive");
		if(!dir.FileExists(archiveInfoPath)) return;

//		Load Archive
		Dictionary archiveData = UF.readFile(archiveInfoPath);
		String newPath = archiveData["Prev"].ToString() + "/Archive.rt";
		
//		Check if db exists
		if(!UF.fileExists(newPath)) return;
		
		DataManager.Singleton.loadDB(newPath);
		
		GD.Print("End Checking");
	}
}
