using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CapitalSix.AspNetCore.Swagger.AutoResponse
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ValidateReferrerAttribute : ActionFilterAttribute
    {
        private IConfiguration? _configuration;

        /// <summary>
        ///     Called when /[action executing].
        /// </summary>
        /// <param name="context">The action context.</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            base.OnActionExecuting(context);
            if (IsValidRequest(context.HttpContext.Request)) return;
            context.Result = new ContentResult
            {
                Content = "Invalid referer header"
            };
            context.HttpContext.Response.StatusCode = (int) HttpStatusCode.ExpectationFailed;
        }

        /// <summary>
        ///     Determines whether /[is valid request] [the specified request].
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        ///     <c>true</c> if [is valid request] [the specified request]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsValidRequest(HttpRequest request)
        {
            var referrerUrl = "";
            if (request.Headers.ContainsKey("Referer")) referrerUrl = request.Headers["Referer"];
            if (string.IsNullOrWhiteSpace(referrerUrl)) return false;

            // get allowed client list to check    
            var allowedUrls = _configuration?.GetSection("CorsOrigins")
                .Get<string[]>()
                ?.Select(url => new Uri(url).Authority)
                .ToList();
            
            //add current host for swagger calls    
            var host = request.Host.Value;

            if (allowedUrls == null) return false;

            allowedUrls.Add(host);
            var isValidClient = allowedUrls.Contains(new Uri(referrerUrl).Authority); // compare with base uri    
            return isValidClient;
        }
    }
}