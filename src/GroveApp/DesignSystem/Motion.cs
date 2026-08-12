using System;

namespace GroveApp.DesignSystem
{
    /// <summary>
    /// Normative Grove Design System Foundations - Motion (Motion.md, Tokens.md).
    /// </summary>
    public static class Motion
    {
        // Duration Tokens (in Milliseconds)
        public const double PressDurationMs = 90.0;  // --d-press: Pointer-down acknowledgement
        public const double FadeDurationMs = 120.0;  // --d-fade: Chrome answering approach or leaving
        public const double SwapDurationMs = 160.0;  // --d-swap: Exchanging representation in place
        public const double ExitDurationMs = 200.0;  // --d-exit: Content leaving the Grid
        public const double PlaceDurationMs = 280.0; // --d-place: Arrival and placement confirmation
        public const double SweepDurationMs = 480.0; // --d-sweep: Single expanding mark

        // TimeSpan Helpers
        public static TimeSpan PressDuration => TimeSpan.FromMilliseconds(GetDurationMs(PressDurationMs));
        public static TimeSpan FadeDuration => TimeSpan.FromMilliseconds(GetDurationMs(FadeDurationMs));
        public static TimeSpan SwapDuration => TimeSpan.FromMilliseconds(GetDurationMs(SwapDurationMs));
        public static TimeSpan ExitDuration => TimeSpan.FromMilliseconds(GetDurationMs(ExitDurationMs));
        public static TimeSpan PlaceDuration => TimeSpan.FromMilliseconds(GetDurationMs(PlaceDurationMs));
        public static TimeSpan SweepDuration => TimeSpan.FromMilliseconds(GetDurationMs(SweepDurationMs));

        /// <summary>
        /// Global reduced motion accessibility override.
        /// Under reduced motion, all duration tokens resolve to 0 ms.
        /// </summary>
        public static bool IsReducedMotionEnabled { get; set; } = false;

        public static double GetDurationMs(double tokenMs) => IsReducedMotionEnabled ? 0.0 : tokenMs;

        /// <summary>
        /// Evaluates a 1D cubic Bezier curve for time ratio x in [0, 1].
        /// </summary>
        public static double EvaluateCubicBezier(double x, double x1, double y1, double x2, double y2)
        {
            x = Math.Clamp(x, 0.0, 1.0);

            // Solve x(u) = x for u in [0, 1] using Newton-Raphson with bisection fallback
            double uLow = 0.0;
            double uHigh = 1.0;
            double u = x;

            for (int i = 0; i < 8; i++)
            {
                double currentX = 3 * (1 - u) * (1 - u) * u * x1 + 3 * (1 - u) * u * u * x2 + u * u * u;
                double err = currentX - x;
                if (Math.Abs(err) < 1e-6) break;

                double dxdu = 3 * (1 - u) * (1 - u) * x1 + 6 * (1 - u) * u * (x2 - x1) + 3 * u * u * (1 - x2);
                if (Math.Abs(dxdu) < 1e-6)
                {
                    if (err > 0) uHigh = u; else uLow = u;
                    u = (uLow + uHigh) * 0.5;
                }
                else
                {
                    double uNext = u - err / dxdu;
                    if (uNext < uLow || uNext > uHigh)
                    {
                        if (err > 0) uHigh = u; else uLow = u;
                        u = (uLow + uHigh) * 0.5;
                    }
                    else
                    {
                        u = uNext;
                    }
                }
            }

            return 3 * (1 - u) * (1 - u) * u * y1 + 3 * (1 - u) * u * u * y2 + u * u * u;
        }

        /// <summary>
        /// Primary easing curve --ease cubic-bezier(0.25, 0.1, 0.25, 1.0)
        /// </summary>
        public static double EvaluateEase(double progress) => EvaluateCubicBezier(progress, 0.25, 0.1, 0.25, 1.0);

        /// <summary>
        /// Placement arrival easing curve --overshoot cubic-bezier(0.2, 1.25, 0.3, 1.0)
        /// </summary>
        public static double EvaluateOvershoot(double progress) => EvaluateCubicBezier(progress, 0.2, 1.25, 0.3, 1.0);
    }
}
