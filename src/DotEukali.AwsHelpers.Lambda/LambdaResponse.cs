using System.Net;
using System.Text;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;

namespace DotEukali.AwsHelpers.Lambda
{
    public class LambdaResponse : APIGatewayProxyResponse
    {
        private static string _allowedHeaders = "*";
        private static string _allowedMethods = "DELETE,GET,POST,PUT,OPTIONS";
        private static string _allowedOrigin = "*";

        private static Func<object?, string?> _responseObjectTransformer = responseBody =>
            responseBody == null ? null
                : JsonSerializer.Serialize(responseBody);

        private static Func<string?, string?> _responseStringTransformer = responseBody =>
            responseBody == null ? null
                : Convert.ToBase64String(Encoding.ASCII.GetBytes(responseBody));

        public LambdaResponse(HttpStatusCode statusCode, string? body = null)
        {
            StatusCode = (int)statusCode;

            if (body != null)
            {
                Body = _responseStringTransformer.Invoke(body);
                IsBase64Encoded = IsBase64String(Body);
            }

            Headers = new Dictionary<string, string>()
            {
                { "Access-Control-Allow-Headers", _allowedHeaders },
                { "Access-Control-Allow-Methods", _allowedMethods },
                { "Access-Control-Allow-Origin", _allowedOrigin }
            };
        }

        public static LambdaResponse InternalServerError(object? body = null) => GetResponse(HttpStatusCode.InternalServerError, body);
        public static LambdaResponse Unauthorized(object? body = null) => GetResponse(HttpStatusCode.Unauthorized, body);
        public static LambdaResponse NotFound(object? body = null) => GetResponse(HttpStatusCode.NotFound, body);
        public static LambdaResponse BadRequest(object? body = null) => GetResponse(HttpStatusCode.BadRequest, body);
        public static LambdaResponse Ok(object? body = null) => GetResponse(HttpStatusCode.OK, body);
        public static LambdaResponse Accepted(object? body = null) => GetResponse(HttpStatusCode.Accepted, body);

        public static LambdaResponse GetResponse(HttpStatusCode statusCode, object? body = null)
        {
            if (body is string stringValue)
            {
                return new LambdaResponse(statusCode, stringValue);
            }

            return new LambdaResponse(statusCode, _responseObjectTransformer.Invoke(body));
        }

        public static void SetAllowedHeaders(string allowedHeaders) => _allowedHeaders = allowedHeaders;
        public static void SetAllowedMethods(string allowedMethods) => _allowedMethods = allowedMethods;
        public static void SetAllowedOrigin(string allowedOrigin) => _allowedOrigin = allowedOrigin;
        public static void SetResponseObjectTransformer(Func<object?, string?> objectTransformer) => _responseObjectTransformer = objectTransformer;
        public static void SetResponseStringTransformer(Func<string?, string?> stringTransformer) => _responseStringTransformer = stringTransformer;

        public static void AddAllowedHeaders(string allowedHeaders) => _allowedHeaders += $",{allowedHeaders.Trim(',')}".Trim(',');
        public static void AddAllowedMethods(string allowedMethods) => _allowedMethods += $",{allowedMethods.Trim(',')}".Trim(',');

        private static bool IsBase64String(string? base64)
        {
            if (base64 == null)
                return false;

            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out _);
        }
    }
}
