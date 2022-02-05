using System.Collections.Generic;
using System.Linq;

namespace BinPacker2D
{
    /// <summary>
    /// Class holding rows (RowVariants). It is the final outcome of packing elements
    /// </summary>
    public class VariantGroup
    {
        /// <summary>
        /// Number of packed elements in this outcome
        /// </summary>
        public int PackedElementsCount
        {
            get
            {
                var tmpCount = 0;
                foreach (RowVariant row in GroupRows)
                {
                    tmpCount += row.packedElements.Count();
                }
                return tmpCount;
            }
        }

        /// <summary>
        /// Rows created in this outcome
        /// </summary>
        public List<RowVariant> GroupRows { get; set; }

        /// <summary>
        /// Elements to pack
        /// </summary>
        public List<RowElement> InputElements { get; set; }

        /// <summary>
        /// Available free space for this outcome
        /// </summary>
        public Position TablePosition { get; set; }

        /// <summary>
        /// Y coordinate of the highest element in this group - less is better
        /// </summary>
        public double UsedArea => GroupRows.Last().packedElements.First().posY + GroupRows.Last().packedElements.First().height; 

        /// <summary>
        /// Used when creating new VariantGroup
        /// </summary>
        /// <param name="rowVariant">First RowVariant in this group</param>
        /// <param name="tablePosition">Available space in this group</param>
        /// <param name="inputElements">All elements to pack, list will be cloned</param>
        public VariantGroup(RowVariant rowVariant, Position tablePosition, List<RowElement> inputElements) //PIERWSZY ELEMENT
        {
            GroupRows = new List<RowVariant> { rowVariant };
            TablePosition = new Position(tablePosition);
            this.InputElements = Extensions.GetClone(inputElements);
        }

        /// <summary>
        /// Create VariantGroup copy and add next RowVariant
        /// </summary>
        /// <param name="groupToCopy">Group to copy from</param>
        /// <param name="rowVariant">RowVariant to insert into this group</param>
        public VariantGroup(VariantGroup groupToCopy, RowVariant rowVariant)
        {
            GroupRows = new List<RowVariant>(groupToCopy.GroupRows) { rowVariant };
            TablePosition = new Position(groupToCopy.TablePosition);
            this.InputElements = Extensions.GetClone(groupToCopy.InputElements);
        }

        /// <summary>
        /// Get next element to pack
        /// </summary>
        /// <returns></returns>
        public RowElement GetNextElement() => GroupRows.Last().GetNextElement();
    }
}
