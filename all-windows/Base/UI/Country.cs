using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartDNSProxy_VPN_Client
{
   public class Country
    {
        // Margins around owner drawn ComboBoxes.
        private const int MarginWidth = 4;
        private const int MarginHeight = 4;

        public Image Flag;
        public string Name;
        public Font Font;

        public Country(Image flag, string name, Font font)
        {
            Flag = flag;
            Name = name;
            Font = font;
        }

        // Set the size needed to display the image and text.S
        private int Width, Height;
        private bool SizeCalculated = false;
        public void MeasureItem(MeasureItemEventArgs e)
        {
            // See if we've already calculated this.
            if (!SizeCalculated)
            {
                SizeCalculated = true;

                // See how much room the text needs.
                SizeF text_size = e.Graphics.MeasureString(Name, Font);

                // The height is the maximum of the image height and text height.
                Height = 2 * MarginHeight + (int)Math.Max(Flag.Height, text_size.Height);

                // The width is the sum of the image and text widths.
                Width = (int)(4 * MarginWidth + Flag.Width + text_size.Width);
            }

            e.ItemWidth = Width;
            e.ItemHeight = Height;
        }

        // Draw the item.
        public void DrawItem(DrawItemEventArgs e)
        {
            // Clear the background appropriately.
            e.DrawBackground();

            // Draw the image.
            float hgt = e.Bounds.Height - 2 * MarginHeight;
            float scale = hgt / Flag.Height;
            float wid = Flag.Width * scale;
            RectangleF rect = new RectangleF(
                e.Bounds.X + MarginWidth,
                e.Bounds.Y + MarginHeight,
                wid, hgt);
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
            e.Graphics.DrawImage(Flag, rect);

            // Draw the text.
            // If we're drawing on the control,
            // draw only the first line of text.
            string visible_text = Name;

            // Make a rectangle to hold the text.
            wid = e.Bounds.Width - rect.Right - 3 * MarginWidth;
            rect = new RectangleF(
                rect.Right + 2 * MarginWidth, rect.Y,
                wid, hgt);
            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Near;
                sf.LineAlignment = StringAlignment.Center;
                e.Graphics.DrawString(visible_text, Font, Brushes.Black, rect, sf);
            }
            //e.Graphics.DrawRectangle(Pens.Blue, Rectangle.Round(rect));

            // Draw the focus rectangle if appropriate.
            e.DrawFocusRectangle();
        }
    }
}
