using Godot;
using System;
using System.Linq;
using System.Diagnostics;
using Godot.Collections;

public class UtilityFunctions : Node
{
//*********************************************************************************************

//FILE HANDLING

//	Returns a Dictionary of the file's contents (Assuming the file is in JSON format)
	public static Dictionary readFile(String filePath)
	{
//		Open the File
		File fileLoader = new File();
		fileLoader.Open(filePath, File.ModeFlags.Read);
//		Get the contents
		Dictionary data = null;
		try
		{
//			Collect the text from the entire file and turn it into 1 line
			String contents = oneLine(fileLoader.GetAsText());
//			Parse the one line string into Dictionary
			data = (Dictionary)JSON.Parse(contents).Result;	
		} catch (Exception e)
		{
			GD.Print(e);
			GD.Print("File is empty, returning null");
		}

		fileLoader.Close();
		return data;
	}
	
	
//	Writes the data from the dictionary into the filePath, generates file, if file doesn't exist
	public static void writeFile(String filePath, Dictionary data)
	{
		File dataFile = new File();
		dataFile.Open(filePath, File.ModeFlags.Write);
//		Append Text to file
		dataFile.StoreLine(JSON.Print(data));
		dataFile.Close();
	}
	
	
//	Returns true if file exists
	public static bool fileExists(String filePath)
	{
		File f = new File();
		return f.FileExists(filePath);
	}
	
//*********************************************************************************************

//STRING HANDLING
	
	//	Makes the entire string into 1 line
	public static String oneLine(String fullString)
	{
//		\n, \r & \r\n are newlines depending on OS
//		\t for tabs & " " for spaces
		String newString = fullString.Replace("\n", "")
							.Replace("\r", "")
							.Replace("\r\n", "")
							.Replace("\t", "");
		return newString;
	}
	
	
//*********************************************************************************************
	
//DATA HANDLING (converters and shit)
//	Generates a string of Child Names form parent node
	public static String[] childNameToStringArr(Node parent)
	{
		String[] output = new String[parent.GetChildCount()];
		for(int i = 0; i < output.Length; i++)
		{
			output[i] = parent.GetChild(i).Name;
		}
		return output;
	}
	
//*********************************************************************************************

//PROCESS HANDLING
//	public static void openFile(String filePath)
//	{
//		Process.Start(filePath);
//	}
	
	
//*********************************************************************************************
	
//CLASS HANDLING becuz godotsharp is down

	public static void showMethods(Type type, bool toFile=false)
	{
		int i = 0;
		Dictionary methods = new Dictionary();
		foreach (var method in type.GetMethods())
		{
			var parameters = method.GetParameters();
			var parameterDescriptions = string.Join
				(", ", method.GetParameters()
							 .Select(x => x.ParameterType + " " + x.Name)
							 .ToArray());

			String combined = method.ReturnType + " " +
					 method.Name + "(" +
					 parameterDescriptions + ")";
			i++;
			if(toFile)
			{
				methods.Add(i + " " + method.Name, combined);
				continue;
			}
			GD.Print(combined);
		}
		
		writeFile("user://methodAsked.txt", methods);
	}
	
	
	public static void showFields(Type type)
	{
		foreach (var field in type.GetFields())
		{
			GD.Print(field.FieldType + " " +
					 field.Name);
		}
	}
}
