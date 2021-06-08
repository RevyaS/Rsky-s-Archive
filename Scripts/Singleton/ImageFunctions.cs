using Godot;
using System;
using System.IO;
using SD = System.Drawing;

public class ImageFunctions : Node
{
	public static ImageFunctions Singleton;
	
	public override void _Ready()
	{
		Singleton = this;
	}
	
	//	Generates an ImageTexture for Jpg, Jpeg, Png and GIF files located somewhere
	public ImageTexture getImageTexture(String path)
	{
		Image img = new Image();
		ImageTexture imgText = new ImageTexture();
//		Load ImageTexture from file
		if(path.Extension() == "gif" || path.Extension() == "jpg")
		{
			byte[] data = System.IO.File.ReadAllBytes(path);
			MemoryStream ms = new MemoryStream(data);
			SD.Bitmap thumb = (SD.Bitmap)SD.Image.FromStream(ms);
//			Compute size that fits the square based on image size and keeping aspect
			int width = thumb.Size.Width, height = thumb.Size.Height;	
//			I need a function to run when generating thumbnail
			SD.Image.GetThumbnailImageAbort myCallback = new SD.Image.GetThumbnailImageAbort(ThumbnailCallback);

//			Generate Image
			SD.Image imgThumb = thumb.GetThumbnailImage(width, height, myCallback, IntPtr.Zero);
			Byte[] thumbData = sdImageToByteArr(imgThumb);

//			Load image of Gif from byte[]
			img.LoadPngFromBuffer(thumbData);	

//			Avoid a file from being locked
//			using( = new SD.Bitmap(path))
//			{

//			}
			
		} else
			img.Load(path);
		
		imgText.CreateFromImage(img);
		return imgText;
	}
	
	
//	I don't know, Bitmap needs a bool function to work, so I made this
	public bool ThumbnailCallback() => false;
	
//	Converters (becuz we're handling images here)
	private Byte[] sdImageToByteArr(SD.Image img)
	{
		MemoryStream ms = new MemoryStream();
//		Generate Stream
//		img.Save(ms, img.RawFormat);
		img.Save(ms, img.RawFormat);
		return  ms.ToArray();
	}
	
	
	private void printSDFormat(SD.Image img)
	{
		GD.Print("Checking SD Format");
		if(img.RawFormat.Equals(SD.Imaging.ImageFormat.Jpeg))
			GD.Print("Jpeg");
		if(img.RawFormat.Equals(SD.Imaging.ImageFormat.Png))
			GD.Print("Png");
		if(img.RawFormat.Equals(SD.Imaging.ImageFormat.Gif))
			GD.Print("Gif");
		if(img.RawFormat.Equals(SD.Imaging.ImageFormat.Bmp))
			GD.Print("Bmp");
		if(img.RawFormat.Equals(SD.Imaging.ImageFormat.MemoryBmp))
			GD.Print("MemoryBmp");
		GD.Print("End Checking");
	}
}
