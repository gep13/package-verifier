// Copyright © 2015 - Present RealDimensions Software, LLC
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// 
// You may obtain a copy of the License at
// 
// 	http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace chocolatey.package.verifier.infrastructure.app.services
{
    using System;
    using System.Globalization;
    using System.Net;
    using NuGet;
    using NuGet.Resources;
    using results;

    public class NuGetService : INuGetService
    {
        public const string API_KEY_HEADER = "X-NuGet-ApiKey";
        public const int MAX_REDIRECTION_COUNT = 20;

        public string ApiKeyHeader { get { return API_KEY_HEADER; } }

        public static Uri get_service_endpoint_url(string baseUrl, string path)
        {
            return new Uri(new Uri(baseUrl), path);
        }

        public HttpClient get_client(string baseUrl, string path, string method, string contentType)
        {
            Uri requestUri = get_service_endpoint_url(baseUrl, path);

            var client = new HttpClient(requestUri)
            {
                ContentType = contentType,
                Method = method,
                UserAgent = "{0}/{1}".format_with(ApplicationParameters.Name, ApplicationParameters.FileVersion)
            };

            return client;
        }

        /// <summary>
        ///   Ensures that success response is received.
        /// </summary>
        /// <param name="client">The client that is making the request.</param>
        /// <param name="expectedStatusCode">The exected status code.</param>
        /// <returns>
        ///   True if success response is received; false if redirection response is received.
        ///   In this case, _baseUri will be updated to be the new redirected Uri and the requrest
        ///   should be retried.
        /// </returns>
        public NuGetServiceGetClientResult ensure_successful_response(HttpClient client, HttpStatusCode? expectedStatusCode = null)
        {
            var result = new NuGetServiceGetClientResult
            {
                Success = false
            };

            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)client.GetResponse();
                if (response != null &&
                    ((expectedStatusCode.HasValue && expectedStatusCode.Value != response.StatusCode) ||

                     // If expected status code isn't provided, just look for anything 400 (Client Errors) or higher (incl. 500-series, Server Errors)
                    // 100-series is protocol changes, 200-series is success, 300-series is redirect.
                     (!expectedStatusCode.HasValue && (int)response.StatusCode >= 400))) throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, NuGetResources.PackageServerError, response.StatusDescription, String.Empty));

                return result;
            }
            catch (WebException e)
            {
                if (e.Response == null) throw;
                response = (HttpWebResponse)e.Response;

                // Check if the error is caused by redirection
                if (response.StatusCode == HttpStatusCode.MultipleChoices ||
                    response.StatusCode == HttpStatusCode.MovedPermanently ||
                    response.StatusCode == HttpStatusCode.Found ||
                    response.StatusCode == HttpStatusCode.SeeOther ||
                    response.StatusCode == HttpStatusCode.TemporaryRedirect)
                {
                    var location = response.Headers["Location"];
                    Uri newUri;
                    if (!Uri.TryCreate(client.Uri, location, out newUri)) throw;

                    result.RedirectUrl = newUri.ToString();
                    return result;
                }

                if (expectedStatusCode != response.StatusCode) throw new InvalidOperationException(String.Format("Failed to process request.{0} '{1}'", Environment.NewLine, response.StatusDescription, e.Message), e);

                result.Success = true;
                return result;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }
        }

    }
}
