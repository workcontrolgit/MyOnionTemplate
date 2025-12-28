namespace MyOnion.WebApi.Options
{
    public class ExecutionTimingOptions
    {
        public const string SectionName = "ExecutionTiming";

        public bool Enabled { get; set; } = true;

        public bool IncludeHeader { get; set; } = true;

        public bool IncludeResultPayload { get; set; } = true;

        public bool LogTimings { get; set; }

        public string HeaderName { get; set; } = "x-execution-time-ms";
    }
}
