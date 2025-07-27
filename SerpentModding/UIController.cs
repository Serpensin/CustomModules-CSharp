using System.Diagnostics;

namespace SerpentModding
{
    /// <summary>
    /// Provides static methods and properties for managing the main form's UI, including window title, icon, 
    /// and animated transitions between registered <see cref="UserControl"/> instances. 
    /// Supports customizable transition directions and easing modes for smooth UI animations.
    /// </summary>
    public static partial class UIController
    {
        private static Form? _mainForm;
        private static string _originalTitle = string.Empty;
        private static readonly Dictionary<string, UserControl> _controls = [];
        private static readonly Dictionary<string, Point> _originalPositions = [];
        /// <summary>
        /// Specifies the direction of the UI transition animation when switching between controls.
        /// </summary>
        public enum TransitionDirection
        {
            /// <summary>
            /// Transition from right to left.
            /// </summary>
            Left,
            /// <summary>
            /// Transition from left to right.
            /// </summary>
            Right,
            /// <summary>
            /// Transition from bottom to top.
            /// </summary>
            Up,
            /// <summary>
            /// Transition from top to bottom.
            /// </summary>
            Down,
            /// <summary>
            /// No transition animation.
            /// </summary>
            None
        }
        /// <summary>
        /// Specifies the available easing modes for UI transition animations.
        /// Easing modes determine the rate of change of the animation over time,
        /// allowing for effects such as acceleration, deceleration, and elasticity.
        /// </summary>
        public enum EasingMode
        {
            /// <summary>
            /// Linear interpolation with a constant rate of change.
            /// </summary>
            Linear,
            /// <summary>
            /// Accelerates from zero velocity (ease-in).
            /// </summary>
            EaseIn,
            /// <summary>
            /// Decelerates to zero velocity (ease-out).
            /// </summary>
            EaseOut,
            /// <summary>
            /// Accelerates and then decelerates (ease-in-out).
            /// </summary>
            EaseInOut,
            /// <summary>
            /// Ends with an elastic bounce effect.
            /// </summary>
            ElasticOut,
            /// <summary>
            /// Ends with a bouncing effect.
            /// </summary>
            BounceOut
        }

        /// <summary>
        /// Initializes the <see cref="UIController"/> with the specified main form instance.
        /// Stores a reference to the main form and its original window title for later use.
        /// </summary>
        /// <param name="main">
        /// The <see cref="Form"/> form instance to associate with the UI controller.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="main"/> parameter is <c>null</c>.
        /// </exception>
        public static void Init(Form main)
        {
            _mainForm = main ?? throw new ArgumentNullException(nameof(main));
            _originalTitle = _mainForm.Text;
        }

        /// <summary>
        /// Sets the window title of the main form.
        /// </summary>
        /// <param name="title">
        /// The new title to set for the main form window.
        /// </param>
        /// <param name="remember">
        /// If <c>true</c>, updates the stored original title to the new value.
        /// If <c>false</c>, only changes the window title temporarily.
        /// </param>
        public static void SetWindowTitle(string title, bool remember = false)
        {
            if (_mainForm == null) return;
            _mainForm.Text = title;
            if (remember) _originalTitle = title;
        }

        /// <summary>
        /// Sets the window title of the main form to a temporary value by appending the specified suffix
        /// to the original title. This is useful for indicating temporary states or progress in the UI.
        /// </summary>
        /// <param name="suffix">
        /// The suffix to append to the original window title. A space is automatically inserted between
        /// the original title and the suffix.
        /// </param>
        public static void SetTemporaryTitle(string suffix)
        {
            if (_mainForm != null)
                _mainForm.Text = _originalTitle + " " + suffix;
        }

        /// <summary>
        /// Resets the window title of the main form to its original value.
        /// This method restores the window title to the value that was present
        /// when the <see cref="UIController"/> was initialized or last updated
        /// using <see cref="SetWindowTitle(string, bool)"/> with <c>remember</c> set to <c>true</c>.
        /// </summary>
        public static void ResetTitle()
        {
            if (_mainForm != null)
                _mainForm.Text = _originalTitle;
        }

        /// <summary>
        /// Sets the window icon of the main form.
        /// </summary>
        /// <param name="icon">
        /// The <see cref="Icon"/> to set as the window icon for the main form.
        /// If <paramref name="icon"/> is <c>null</c>, the icon will not be changed.
        /// </param>
        public static void SetWindowIcon(Icon icon)
        {
            if (_mainForm != null && icon != null)
                _mainForm.Icon = icon;
        }

        /// <summary>
        /// Registers a <see cref="UserControl"/> with a unique name for later display and animated transitions.
        /// </summary>
        /// <param name="name">
        /// The unique name to associate with the control. Must not already be registered.
        /// </param>
        /// <param name="control">
        /// The <see cref="UserControl"/> instance to register. Must not already be registered under a different name.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if a control with the specified <paramref name="name"/> is already registered.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the specified <paramref name="control"/> instance is already registered under a different name.
        /// </exception>
        public static void RegisterControl(string name, UserControl control)
        {
            if (_controls.ContainsKey(name))
                throw new ArgumentException($"A control with the name '{name}' is already registered.", nameof(name));
            var existing = _controls.FirstOrDefault(kvp => kvp.Value == control);
            if (!string.IsNullOrEmpty(existing.Key))
                throw new InvalidOperationException($"This UserControl instance is already registered under the name '{existing.Key}'.");

            _controls.Add(name, control);
            _originalPositions[name] = control.Location;
        }

