using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace cw3.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private StreamWriter append;
        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
            append = File.AppendText("requestsLog.txt");
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            httpContext.Request.EnableBuffering();
            var request = httpContext.Request;
            append.WriteLine("=====================================================");
            append.WriteLine(request.Method);
            append.WriteLine(request.Path);
            using (Stream receiveStream = request.Body)
            {
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8, true, 1024, true))
                {
                    append.WriteLine(await readStream.ReadToEndAsync());
                }
                httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
            }
            foreach (var keyValuePair in request.Query)
            {
                append.WriteLine(keyValuePair);
            }
            append.WriteLine("=====================================================");
            append.Flush();
            
            await _next(httpContext); }
    }
}