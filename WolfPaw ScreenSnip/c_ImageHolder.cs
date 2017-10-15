﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FontAwesome.Sharp;

namespace WolfPaw_ScreenSnip
{
	public class c_ImageHolder : IDisposable
	{
		public enum imageBorder
		{
			none,
			singleLine,
			doubleLine,
			dashed,
			dotted
		}

		public enum directions
		{
			none,
			up,
			down,
			left,
			right
		}

		#region ========== Variables ==========
		
		//Size
		private Size size;
		private int width;
		public int Width { get { return width; } }
		private int height;
		public int Height { get { return height; } }
		public Size Size { get { return size; } set { setSize(value); } }
		//Position
		private Point position;
		private int left;
		public int Left { get { return left; } }
		private int top;
		public int Top { get { return top; } }
		public Point Position { get { return position; } set { setPosition(value); } }
		private int layerIndex;
		public int LayerIndex { get { return layerIndex; } set { setLayerIndex(value); } }
		//Image
		private Bitmap image;
		public Bitmap Image { get { return image; } set { setImage(value); } }
		public Bitmap scaledImage = null;
		public BitmapData bmd;
		private imageBorder border;
		public imageBorder Border { get { return border; } set { setBorder(value); }  }
		//Other
		public f_Screen parent { get; set; }
		public List<c_ImageHolder> selfContainingList { get; set; }
		public bool selected = false;
		public bool mouseOver = false;
		public bool panelShowing = false;
		public int panelTimeLeft = 0;

		public buttons _buttons = new buttons();
		
		public c_ImageHolder()
		{
			btn b_Resize = new btn() {
				image1 = IconChar.ArrowsAlt.ToBitmap(20, Color.Black),
				image2 = IconChar.ArrowsAlt.ToBitmap(20, Color.White),
				pos = 0,
				anchor = btn.anchors.left,
				borderWidth = 2,
				value = 0,
				hiddenAtVal = btn.hiddenVal.W065,
				tooltiptext = "Resize\r\nResize image to iriginal dimentions"
			};
			_buttons.add(b_Resize);

			btn b_FullScreen = new btn()
			{
				image1 = IconChar.Arrows.ToBitmap(20, Color.Black),
				image2 = IconChar.Arrows.ToBitmap(20, Color.White),
				pos = 1,
				anchor = btn.anchors.left,
				borderWidth = 2,
				value = 1,
				hiddenAtVal = btn.hiddenVal.W175,
				tooltiptext = "Full Screen\r\nResize image to fit the screen size\r\n(keeping aspect ratio)"
			};
			_buttons.add(b_FullScreen);

			btn b_LayerUp = new btn()
			{
				image1 = IconChar.ArrowCircleOUp.ToBitmap(20, Color.Black),
				image2 = IconChar.ArrowCircleOUp.ToBitmap(20, Color.White),
				pos = 2,
				anchor = btn.anchors.left,
				borderWidth = 2,
				value = 2,
				hiddenAtVal = btn.hiddenVal.W175,
				tooltiptext = "Layer Up\r\nMoves the image to a higher layer"
			};
			_buttons.add(b_LayerUp);

			btn b_LayerDown = new btn()
			{
				image1 = IconChar.ArrowCircleODown.ToBitmap(20, Color.Black),
				image2 = IconChar.ArrowCircleODown.ToBitmap(20, Color.White),
				pos = 3,
				anchor = btn.anchors.left,
				borderWidth = 2,
				value = 3,
				hiddenAtVal = btn.hiddenVal.W175,
				tooltiptext = "Layer Down\r\nMoves the image to a lower layer"
			};
			_buttons.add(b_LayerDown);

			btn b_CMS = new btn()
			{
				image1 = IconChar.CaretUp.ToBitmap(20, Color.Black),
				image2 = IconChar.CaretUp.ToBitmap(20, Color.White),
				pos = 4,
				anchor = btn.anchors.right,
				borderWidth = 2,
				value = 10,
				hiddenAtVal = btn.hiddenVal.FullWidthOnly,
				tooltiptext = "Open Menu\r\nOpens the dropdown menu allowing you to see more buttoins than can appear currently"
			};
			_buttons.add(b_CMS);

			btn b_EditImage = new btn()
			{
				image1 = IconChar.PaintBrush.ToBitmap(20, Color.Black),
				image2 = IconChar.PaintBrush.ToBitmap(20, Color.White),
				pos = 3,
				anchor = btn.anchors.right,
				borderWidth = 2,
				value = 4,
				hiddenAtVal = btn.hiddenVal.W135,
				tooltiptext = "Edit Image\r\nOpens this image for editing"
			};
			_buttons.add(b_EditImage);

			btn b_SaveImage = new btn()
			{
				image1 = IconChar.FloppyO.ToBitmap(20, Color.Black),
				image2 = IconChar.FloppyO.ToBitmap(20, Color.White),
				pos = 2,
				anchor = btn.anchors.right,
				borderWidth = 2,
				value = 5,
				hiddenAtVal = btn.hiddenVal.W135,
				tooltiptext = "Save Image\r\nSaves this image into a separate file"
			};
			_buttons.add(b_SaveImage);

			btn b_CopyImage = new btn()
			{
				image1 = IconChar.FilesO.ToBitmap(20, Color.Black),
				image2 = IconChar.FilesO.ToBitmap(20, Color.White),
				pos = 1,
				anchor = btn.anchors.right,
				borderWidth = 2,
				value = 6,
				hiddenAtVal = btn.hiddenVal.W135,
				tooltiptext = "Copy Image\r\nCopies this image to the clipboard"
			};
			_buttons.add(b_CopyImage);

			btn b_DeleteImage = new btn()
			{
				image1 = IconChar.TrashO.ToBitmap(20, Color.Black),
				image2 = IconChar.TrashO.ToBitmap(20, Color.White),
				pos = 0,
				anchor = btn.anchors.right,
				borderWidth = 2,
				value = -1,
				hiddenAtVal = btn.hiddenVal.W065,
				tooltiptext = "Delete Image\r\nRemoves this image from the screen"
			};
			_buttons.add(b_DeleteImage);


		}


