using System.Drawing.Drawing2D;
using Timer = System.Windows.Forms.Timer;

namespace CustomModules_CSharp.KITTScanner
{
    /// <summary>
    /// Provides a KITT-style scanning beam animation for a target <see cref="Control"/>.
    /// </summary>
    public class KITTScanner
    {
        private readonly Control target;
        private readonly Timer timer;

        private int position;
        private int direction = 1;
        private bool isRunning = false;

        /// <summary>
        /// Gets or sets the width of the scanning beam in pixels.
        /// </summary>
        public int BeamWidth { get; set; } = 100;

        /// <summary>
        /// Gets or sets the speed of the scanning beam in pixels per tick.
        /// </summary>
        public int Speed { get; set; } = 10;

        /// <summary>
        /// Gets or sets the main color of the scanning beam.
        /// </summary>
        public Color MainColor { get; set; } = Color.Red;

        /// <summary>
        /// Initializes a new instance of the <see cref="KITTScanner"/> class for the specified target control.
        /// </summary>
        /// <param name="target">The control on which to display the scanning beam.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="target"/> is <c>null</c>.</exception>
        public KITTScanner(Control target)
        {
            this.target = target ?? throw new ArgumentNullException(nameof(target));
            this.timer = new Timer { Interval = 30 };
            this.timer.Tick += OnTick;
            this.target.Paint += OnPaint!;
        }

        /// <summary>
        /// Starts the scanning animation.
        /// </summary>
        public void Start()
        {
            if (isRunning) return;
            isRunning = true;
            position = 0;
            direction = 1;
            timer.Start();
        }

        /// <summary>
        /// Stops the scanning animation.
        /// </summary>
        public void Stop()
        {
            if (!isRunning) return;
            isRunning = false;
            timer.Stop();
            target.Invalidate();
        }

        /// <summary>
        /// Handles the timer tick event to update the beam position and direction.
        /// </summary>
        private void OnTick(object? sender, EventArgs e)
        {
            position += Speed * direction;

            if (position + BeamWidth >= target.Width || position <= 0)
            {
                direction *= -1;
            }

            target.Invalidate();
        }

        /// <summary>
        /// Handles the Paint event to render the scanning beam.
        /// </summary>
        private void OnPaint(object? sender, PaintEventArgs e)
        {
            if (!isRunning) return;

            Rectangle beamRect = new(position, 0, BeamWidth, target.Height);

            using LinearGradientBrush gradient = new(
                beamRect,
                Color.Transparent,
                Color.Transparent,
                LinearGradientMode.Horizontal);
            Color[] colors =
            [
                Color.FromArgb(0, MainColor),           // Start - transparent
                Color.FromArgb(200, MainColor),         // Soft glow
                Color.FromArgb(255, MainColor),         // Center - max
                Color.FromArgb(200, MainColor),         // Soft glow
                Color.FromArgb(0, MainColor)            // End - transparent
            ];

            float[] positions = [0f, 0.25f, 0.5f, 0.75f, 1f];

            ColorBlend blend = new()
            {
                Colors = colors,
                Positions = positions
            };

            gradient.InterpolationColors = blend;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.FillRectangle(gradient, beamRect);
        }
    }
}
