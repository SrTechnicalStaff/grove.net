namespace GroveApp.Models
{
    public class SpentCell
    {
        public int CellX { get; set; }
        public int CellY { get; set; }
        public double Energy { get; set; } = 1.0; // Decays from 1.0 down to 0.0

        public SpentCell(int cellX, int cellY)
        {
            CellX = cellX;
            CellY = cellY;
            Energy = 1.0;
        }
    }
}
