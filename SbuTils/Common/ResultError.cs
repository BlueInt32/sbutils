namespace SbuTils.Common
{
    public class ResultError<TErrorCodeEnum>
    {
        /// <summary>
        /// Error describing succintely the error, e.g. CERTIFICATION_NOT_FOUND
        /// </summary>
        public TErrorCodeEnum? Code { get; set; }

        /// <summary>
        /// More human readable details about the error
        /// </summary>
        public string? Message { get; set; }

        public object? Data { get; set; }
    }
}
