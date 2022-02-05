using BinPacker2D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace BinPackerGUI
{
	public partial class Form1 : Form
	{
		Graphics drawing_table;
		
		double Table_Width;
		double Table_Height;

        private List<RowElement> _inputElements;
        private Packer _packer2D = new Packer();

		public Form1(double tw, double th)
        {
            InitializeComponent();

			this.Table_Height = th;
            this.Table_Width = tw;
            CreateSampleElements();

			var finishedElements = _packer2D.PackElements(400, 520, 5, 5, 5, 5, 5, _inputElements);

            this.DoubleBuffered = true;
            this.AutoScroll = true;
            
            Initialize_Graphics();
            DrawSummaryGraphics(finishedElements);
            label1.Text = $"Packed count: {finishedElements.Count}";

            var tLines = new List<string>();
            foreach (var e in finishedElements)
            {
                var st = $"{$"{e.sortIndex}",-3} {$"X: {e.posX}",-12} {$"Y: {e.posY}",-12} {$"W: {e.width}",-12} {$"H: {e.height}",-12}";
                tLines.Add(st);
            }

            richTextBox1.Lines = tLines.ToArray();
		}

		private void CreateSampleElements()
        {
            _inputElements = new List<RowElement>();

            Random rand = new Random();

            for (int i = 0; i < 2; i++)
            {
                _inputElements.Add(new RowElement
                {
                    width = rand.Next(80, 100),
                    height = rand.Next(40, 50)
                });
            }

			for (int i = 0; i < 15; i++)
            {
				_inputElements.Add(new RowElement
                {
                    width = rand.Next(60, 70), 
                    height = rand.Next(36, 50)
				});
			}

            for (int i = 0; i < 60; i++)
            {
                _inputElements.Add(new RowElement
                {
                    width = rand.Next(10, 15),
                    height = rand.Next(10, 15)
                });
            }


		}

		/// <summary>
		/// Initialize new drawing table
		/// </summary>
		private void Initialize_Graphics()
		{
			Drawing_Picture.Image = new Bitmap(Drawing_Picture.Width, Drawing_Picture.Height);
			
			drawing_table = Graphics.FromImage(Drawing_Picture.Image);
			drawing_table.SmoothingMode = SmoothingMode.HighQuality;
            drawing_table.TranslateTransform(0, Drawing_Picture.Height - 1);
			
			drawing_table.Clear(Color.White);
		}

		private void Drawing_Picture_Paint(object sender, PaintEventArgs e)
		{
			base.OnPaint(e);
        }

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			e.Graphics.ScaleTransform(1.0f, -1.0f);
			e.Graphics.TranslateTransform(0, -this.ClientRectangle.Height);
			e.Graphics.DrawLine(Pens.Black, 0, 0, 100, 100);
		}

		private void DrawSummaryGraphics(List<RowElement> packedElements)
        {
            int multiply = 1;
            int posXoffset = 0;
            int posYoffset = 400;

            Bitmap bitmap = new Bitmap(Drawing_Picture.Width, Drawing_Picture.Height,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Pen pencil = new Pen(Color.Black, 1);
            Pen pencil_2 = new Pen(Color.Green, 1);
            drawing_table = Graphics.FromImage(bitmap);

            using (System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red))
            {
                double totalHeight = 0;
				foreach (BinPacker2D.RowElement element in packedElements)
                {
                    totalHeight += element.height;
                    int posX = multiply * int.Parse(element.posX.ToString()) + posXoffset;
                    int w = multiply * int.Parse(element.width.ToString());
                    int h = multiply * int.Parse(element.height.ToString());
                    int posY = 0;
                    posY = multiply * posYoffset - int.Parse(element.height.ToString()) - int.Parse(element.posY.ToString());

                    drawing_table.FillRectangle(myBrush, new Rectangle(posX, posY, w, h));
                    drawing_table.DrawRectangle(pencil, new Rectangle(posX, posY, w, h));
                }
				int ax = multiply * posXoffset;
                int ay = multiply * 0;
                int aw = multiply * int.Parse(Table_Width.ToString());
                int ah = multiply * int.Parse(this.Table_Height.ToString());

                drawing_table.DrawRectangle(pencil_2, new Rectangle(ax, ay, aw, ah));


            } 
            Drawing_Picture.Image = bitmap;
        }

        

       
    }


}
