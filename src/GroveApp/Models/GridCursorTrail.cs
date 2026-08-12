using System;

namespace GroveApp.Models
{
    public class SpentCell
    {
        public double WorldX { get; set; }
        public double WorldY { get; set; }
        public double WorldWidth { get; set; }
        public double WorldHeight { get; set; }
        public double Energy { get; set; } = 0.60; // ADR-050 E0; decays with Tokens.CursorTrailDecay

        public SpentCell(double worldX, double worldY, double worldWidth, double worldHeight)
        {
            WorldX = worldX;
            WorldY = worldY;
            WorldWidth = Math.Max(1.0, worldWidth);
            WorldHeight = Math.Max(1.0, worldHeight);
            Energy = 0.60;
        }
    }
}
