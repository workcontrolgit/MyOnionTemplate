namespace MyOnion.Application.Common.Results
{
    public class PagedDataTableResult<T> : Result<T>
    {
        private PagedDataTableResult(bool isSuccess, T value, string message, IReadOnlyCollection<string> errors, int draw, RecordsCount recordsCount)
            : base(isSuccess, value, message, errors)
        {
            Draw = draw;
            RecordsFiltered = recordsCount?.RecordsFiltered ?? 0;
            RecordsTotal = recordsCount?.RecordsTotal ?? 0;
        }

        public int Draw { get; }

        public int RecordsFiltered { get; }

        public int RecordsTotal { get; }

        public static PagedDataTableResult<T> Success(T value, int draw, RecordsCount recordsCount, string message = null)
        {
            return new PagedDataTableResult<T>(true, value, message, Array.Empty<string>(), draw, recordsCount);
        }

        public static PagedDataTableResult<T> Failure(string message, int draw, RecordsCount recordsCount = null, IEnumerable<string> errors = null)
        {
            var errorList = BuildErrors(message, errors);
            return new PagedDataTableResult<T>(false, default!, message, errorList, draw, recordsCount);
        }
    }
}
