# BinPacker2D-in-order
This library is used for packing elements on the 2D plane for laser cutting. Size of the working area is 520x400 (width x height) and can be adjusted, along with margins for each side and spacing between elements. Algorithm is optimizing material usage along Y axis, so elements are packed from left to right. 

This isn't a full bin packing algorithm, because elements have to be placed in order - this is a crucial requirement for the production enviroment. It impacts material usage (leaving empty spots), but allows to sort final products much faster which would be otherwise randomly placed over one or many generated pages making it hard/nearly impossible to do without mistakes (elements are cut in foil, they are impossible to distinguish just by looking at them and in a single run we generate pages for about 40 distinct shapes - multiply that by 2 or more pieces of each shape and you get an idea why it's so important to pack them in order).

## REMARKS:
- **SAME ELEMENTS HAVE TO BE NEXT TO EACH OTHER** - it's necessary to speed up packing of created products after cutting them.
- Algorithm is done, when there is no more free space in the table, or we inserted all of the elements in all possible variants. Unused elements will come back for the next run, on the new page
- Some of the properties are used outside of this scope
- Along the way we store rows (***RowVariant***) of elements in a ***VariantGroup***. This way we can compare all of the outcomes when they are completed - comparing just a single row is much more memory efficient, but in real-life usage often leads to situation where we end up with 2 rows instead of one.




## Logic of inserting elements into rows:
- Before packing, orient all the elements, so that height is larger than width and sort elements by width descending.

1. Insert first element into starting position, if it can also fit oriented horizontally create second ***RowVariant*** with rotated copy of the element.
![1](https://user-images.githubusercontent.com/78303091/152651148-5335fec5-8c19-4864-b78b-3ab69da3d841.jpg)

- Now we have a ***main row*** for the ***RowVariant*** - it has a height of the first element and width of the Table.
![2](https://user-images.githubusercontent.com/78303091/152651149-e9631c6b-0f19-4ade-a5e3-913fcd578b37.jpg)

- Each time we create a rotated copy of an element we also create new ***RowVariant***.

2. We do the same for the next element.
![3](https://user-images.githubusercontent.com/78303091/152651150-52fd831d-fa0e-42cf-b4fd-29bceb61b140.jpg)

-  If it's height is smaller than previous element, then we create a ***split*** of this row => one row has height of the currently inserted element and we have second row on top of it for later use.
![4](https://user-images.githubusercontent.com/78303091/152651151-09cb700e-5b06-4709-8d98-dfe9d517c208.jpg)

3. We insert elements along ***main row***  X coordinate (while also saving row ***splits*** for later use + creating a copy of ***RowVariant*** with rotated element) until we run out of elements, or next element can't fit in this row.
![5](https://user-images.githubusercontent.com/78303091/152651144-c91348f9-953d-48c3-8cc5-00c1e7a00169.jpg)

4. When the ***main row*** is done (no more free space in it's X axis) and there are still unpacked elements, we go back to previous empty ***split row*** where next element can fit and treat it as our new ***row***.
![6](https://user-images.githubusercontent.com/78303091/152651146-df506e99-fb62-49d6-a9e9-4a0b0e0b06d8.jpg)

5. When finally there is no more free space in the ***main row*** (we checked all the inside rows/splits), we create a new ***main row*** on top of the first one. Repeat from step 1.
6. When we don't have free elements or free space in the whole Table, we can compare all of the outcomes (***VariantGroups***). Each of them holds it's own rows (***RowVariant***) and packed elements.
7. Select ***VariantGroup*** with most packed elements. If 2 VariantsGroups have same number of packed elements, the we select one with lower Y coordinate (less space used).








