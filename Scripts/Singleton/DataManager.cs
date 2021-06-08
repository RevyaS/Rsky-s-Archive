using Godot;
using System;
using System.Data.SQLite;
using GC = Godot.Collections;
using UF = UtilityFunctions;
using SI = System.IO;

public enum CategoryEnum{Images, Videos};


public class DataManager : Node
{
	private string connComm = "";
	private string basePath = ""; //Folder where archie is located
	public static string imgPath = "";
	
	public static DataManager Singleton;
	
//	Data and process operations
	Thread t;
	int progressMax; //Max value for current process
	int progVal; //Current progress, -1 refers to a finished progress
	bool isInProcess;
	
	public bool IsInProcess
	{
		get{return isInProcess;}
	}
	
	public override void _Ready()
	{
		t = new Thread();
		Singleton = this;
	}
	
//	Initializes database tables
	public void initDB(String dbPath)
	{
		loadDB(dbPath);
		using(SQLiteConnection db = new SQLiteConnection(connComm))
		{
			db.Open();
			
	//		Initialize tables
			SQLiteCommand comm = new SQLiteCommand(db);
	//		Games, Images and Videos table
			comm.CommandText = @"create table Games(
				ID integer not null primary key autoincrement,
				Title text,
				Description text,
				Author text,
				ReleaseDate date,
				Type text,
				Genre text
			);
			create table Images(
				ID integer not null primary key autoincrement,
				Title text,
				Artist text,
				Types text,
				Tags text,
				LastUpdate date,
				Size integer
			);
			create table Videos(
				ID integer not null primary key autoincrement,
				Title text,
				Description text,
				Producer text,
				Type text,
				ReleaseDate date,
				Genre text
			);";
			comm.ExecuteNonQuery();
			
			db.Close();
			GD.Print("Query Ended");
		}

	}

	
//	Loads the db
	public void loadDB(String dbPath)
	{
		GD.Print("Loading " + dbPath);
		connComm = "Data Source=" + dbPath + ";Version=3;";
		GD.Print(connComm);
		
		basePath = dbPath.GetBaseDir();
		imgPath = basePath + "/Images";
		
		EmitSignal("DBUpdated", isDBNull());
	}


//GETTER FUNCTIOND	
//	Reads the database and returns an array of dictionary of albums one by one
	public GC.Array<GC.Dictionary> getAlbums()
	{
		GC.Array<GC.Dictionary> datas = new GC.Array<GC.Dictionary>();
		
		using(SQLiteConnection db = new SQLiteConnection(connComm))
		{
			db.Open();
		
			SQLiteCommand comm = new SQLiteCommand(db);
			comm.CommandText = "select * from Images";
			SQLiteDataReader rdr = comm.ExecuteReader();
			
			while(rdr.Read())
			{
				GC.Dictionary data = new GC.Dictionary();
				data.Add(rdr.GetName(0), rdr.GetInt32(0)); //ID
				data.Add(rdr.GetName(1), rdr.GetString(1)); //Title
				data.Add(rdr.GetName(2), rdr.GetString(2)); //Artist
				data.Add(rdr.GetName(3), rdr.GetString(3)); //Types
				data.Add(rdr.GetName(4), rdr.GetString(4)); //Tags
				data.Add(rdr.GetName(5), rdr.GetString(5)); //LastData
				data.Add(rdr.GetName(6), rdr.GetInt32(6)); //Size
				datas.Add(data);
			}
			db.Close();	
		}
	
		return datas;
	}
	

//DELETE FUNCTIONS
	// Deletes an entry along with its files
	public void delete(CategoryEnum cat, int id)
	{
		EmitSignal("DBProcessStart");
		isInProcess = true;
		
		if(t.IsActive()) 
			t.WaitToFinish();
		
		GD.Print(t.IsActive());
		switch(cat)
		{
			case CategoryEnum.Images:
				t.Start(this, "deleteAlbum", id);
				break;
				
		}
	}


	// Deletes the album from the Database
	private void deleteAlbum(int id)
	{
		String filePath = imgPath + "/" + id;
		
		//Remove id from database
		using(SQLiteConnection db = new SQLiteConnection(connComm))
		{
			db.Open();

			SQLiteCommand comm = new SQLiteCommand(db);
			comm.CommandText = $"select Size from Images where ID={id}";
			SQLiteDataReader rdr = comm.ExecuteReader();

			// Get 1st line
			rdr.Read();
//			Get file amounts 1st to show progress
			progressMax = rdr.GetInt32(0);
			rdr.Close();
			
			comm.CommandText = $"delete from Images where ID={id}";
			comm.ExecuteNonQuery();
			db.Close();
		}
	
		// delete the files that is connected to the database
		deleteDir(filePath);

		EmitSignal("DBProcessDone");
	}


// Deletes all files and within the directory and also the directory
	private void deleteDir(String dirLocation)
	{
		SI.DirectoryInfo di = new SI.DirectoryInfo(dirLocation);
		progVal = 0;
		foreach(SI.FileInfo file in di.GetFiles())
		{
			file.Delete();
			EmitSignal("DBProgress", progressMax, progVal++);
		}
		SI.Directory.Delete(dirLocation);

	}

//UPLOAD FUNCTIONS

//	General upload function
	public void upload(CategoryEnum cat, GC.Dictionary data)
	{
		EmitSignal("DBProcessStart");
		isInProcess = true;
		
		if(t.IsActive()) t.WaitToFinish();
		
		GD.Print(t.IsActive());
		switch(cat)
		{
			case CategoryEnum.Images:
				t.Start(this, "addAlbum", data);
				break;
				
		}
	}


//	Adds album
	private void addAlbum(GC.Dictionary albumData)
	{
//		Id to reference file location
		long id;
		String[] fileList = albumData["Files"] as String[];
		
		using(SQLiteConnection db = new SQLiteConnection(connComm))
		{
			db.Open();
			GD.Print("Generating Album");	
	//		Use using to avoid locking the database
			using(SQLiteCommand comm = new SQLiteCommand(db))
			{
				comm.CommandText = $@"insert into Images(Title, Artist, Types, Tags, LastUpdate, Size) values(
					'{albumData["Title"]}',
					'{albumData["Artist"]}',
					'{albumData["Types"]}',
					'{albumData["Tags"]}',
					DATE(),
					{fileList.Length}
				)";
				comm.ExecuteNonQuery();
				
		//		Get id of new entry
				id = db.LastInsertRowId;

			}
			db.Close();
		}
		
//		Create file and folder		
		String albumPath = imgPath+"/"+id;
		copyFiles(albumPath, fileList);
		EmitSignal("DBProcessDone");
		GD.Print("Album Generated");
	}


//File Copier
	private void copyFiles(String dest, String[] fileList)
	{
		Directory dir = new Directory();
		dir.MakeDir(dest);
		for(progVal = 0; progVal < fileList.Length; progVal++)
		{
			String fileDest = dest + "/" + progVal + "." + fileList[progVal].Extension();
			dir.Copy(fileList[progVal], fileDest);
			EmitSignal("DBProgress", fileList.Length, progVal);
		}
	}

//	Getters
	public bool isDBNull() => connComm == "";
	
//	Signals
//	Emits when database is either deleted or updated
	[Signal]
	public delegate void DBUpdated(bool dbNull);

//	Emits when database starts
	[Signal]
	public delegate void DBProcessStart();

//	Emits when database is done processing
	[Signal]
	public delegate void DBProcessDone();
	
//	Emits to transmit info on progress
	[Signal]
	public delegate void DBProgress(int max, int val);
	
}
