﻿using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static WolfPaw_ScreenSnip.c_ImageHolder;

namespace WolfPaw_ScreenSnip
{
	public partial class f_Screen : Form
	{
		//-------VARIABLES
		//WINDOWS
		public f_SettingPanel child = null;
		public Form1 parent = null;
		public f_previewWindow pw = null;

		//FORMAT ARRAYS
		string[] imageFormats = new string[] { "image/gif", "image/jpeg", "image/pjpeg", "image/png", "image/x-png", "image/tiff", "image/bmp", "image/x-xbitmap", "image/x-jg", "image/x-emf", "image/x-wmf" };
		string[] stringFormats = new string[] { "text/plain", "text/html", "text/xml", "text/richtext", "text/scriptlet" };
		
		//DRAG N DROP {
		bool handleDrag = false;
		Font dragableFont = new Font("Consolas", 12, FontStyle.Regular);
		Bitmap dragableImage = null;
		Size dragableSize = new Size(1, 1);
		Point dragablePoint = new Point(0, 0);
		// }

		//TOOLS {
		public Color toolColor = Color.Black;
		private int CurrentTool;
		public int currentTool
		{
			get { return CurrentTool; }
			set { CurrentTool = value; changeTool(CurrentTool); }
		}
		c_ImageHolder cResizer = null;
		// }

		//RENDERING {
		public List<c_ImageHolder> Limages = new List<c_ImageHolder>();
		public bool mdown = false;
		public c_ImageHolder selectedImage = null;
		public c_ImageHolder mouseOverImage = null;
		public Point imageDragPoint = new Point();
		c_RenderHandler renhan = null;
		edges ed = edges.none;
		corners cor = corners.none;
		// }

		//COLORS {
		/// <summary> Color of handle above image containing buttons </summary>
		Color c_HandleColor = Color.FromArgb(255, 153, 180, 209);
		/// <summary> Color of border surrounding image when clicked on </summary>
		Color c_DefaultBorderColor = Color.FromArgb(255, 0, 0, 0);
		/// <summary> Color of border surrounging image when mouse is over image</summary>
		Color c_MouseOverBorderColor = Color.FromArgb(255, 180, 180, 180);
		/// <summary> Color of background of the Screen </summary>
		Color c_ScreenBackgroundColor = Color.FromArgb(255, 220, 220, 220);

		// }


		//-------FUNCTIONS
		public f_Screen()
		{
			InitializeComponent();

			Load += F_Screen_Load;
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
				return cp;
			}
		}

		private void F_Screen_Load(object sender, EventArgs e)
		{
			c_ScreenBackgroundColor = Properties.Settings.Default.s_ScreenBGColor;
			c_MouseOverBorderColor = Properties.Settings.Default.s_CutoutMouseOverColor;
			c_DefaultBorderColor = Properties.Settings.Default.s_CutoutSelectionColor;
			c_HandleColor = Properties.Settings.Default.s_CutoutPanelColor;
			BackColor = c_ScreenBackgroundColor;
			
			Bitmap b = IconChar.Desktop.ToBitmap(128, Color.Black);
			b.MakeTransparent(Color.White);
			System.IntPtr icH = b.GetHicon();
			this.Icon = System.Drawing.Icon.FromHandle(icH);

			if (Properties.Settings.Default.s_ShowPreview && 
				Properties.Settings.Default.s_LastPreviewMode == 0)
			{
				pw = new f_previewWindow();
				pw.Show();
			}

			if (Properties.Settings.Default.s_ToolbarPanel == 1)
			{
				p_Tools.Width = 200;
				ts_Tools.Hide();
			}
			else
			{
				p_Tools.Width = 0;
				if (Properties.Settings.Default.s_ToolbarPanel == 2)
				{
					ts_Tools.Show();
				}
			}
			
			renhan = new c_RenderHandler(Limages);
		}
		
		public void addImage(Bitmap img)
		{
			if (Properties.Settings.Default.s_ToolbarPanel == 2)
			{
				addImage(img, new Point(5, ts_Tools.Height + 5));
			}
			else
			{
				addImage(img, new Point(5, 5));
			}
		}

		public void addImage(Bitmap img, Point pos)
		{
			if (img != null)
			{
				var box = new c_ImageHolder();
				box.parent = this;

				box.Size = new Size(img.Width, img.Height);

				box.Position = new Point(pos.X, pos.Y);

				box.Image = img;

				box.LayerIndex = Limages.Count;
				box.selfContainingList = Limages;

				Limages.Add(box);
			}
		}