		#endregion

		#region Size
		public void setSize(Size s)
		{
			size = s;
			width = s.Width;
			height = s.Height;
		}

		public Size getSize()
		{
			return size;
		}

		public int getWidth()
		{
			return width;
		}

		public int getHeight()
		{
			return height;
		}

		public Rectangle bounds()
		{
			return new Rectangle(position, size);
		}
		#endregion

		#region Position
		public void setPosition(Point p)
		{
			position = p;
			left = p.X;
			top = p.Y;
		}

		public Point getPosition()
		{
			return position;
		}

		public int getLeft()
		{
			return left;
		}

		public int getTop()
		{
			return top;
		}

		public void setLayerIndex(int lInd)
		{
			layerIndex = lInd;
		}

		public int getLayerIndex()
		{
			return layerIndex;
		}
		
		#endregion

		#region Image
		public bool lockBits()
		{
			try
			{
				bmd = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return false;
			}
		}

		public bool unlockBits()
		{
			try
			{
				image.UnlockBits(bmd);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return false;
			}
		}

		public BitmapData getBitmapData()
		{
			lockBits();
			return bmd;
		}

		public void setImage(Bitmap img)
		{
			image = img;
			createScaledImage();
		}

		public Bitmap getImage()
		{
			return image;
		}

		public void createScaledImage()
		{
			scaledImage = new Bitmap(getWidth(), getHeight());
			using (Graphics g = Graphics.FromImage(scaledImage))
			{
				g.DrawImage(image, new Rectangle(new Point(0, 0), scaledImage.Size), new Rectangle(new Point(0, 0), image.Size), GraphicsUnit.Pixel);
			}
		}

		public Bitmap getScaledImage()
		{
			return scaledImage;
		}

		public void setBorder(imageBorder brd)
		{
			border = brd;
		}

		public imageBorder getBorder()
		{
			return border;
		}

		public void resizeImage()
		{
			createScaledImage();

			//TODO: ADD FUNCTIONALITY
		}

		#endregion


		#region mouse position

		public bool isOverButton(Point P, btn b)
		{
			int left = 0;

			if (P.Y >= 0 && P.Y <= 20)
			{
				if (b.anchor == btn.anchors.left)
				{
					left = b.pos * 22;
				}
				else
				{
					left = width - (((b.pos + 1) * 20) + (b.pos * 2));
				}
			}

			Rectangle r = new Rectangle(left, 0, 20, 20);

			return r.Contains(P);
		}

		public bool isOverAButton(Point P)
		{
			return false;
		}

		public int overWhichButton(Point P)
		{
			return 0;
		}

		public bool isOverEdge(Point P)
		{
			return false;
		}

		public int overWhichEdge(Point P)
		{
			return 0;
		}

		public bool isOverCorner(Point P)
		{
			return false;
		}

		public int overWhichCorner(Point P)
		{
			return 0;
		}

		#endregion


		#region OtherFunctions

