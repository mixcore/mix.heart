Get Primary Keys:

```csharp
var keyName = context.Model.FindEntityType(typeof(TModel)).FindPrimaryKey().Properties
.Select(x => x.Name).ToList();
var entityType = context.Model.GetEntityTypes(typeof(TModel));
var key = entityType.key();
```