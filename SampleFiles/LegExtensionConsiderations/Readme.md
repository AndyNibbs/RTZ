In this folder a selection of files testing around RTZ 1.2 schema which specifically ADDS the option to include an extensions element within a leg element. 

**10SimpleLegExtensions.rtz**
**11SimpleLegExtension.rtz**

These are RTZ 1.0 and 1.1 files which demonstrated that having a leg extension fails against the schema.

**12SimpleLegExtension.rtz**

This was used to test that the 1.2 schema change did allow leg extensions. 

**12AndOldSTMSchema.rtz**

This shows that coupling 1.2 RTZ with STM extensions and the existing STM schema results in failure. 

**12AndUpdatedUnofficalSTMSchema.rtz**

This shows that a new schema stm_extensions_02042020_unofficial.xsd which refers to RTZ 1.2 schema "fixes" the error. 