namespace MyOnion.Application.Specifications
{
    public abstract class BaseSpecification<T> : ISpecification<T>
    {
        protected BaseSpecification(Expression<Func<T, bool>> criteria = null)
        {
            Criteria = criteria;
        }

        public Expression<Func<T, bool>> Criteria { get; }

        public List<Expression<Func<T, object>>> Includes { get; } = [];

        public List<string> IncludeStrings { get; } = [];

        public int? Take { get; private set; }

        public int? Skip { get; private set; }

        public bool IsPagingEnabled { get; private set; }

        public string OrderBy { get; private set; }

        protected void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }

        protected void AddInclude(string includeString)
        {
            IncludeStrings.Add(includeString);
        }

        protected void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }

        protected void ApplyOrderBy(string orderBy)
        {
            OrderBy = orderBy;
        }
    }
}
