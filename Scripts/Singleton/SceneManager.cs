using Godot;
using System;

public class SceneManager : Node
{
//	ChangeScene is non static so a non static referene would be useful
	public static SceneManager Singleton;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Singleton= this;
	}


//*	Returns an instance of a scene (must be converted before use)	
	public static object getSceneInstance(String ScenePath)
		=> getPackedScene(ScenePath).Instance();
	
	
//	Returns PackedScene from path
	public static PackedScene getPackedScene(String ScenePath) 
		=> (PackedScene)ResourceLoader.Load(ScenePath);
		
		
//	Stacks a page scene above view
	public Node stackPage(Node page)
	{
		GetTree().Root.AddChild(page);
		return page;
	}
	
	
//	Stacks page based on string
	public Node stackPage(String pagePath)
	{
		return stackPage(getSceneInstance(pagePath) as Node);
	}
	
	
//*	Removes all children and subchildren of a node or inheritors of node
	public static void clearChildren(Node node)
	{
		int childCount = node.GetChildCount();
		for(int i = 0; i < childCount; i++)
		{
			Node child = (Node)node.GetChild(0);
			node.RemoveChild(child);
			child.QueueFree();
		}
	}
}
