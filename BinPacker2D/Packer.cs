using System;
using System.Collections.Generic;
using System.Linq;

namespace BinPacker2D
{
    public class Packer
    {
        /*
         * REMARKS:
         * - SAME ELEMENTS HAVE TO BE NEXT TO EACH OTHER - it's necessary to speed up packing of created products after cutting them.
         * - Algorithm is done, when there is no more free space in the table, or we inserted all of the elements in all possible variants. Unused elements will come back for the next run, on the new page
         * - Some of the properties are used outside of this scope
         * - Along the way we store rows (RowVariant) of elements in a VariantGroup. This way we can compare all of the outcomes when they are completed - comparing just a single row is much more memory efficient, but in real-life usage often leads to situation where we end up with 2 rows instead of one.
         */

        /*
         * 1. Insert first element into starting position, if it can also fit oriented horizontally create second RowVariant with rotated copy of the element.
         * - Now we have a MAIN ROW for the RowVariant - it has a height of the first element and width of the Table.
         * - Each time we create a rotated copy of an element we also create new Variant.
         * 2. We do the same for the next element.
         * - If it's height is smaller than previous element, then we create a SPLIT of this row => one row has height of the currently inserted element and we have second row on top of it for later use.
         * 3. We insert elements along MAIN ROW Y coordinate (while also saving row SPLITS for later use + creating copied Variants with rotated element) until we run out of elements, or next element can't fit in this row.
         * 4. When the MAIN ROW is done (no more free space in it's Y axis) and there are still unpacked elements, we go back to previous empty SPLIT row where next element can fit and treat it as our new ROW.
         * 5. When finally there is no more free space in the MAIN ROW (we checked all the inside rows/splits), we create a new MAIN ROW on top of the first one. Repeat from step 1.
         * 6. When we don't have free elements or free space in the whole TABLE we can compare all of the outcomes (VariantGroups). Each of them holds it's own rows (RowVariant) and packed elements.
         * 7. Select VairantGroup with most packed elements. If 2 VariantsGroups have same number of elements, the we select one with lower Y coordinate (less space used).
         */

        /// <summary>
        /// List holding row variants for inserting elements
        /// </summary>
        private List<RowVariant> _variantsToCheck;

        /// <summary>
        /// List holding finished row variants - row is finished if there are no more elements in a sequence, or when next element can't fit inside this (MAIN) row
        /// </summary>
        private List<RowVariant> _completedRows;

        private List<VariantGroup> _variantGroups;

        private List<VariantGroup> _finishedVariantGroups;