		public void saveImage()
		{
			Bitmap bmp = getScaledImage();
			//TODO: SAVE
		}

		public void copyImage()
		{
			Bitmap bmp = getScaledImage();
			Clipboard.SetImage(bmp);
		}

		public void select()
		{
			foreach(c_ImageHolder imgh in selfContainingList)
			{
				imgh.unselect();
			}
			selected = true;
		}

		public void unselect()
		{
			selected = false;
		}

		public void showPanel()
		{
			panelShowing = true;
			setPanelTimer(10);
		}

		public void hidePanel()
		{
			panelShowing = false;
			setPanelTimer(0);
		}
		
		public void setPanelTimer(int timeout)
		{
			panelTimeLeft = timeout;
		}

		public void move(directions dir, int len)
		{
			switch (dir)
			{
				case directions.down:
					setPosition(new Point(position.X, position.Y + len));
					break;

				case directions.left:
					setPosition(new Point(position.X - len, position.Y));
					break;

				case directions.right:
					setPosition(new Point(position.X + len, position.Y));
					break;

				case directions.up:
					setPosition(new Point(position.X, position.Y - len));
					break;
			}
		}





		#endregion

		/// <summary>
		/// Dispose Calls
		/// </summary>
		#region IDisposable Support
		public void Dispose()
		{
			Dispose(true);
		}
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					image.Dispose();
					Image.Dispose();
					scaledImage.Dispose();
					parent.Dispose();
				}
				
				disposedValue = true;
			}
		}
		
		void IDisposable.Dispose()
		{
			Dispose(true);
		}
		#endregion

	}

	public class buttons
	{
		public List<btn> btns = new List<btn>();
		private int wid;
		public int panelWidth { get { return wid; } set { wid = value; setupButtons(wid); } }
		public btn.hiddenVal currentValue = btn.hiddenVal.FullWidth;
		/// <summary>
		/// <para>1,	//Resize	 </para>
		/// <para>1,	//Fullscreen </para>
		/// <para>1,	//Layerup	 </para>
		/// <para>1,	//Layerdown	 </para>
		/// <para>0,	//CMS		 </para>
		/// <para>1,	//Edit		 </para>
		/// <para>1,	//Save		 </para>
		/// <para>1,	//Copy		 </para>
		/// <para>1	//Delete		 </para>
		/// </summary>
		public int[] visibleButtons = new int[] 
		{
			1,	//Resize
			1,	//Fullscreen
			1,	//Layerup
			1,	//Layerdown
			0,	//CMS
			1,	//Edit
			1,	//Save
			1,	//Copy
			1	//Delete
		};

		public void add(btn button)
		{
			btns.Add(button);
		}

		public void remove(btn button)
		{
			btns.Remove(button);
		}

		public void getCurrentValue(int w)
		{
			if (w >= 175)
			{
				currentValue = btn.hiddenVal.FullWidth;
			}
			else if (w >= 135)
			{
				currentValue = btn.hiddenVal.W175;
			}
			else if (w >= 65)
			{
				currentValue = btn.hiddenVal.W135;
			}
			else
			{
				currentValue = btn.hiddenVal.W065;
			}
		}
		
		public void setupButtons(int w)
		{
			getCurrentValue(w);

			foreach (btn b in btns)
			{
				if (b.hiddenAtVal != btn.hiddenVal.FullWidthOnly)
				{
					if (currentValue < b.hiddenAtVal)
					{
						b.visible = true;
					}
					else
					{
						b.visible = false;
					}
				}
				else
				{
					if (currentValue > btn.hiddenVal.FullWidth)
					{
						b.visible = true;
					}
					else
					{
						b.visible = false;
					}
				}
			}

		}
	}

	public class btn
	{
		public enum anchors
		{ left, right }

		public enum hiddenVal
		{
			FullWidth,
			FullWidthOnly,
			W175,
			W135,
			W065
		}

		public Bitmap image1 { get; set; }
		public Bitmap image2 { get; set; }
		public int pos { get; set; }
		public anchors anchor { get; set; }
		public int borderWidth { get; set; }
		public int value { get; set; }
		public hiddenVal hiddenAtVal { get; set; }
		public string tooltiptext { get; set; }
		public bool visible = true;

		/// <summary>
		/// Bitmap image1  |  
		/// Bitmap image2  |  
		/// int pos  |  
		/// anchors anchor  |  
		/// int borderWidth  |  
		/// int value  |  
		/// hiddenVal hiddenAtVal  |  
		/// string tooltiptext
		/// </summary>
		public btn()
		{

		}
	}

}
