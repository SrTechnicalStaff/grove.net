namespace GroveApp.Models
{
    public class SpentCell
    {
        public int CellX { get; set; }
        public int CellY { get; set; }
        public double Energy { get; set; } = 0.60; // ADR-050 E0; decays with Tokens.CursorTrailDecay

        public SpentCell(int cellX, int cellY)
        {
            CellX = cellX;
            CellY = cellY;
            Energy = 0.60;
        }
    }
}
