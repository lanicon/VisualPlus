namespace VisualPlus.Toolkit.Controls.DataVisualization
{
    #region Namespace

    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    using VisualPlus.Enumerators;
    using VisualPlus.Localization.Category;
    using VisualPlus.Localization.Descriptions;
    using VisualPlus.Managers;
    using VisualPlus.Renders;
    using VisualPlus.Structure;
    using VisualPlus.Toolkit.Components;
    using VisualPlus.Toolkit.VisualBase;

    #endregion

    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(ProgressBar))]
    [DefaultEvent("Click")]
    [DefaultProperty("Value")]
    [Description("The Visual ProgressBar")]
    [Designer(ControlManager.FilterProperties.VisualProgressBar)]
    public class VisualProgressBar : ProgressBase
    {
        #region Variables

        private Border _border;
        private ColorState _colorState;
        private Hatch _hatch;
        private Timer _marqueeTimer;
        private bool _marqueeTimerEnabled;
        private int _marqueeWidth;
        private int _marqueeX;
        private int _marqueeY;
        private Orientation _orientation;
        private bool _percentageVisible;
        private ProgressBarStyle _progressBarStyle;
        private Color _progressColor;
        private Image _progressImage;
        private StringAlignment _valueAlignment;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="T:VisualPlus.Toolkit.Controls.DataVisualization.VisualProgressBar" /> class.
        /// </summary>
        public VisualProgressBar()
        {
            Maximum = 100;
            _hatch = new Hatch();
            _colorState = new ColorState();
            _orientation = Orientation.Horizontal;
            _marqueeWidth = 20;
            Size = new Size(100, 20);
            _percentageVisible = true;
            _border = new Border();
            _progressBarStyle = ProgressBarStyle.Blocks;
            _valueAlignment = StringAlignment.Center;
            UpdateTheme(Settings.DefaultValue.DefaultStyle);
        }

        #endregion

        #region Properties

        [TypeConverter(typeof(ColorStateConverter))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Category(Propertys.Appearance)]
        public ColorState BackColorState
        {
            get => _colorState;

            set
            {
                if (value == _colorState)
                {
                    return;
                }

                _colorState = value;
                Invalidate();
            }
        }

        [TypeConverter(typeof(BorderConverter))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Category(Propertys.Appearance)]
        public Border Border
        {
            get => _border;

            set
            {
                _border = value;
                Invalidate();
            }
        }

        [TypeConverter(typeof(HatchConverter))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Category(Propertys.Behavior)]
        public Hatch Hatch
        {
            get => _hatch;

            set
            {
                _hatch = value;
                Invalidate();
            }
        }

        [Category(Propertys.Layout)]
        [Description(Property.Size)]
        public int MarqueeWidth
        {
            get => _marqueeWidth;

            set
            {
                _marqueeWidth = value;
                Invalidate();
            }
        }

        [Category(Propertys.Behavior)]
        [Description(Property.Orientation)]
        public Orientation Orientation
        {
            get => _orientation;

            set
            {
                _orientation = value;

                if (_orientation == Orientation.Horizontal)
                {
                    if (Width < Height)
                    {
                        int temp = Width;
                        Width = Height;
                        Height = temp;
                    }
                }
                else
                {
                    // Vertical
                    if (Width > Height)
                    {
                        int temp = Width;
                        Width = Height;
                        Height = temp;
                    }
                }

                // Resize check
                OnResize(EventArgs.Empty);

                Invalidate();
            }
        }

        [DefaultValue(Settings.DefaultValue.TextVisible)]
        [Category(Propertys.Appearance)]
        [Description(Property.Visible)]
        public bool PercentageVisible
        {
            get => _percentageVisible;

            set
            {
                _percentageVisible = value;
                Invalidate();
            }
        }

        [Category(Propertys.Appearance)]
        [Description(Property.Color)]
        public Color ProgressColor
        {
            get => _progressColor;

            set
            {
                _progressColor = value;
                Invalidate();
            }
        }

        [Category(Propertys.Appearance)]
        [Description(Property.Image)]
        public Image ProgressImage
        {
            get => _progressImage;

            set
            {
                _progressImage = value;
                Invalidate();
            }
        }

        [DefaultValue(typeof(ProgressBarStyle))]
        [Category(Propertys.Behavior)]
        [Description(Property.ProgressBarStyle)]
        public ProgressBarStyle Style
        {
            get => _progressBarStyle;

            set
            {
                _progressBarStyle = value;
                Invalidate();
            }
        }

        [Category(Propertys.Layout)]
        [Description(Property.Alignment)]
        public StringAlignment ValueAlignment
        {
            get => _valueAlignment;

            set
            {
                _valueAlignment = value;
                Invalidate();
            }
        }

        #endregion

        #region Events

        public void UpdateTheme(Styles style)
        {
            StyleManager = new VisualStyleManager(style);

            ForeColor = StyleManager.FontStyle.ForeColor;
            ForeColorDisabled = StyleManager.FontStyle.ForeColorDisabled;

            _colorState.Enabled = StyleManager.ProgressStyle.BackProgress;
            _colorState.Disabled = StyleManager.ProgressStyle.ProgressDisabled;

            _hatch.BackColor = StyleManager.ProgressStyle.Hatch;
            _hatch.ForeColor = Color.FromArgb(40, _hatch.BackColor);

            _progressColor = StyleManager.ProgressStyle.Progress;

            _border.Color = StyleManager.ShapeStyle.Color;
            _border.HoverColor = StyleManager.BorderStyle.HoverColor;

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics _graphics = e.Graphics;
            _graphics.Clear(Parent.BackColor);
            _graphics.SmoothingMode = SmoothingMode.HighQuality;
            _graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            _graphics.TextRenderingHint = TextRenderingHint;
            Rectangle _clientRectangle = new Rectangle(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width - 1, ClientRectangle.Height - 1);
            ControlGraphicsPath = VisualBorderRenderer.CreateBorderTypePath(_clientRectangle, _border);
            _graphics.FillRectangle(new SolidBrush(BackColor), _clientRectangle);

            Color _backColor = Enabled ? BackColorState.Enabled : BackColorState.Disabled;
            VisualBackgroundRenderer.DrawBackground(_graphics, _backColor, BackgroundImage, MouseState, _clientRectangle, _border);

            DrawProgress(_orientation, _graphics);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            e.Graphics.Clear(BackColor);
        }

        private void DrawProgress(Orientation orientation, Graphics graphics)
        {
            if (_progressBarStyle == ProgressBarStyle.Marquee)
            {
                if (!DesignMode && Enabled)
                {
                    StartTimer();
                }

                if (!Enabled)
                {
                    StopTimer();
                }

                if (Value == Maximum)
                {
                    StopTimer();
                    DrawProgressContinuous(graphics);
                }
                else
                {
                    DrawProgressMarquee(graphics);
                }
            }
            else
            {
                int _indexValue;

                GraphicsPath _progressPath;
                Rectangle _progressRectangle;

                switch (orientation)
                {
                    case Orientation.Horizontal:
                        {
                            _indexValue = (int)Math.Round(((Value - Minimum) / (double)(Maximum - Minimum)) * (Width - 2));
                            _progressRectangle = new Rectangle(_border.Thickness, _border.Thickness, _indexValue - _border.Thickness, Height - _border.Thickness);
                            _progressPath = VisualBorderRenderer.CreateBorderTypePath(_progressRectangle, _border);
                        }

                        break;
                    case Orientation.Vertical:
                        {
                            _indexValue = (int)Math.Round(((Value - Minimum) / (double)(Maximum - Minimum)) * (Height - 2));
                            _progressRectangle = new Rectangle(_border.Thickness, Height - _indexValue - _border.Thickness, Width - _border.Thickness, _indexValue);
                            _progressPath = VisualBorderRenderer.CreateBorderTypePath(_progressRectangle, _border);
                        }

                        break;
                    default:
                        {
                            throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null);
                        }
                }

                if (_indexValue > 1)
                {
                    graphics.SetClip(ControlGraphicsPath);
                    VisualBackgroundRenderer.DrawBackground(graphics, _progressColor, _progressRectangle);
                    VisualControlRenderer.DrawHatch(graphics, _hatch, _progressPath);
                    graphics.ResetClip();
                }
            }

            DrawText(graphics);
        }

        private void DrawProgressContinuous(Graphics graphics)
        {
            GraphicsPath _progressPath = new GraphicsPath();
            _progressPath.AddRectangle(new Rectangle(0, 0, (Value / Maximum) * ClientRectangle.Width, ClientRectangle.Height));
            graphics.FillPath(new SolidBrush(_progressColor), _progressPath);
        }

        private void DrawProgressMarquee(Graphics graphics)
        {
            Rectangle _progressRectangle;

            switch (_orientation)
            {
                case Orientation.Horizontal:
                    {
                        _progressRectangle = new Rectangle(_marqueeX, 0, _marqueeWidth, ClientRectangle.Height);
                        break;
                    }

                case Orientation.Vertical:
                    {
                        _progressRectangle = new Rectangle(0, _marqueeY, ClientRectangle.Width, ClientRectangle.Height);
                        break;
                    }

                default:
                    {
                        throw new ArgumentOutOfRangeException();
                    }
            }

            GraphicsPath _progressPath = new GraphicsPath();
            _progressPath.AddRectangle(_progressRectangle);

            graphics.FillPath(new SolidBrush(_progressColor), _progressPath);

            VisualControlRenderer.DrawHatch(graphics, _hatch, _progressPath);

            graphics.ResetClip();
        }

        private void DrawText(Graphics graphics)
        {
            // Draw value as a string
            string percentValue = Convert.ToString(Convert.ToInt32(Value)) + "%";

            // Toggle percentage
            if (_percentageVisible)
            {
                StringFormat stringFormat = new StringFormat
                    {
                        Alignment = _valueAlignment,
                        LineAlignment = StringAlignment.Center
                    };

                graphics.DrawString(
                    percentValue,
                    Font,
                    new SolidBrush(ForeColor),
                    new Rectangle(0, 0, Width, Height + 2),
                    stringFormat);
            }
        }

        private void MarqueeTimer_Tick(object sender, EventArgs e)
        {
            switch (_orientation)
            {
                case Orientation.Horizontal:
                    {
                        _marqueeX++;
                        if (_marqueeX > ClientRectangle.Width)
                        {
                            _marqueeX = -_marqueeWidth;
                        }

                        break;
                    }

                case Orientation.Vertical:
                    {
                        _marqueeY++;
                        if (_marqueeY > ClientRectangle.Height)
                        {
                            _marqueeY = -_marqueeWidth;
                        }

                        break;
                    }

                default:
                    {
                        throw new ArgumentOutOfRangeException();
                    }
            }

            Invalidate();
        }

        private void StartTimer()
        {
            if (_marqueeTimerEnabled)
            {
                return;
            }

            if (_marqueeTimer == null)
            {
                _marqueeTimer = new Timer { Interval = 10 };
                _marqueeTimer.Tick += MarqueeTimer_Tick;
            }

            switch (_orientation)
            {
                case Orientation.Horizontal:
                    {
                        _marqueeX = -_marqueeWidth;
                        break;
                    }

                case Orientation.Vertical:
                    {
                        _marqueeY = -_marqueeWidth;
                        break;
                    }

                default:
                    {
                        throw new ArgumentOutOfRangeException();
                    }
            }

            _marqueeTimer.Stop();
            _marqueeTimer.Start();

            _marqueeTimerEnabled = true;

            Invalidate();
        }

        private void StopTimer()
        {
            if (_marqueeTimer == null)
            {
                return;
            }

            _marqueeTimer.Stop();

            Invalidate();
        }

        #endregion
    }
}