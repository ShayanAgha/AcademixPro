namespace AcademixPro.Helpers
{
    /// <summary>
    /// Centralized color palette for the entire application.
    /// Modern light theme with clean whites, soft grays, and vivid accents.
    /// </summary>
    public static class AppColors
    {
        // ── Background Layers ──────────────────────────────────────────
        public static readonly Color Background      = Color.FromArgb(244, 245, 249);   // Soft cool-gray canvas
        public static readonly Color Surface         = Color.FromArgb(255, 255, 255);   // Pure white cards/panels
        public static readonly Color SurfaceLight    = Color.FromArgb(237, 239, 245);   // Input fields / alt rows
        public static readonly Color SurfaceHover    = Color.FromArgb(224, 228, 238);   // Hover states

        // ── Accent Colors ──────────────────────────────────────────────
        public static readonly Color Primary         = Color.FromArgb(79, 70, 229);     // Rich indigo
        public static readonly Color PrimaryDark     = Color.FromArgb(67, 56, 202);     // Darker indigo
        public static readonly Color PrimaryLight    = Color.FromArgb(129, 120, 247);   // Lighter indigo
        public static readonly Color Secondary       = Color.FromArgb(139, 92, 246);    // Violet

        // ── Semantic Colors ────────────────────────────────────────────
        public static readonly Color Success         = Color.FromArgb(22, 163, 74);     // Green
        public static readonly Color Warning         = Color.FromArgb(234, 179, 8);     // Amber
        public static readonly Color Danger          = Color.FromArgb(220, 38, 38);     // Red
        public static readonly Color Info            = Color.FromArgb(14, 165, 233);    // Sky blue

        // ── Text Colors ────────────────────────────────────────────────
        public static readonly Color TextPrimary     = Color.FromArgb(30, 30, 46);      // Near-black
        public static readonly Color TextSecondary   = Color.FromArgb(100, 106, 124);   // Slate gray
        public static readonly Color TextMuted       = Color.FromArgb(148, 155, 172);   // Light gray

        // ── Border ─────────────────────────────────────────────────────
        public static readonly Color Border          = Color.FromArgb(210, 215, 226);   // Subtle border
        public static readonly Color BorderLight     = Color.FromArgb(226, 230, 240);   // Even lighter border

        // ── Card stat colors ───────────────────────────────────────────
        public static readonly Color CardBlue        = Color.FromArgb(37, 99, 235);     // Blue
        public static readonly Color CardPurple      = Color.FromArgb(124, 58, 237);    // Purple
        public static readonly Color CardEmerald     = Color.FromArgb(5, 150, 105);     // Emerald
        public static readonly Color CardAmber       = Color.FromArgb(217, 119, 6);     // Amber
        public static readonly Color CardRose        = Color.FromArgb(225, 29, 72);     // Rose
    }
}
