using System.Text.Json.Serialization;

namespace MyOnion.Application.Common.Results
{
    public class Result
    {
        protected Result(bool isSuccess, string message, IReadOnlyCollection<string> errors)
        {
            IsSuccess = isSuccess;
            Message = message;
            Errors = errors ?? Array.Empty<string>();
        }

        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        public string Message { get; }

        public IReadOnlyCollection<string> Errors { get; }

        public double? ExecutionTimeMs { get; private set; }

        public void SetExecutionTime(double? executionTimeMs)
        {
            ExecutionTimeMs = executionTimeMs;
        }

        public static Result Success(string message = null)
        {
            return new Result(true, message, Array.Empty<string>());
        }

        public static Result Failure(string message, IEnumerable<string> errors = null)
        {
            var errorList = BuildErrors(message, errors);
            return new Result(false, message, errorList);
        }

        public static Result<T> Success<T>(T value, string message = null)
        {
            return Result<T>.Success(value, message);
        }

        public static Result<T> Failure<T>(string message, IEnumerable<string> errors = null)
        {
            return Result<T>.Failure(message, errors);
        }

        protected static IReadOnlyCollection<string> BuildErrors(string message, IEnumerable<string> errors)
        {
            if (errors != null)
            {
                var materialized = errors.Where(e => !string.IsNullOrWhiteSpace(e)).ToArray();
                if (materialized.Length > 0)
                {
                    return materialized;
                }
            }

            return string.IsNullOrWhiteSpace(message)
                ? Array.Empty<string>()
                : new[] { message };
        }
    }

    public class Result<T> : Result
    {
        protected Result(bool isSuccess, T value, string message, IReadOnlyCollection<string> errors)
            : base(isSuccess, message, errors)
        {
            Value = value;
        }

        [JsonPropertyOrder(100)]
        public T Value { get; }

        public static Result<T> Success(T value, string message = null)
        {
            return new Result<T>(true, value, message, Array.Empty<string>());
        }

        public new static Result<T> Failure(string message, IEnumerable<string> errors = null)
        {
            var errorList = BuildErrors(message, errors);
            return new Result<T>(false, default!, message, errorList);
        }
    }
}
