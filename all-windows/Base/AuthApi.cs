using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SmartDNSProxy_VPN_Client
{
    internal static class AuthApi
    {
        private static readonly HttpClient RestClient;

#if DEBUG
        private const string AuthApiUrl = "http://199.241.146.241:5000/auth/";
#else
        private const string AuthApiUrl = "https://auth-api.glbls.net:5000/auth/";
#endif

        static AuthApi()
        {
            RestClient = new HttpClient();
            foreach (var header in RestClient.DefaultRequestHeaders)
            {
                RestClient.DefaultRequestHeaders.Remove(header.Key);
            }
            RestClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static async Task<AuthApiResponse> Authorize(string username, string password)
        {
            string hash = GenerateMd5Hash(username + password);
            var content = new StringContent($"{{\"hash\": \"{hash}\"}}", Encoding.UTF8, "application/json");
            HttpResponseMessage response;
            try
            {
                response = await RestClient.PostAsync(AuthApiUrl + username, content);
            }
            catch (HttpRequestException)
            {
                return AuthApiResponse.ServerError;
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return AuthApiResponse.Success;
                case HttpStatusCode.NotFound:
                case HttpStatusCode.MethodNotAllowed:
                    return AuthApiResponse.InvalidCredentials;
                case HttpStatusCode.Unauthorized:
                    return AuthApiResponse.AccountDisabled;
                default:
                    return AuthApiResponse.ServerError;
            }
        }

        private static string GenerateMd5Hash(string input)
        {
            var md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            foreach (byte b in hashBytes)
                sb.Append(b.ToString("X2"));
            return sb.ToString().ToLower();
        }
    }

    public enum AuthApiResponse
    {
        Success,
        InvalidCredentials,
        AccountDisabled,
        ServerError
    }
}