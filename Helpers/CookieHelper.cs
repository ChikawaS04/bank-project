namespace BankApp.Helpers
{
    public static class CookieHelper
    {
        /// <summary>
        /// Sets a cookie in the HTTP response.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="key">The name of the cookie.</param>
        /// <param name="value">The value to store in the cookie.</param>
        /// <param name="expireTime">Optional expiration time in minutes.</param>
        public static void CookieSet(HttpContext context, string key, string value, int? expireTime)
        {
            var options = new CookieOptions
            {
                Expires = expireTime.HasValue
                    ? DateTime.Now.AddMinutes(expireTime.Value)
                    : DateTime.Now.AddMilliseconds(10)
            };

            context.Response.Cookies.Append(key, value, options);
        }
    }

}