        /// <summary>
        /// Removes a registered <see cref="UserControl"/> by its unique name.
        /// </summary>
        /// <param name="name">The unique name of the control to remove.</param>
        /// <exception cref="KeyNotFoundException">Thrown if no control is registered with the specified <paramref name="name"/>.</exception>
        public static void RemoveControl(string name)
        {
            if (!_controls.ContainsKey(name))
                throw new KeyNotFoundException($"No control registered with the name '{name}'.");
            _controls.Remove(name);
            _originalPositions.Remove(name);
        }

        /// <summary>
        /// Shows the specified registered <see cref="UserControl"/> by name, optionally animating the transition
        /// from the currently visible control using the specified direction, duration, and easing mode.
        /// </summary>
        /// <param name="name">
        /// The unique name of the control to display. Must be registered using <see cref="RegisterControl(string, UserControl)"/>.
        /// </param>
        /// <param name="direction">
        /// The <see cref="TransitionDirection"/> specifying the direction of the transition animation.
        /// Use <see cref="TransitionDirection.None"/> for an instant switch without animation.
        /// </param>
        /// <param name="durationMs">
        /// The duration of the transition animation in milliseconds. Ignored if <paramref name="direction"/> is <see cref="TransitionDirection.None"/>.
        /// </param>
        /// <param name="easingMode">
        /// The <see cref="EasingMode"/> to use for the animation's rate of change. Ignored if <paramref name="direction"/> is <see cref="TransitionDirection.None"/>.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the <see cref="UIController"/> has not been initialized with a main form.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if no control is registered with the specified <paramref name="name"/>.
        /// </exception>
        public static void ShowControl(string name, TransitionDirection direction = TransitionDirection.None, int durationMs = 400, EasingMode easingMode = EasingMode.Linear)
        {
            if (_mainForm == null)
                throw new InvalidOperationException("UIController is not initialized. Call Init(Form main) before using ShowControl.");
            if (!_controls.ContainsKey(name))
                throw new KeyNotFoundException($"No control registered with the name '{name}'.");

            if (direction == TransitionDirection.None)
            {
                foreach (var ctrl in _controls.Values)
                {
                    ctrl.Visible = false;
                    ctrl.SendToBack();
                }

                var target = _controls[name];
                target.Location = _originalPositions[name];
                target.Visible = true;
                target.BringToFront();
            }
            else
            {
                var current = _controls.Values.FirstOrDefault(c => c.Visible);
                var next = _controls[name];

                if (current == null || current == next)
                {
                    foreach (var ctrl in _controls.Values)
                    {
                        ctrl.Visible = false;
                        ctrl.SendToBack();
                    }

                    next.Location = _originalPositions[name];
                    next.Visible = true;
                    next.BringToFront();
                    return;
                }

                AnimateTransition(current, next, direction, durationMs, easingMode);
            }
        }

        /// <summary>
        /// Animates the transition between two <see cref="UserControl"/> instances within the main form.
        /// Moves the <paramref name="from"/> control out and the <paramref name="to"/> control in,
        /// according to the specified <paramref name="direction"/>, over the given <paramref name="durationMs"/>,
        /// using the provided <paramref name="easingMode"/> for the animation curve.
        /// </summary>
        /// <param name="from">
        /// The currently visible <see cref="UserControl"/> to animate out.
        /// </param>
        /// <param name="to">
        /// The <see cref="UserControl"/> to animate in and display after the transition.
        /// </param>
        /// <param name="direction">
        /// The <see cref="TransitionDirection"/> specifying the direction of the transition animation.
        /// </param>
        /// <param name="durationMs">
        /// The duration of the transition animation in milliseconds.
        /// </param>
        /// <param name="easingMode">
        /// The <see cref="EasingMode"/> to use for the animation's rate of change.
        /// </param>
        private static void AnimateTransition(UserControl from, UserControl to, TransitionDirection direction, int durationMs, EasingMode easingMode = EasingMode.Linear)
        {
            if (_mainForm == null) return;

            var size = _mainForm.ClientSize;

            string fromKey = _controls.FirstOrDefault(kvp => kvp.Value == from).Key;
            string toKey = _controls.FirstOrDefault(kvp => kvp.Value == to).Key;

            if (!_originalPositions.TryGetValue(fromKey, out var fromOrigin)) fromOrigin = Point.Empty;
            if (!_originalPositions.TryGetValue(toKey, out var toOrigin)) toOrigin = Point.Empty;

            Point offset = direction switch
            {
                TransitionDirection.Left => new Point(size.Width, 0),
                TransitionDirection.Right => new Point(-size.Width, 0),
                TransitionDirection.Up => new Point(0, size.Height),
                TransitionDirection.Down => new Point(0, -size.Height),
                _ => Point.Empty
            };

            to.Location = new Point(toOrigin.X + offset.X, toOrigin.Y + offset.Y);
            to.Visible = true;
            to.BringToFront();

            var stopwatch = Stopwatch.StartNew();
            var timer = new System.Windows.Forms.Timer { Interval = 10 };

            timer.Tick += (s, e) =>
            {
                double elapsed = stopwatch.Elapsed.TotalMilliseconds;
                double progress = Math.Min(1.0, elapsed / durationMs);

                double eased = easingMode switch
                {
                    EasingMode.Linear => progress,
                    EasingMode.EaseIn => EaseIn(progress),
                    EasingMode.EaseOut => EaseOut(progress),
                    EasingMode.EaseInOut => EaseInOut(progress),
                    EasingMode.ElasticOut => ElasticOut(progress),
                    EasingMode.BounceOut => BounceOut(progress),
                    _ => progress
                };

                from.Location = new Point(
                    fromOrigin.X + (int)(offset.X * eased),
                    fromOrigin.Y + (int)(offset.Y * eased));

                to.Location = new Point(
                    toOrigin.X + (int)(offset.X * (eased - 1)),
                    toOrigin.Y + (int)(offset.Y * (eased - 1)));

                if (progress >= 1.0)
                {
                    timer.Stop();
                    timer.Dispose();
                    stopwatch.Stop();

                    from.Visible = false;
                    from.Location = fromOrigin;
                    to.Location = toOrigin;
                }
            };

            timer.Start();
        }


