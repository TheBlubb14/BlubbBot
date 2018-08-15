using System;
using System.Web;

namespace BlubbBot
{
    internal static class Extension
    {
        public static Uri AddParameter(this Uri uri, string key, object value)
        {
            var builder = new UriBuilder(uri);
            var query = HttpUtility.ParseQueryString(builder.Query);

            query.Add(key, value.ToString());
            builder.Query = query.ToString();
            uri = builder.Uri;

            return uri;
        }
    }
}
