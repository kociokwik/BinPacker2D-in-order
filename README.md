# BinPacker2D-in-order

REMARKS:
- SAME ELEMENTS HAVE TO BE NEXT TO EACH OTHER - it's necessary to speed up packing of created products after cutting them.
- Algorithm is done, when there is no more space in the table, or we inserted all of the elements in all possible variants. Unused elements will come back for the next run, on the new page
- Some of the properties are used outside of this scope




Logic of inserting elements into rows:
1. Insert first element into starting position, if it can also fit oriented horizontally create second RowVariant with rotated copy of the element.
![1](https://user-images.githubusercontent.com/78303091/152651148-5335fec5-8c19-4864-b78b-3ab69da3d841.jpg)

- Now we have a MAIN ROW for the RowVariant - it has a height of the first element and width of the Table.
![2](https://user-images.githubusercontent.com/78303091/152651149-e9631c6b-0f19-4ade-a5e3-913fcd578b37.jpg)

- Each time we create a rotated copy of an element we also create new Variant.

2. We do the same for the next element.
![3](https://user-images.githubusercontent.com/78303091/152651150-52fd831d-fa0e-42cf-b4fd-29bceb61b140.jpg)

-  If it's height is smaller than previous element, then we create a SPLIT of this row => one row has height of the currently inserted element and we have second row on top of it for later use.
![4](https://user-images.githubusercontent.com/78303091/152651151-09cb700e-5b06-4709-8d98-dfe9d517c208.jpg)

3. We insert elements along MAIN ROW Y coordinate (while also creating Variants with rotated element and SPLITS to use later) until we run out of elements, or next element can't fit in this row.
![5](https://user-images.githubusercontent.com/78303091/152651144-c91348f9-953d-48c3-8cc5-00c1e7a00169.jpg)

4. When the MAIN ROW is done (no more free space in it's Y axis) and there are still unpacked elements, we go back to previous empty SPLIT where next element can fit and treat it as our new ROW.
![6](https://user-images.githubusercontent.com/78303091/152651146-df506e99-fb62-49d6-a9e9-4a0b0e0b06d8.jpg)

5. When finally there is no more free space in the MAIN ROW (we checked all the inside rows/splits), we create a new MAIN ROW on top of the first one. Repeat from step 1.
6. When we don't have free elements or free space in the whole TABLE we can compare all of the outcomes (VariantGroups). Each of them holds it's own RowVariants and packed elements.
7. Select VairantGroup with most packed elements. If 2 VariantsGroups have same number of elements, the we select one with lower Y coordinate (less space used).








