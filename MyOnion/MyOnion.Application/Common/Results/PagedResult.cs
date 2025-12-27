using MyOnion.Application.Parameters;

namespace MyOnion.Application.Common.Results
{
    public class PagedResult<T> : Result<T>
    {
        private PagedResult(bool isSuccess, T value, string message, IReadOnlyCollection<string> errors, int pageNumber, int pageSize, RecordsCount recordsCount)
            : base(isSuccess, value, message, errors)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            RecordsFiltered = recordsCount?.RecordsFiltered ?? 0;
            RecordsTotal = recordsCount?.RecordsTotal ?? 0;
        }

        public int PageNumber { get; }

        public int PageSize { get; }

        public int RecordsFiltered { get; }

        public int RecordsTotal { get; }

        public static PagedResult<T> Success(T value, int pageNumber, int pageSize, RecordsCount recordsCount, string message = null)
        {
            return new PagedResult<T>(true, value, message, Array.Empty<string>(), pageNumber, pageSize, recordsCount);
        }

        public static PagedResult<T> Failure(string message, int pageNumber, int pageSize, RecordsCount recordsCount = null, IEnumerable<string> errors = null)
        {
            var errorList = BuildErrors(message, errors);
            return new PagedResult<T>(false, default!, message, errorList, pageNumber, pageSize, recordsCount);
        }
    }
}
