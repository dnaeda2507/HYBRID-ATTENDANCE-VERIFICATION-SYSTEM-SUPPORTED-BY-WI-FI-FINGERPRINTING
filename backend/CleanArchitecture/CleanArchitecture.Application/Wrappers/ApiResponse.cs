using System;
using System.Collections.Generic;

namespace CleanArchitecture.Core.Wrappers
{
    public class ApiResponse<T>
    {
        /// <summary>
        /// True if the request succeeded; false if it failed.
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        /// The data returned on success. Will be default(T) if Success==false.
        /// </summary>
        public T Data { get; private set; }

        /// <summary>
        /// A user‐friendly message, e.g. “Course created” on success or an error description.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// A collection of error codes or validation messages. Empty on Success.
        /// </summary>
        public IEnumerable<string> Errors { get; private set; }

        private ApiResponse() { }

        /// <summary>
        /// Creates a successful response.
        /// </summary>
        public static ApiResponse<T> Ok(T data, string message = null) =>
            new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message,
                Errors = Array.Empty<string>()
            };

        /// <summary>
        /// Creates a failed response.
        /// </summary>
        public static ApiResponse<T> Fail(IEnumerable<string> errors, string message = null) =>
            new ApiResponse<T>
            {
                Success = false,
                Data = default,
                Message = message,
                Errors = errors ?? Array.Empty<string>()
            };

        /// <summary>
        /// Convenience overload for a single error.
        /// </summary>
        public static ApiResponse<T> Fail(string error, string message = null) =>
            Fail(new[] { error }, message);
    }

    /// <summary>
    /// Non‐generic version for endpoints that don't return a data object.
    /// </summary>
    public class ApiResponse
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }
        public IEnumerable<string> Errors { get; private set; }

        private ApiResponse() { }

        public static ApiResponse Ok(string message = null) =>
            new ApiResponse
            {
                Success = true,
                Message = message,
                Errors = Array.Empty<string>()
            };

        public static ApiResponse Fail(IEnumerable<string> errors, string message = null) =>
            new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors ?? Array.Empty<string>()
            };

        public static ApiResponse Fail(string error, string message = null) =>
            Fail(new[] { error }, message);
    }
}
