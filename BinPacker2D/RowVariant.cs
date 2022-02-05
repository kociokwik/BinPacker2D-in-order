using System.Collections.Generic;
using System.Linq;

namespace BinPacker2D
{
    /// <summary>
    /// Represents single row of elements. Each row is defined by the height of the first inserted element.
    /// </summary>
    public class RowVariant
    {
        /// <summary>
        /// Used for splitting row later in the process
        /// </summary>
        public double smallestElementDimension;

        /// <summary>
        /// Spacing to apply between packed elements
        /// </summary>
        public double spacing;

        public List<RowElement> packedElements = new List<RowElement>();
        public List<RowElement> inputElements;

        /// <summary>
        /// List of splitted rows
        /// </summary>
        public List<Position> topSplits = new List<Position>();

        private Position _space;

        /// <summary>
        /// Free space on the right side of the last inserted element with height equal to height of prev. element
        /// </summary>
        public Position Space
        {
            get => _space;
            set => _space = new Position(value);
        }

        /// <summary>
        /// Make a copy of the rowVariant and insert rotated element
        /// </summary>
        /// <param name="rowVariantToCopy">Current rowVariant</param>
        /// <param name="element">Element to insert - WILL BE ROTATED</param>
        private RowVariant(RowVariant rowVariantToCopy, RowElement element)
        {
            smallestElementDimension = rowVariantToCopy.smallestElementDimension;
            spacing = rowVariantToCopy.spacing;
            inputElements = rowVariantToCopy.inputElements;
            packedElements = new List<RowElement>(rowVariantToCopy.packedElements);
            topSplits = new List<Position>(rowVariantToCopy.topSplits);
            Space = rowVariantToCopy.Space;
            Insert(element.GetRotatedCopy());
        }

        /// <summary>
        /// Create new RowVariant
        /// </summary>
        /// <param name="space">Available Table Space</param>
        /// <param name="spacing">Spacing between elements</param>
        /// <param name="elementsInput">Elements to pack</param>
        /// <param name="isRotated">If true, will rotate first element before inserting</param>
        public RowVariant(Position space, double spacing, List<RowElement> elementsInput, bool isRotated)
        {
            var rowHeight = isRotated ? elementsInput.First().width : elementsInput.First().height;

            //
            // Used for splitting row later in the process
            smallestElementDimension = elementsInput.Last().height < elementsInput.Last().width
                ? elementsInput.Last().height
                : elementsInput.Last().width;

            this.spacing = spacing;
            Space = new Position(rowHeight, space.width, space.posX, space.posY);
            inputElements = elementsInput;
            
            if (!isRotated) Insert(inputElements.First());
            else Insert(inputElements.First().GetRotatedCopy());
        }

        /// <summary>
        /// Inserts copy of the element and adjusts free space of this RowVariant
        /// </summary>
        /// <param name="element">Element to copy and insert</param>
        public void Insert(RowElement element)
        {
            var elementCopy = new RowElement(element, Space.posX, Space.posY);
            packedElements.Add(elementCopy);
            this.GetNewSpace(element);
        }

        /// <summary>
        /// Create new Space for the RowVariant. If possible, create split position for later checking
        /// </summary>
        /// <param name="element">Element to subtract from current space</param>
        public void GetNewSpace(RowElement element)
        {
            //
            // If element leaves free space above it, it will be used as a row later
            // If this space is smaller than smallest element in input, then it makes no sense to keep such row as a free space
            if (element.height + spacing + smallestElementDimension < Space.height)
            {
                var topPosition = new Position(Space.height - element.height - spacing,
                                                    Space.width,
                                                    Space.posX,
                                                    Space.posY + element.height + spacing);
                this.topSplits.Add(topPosition);
            }
            this.Space.height = element.height;
            this.Space.posX = Space.posX + element.width + spacing;
            this.Space.width = Space.width - element.width - spacing;
        }

        /// <summary>
        /// Gets next element to pack
        /// </summary>
        /// <returns>Null if there are no more elements to pack</returns>
        public RowElement GetNextElement()
        {
            var currentIndex = 0;
            if (packedElements.Count != 0) currentIndex = packedElements.Last().sortIndex + 1;

            return inputElements.Find(x => x.sortIndex == currentIndex);
        }

        /// <summary>
        /// Inserts next element into current MAIN ROW. When there is no more free space, tries to insert into next available split
        /// </summary>
        /// <param name="variantsToCheck">New Variants will be inserted here to check later</param>
        /// <param name="finishedVariants">For Variants with no more elements to pack or with no free space</param>
        /// <param name="insertIntoNextLine">If true, tries to insert into next available split</param>
        private void InsertNextElement(ref List<RowVariant> variantsToCheck, ref List<RowVariant> finishedVariants, bool insertIntoNextLine)
        {
            var element = this.GetNextElement();

            //
            // No more elements to pack
            if (element == null)
            {
                finishedVariants.Add(this);
                return;
            }

            if (insertIntoNextLine)
            {
                //
                // If there are split rows in current MAIN ROW, set last split as current space
                if (topSplits.Count > 0)
                {
                    Space = topSplits.Last();
                    topSplits.RemoveAt(topSplits.Count - 1);
                }
                else
                {
                    finishedVariants.Add(this);
                    return;
                }
            }

            //
            // Insert element if it fits inside current Space
            element.CheckFitting(Space, out var canFitBothRotations, out var canFitVertical, out var canFitHorizontal);

            if (canFitBothRotations)
            {
                //
                // Create a copy of the RowVariant with rotated element for later checking
                if (!element.IsSquare)
                {
                    var variant2 = new RowVariant(this, element);
                    variantsToCheck.Add(variant2);
                }

                this.Insert(element);
                this.InsertNextElement(ref variantsToCheck, ref finishedVariants, false);
            }
            else if (canFitVertical)
            {
                this.Insert(element);
                this.InsertNextElement(ref variantsToCheck, ref finishedVariants, false);
            }
            else if (canFitHorizontal)
            {
                this.Insert(element.GetRotatedCopy());
                this.InsertNextElement(ref variantsToCheck, ref finishedVariants, false);
            }
            else
            {
                //
                // No more free space in current (bottom) split, try next(top) one
                this.InsertNextElement(ref variantsToCheck, ref finishedVariants, true);
            }
        }

        /// <summary>
        /// Inserts next element into current MAIN ROW, when there is no more free space, tries to insert into next available split. Removes first element from variantsToCheck list
        /// </summary>
        /// <param name="variantsToCheck">New Variants will be inserted here to check later</param>
        /// <param name="finishedVariants">For Variants with no more elements to pack or with no free space in current MAIN ROW</param>
        public void InsertElement(ref List<RowVariant> variantsToCheck, ref List<RowVariant> finishedVariants)
        {
            variantsToCheck.RemoveAt(0);

            InsertNextElement(ref variantsToCheck, ref finishedVariants, false);
        }
        
    }
}
