using System.Net.Http.Json;
using Filmowanie.Abstractions.Configuration;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.Constants;
using Filmowanie.Account.DTOs.Incoming;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Filmowanie.Account.Services;

// TODO UTs
internal sealed class GoogleAuthService : IGoogleAuthService
{
    private readonly ILogger<GoogleAuthService> log;
    private readonly GoogleAuthOptions options;
    private readonly IHttpClientFactory httpClientFactory;

    public GoogleAuthService(ILogger<GoogleAuthService> log, IHttpClientFactory httpClientFactory, IOptions<GoogleAuthOptions> options)
    {
        this.httpClientFactory = httpClientFactory;
        this.log = log;
        this.options = options.Value;
    }

    public Task<Maybe<GoogleUserData>> GetUserData(Maybe<GoogleCode> maybeToken, CancellationToken cancelToken) => maybeToken.AcceptAsync(GetUserIdentity, this.log, cancelToken);

    private async Task<Maybe<GoogleUserData>> GetUserIdentity(GoogleCode maybeToken, CancellationToken cancelToken)
    {
        if (!this.options.Enabled)
            return new Error<GoogleUserData>("This functionality is not enabled. Change the config.", Abstractions.Enums.ErrorType.InvalidState);

        try
            {
                var client = this.httpClientFactory.CreateClient(HttpClientNames.Google);
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = this.options.ClientId,
                    ["client_secret"] = this.options.ClientSecret,
                    ["redirect_uri"] = this.options.RedirectUri,
                    ["code"] = maybeToken.Value,
                    ["grant_type"] = "authorization_code"
                });
                var authResult = await client.PostAsync(this.options.TokenUri, content, cancelToken);

                if (!authResult.IsSuccessStatusCode)
                    return new Error<GoogleUserData>("Can't authorize user! Probably code is wrong! " + await authResult.Content.ReadAsStringAsync(), Abstractions.Enums.ErrorType.IncomingDataIssue);

                var authResultJson = await authResult.Content.ReadFromJsonAsync<GoogleAuthorizationResponseDTO>(cancelToken);

                using var userInfoMessage = new HttpRequestMessage(HttpMethod.Get, this.options.DiscoveryUri);
                userInfoMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResultJson!.AccessToken);
                var getUserResponse = await client.SendAsync(userInfoMessage, cancelToken);

                if (!getUserResponse.IsSuccessStatusCode)
                    return new Error<GoogleUserData>("Can't get user info! " + await getUserResponse.Content.ReadAsStringAsync(), Abstractions.Enums.ErrorType.IncomingDataIssue);

                var userInfo = await getUserResponse.Content.ReadFromJsonAsync<GoogleUserInfoResponseDTO>(cancelToken);
                var name = $"{userInfo!.GivenName} {userInfo.FamilyName}";
                var result = new GoogleUserData(userInfo.Mail, name, authResultJson.AccessToken, authResultJson.RefreshToken);
                return result.AsMaybe();
            }
            catch (HttpRequestException ex)
            {
                var msg = "Network Error occurred during getting user info from google.";
                this.log.LogError(ex, msg);
                return new Error<GoogleUserData>(msg, Abstractions.Enums.ErrorType.Network);
            }
            catch (Exception ex)
            {
                var msg = "Unspecified Error occurred during getting user info from google.";
                this.log.LogError(ex, msg);
                return new Error<GoogleUserData>(msg, Abstractions.Enums.ErrorType.Unknown);
            }
    }
}