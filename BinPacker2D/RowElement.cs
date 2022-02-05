using System.Collections.Generic;

namespace BinPacker2D
{
    public static class Extensions
    {
        public static List<RowElement> GetClone(this List<RowElement> source)
        {
            return source.ConvertAll(item => new RowElement(item));
        }
    }

    /// <summary>
    /// Represent element to pack
    /// </summary>
    public class RowElement
    {
        public int index;
        public int sortIndex;
        public double posX;
        public double posY;
        public double width;
        public double height;
        public bool isRotated = false;

        public bool IsSquare => width == height;

        public RowElement() { }

        /// <summary>
        /// Creating a copy of an element
        /// </summary>
        /// <param name="elementToCopy">Element to copy from</param>
        public RowElement(RowElement elementToCopy)
        {
            index = elementToCopy.index;
            sortIndex = elementToCopy.sortIndex;
            posX = elementToCopy.posX;
            posY = elementToCopy.posY;
            width = elementToCopy.width;
            height = elementToCopy.height;
            isRotated = elementToCopy.isRotated;
        }

        /// <summary>
        /// Creating a copy of an element with new coordinates
        /// </summary>
        /// <param name="elementToCopy">Element to copy from</param>
        /// <param name="posX">New X coordinate</param>
        /// <param name="posY">New Y coordinate</param>
        public RowElement(RowElement elementToCopy, double posX, double posY)
        {
            index = elementToCopy.index;
            sortIndex = elementToCopy.sortIndex;
            this.posX = posX;
            this.posY = posY;
            width = elementToCopy.width;
            height = elementToCopy.height;
            isRotated = elementToCopy.isRotated;
        }

        /// <summary>
        /// Check if this Element can fit inside given space
        /// </summary>
        /// <param name="space">Space to check</param>
        /// <param name="canFitBoth">Element can fit in both orientations</param>
        /// <param name="canFitVertical">Element can fit vertically</param>
        /// <param name="canFitHorizontal">Element can fit horizontally</param>
        public void CheckFitting(Position space, out bool canFitBoth, out bool canFitVertical, out bool canFitHorizontal)
        {
            if (width <= space.width && height <= space.height)
            {
                canFitVertical = true;
            }
            else canFitVertical = false;

            if (width <= space.height && height <= space.width)
            {
                canFitHorizontal = true;
            }
            else canFitHorizontal = false;

            canFitBoth = canFitVertical & canFitHorizontal;
        }
        public RowElement GetRotatedCopy()
        {
            var w = width;
            var elementCopy = new RowElement
            {
                isRotated = !isRotated,
                width = height,
                height = w,
                index = index,
                sortIndex = sortIndex,
                posX = posX,
                posY = posY
            };
            return elementCopy;
        }
    }
}
