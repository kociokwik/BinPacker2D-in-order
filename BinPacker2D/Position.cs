namespace BinPacker2D
{
    /// <summary>
    /// Represents space for packing elements
    /// </summary>
    public class Position
    {
        public double posX, posY;
        public double width;
        public double height;

        /// <summary>
        /// Create a copy of the Position
        /// </summary>
        /// <param name="copyPos">Position to copy</param>
        public Position(Position copyPos)
        {
            width = copyPos.width;
            height = copyPos.height;
            posX = copyPos.posX;
            posY = copyPos.posY;
        }

        /// <summary>
        /// Create new position
        /// </summary>
        /// <param name="height">Height of the new Position</param>
        /// <param name="width">Width of the new Position</param>
        /// <param name="posX">X coordinate of the new Position</param>
        /// <param name="posY">Y coordinate of the new Position</param>
        public Position(double height, double width, double posX, double posY)
        {
            this.height = height;
            this.width = width;
            this.posX = posX;
            this.posY = posY;
        }
    }
}
