using System;
using System.Windows.Forms;
using System.Drawing;

namespace tcp_moe_client.Classes
{
    public class CProgressBar : ProgressBar
    {
        public CProgressBar()
        {
            this.SetStyle(ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rec = e.ClipRectangle;

            rec.Width = (int)(rec.Width * ((double)Value / Maximum)) - 4;
            if (ProgressBarRenderer.IsSupported)
                ProgressBarRenderer.DrawHorizontalBar(e.Graphics, e.ClipRectangle);
            rec.Height = rec.Height - 4;
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(170, 0, 0)), 2, 2, rec.Width, rec.Height);
        }
    }
}
