using SLEP.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace SLEP.UIModule.Resources
{
	
	public class MushraScoreTickBar : TickBar
	{
	
		protected override void OnRender(DrawingContext dc)
		{
			
				try
				{

				
					var size = new Size(base.ActualWidth, base.ActualHeight);
					//Size size = new Size(base.ActualWidth, 26);
					int tickCount = (int)((this.Maximum - this.Minimum) / this.TickFrequency);

					Double tickFrequencySize;

					// Calculate tick's setting
					tickFrequencySize = (size.Width * this.TickFrequency / (this.Maximum - this.Minimum));

					FormattedText formattedText = null;
					double num = this.Maximum - this.Minimum;

					var drawAtX = 0.0;
					var drawAtY = -140.0;

					var stringWidth = GetWidthOfTextInPixels();

					// Draw each tick text
					for (var count = 0; count <= tickCount; count++)
					{
						
							if (count <= tickCount / 2)
							{
								drawAtX = (tickFrequencySize * (count + 0.05)) + 10;
							}
							else
							{
								drawAtX = (tickFrequencySize * count) + 5;
							}

							if (SliderContent.Comment.Count != 0 && SliderContent.Score.Count != 0)
							{
								if (SliderContent.Comment[count] != "NoShow")
								{
									var comment = SliderContent.Comment[count];
									 comment  = WordWrap(comment, 15);
									
									var score = SliderContent.Score[count];
									formattedText = new FormattedText(comment, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Helvetica Neue - thin"), 12, Brushes.Black);
									dc.PushTransform(new RotateTransform(90, drawAtX, drawAtY));
									dc.DrawText(formattedText, new Point(drawAtX, drawAtY));
									dc.Pop();
									var len = formattedText.Width;
									formattedText = new FormattedText(score, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Helvetica Neue - thin"), 12, Brushes.Black);
									dc.PushTransform(new RotateTransform(90, drawAtX , drawAtY + stringWidth));
									dc.DrawText(formattedText, new Point(drawAtX , drawAtY + stringWidth));
									dc.Pop();
							
								}

							}
						
						
					}
					

				}
				catch (Exception e)
				{
					throw new Exception(e.Message);
				}
						
		}

		protected const string _newline = "\r\n";

		private string WordWrap(string the_string, int width)
		{
			int pos, next;
			StringBuilder sb = new StringBuilder();

			// Lucidity check
			if (width < 1)
				return the_string;

			// Parse each line of text
			for (pos = 0; pos < the_string.Length; pos = next)
			{
				// Find end of line
				int eol = the_string.IndexOf(_newline, pos);

				if (eol == -1)
					next = eol = the_string.Length;
				else
					next = eol + _newline.Length;

				// Copy this line of text, breaking into smaller lines as needed
				if (eol > pos)
				{
					do
					{
						int len = eol - pos;

						if (len > width)
							len = BreakLine(the_string, pos, width);

						sb.Append(the_string, pos, len);
						sb.Append(_newline);

						// Trim whitespace following break
						pos += len;

						while (pos < eol && Char.IsWhiteSpace(the_string[pos]))
							pos++;

					} while (eol > pos);
				}
				else sb.Append(_newline); // Empty line
			}

			return sb.ToString();
		}

		/// <summary>
		/// Locates position to break the given line so as to avoid
		/// breaking words.
		/// </summary>
		/// <param name="text">String that contains line of text</param>
		/// <param name="pos">Index where line of text starts</param>
		/// <param name="max">Maximum line length</param>
		/// <returns>The modified line length</returns>
		private int BreakLine(string text, int pos, int max)
		{
			// Find last whitespace in line
			int i = max - 1;
			while (i >= 0 && !Char.IsWhiteSpace(text[pos + i]))
				i--;
			if (i < 0)
				return max; // No whitespace found; break at maximum length
							// Find start of whitespace
			while (i >= 0 && Char.IsWhiteSpace(text[pos + i]))
				i--;
			// Return length of text before whitespace
			return i + 1;
		}

		public float GetWidthOfTextInPixels()
		{
			var widthList = new List<float>();
			
			using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(new System.Drawing.Bitmap(1, 1)))
			{
				var font = new System.Drawing.Font("Helvetica Neue - thin", 12);
				System.Drawing.SizeF size;
				SliderContent.Comment.ToList().ForEach(item =>
				{
					size = graphics.MeasureString(item, font);
					widthList.Add(size.Width);
				});
				widthList.Sort();
				var maxWidth = widthList.ToList().LastOrDefault();
				
				return maxWidth < 105? maxWidth : 105;
			}
		}
	}
}
