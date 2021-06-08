using Godot;
using System;
using System.Data.SQLite;
using GC = Godot.Collections;
using UF = UtilityFunctions;

public enum AdminActions {NewArchive}

public class Admin : ColorRect
{
//	Components
	FileDialog fd;
	Label tooltip;
	Button uploadImage;
	
//	Values
	AdminActions state;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{	
		fd = GetNode<FileDialog>("FileDialog");
		tooltip = GetNode<Label>("VBoxContainer/Label");
		uploadImage = GetNode<Button>("VBoxContainer/UploadImages");
		checkArchive();
	}
	
	
//	Init Functions
//	Generates Archive Root required folder and adds Archive.rt database
	private void generateArchive(String path)
	{
		Directory dir = new Directory();
//		Generate Games folders
		dir.MakeDir(path + "\\Games");
//		Generate Video folders
		dir.MakeDir(path + "\\Videos");
//		Generate Image folders
		dir.MakeDir(path + "\\Images");
//		Generate Archive.rt database
		String dbPath = path + "\\Archive.rt";
		SQLiteConnection.CreateFile(dbPath);
		GD.Print("Generated Fields Done");
		tooltip.Text = "Generating Archive";
//		Initialize database
		DataManager.Singleton.initDB(dbPath);
		
//		Generate file to store this as prev archive
		GC.Dictionary prev = new GC.Dictionary();
		prev.Add("Prev", path);
		UF.writeFile(GlobalData.archiveInfoPath, prev);
//		Recheck Archive
		
		GlobalData.loadArchive();
		checkArchive();
	}
	

//	Signal Functions
	private void selectFolder()
	{

	}
	
	
	private void selectedDir(String path)
	{
		if(state == AdminActions.NewArchive)
		{
			GD.Print("Selected new archive root: " + path);
			if(isValidRoot(path))
			{
				generateArchive(path);
			}
		}

		else
			GD.Print("Selected " + path);
	}


//	Creates a new archive when New Archive button is pressed
	private void newArchive()
	{
		fd.WindowTitle = "Select Archive Root";
		fd.Show();
		
		state = AdminActions.NewArchive;
	}


//	Sub functions
	private void checkArchive()
	{
//		Check if we can load archive
		tooltip.Text = "No Archive loaded";
		uploadImage.Disabled = true;
		
//		Check if No db is loaded	
		if(DataManager.Singleton.isDBNull()) return;
		GD.Print("DB Exists");
		
		tooltip.Text = "";
		uploadImage.Disabled = false;
	}


//	Checks if directory doesn't have database yet
	private bool isValidRoot(String path)
	{
		Directory dir = new Directory();
		String archiveFile = path + "/Archive.rt";
		GD.Print(archiveFile);
		if(UF.fileExists(archiveFile))
		{
			GD.Print("Root already exists");
			return false;
		}
		if(dir.DirExists(path + "/Games")) return false;
		if(dir.DirExists(path + "/Images")) return false;
		if(dir.DirExists(path + "/Videos")) return false;
		GD.Print("Valid Root");
		
		return true;
	}
}