        /// <summary>
        /// Cubic ease-in function for UI transition animations.
        /// Produces an accelerating curve, starting slow and speeding up toward the end.
        /// </summary>
        /// <param name="t">
        /// The normalized time of the animation, ranging from 0 (start) to 1 (end).
        /// </param>
        /// <returns>
        /// The eased value at time <paramref name="t"/>, between 0 and 1.
        /// </returns>
        private static double EaseIn(double t)
        {
            return t * t * t;
        }

        /// <summary>
        /// Cubic ease-out function for UI transition animations.
        /// Produces a decelerating curve, starting fast and slowing down toward the end.
        /// </summary>
        /// <param name="t">
        /// The normalized time of the animation, ranging from 0 (start) to 1 (end).
        /// </param>
        /// <returns>
        /// The eased value at time <paramref name="t"/>, between 0 and 1.
        /// </returns>
        private static double EaseOut(double t)
        {
            return 1 - Math.Pow(1 - t, 3);
        }

        /// <summary>
        /// Cubic ease-in-out function for UI transition animations.
        /// Produces an animation that accelerates in the first half and decelerates in the second half,
        /// resulting in a smooth start and end to the transition.
        /// </summary>
        /// <param name="t">
        /// The normalized time of the animation, ranging from 0 (start) to 1 (end).
        /// </param>
        /// <returns>
        /// The eased value at time <paramref name="t"/>, between 0 and 1.
        /// </returns>
        private static double EaseInOut(double t)
        {
            return t < 0.5
                ? 4 * t * t * t
                : 1 - Math.Pow(-2 * t + 2, 3) / 2;
        }

        /// <summary>
        /// Elastic ease-out function for UI transition animations.
        /// Produces an elastic effect, where the animation overshoots and oscillates before settling.
        /// </summary>
        /// <param name="t">
        /// The normalized time of the animation, ranging from 0 (start) to 1 (end).
        /// </param>
        /// <returns>
        /// The eased value at time <paramref name="t"/>, between 0 and 1.
        /// </returns>
        private static double ElasticOut(double t)
        {
            const double c4 = 2 * Math.PI / 3;
            const double epsilon = 1e-8;
            if (Math.Abs(t) < epsilon)
                return 0;
            if (Math.Abs(t - 1) < epsilon)
                return 1;
            return Math.Pow(2, -10 * t) * Math.Sin((t * 10 - 0.75) * c4) + 1;
        }

        /// <summary>
        /// Bounce ease-out function for UI transition animations.
        /// Produces a bouncing effect, where the animation overshoots and rebounds before settling.
        /// </summary>
        /// <param name="t">
        /// The normalized time of the animation, ranging from 0 (start) to 1 (end).
        /// </param>
        /// <returns>
        /// The eased value at time <paramref name="t"/>, between 0 and 1.
        /// </returns>
        private static double BounceOut(double t)
        {
            const double n1 = 7.5625;
            const double d1 = 2.75;

            if (t < 1 / d1)
                return n1 * t * t;
            else if (t < 2 / d1)
            {
                t -= 1.5 / d1;
                return n1 * t * t + 0.75;
            }
            else if (t < 2.5 / d1)
            {
                t -= 2.25 / d1;
                return n1 * t * t + 0.9375;
            }
            else
            {
                t -= 2.625 / d1;
                return n1 * t * t + 0.984375;
            }
        }

#if DEBUG
        /// <summary>
        /// Resets the static state of the UIController for testing purposes.
        /// </summary>
        public static void Test_Reset()
        {
            _mainForm = null;
            _originalTitle = string.Empty;
            _controls.Clear();
            _originalPositions.Clear();
        }
#endif
    }
}
