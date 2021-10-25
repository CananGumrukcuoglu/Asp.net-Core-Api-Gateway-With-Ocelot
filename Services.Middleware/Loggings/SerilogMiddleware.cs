using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Serilog.Events;
using Services.Common.Extensions;
using Services.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Services.Middleware.Loggings
{
    public class SerilogMiddleware
    {
        private const string MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms. RequestId: {RequestId} Request Body {Request} and Response {Response}. Client Ip Addres is : {IpAddress} Host machine is :{HostMachine}";

        private static readonly Serilog.ILogger _logger = Serilog.Log.ForContext<SerilogMiddleware>();

        private static readonly HashSet<string> HeaderWhitelist = new HashSet<string>
            {"CONTENT-TYPE", "CONTENT-LENGTH", "USER-AGENT", "AUTHORIZATION", "CULTURE"};

        private readonly RequestDelegate _next;

        public SerilogMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            var start = Stopwatch.GetTimestamp();
            string response = string.Empty;
            string request = string.Empty;
            string machineName = Environment.MachineName;
            try
            {
                var requestId = Guid.NewGuid().ToString();
                request = await FormatRequest(httpContext.Request);
                var originalBodyStream = httpContext.Response.Body;
                using (var responseBody = new MemoryStream())
                {
                    httpContext.Response.Body = responseBody;
                    await _next(httpContext);
                    response = await FormatResponse(httpContext.Response);
                    await responseBody.CopyToAsync(originalBodyStream);
                }

                var elapsedMs = TimeMeasurement.GetElapsedMilliseconds(start, Stopwatch.GetTimestamp());

                var statusCode = httpContext.Response?.StatusCode;
                LogEventLevel level = GetLogeventLevel(statusCode);

                var log = level == LogEventLevel.Information ? _logger : LogForErrorContext(httpContext);
                log.Write(level, MessageTemplate, httpContext.Request.Method, GetPath(httpContext), statusCode,
                    elapsedMs, requestId, request, response, httpContext.GetRemoteIPAddress(true).ToString(),
                    machineName);
            }
            catch (Exception ex) when (LogException(httpContext,
                TimeMeasurement.GetElapsedMilliseconds(start, Stopwatch.GetTimestamp()), ex, request, response,
                httpContext.GetRemoteIPAddress(true).ToString(), machineName))
            {
            }
        }

        private static LogEventLevel GetLogeventLevel(int? statusCode)
        {
            if (statusCode == 200)
                return LogEventLevel.Information;
            else if (statusCode > 200 && statusCode < 500)
                return LogEventLevel.Warning;
            else
                return LogEventLevel.Error;
        }

        private bool LogException(HttpContext httpContext, double elapsedMs, Exception ex, string request,
            string response, string ipAdd, string machineName)
        {
            var requestId = Guid.NewGuid().ToString();
            var log = LogForErrorContext(httpContext);
            if (request.Length + response.Length > 1200)
            {
                log.Error(ex, MessageTemplate, httpContext.Request.Method, GetPath(httpContext), 500, elapsedMs,
                    requestId, string.Empty, response, ipAdd, machineName);
                log.Error(ex, MessageTemplate, httpContext.Request.Method, GetPath(httpContext), 500, elapsedMs,
                    requestId, request, string.Empty, ipAdd, machineName);
            }
            else
            {
                log.Error(ex, MessageTemplate, httpContext.Request.Method, GetPath(httpContext), 500, elapsedMs,
                    requestId, request, response, ipAdd, machineName);
            }

            return false;
        }

        private static IFormCollection GetFormData(HttpContext httpContext)
        {
            return httpContext.Request.HasFormContentType ? httpContext.Request.Form : null;
        }

        private static Serilog.ILogger LogForErrorContext(HttpContext httpContext)
        {
            var request = httpContext.Request;

            var loggedHeaders = request.Headers
                .Where(h => HeaderWhitelist.Contains(h.Key.ToUpperInvariant()))
                .ToDictionary(h => h.Key, h => h.Value.ToString());

            var result = _logger
                .ForContext("RequestHeaders", loggedHeaders, destructureObjects: true)
                .ForContext("RequestHost", request.Host)
                .ForContext("RequestProtocol", request.Protocol);

            return result;
        }

        private static string GetPath(HttpContext httpContext)
        {
            return httpContext.Features.Get<IHttpRequestFeature>()?.RawTarget ?? httpContext.Request.Path.ToString();
        }

        private static async Task<string> FormatRequest(HttpRequest request)
        {
            request.EnableBuffering();

            var requestBodyStream = new MemoryStream();

            await request.Body.CopyToAsync(requestBodyStream);
            requestBodyStream.Seek(0, SeekOrigin.Begin);


            var requestBodyText = new StreamReader(requestBodyStream).ReadToEnd();

            string pattern = @"\""dspassword\"": [""]{0,1}[0-9]{6}[""]{0,1}";
            RegexOptions options = RegexOptions.Multiline | RegexOptions.IgnoreCase;
            requestBodyText = Regex.Replace(requestBodyText, pattern, "\"dsPassword\": ******", options);

            try
            {
                string[] splitPatterns = new string[]
                {
                    "application/octet-stream", "application/pdf", "image/png", "image/jpeg", "video/mp4",
                    "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                };
                foreach (var splitPattern in splitPatterns)
                {
                    var splitted = Regex.Split(requestBodyText, splitPattern);
                    requestBodyText = splitted.First();
                }
            }
            catch
            {
            }

            requestBodyStream.Seek(0, SeekOrigin.Begin);
            request.Body = requestBodyStream;

            return $"{request.Scheme} {request.Host}{request.Path} {request.QueryString} {requestBodyText}";
        }

        private static async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);

            string text = await new StreamReader(response.Body).ReadToEndAsync();
            try

            {
                string[] splitPatterns = new string[] { "PK\u0003", "\u0001" };
                foreach (var pattern in splitPatterns)
                {
                    var splitted = Regex.Split(text, pattern);
                    if (splitted.Count() > 1)
                        text = splitted.First() + "file-content-result";
                }
                if (text.Length > 1000)
                {
                    text = text.Substring(0, 1000);
                }
            }
            catch
            {
            }

            response.Body.Seek(0, SeekOrigin.Begin);
            return $"{response.StatusCode}: {text}";
        }
    }
}