		private void f_Screen_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (child != null) { child.Close(); }
			if (pw != null) { pw.Close(); }
			
			foreach (c_ImageHolder c in Limages)
			{
				try
				{
					c.Dispose();
				} catch { }
			}

			Limages = null;

			try
			{
				GC.AddMemoryPressure(GC.GetTotalMemory(true));
				GC.Collect(1, GCCollectionMode.Forced, true);
			} catch { }

			parent.Activate();
		}

		public void f_Screen_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right ||
				e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Escape)
			{
				var c = c_ImgGen.returnCutouts(this);
				c_ImageHolder u = null;
				foreach (var cc in Limages)
				{
					if (cc.selected)
					{
						u = cc;
						break;
					}
				}

				int add = 1;
				if (e.Control) { add = 5; }

				if (u != null)
				{
					c_ImageHolder.directions dir = c_ImageHolder.directions.none;
					switch (e.KeyCode)
					{
						case Keys.Left:
							dir = c_ImageHolder.directions.left;
							break;

						case Keys.Right:
							dir = c_ImageHolder.directions.right;
							break;

						case Keys.Up:
							dir = c_ImageHolder.directions.up;
							break;

						case Keys.Down:
							dir = c_ImageHolder.directions.down;
							break;

						case Keys.Escape:
							u.selected = false;
							Invalidate();
							break;
					}

					if (dir != c_ImageHolder.directions.none)
					{
						u.move(dir, add);
						Invalidate();
					}
				}
				else if (e.KeyCode == Keys.Escape)
				{

				}
				else if (e.KeyCode == Keys.F11)
				{
					toggleFullScreen();
				}

			}
			else
			{
				parent.Form1_KeyDown(sender, e);
			}
		}

		public void toggleFullScreen()
		{
			if (WindowState == FormWindowState.Maximized)
			{
				WindowState = FormWindowState.Normal;
				FormBorderStyle = FormBorderStyle.Sizable;
			}
			else
			{
				FormBorderStyle = FormBorderStyle.None;
				WindowState = FormWindowState.Maximized;
				BringToFront();

			}
		}

		#region drag

		private void f_Screen_DragDrop(object sender, DragEventArgs e)
		{
			handleDrag = false;

			addImage(dragableImage, dragablePoint);

			dragableSize = new Size(0, 0);
			dragableImage = null;
			dragablePoint = new Point(0, 0);

			Refresh();
		}

		private void f_Screen_DragEnter(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.Copy;

			if (e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetDataPresent(typeof(String)))
			{
				string[] item = new string[1];

				if (e.Data.GetDataPresent(DataFormats.FileDrop))
				{
					item = e.Data.GetData(DataFormats.FileDrop) as string[];
				}
				else
				{
					try
					{
						item[0] = e.Data.GetData(DataFormats.Text).ToString();
					}
					catch
					{
						item = null;
					}
				}



				//TODO: Make Image mime detection recursive!!
				if (item != null && e.Effect == DragDropEffects.Copy)
				{
					foreach (string s in item)
					{
						if (e.Data.GetDataPresent(DataFormats.FileDrop))
						{
							string ret = c_Converter.fileToMime(s).ToString() ?? "";
							if (imageFormats.Contains(ret))
							{
								dragableImage = c_Converter.fileToImage(s);
								dragableSize = dragableImage.Size;
								handleDrag = true;
							}
							else if (stringFormats.Contains(ret))
							{
								dragableImage = textToImg(s);
								handleDrag = true;
							}
							else
							{
								e.Effect = DragDropEffects.None;
							}
						}
						else
						{
							try
							{
								string ss = s.Replace("\t", "              ");
								dragableImage = textToImg(ss);
								handleDrag = true;
							}
							catch
							{
								e.Effect = DragDropEffects.None;
							}
						}

					}
				}
			}

		}

		private void f_Screen_DragOver(object sender, DragEventArgs e)
		{
			if (handleDrag)
			{
				Refresh();
				using (Graphics g = Graphics.FromHwnd(this.Handle))
				{
					Point p = PointToClient(new Point(e.X - dragableSize.Width / 2, e.Y - 10));
					dragablePoint = p;
					int w = dragableSize.Width;
					int h = dragableSize.Height;

					Rectangle[] rects = new Rectangle[1];
					rects[0] = new Rectangle(p.X + 1, p.Y + 9 + 1, dragableSize.Width - 2, dragableSize.Height - 2 - 9);
					g.DrawRectangles(Pens.Black, rects);


					g.FillRectangle(Brushes.CornflowerBlue, new RectangleF(p.X + 1, p.Y, w - 1, 10));
				}
			}
		}

		#endregion

		public Bitmap textToImg(string s)
		{
			Size textSize = TextRenderer.MeasureText(s, dragableFont);
			Bitmap di = new Bitmap(textSize.Width + 10, textSize.Height + 10);
			using (Graphics g = Graphics.FromImage(di))
			{
				g.DrawString(s, dragableFont, Brushes.Black, new PointF(5, 5));
			}
			dragableSize = di.Size;
			return di;
		}


		public void showToolBar()
		{
			if (child != null && !child.IsDisposed) { child.Close(); }
			p_Tools.Width = 200;
			ts_Tools.Hide();
			Properties.Settings.Default.s_ToolbarPanel = 1;
			Properties.Settings.Default.Save();
		}

		public void hideToolBar()
		{
			child = new f_SettingPanel();
			child.parent = this;
			child.Show();
			Properties.Settings.Default.s_ToolbarPanel = 0;
			Properties.Settings.Default.Save();
			p_Tools.Width = 0;
			ts_Tools.Hide();
		}

		public void showToolStrip()
		{
			p_Tools.Width = 0;
			if (child != null && !child.IsDisposed) { child.Close(); }
			ts_Tools.Show();
			Properties.Settings.Default.s_ToolbarPanel = 2;
			Properties.Settings.Default.Save();
		}

		private void btn_Dock_Click(object sender, EventArgs e)
		{
			hideToolBar();
		}

		private void btn_ToolStrip_Click(object sender, EventArgs e)
		{
			showToolStrip();
		}

		private void btn_ToolWindow_Click(object sender, EventArgs e)
		{
			showToolBar();
		}

		private void btn_ToolPanel_Click(object sender, EventArgs e)
		{
			hideToolBar();
		}

		public bool panelOpen()
		{
			return p_Tools.Width == 200;
		}

		public bool toolbarOpen()
		{
			return ts_Tools.Visible;
		}

		public void toggleToolbar()
		{
			ts_Tools.Visible = !ts_Tools.Visible;
		}

		public void togglePanel()
		{
			p_Tools.Width = (p_Tools.Width == 0 ? 200 : 0);
		}

		public void paste()
		{
			if (Clipboard.ContainsImage())
			{
				addImage((Bitmap)Clipboard.GetImage());
			}
			else if (Clipboard.ContainsText())
			{
				addImage(textToImg(Clipboard.GetText()));
			}
		}


		private void t_Tick_Tick(object sender, EventArgs e)
		{
			foreach(c_ImageHolder c in Limages)
			{
				if (c.panelShowing)
				{
					if (c.panelTimeLeft > 0)
						c.panelTimeLeft--;
					else
						c.hidePanel();
				}
			}

			if (pw != null && !pw.IsDisposed)
			{
				pw.refreshImage(this);
			}
			
		}

		
		private void btn_Dock_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{

			if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right ||
				e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
			{
				e.IsInputKey = true;
			}

		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			var cr = new c_returnGraphicSettings();
			e.Graphics.InterpolationMode = cr.getIM();
			e.Graphics.PixelOffsetMode = cr.getPOM();
			e.Graphics.SmoothingMode = cr.getSM();

			//TODO: PAINT!!
			organizeImageList();
			foreach (c_ImageHolder c in Limages)
			{
				if (c != null)
				{
					e.Graphics.DrawImage(c.getImage(), new Rectangle(c.Position, c.Size), new Rectangle(new Point(0, 0), c.Image.Size), GraphicsUnit.Pixel);

					if (c.selected)
					{
						e.Graphics.DrawRectangle(new Pen(c_DefaultBorderColor), new Rectangle(c.Position, c.Size));
					}
					else if (c.mouseOver)
					{
						e.Graphics.DrawRectangle(new Pen(c_MouseOverBorderColor), new Rectangle(c.Position, c.Size));
					}
					if (c.panelShowing)
					{
						e.Graphics.FillRectangle(new SolidBrush(c_HandleColor), new RectangleF(c.Position, new Size(c.Width, 20)));

						e.Graphics.DrawImage(renhan.renderButtons(PointToClient(Cursor.Position), c), new PointF(c.Position.X, c.Position.Y));
					}
				}
			}

		}

		private void f_Screen_SizeChanged(object sender, EventArgs e)
		{

		}

		public void organizeImageList()
		{
			Limages.Sort(new intComparer());
		}
		
		private void f_Screen_MouseClick(object sender, MouseEventArgs e)
		{
			foreach (c_ImageHolder c in Limages)
			{
				if (renhan.pointInPosition(e.Location, new Rectangle(c.Position, c.Size)))
				{
					c.select();
				}
			}
			Invalidate();
		}

		public bool buttonPress = false;

		private void f_Screen_MouseDown(object sender, MouseEventArgs e)
		{
			Limages.Sort(new intComparerDesc());
			foreach (c_ImageHolder c in Limages)
			{
				if (renhan.pointInPosition(e.Location, new Rectangle(c.Position, c.Size)))
				{
					c.select();
					selectedImage = c;
					imageDragPoint = new Point(e.X - c.Left, e.Y - c.Top);

					buttonPress = false;
					if (c.isOverAButton(imageDragPoint))
					{
						buttonPress = true;
					}

					if (!c.isOverAnEdge(imageDragPoint) && !c.isOverACorner(imageDragPoint)) { resize = false; cResizer = null; ed = edges.none; cor = corners.none; }
					else { resize = true; cResizer = c; if (c.isOverAnEdge(imageDragPoint)){ ed = c.overWhichEdge(imageDragPoint); cor = corners.none; } else { cor = c.overWhichCorner(imageDragPoint); ed = edges.none; } }
					mdown = true;
					break;
				}
			}
			//resize = false;
			Invalidate();
		}

		private void f_Screen_MouseUp(object sender, MouseEventArgs e)
		{
			selectedImage = null;
			mdown = false;

			Limages.Sort(new intComparerDesc());
			foreach (c_ImageHolder c in Limages)
			{
				if (renhan.pointInPosition(e.Location, new Rectangle(c.Position, c.Size)))
				{
					Point pp = new Point(e.X - c.Left, e.Y - c.Top);
					if (c.isOverAButton(pp) && !resize && buttonPress)
					{
						btn b = c.overWhichButton(pp);
						//TODO: HANDLE BUTTON CLICKS
						if (c._buttons.currentValue == btn.hiddenVal.W065)
						{
							if(b.value == 10)
							{
								cms_Panel.Visible = true;
								cms_Panel.Show(this, e.Location);
								selectedImage = c;
							}
						}
						else if (c._buttons.currentValue == btn.hiddenVal.W135)
						{
							switch (b.value)
							{
								case 0:
									c.Size = c.Image.Size;
									break;

								case 10:
									cms_Panel.Visible = true;
									cms_Panel.Show(this, e.Location);
									selectedImage = c;
									break;

								case -1:
									Limages.Remove(c);
									GC.Collect();
									Invalidate();
									break;
							}
						}
						else if (c._buttons.currentValue == btn.hiddenVal.W175)
						{
							switch (b.value)
							{
								case 0:
									c.Size = c.Image.Size;
									break;

								case 10:
									cms_Panel.Visible = true;
									cms_Panel.Show(this, e.Location);
									selectedImage = c;
									break;

								case 4:
									//TODO: EDIT!!
									break;

								case 5:
									c.saveImage();
									break;

								case 6:
									c.copyImage();
									break;

								case -1:
									Limages.Remove(c);
									GC.Collect();
									Invalidate();
									break;
							}
						}
						else if (c._buttons.currentValue == btn.hiddenVal.FullWidth)
						{
							switch (b.value)
							{
								case 0:
									c.Size = c.Image.Size;
									break;

								case 1:
									c.fullscreen();
									break;

								case 2:
									c.LayerUp();
									break;

								case 3:
									c.LayerDown();
									break;

								case 4:
									//TODO: EDIT!!
									break;

								case 5:
									c.saveImage();
									break;

								case 6:
									c.copyImage();
									break;

								case -1:
									Limages.Remove(c);
									GC.Collect();
									Invalidate();
									break;


							}
						}

						//TODO: Button click
					}

					break;
				}

			}

			Invalidate();
		}

		bool resize = false;

		//TODO: MOUSE MOVE!!
		private void f_Screen_MouseMove(object sender, MouseEventArgs e)
		{
			foreach (c_ImageHolder cc in Limages)
			{
				Point pedge = new Point(e.X - cc.Left, e.Y - cc.Top);
				if (cc.bounds().Contains(e.Location))
				{
					if (cc.isOverAnEdge(pedge))
					{
						edges ed = cc.overWhichEdge(pedge);
						if (ed == edges.bottom || ed == edges.top)
						{
							Cursor = Cursors.SizeNS;
						}
						else if (ed == edges.left || ed == edges.right)
						{
							Cursor = Cursors.SizeWE;
						}
					}
					else if (cc.isOverACorner(pedge))
					{
						corners cir = cc.overWhichCorner(pedge);
						if (cir == corners.leftBottom)
						{
							Cursor = Cursors.SizeNESW;
						}
						else if (cir == corners.leftTop)
						{
							Cursor = Cursors.SizeNWSE;
						}
						else if (cir == corners.rightBottom)
						{
							Cursor = Cursors.SizeNWSE;
						}
						else if (cir == corners.rightTop)
						{
							Cursor = Cursors.SizeNESW;
						}

					}
					else
					{
						Cursor = Cursors.Default;
					}
				}
			}

			if (mdown && resize)
			{
				var cc = cResizer;
				if (ed != edges.none)
				{
					if (ed == edges.bottom)
					{
						if (e.Location.Y - cc.Top > 20)
						{
							cc.Size = new Size(cc.Width, e.Location.Y - cc.Top);
						}
						else
						{
							cc.Size = new Size(cc.Width, 21);
						}
					}
					else if (ed == edges.top)
					{
						int top = cc.Top;
						if (cc.Height - (cc.Top - top) > 20)
						{
							cc.Position = new Point(cc.Left, e.Location.Y);
							cc.Size = new Size(cc.Width, cc.Height - (cc.Top - top));
						}
						else
						{

							cc.Size = new Size(cc.Width, 21);
						}
					}
					else if (ed == edges.left)
					{
						int left = cc.Left;
						if (cc.Width - (cc.Left - left) > 20)
						{
							cc.Position = new Point(e.Location.X, cc.Top);
							cc.Size = new Size(cc.Width - (cc.Left - left), cc.Height);
						}
						else
						{
							cc.Size = new Size(21, cc.Height);
							cc.Position = new Point(cc.Position.X - 2, cc.Position.Y);
						}
					}
					else if (ed == edges.right)
					{
						if (e.Location.X - cc.Left > 20)
						{
							cc.Size = new Size(e.Location.X - cc.Left, cc.Height);
						}
						else
						{
							cc.Size = new Size(21, cc.Height);
						}
					}
				}
				else if (cor != corners.none)
				{
					if(cor == corners.leftBottom)
					{
						int left = cc.Left;

						cc.Position = new Point(e.Location.X, cc.Top);
						cc.Size = new Size(cc.Width - (cc.Left - left), e.Location.Y - cc.Top);

						if (e.Location.Y - cc.Top <= 20)
						{
							cc.Size = new Size(cc.Width, 21);
						}
						if (cc.Width - (cc.Left - left) <= 20)
						{
							cc.Size = new Size(21, cc.Height);
						}

					}
					else if (cor == corners.leftTop)
					{
						
						//TOP
						int top = cc.Top;
						int left = cc.Left;

						cc.Position = new Point(e.Location.X, e.Location.Y);
						cc.Size = new Size(cc.Width - (cc.Left - left), cc.Height - (cc.Top - top));

						if (cc.Height - (cc.Top - top) <= 20)
						{
							cc.Size = new Size(cc.Width, 21);
							cc.Position = new Point(cc.Position.X, top);
						}
						if (cc.Width - (cc.Left - left) <= 20)
						{
							cc.Size = new Size(21, cc.Height);
							cc.Position = new Point(left, cc.Position.Y);
						}

					}
					else if (cor == corners.rightBottom)
					{
						//BOTTOM
						if (e.Location.Y - cc.Top > 20)
						{
							cc.Size = new Size(cc.Width, e.Location.Y - cc.Top);
						}
						else
						{
							cc.Size = new Size(cc.Width, 21);
						}
						//RIGHT
						if (e.Location.X - cc.Left > 20)
						{
							cc.Size = new Size(e.Location.X - cc.Left, cc.Height);
						}
						else
						{
							cc.Size = new Size(21, cc.Height);
						}
					}
					else if (cor == corners.rightTop)
					{
						//TOP
						int top = cc.Top;
						if (cc.Height - (cc.Top - top) > 20)
						{
							cc.Position = new Point(cc.Left, e.Location.Y);
							cc.Size = new Size(cc.Width, cc.Height - (cc.Top - top));
						}
						else
						{
							cc.Size = new Size(cc.Width, 21);
						}
						//RIGHT
						if (e.Location.X - cc.Left > 20)
						{
							cc.Size = new Size(e.Location.X - cc.Left, cc.Height);
						}
						else
						{
							cc.Size = new Size(21, cc.Height);
						}
					}
				}
			}
			
			if (mdown && selectedImage != null && !resize)
			{
				buttonPress = false;
				selectedImage.Position = new Point(e.X - imageDragPoint.X, e.Y - imageDragPoint.Y);
				Invalidate();
			}
			else
			{
				
				c_ImageHolder img = null;
				
				foreach (c_ImageHolder cc in Limages)
				{
					cc.mouseOver = false;
				}
				if (renhan.pointOverAny(e.Location, out img))
				{
					mouseOverImage = img;
					mouseOverImage.mouseOver = true;
					if (renhan.pointInPosition(e.Location, new Rectangle(mouseOverImage.Position, new Size(mouseOverImage.Width, 20))))
					{
						mouseOverImage.showPanel();
					}
				}
				else
				{
					mouseOverImage = null;
				}
				Invalidate();
			}
		}
		
		//----TOOL STUFF
		private void num_ToolSize_ValueChanged(object sender, EventArgs e)
		{
			
		}

		private void num_ToolSize_ValueChanged_1(object sender, EventArgs e)
		{
			
		}

		public void changeTool(int tool)
		{
			if (tool == 0)
			{
				showHideEditLayer(false);
			}
			else
			{
				showHideEditLayer(true);
			}
		}

		public Point[] getDrawnPoints()
		{
			//TODO: Add functionality
			return null;
		}

		public void showHideEditLayer(bool show)
		{
			if (show)
			{

			}
			else
			{

			}
		}

		private void btn_Manipulate_Click(object sender, EventArgs e)
		{
			currentTool = 0;
		}

		private void btn_Pen_Click(object sender, EventArgs e)
		{
			currentTool = 1;
		}

		private void btn_Marker_Click(object sender, EventArgs e)
		{
			currentTool = 2;
		}

		private void btn_Line_Click(object sender, EventArgs e)
		{
			currentTool = 3;
		}

		public bool hasSelectedImage()
		{
			return (selectedImage != null && Limages.Contains(selectedImage));
		}

		private void cms_btn_Resize_Click(object sender, EventArgs e)
		{
			if(hasSelectedImage())
			{
				selectedImage.Size = selectedImage.Image.Size;
			}
		}

		private void cms_btn_Fit_Click(object sender, EventArgs e)
		{
			if (hasSelectedImage())
			{
				selectedImage.fullscreen();
			}
		}

		private void cms_btn_LayerUp_Click(object sender, EventArgs e)
		{
			if (hasSelectedImage())
			{
				selectedImage.LayerUp();
			}
		}

		private void cms_btn_LayerDown_Click(object sender, EventArgs e)
		{
			if (hasSelectedImage())
			{
				selectedImage.LayerDown();
			}
		}

		private void cms_btn_EditImage_Click(object sender, EventArgs e)
		{

		}

		private void cms_btn_Save_Click(object sender, EventArgs e)
		{
			if (hasSelectedImage())
			{
				selectedImage.saveImage();
			}
		}

		private void cms_btn_Copy_Click(object sender, EventArgs e)
		{
			if (hasSelectedImage())
			{
				selectedImage.copyImage();
			}
		}

		private void cms_btn_Delete_Click(object sender, EventArgs e)
		{
			if (hasSelectedImage())
			{
				Limages.Remove(selectedImage);
				GC.Collect();
				Invalidate();
			}
		}
	}

	#region OTHER CLASSES / COMPARERS

	public class myToolstrip : ToolStrip
	{
		public myToolstrip()
		{
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
				return cp;
			}
		}
	}

	public class myPanel : Panel
	{

		public myPanel()
		{
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
		}
		
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
				return cp;
			}
		}
	}

	public class intComparer : IComparer<c_ImageHolder>
	{
		public int Compare(c_ImageHolder a, c_ImageHolder b)
		{
			return (a.LayerIndex > b.LayerIndex ? 1 : -1);
		}
	}

	public class intComparerDesc : IComparer<c_ImageHolder>
	{
		public int Compare(c_ImageHolder a, c_ImageHolder b)
		{
			return (a.LayerIndex < b.LayerIndex ? 1 : -1);
		}
	}

#endregion

}
