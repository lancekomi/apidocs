using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using MaterialSkin.Animations;

namespace MaterialSkin.Controls
{
    public class MaterialFlatButton : Button, IMaterialControl
    {
        [Browsable(false)]
        public int Depth { get; set; }
        [Browsable(false)]
        public MaterialSkinManager SkinManager => MaterialSkinManager.Instance;
        [Browsable(false)]
        public MouseState MouseState { get; set; }
        public bool Primary { get; set; }

        private readonly AnimationManager _animationManager;
        private readonly AnimationManager _hoverAnimationManager;

        private SizeF _textSize;

        private Image _icon;
        public Image Icon
        {
            get { return _icon; }
            set
            {
                _icon = value;
                if (AutoSize)
                    Size = GetPreferredSize();
                Invalidate();
            }
        }

        public MaterialFlatButton()
        {
            Primary = false;

            _animationManager = new AnimationManager(false)
            {
                Increment = 0.03,
                AnimationType = AnimationType.EaseOut
            };
            _hoverAnimationManager = new AnimationManager
            {
                Increment = 0.07,
                AnimationType = AnimationType.Linear
            };

            _hoverAnimationManager.OnAnimationProgress += sender => Invalidate();
            _animationManager.OnAnimationProgress += sender => Invalidate();

            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            AutoSize = true;
            Margin = new Padding(4, 6, 4, 6);
            Padding = new Padding(0);
        }

        public override string Text
        {
            get { return base.Text; }
            set
            {
                base.Text = value;
                _textSize = CreateGraphics().MeasureString(value.ToUpper(), SkinManager.ROBOTO_MEDIUM_10);
                if (AutoSize)
                    Size = GetPreferredSize();
                Invalidate();
            }
        }

        private GraphicsPath GetRoundPath(RectangleF Rect, int radius)
        {
            float num = (float)radius / 2f;
            GraphicsPath graphicsPath = new GraphicsPath();
            graphicsPath.AddArc(Rect.X, Rect.Y, (float)radius, (float)radius, 180f, 90f);
            graphicsPath.AddLine(Rect.X + num, Rect.Y, Rect.Width - num, Rect.Y);
            graphicsPath.AddArc(Rect.X + Rect.Width - (float)radius, Rect.Y, (float)radius, (float)radius, 270f, 90f);
            graphicsPath.AddLine(Rect.Width, Rect.Y + num, Rect.Width, Rect.Height - num);
            graphicsPath.AddArc(Rect.X + Rect.Width - (float)radius, Rect.Y + Rect.Height - (float)radius, (float)radius, (float)radius, 0f, 90f);
            graphicsPath.AddLine(Rect.Width - num, Rect.Height, Rect.X + num, Rect.Height);
            graphicsPath.AddArc(Rect.X, Rect.Y + Rect.Height - (float)radius, (float)radius, (float)radius, 90f, 90f);
            graphicsPath.AddLine(Rect.X, Rect.Height - num, Rect.X, Rect.Y + num);
            graphicsPath.CloseFigure();
            return graphicsPath;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
		    Graphics graphics = pevent.Graphics;
		    graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
		    graphics.Clear(base.Parent.BackColor);
		    RectangleF rect = new RectangleF(0f, 0f, (float)base.Width, (float)base.Height);
		    GraphicsPath roundPath = GetRoundPath(rect, 35);
		    base.Region = new Region(roundPath);
		    using (Pen pen = new Pen(Color.FromArgb(0, 255, 255, 255), 1f))
		    {
			    pen.Alignment = PenAlignment.Inset;
			    pevent.Graphics.DrawPath(pen, roundPath);
		    }
		    Color flatButtonHoverBackgroundColor = SkinManager.GetFlatButtonHoverBackgroundColor();
		    using (Brush brush = new SolidBrush(Color.FromArgb((int)(_hoverAnimationManager.GetProgress() * (double)(int)flatButtonHoverBackgroundColor.A), flatButtonHoverBackgroundColor.RemoveAlpha())))
		    {
			    graphics.FillRectangle(brush, base.ClientRectangle);
		    }
		    if (_animationManager.IsAnimating())
		    {
			    graphics.SmoothingMode = SmoothingMode.AntiAlias;
			    for (int i = 0; i < _animationManager.GetAnimationCount(); i++)
			    {
				    double progress = _animationManager.GetProgress(i);
				    Point source = _animationManager.GetSource(i);
				    using (Brush brush2 = new SolidBrush(Color.FromArgb((int)(101.0 - progress * 100.0), Color.Black)))
				    {
					    int num = (int)(progress * (double)base.Width * 2.0);
					    graphics.FillEllipse(brush2, new Rectangle(source.X - num / 2, source.Y - num / 2, num, num));
				    }
			    }
			    graphics.SmoothingMode = SmoothingMode.None;
		    }
		    Rectangle rect2 = new Rectangle(8, 6, 24, 24);
		    if (string.IsNullOrEmpty(Text))
		    {
			    rect2.X += 2;
		    }
		    if (Icon != null)
		    {
			    graphics.DrawImage(Icon, rect2);
		    }
		    Rectangle clientRectangle = base.ClientRectangle;
		    if (Icon != null)
		    {
			    clientRectangle.Width -= 44;
			    clientRectangle.X += 36;
		    }
		    graphics.DrawString(Text, SkinManager.ROBOTO_MEDIUM_10, base.Enabled ? (Primary ? SkinManager.ColorScheme.PrimaryBrush : SkinManager.GetPrimaryTextBrush()) : SkinManager.GetFlatButtonDisabledTextBrush(), clientRectangle, new StringFormat
		    {
			    Alignment = StringAlignment.Center,
			    LineAlignment = StringAlignment.Center
		    });
        }

        private Size GetPreferredSize()
        {
            return GetPreferredSize(new Size(0, 0));
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            int num = 90;
            if (Icon != null)
            {
                num += 28;
            }
            return new Size((int)Math.Ceiling(_textSize.Width) + num, 38);
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (DesignMode) return;

            MouseState = MouseState.OUT;
            MouseEnter += (sender, args) =>
            {
                MouseState = MouseState.HOVER;
                _hoverAnimationManager.StartNewAnimation(AnimationDirection.In);
                Invalidate();
            };
            MouseLeave += (sender, args) =>
            {
                MouseState = MouseState.OUT;
                _hoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
                Invalidate();
            };
            MouseDown += (sender, args) =>
            {
                if (args.Button == MouseButtons.Left)
                {
                    MouseState = MouseState.DOWN;

                    _animationManager.StartNewAnimation(AnimationDirection.In, args.Location);
                    Invalidate();
                }
            };
            MouseUp += (sender, args) =>
            {
                MouseState = MouseState.HOVER;

                Invalidate();
            };
        }
    }
}