        /// <summary>
        /// Packs elements on 2D plane (Table).
        /// </summary>
        /// <param name="tableHeight">Height of the packing space</param>
        /// <param name="tableWidth">Width of the packing space</param>
        /// <param name="elementsSpacing">Space between elements</param>
        /// <param name="leftTableMargin">Left margin of the Table</param>
        /// <param name="rightTableMargin">Right margin of the Table</param>
        /// <param name="topTableMargin">Upper margin of the Table</param>
        /// <param name="bottomTableMargin">Bottom margin of the Table</param>
        /// <param name="inputElements">Elements to pack</param>
        /// <returns>Packed elements with their coordinates on the Table. Same elements have to be placed next to each other</returns>
        public List<RowElement> PackElements(double tableHeight,
                                            double tableWidth,
                                            double elementsSpacing,
                                            double leftTableMargin,
                                            double rightTableMargin,
                                            double topTableMargin,
                                            double bottomTableMargin,
                                            List<RowElement> inputElements)
        {
            //
            // Orient all the elements, so that height is larger than width
            foreach (var e in inputElements)
            {
                if (e.height < e.width)
                {
                    (e.width, e.height) = (e.height, e.width);
                    e.isRotated = true;
                }
            }

            //
            //Sort elements by width
            inputElements = inputElements.OrderByDescending(x => x.width).ToList();

            _variantsToCheck = new List<RowVariant>();
            _completedRows = new List<RowVariant>();
            _variantGroups = new List<VariantGroup>();
            _finishedVariantGroups = new List<VariantGroup>();

            var sortIndexCounter = 0;
            foreach (var element in inputElements)
            {
                element.sortIndex = sortIndexCounter++;
            }

            

            Position tablePosition;

            bool deleteFirstGroup = false;
            double eH;
            double eW;


            double bestUnusedArea = double.MaxValue;
            int bestPackedCount = 0;
            int bestPackedIndex = -1;
            int cnt = 0;



            //
            // tablePosition coordinates adjusted to margins and spacing
            double posX, posY;
            double posW, posH;
            double tabW, tabH;

            //
            // Set table length and width
            posH = tableHeight;
            posW = tableWidth;

            //
            // Set first position coordinates to default
            posX = 0;
            posY = 0;

            //
            // Correct first position coordinates
            // according to selected margin
            //
            posX += leftTableMargin;
            posY += bottomTableMargin;

            //
            // Correct table width and height
            // according to selected margin
            //
            posW -= (leftTableMargin + rightTableMargin);
            posH -= (topTableMargin + bottomTableMargin);

            do
            {
                //
                // Clear temporary lists
                _variantsToCheck = new List<RowVariant>();
                _completedRows = new List<RowVariant>();

                var freeSpace = true;
                
                #region Insert first element into the new MAIN ROW

                if (_variantGroups.Count == 0) //Initialize new Table and insert first element from the sequence
                {
                    eH = inputElements.First().height; 
                    eW = inputElements.First().width;

                    tablePosition = new Position(posH, posW, posX, posY);

                    //
                    //Check if element can fit in both orientations, if so, create 2 variants
                    if ((eH <= posH && eW <= posW) && (eW <= posH && eH <= posW))
                    {
                        var variant = new RowVariant(tablePosition, elementsSpacing, inputElements, false);
                        _variantsToCheck.Add(variant);

                        //
                        //If element is not a square, create second variant
                        if (!inputElements.First().IsSquare)
                        {
                            var variantRotated = new RowVariant(tablePosition, elementsSpacing, inputElements, true);
                            _variantsToCheck.Add(variantRotated);
                        }
                    }
                    else if (eH <= posH && eW <= posW)
                    {
                        var variant = new RowVariant(tablePosition, elementsSpacing, inputElements, false);
                        _variantsToCheck.Add(variant);
                    }
                    else if (eW <= posH && eH <= posW)
                    {
                        var variantRotated = new RowVariant(tablePosition, elementsSpacing, inputElements, true);
                        _variantsToCheck.Add(variantRotated);
                    }
                    else
                    {
                        //First element can't fit inside the Table, - this is impossible in normal usage of this Class
                        throw new ArgumentOutOfRangeException(nameof(inputElements),
                            "First element doesn't fit inside the Table, all elements must be able to fit");
                    }


                }
                else //Create a new row and insert it's first element
                {
                    //
                    // No more elements, this RowVariant is finished
                    if (_variantGroups.First().GetNextElement() == null)
                    {
                        _finishedVariantGroups.Add(_variantGroups.First());
                        _variantGroups.RemoveAt(0);
                        continue;
                    }

                    inputElements = _variantGroups.First().InputElements;

                    //
                    // Adjust free Table space for this VariantGroup
                    _variantGroups.First().TablePosition.height -= (_variantGroups.First().GroupRows.Last().packedElements.First().height + elementsSpacing);
                    _variantGroups.First().TablePosition.posY += (_variantGroups.First().GroupRows.Last().packedElements.First().height + elementsSpacing);

                    eH = _variantGroups.First().GetNextElement().height;
                    eW = _variantGroups.First().GetNextElement().width;
                    tabH = _variantGroups.First().TablePosition.height;
                    tabW = _variantGroups.First().TablePosition.width;
                    posX = _variantGroups.First().TablePosition.posX;
                    posY = _variantGroups.First().TablePosition.posY;

                    tablePosition = new Position(tabH, tabW, posX, posY);

                    //
                    //Check if element can fit in both orientations, if so, create 2 variants
                    if ((eH <= tabH && eW <= tabW) && (eW <= tabH && eH <= tabW))
                    {
                        var variant = new RowVariant(tablePosition, elementsSpacing, inputElements, false);
                        _variantsToCheck.Add(variant);

                        //
                        //If element is a square, don't create new variant
                        if (!inputElements.First().IsSquare)
                        {
                            var variantRotated = new RowVariant(tablePosition, elementsSpacing, inputElements, true);
                            _variantsToCheck.Add(variantRotated);
                        }
                    }
                    else if (eH <= tabH && eW <= tabW)
                    {
                        var variant = new RowVariant(tablePosition, elementsSpacing, inputElements, false);
                        _variantsToCheck.Add(variant);
                    }
                    else if (eW <= tabH && eH <= tabW)
                    {
                        var variantRotated = new RowVariant(tablePosition, elementsSpacing, inputElements, true);
                        _variantsToCheck.Add(variantRotated);
                    }
                    else // End of space for this VariantGroup
                    {
                        freeSpace = false;
                        _finishedVariantGroups.Add(_variantGroups.First());
                        _variantGroups.RemoveAt(0);
                    }
                }

                #endregion

                //
                // If there is no more space in this RowVariant, continue to next
                if (!freeSpace) continue;

                //
                // Inserting rest of the elements into current MAIN ROW
                while (_variantsToCheck.Count > 0)
                {
                    _variantsToCheck.First().InsertElement(ref _variantsToCheck, ref _completedRows);
                }

                //
                // End of inserting elements, adjusting sorting index
                // Insert created row variants into a copy of the first group. Original group will be deleted
                // We do this, so we can later compare outcomes based on their total used space  
                if (deleteFirstGroup)
                {
                    foreach (RowVariant row in _completedRows)
                    {
                        _variantGroups.Add(new VariantGroup(_variantGroups.First(), row));

                        foreach (RowElement item in _variantGroups.Last().GroupRows.Last().packedElements)
                        {
                            var indexRemove = _variantGroups.Last().InputElements.FindIndex(x => x.index == item.index);
                            if (indexRemove != -1) _variantGroups.Last().InputElements.RemoveAt(indexRemove);
                        }

                        var newSortInd = 0;
                        foreach (var inputE in _variantGroups.Last().InputElements)
                        {
                            inputE.sortIndex = newSortInd++;
                        }
                    }
                }
                else
                {
                    foreach (RowVariant row in _completedRows)
                    {
                        _variantGroups.Add(new VariantGroup(row, tablePosition, inputElements));

                        foreach (RowElement item in _variantGroups.Last().GroupRows.Last().packedElements)
                        {
                            var indexRemove = _variantGroups.Last().InputElements.FindIndex(x => x.index == item.index);
                            if (indexRemove != -1) _variantGroups.Last().InputElements.RemoveAt(indexRemove);
                        }

                        var newSortInd = 0;
                        foreach (var inputE in _variantGroups.Last().InputElements)
                        {
                            inputE.sortIndex = newSortInd++;
                        }
                    }
                }

                if (deleteFirstGroup) _variantGroups.RemoveAt(0);
                else deleteFirstGroup = true;

            } while (_variantGroups.Count > 0);
            

            cnt = 0;

            //
            // Select the best variant
            // Best variant is the one with the most packed elements
            foreach (VariantGroup finalGroup in _finishedVariantGroups)
            {
                if (cnt == 0) bestUnusedArea = finalGroup.UsedArea;

                if (finalGroup.PackedElementsCount >= bestPackedCount)
                {
                    if (finalGroup.PackedElementsCount > bestPackedCount)
                    {
                        bestPackedCount = finalGroup.PackedElementsCount;
                        bestPackedIndex = cnt;
                    }
                    else if (finalGroup.UsedArea < bestUnusedArea) // If two outcomes have same packed elements count, select one with less space used
                    {
                        bestUnusedArea = finalGroup.UsedArea;
                        bestPackedCount = finalGroup.PackedElementsCount;
                        bestPackedIndex = cnt;
                    }

                }
                cnt++;
            }



            List<RowElement> returnElements = new List<RowElement>();
            foreach (RowVariant row in _finishedVariantGroups[bestPackedIndex].GroupRows)
            {
                foreach (var e in row.packedElements)
                {
                    returnElements.Add(e);
                    Console.WriteLine($"{$"{e.sortIndex}",-3} {$"X: {e.posX}",-12} {$"Y: {e.posY}",-12} {$"W: {e.width}",-12} {$"H: {e.height}",-12}");
                }
            }

            return returnElements;
        }
    }
}
