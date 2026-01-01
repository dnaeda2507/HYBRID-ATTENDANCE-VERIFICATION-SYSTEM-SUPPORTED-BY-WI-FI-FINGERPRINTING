using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Wrappers;
using Microsoft.AspNetCore.Http;

namespace CleanArchitecture.WebApi.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                ApiResponse result;

                switch (error)
                {
                    case ApiException e:
                        // custom application error
                        response.StatusCode = (int)HttpStatusCode.OK;
                        result = ApiResponse.Fail(e.Message);
                        break;

                    case ValidationException e:
                        // validation errors
                        response.StatusCode = (int)HttpStatusCode.OK;
                        result = ApiResponse.Fail(e.Errors, "Some validation errors occurred.");
                        break;

                    case KeyNotFoundException _:
                        // not found error
                        response.StatusCode = (int)HttpStatusCode.OK;
                        result = ApiResponse.Fail("Resource not found.");
                        break;

                    case ArgumentException e:
                        // bad argument
                        response.StatusCode = (int)HttpStatusCode.OK;
                        result = ApiResponse.Fail(e.Message);
                        break;

                    case UnauthorizedAccessException e:
                        // unauthorized
                        response.StatusCode = (int)HttpStatusCode.OK;
                        result = ApiResponse.Fail(e.Message);
                        break;

                    case InvalidOperationException e:
                        // unhandled invalid op
                        response.StatusCode = (int)HttpStatusCode.OK;
                        result = ApiResponse.Fail(e.Message);
                        break;

                    default:
                        // unhandled error
                        response.StatusCode = (int)HttpStatusCode.OK;
                        result = ApiResponse.Fail("An unexpected error occurred.");
                        break;
                }

                var payload = JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });

                await response.WriteAsync(payload);
            }
        }
    }
}
