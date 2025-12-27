namespace MyOnion.Application.Helpers
{
    public class DataShapeHelper<T> : IDataShapeHelper<T>
    {
        private const string AllFieldsKey = "*";

        private static readonly PropertyInfo[] Properties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance);

        private static readonly IReadOnlyList<PropertyAccessor> AllAccessors = Properties
            .Select(CreateAccessor)
            .ToArray();

        private static readonly IReadOnlyDictionary<string, PropertyAccessor> AccessorLookup =
            AllAccessors.ToDictionary(a => a.Name.ToLowerInvariant());

        private static readonly ConcurrentDictionary<string, IReadOnlyList<PropertyAccessor>> AccessorCache = new();

        public IEnumerable<Entity> ShapeData(IEnumerable<T> entities, string fieldsString)
        {
            var accessors = ResolveAccessors(fieldsString);
            return FetchData(entities, accessors);
        }

        public Task<IEnumerable<Entity>> ShapeDataAsync(IEnumerable<T> entities, string fieldsString)
        {
            return Task.FromResult(ShapeData(entities, fieldsString));
        }

        public Entity ShapeData(T entity, string fieldsString)
        {
            var accessors = ResolveAccessors(fieldsString);
            return FetchDataForEntity(entity, accessors);
        }

        private static IReadOnlyList<PropertyAccessor> ResolveAccessors(string fieldsString)
        {
            var normalizedKey = NormalizeFields(fieldsString);
            if (normalizedKey == AllFieldsKey)
            {
                return AllAccessors;
            }

            return AccessorCache.GetOrAdd(normalizedKey, BuildAccessorList);
        }

        private static IReadOnlyList<PropertyAccessor> BuildAccessorList(string normalizedKey)
        {
            var tokens = normalizedKey.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var accessors = new List<PropertyAccessor>(tokens.Length);

            foreach (var token in tokens)
            {
                if (AccessorLookup.TryGetValue(token, out var accessor))
                {
                    accessors.Add(accessor);
                }
            }

            return accessors;
        }

        private static string NormalizeFields(string fieldsString)
        {
            if (string.IsNullOrWhiteSpace(fieldsString))
            {
                return AllFieldsKey;
            }

            var fields = fieldsString
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(f => f.Trim())
                .Where(f => !string.IsNullOrEmpty(f))
                .Select(f => f.ToLowerInvariant())
                .ToArray();

            return fields.Length == 0 ? AllFieldsKey : string.Join(",", fields);
        }

        private static IEnumerable<Entity> FetchData(IEnumerable<T> entities, IReadOnlyList<PropertyAccessor> accessors)
        {
            var shapedData = new List<Entity>();

            foreach (var entity in entities)
            {
                var shapedObject = FetchDataForEntity(entity, accessors);
                shapedData.Add(shapedObject);
            }

            return shapedData;
        }

        private static Entity FetchDataForEntity(T entity, IReadOnlyList<PropertyAccessor> accessors)
        {
            var shapedObject = new Entity();

            foreach (var accessor in accessors)
            {
                var value = accessor.Getter(entity);
                shapedObject.TryAdd(accessor.Name, value);
            }

            return shapedObject;
        }

        private static PropertyAccessor CreateAccessor(PropertyInfo property)
        {
            var parameter = Expression.Parameter(typeof(T), "entity");
            var propertyExpression = Expression.Property(parameter, property);
            var convertExpression = Expression.Convert(propertyExpression, typeof(object));
            var getter = Expression.Lambda<Func<T, object>>(convertExpression, parameter).Compile();

            return new PropertyAccessor(property.Name, getter);
        }

        private sealed record PropertyAccessor(string Name, Func<T, object> Getter);
    }
}
