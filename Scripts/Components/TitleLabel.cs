using Godot;
using System;

public class TitleLabel : Label
{
//	Constants
	Color lighter = new Color(1.98f, 0.92f, 1.98f, 1f);
	Color def = new Color(1.38f, 0.62f, 1.38f, 1.0f);
	
//	References
	Tween tween;

//	Data
	bool toLight = true;	
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		tween = GetNode<Tween>("Tween");
		tween.Connect("tween_all_completed", this, nameof(tweenCompleted));
		tweenCompleted();
	}
	
	
//	Triggers everytime tween ends
	private void tweenCompleted()
	{
//		GD.Print("Tween Start");
		
		String property = "custom_colors/font_color_shadow";
		Color target = (toLight) ? lighter : def;
		toLight = (toLight) ? false : true;

		tween.InterpolateProperty(this, property, Get(property), target,
			1f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
		
		tween.Start();
	}

}
