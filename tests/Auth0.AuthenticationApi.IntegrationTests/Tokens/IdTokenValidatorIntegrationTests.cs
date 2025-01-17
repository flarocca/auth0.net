﻿using Auth0.AuthenticationApi.Models;
using Auth0.AuthenticationApi.Tokens;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Auth0.Tests.Shared;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Auth0.AuthenticationApi.IntegrationTests.Tokens
{
    public class IdTokenValidatorIntegrationTests : TestBase, IAsyncLifetime
    {
        private ManagementApiClient _managementApiClient;
        private Connection _connection;
        private User _user;
        private const string Password = "4cX8awB3T%@Aw-R:=h@ae@k?";

        public async Task InitializeAsync()
        {
            string token = await GenerateManagementApiToken();

            _managementApiClient = new ManagementApiClient(token, GetVariable("AUTH0_MANAGEMENT_API_URL"));

            var tenantSettings = await _managementApiClient.TenantSettings.GetAsync();

            if (string.IsNullOrEmpty(tenantSettings.DefaultDirectory))
            {
                throw new Exception("Tests require a tenant with a Default Directory selected.\r\n" +
                    "Enable OAuth 2.0 API Authorization under Account Settings | General and " +
                    "select a Default Directory under Account Settings | General");
            }

            // We will need a connection to add the users to...
            _connection = await _managementApiClient.Connections.CreateAsync(new ConnectionCreateRequest
            {
                Name = Guid.NewGuid().ToString("N"),
                Strategy = "auth0",
                EnabledClients = new[] { GetVariable("AUTH0_CLIENT_ID"), GetVariable("AUTH0_MANAGEMENT_API_CLIENT_ID") }
            });

            // And add a dummy user to test against
            _user = await _managementApiClient.Users.CreateAsync(new UserCreateRequest
            {
                Connection = _connection.Name,
                Email = $"{Guid.NewGuid():N}@nonexistingdomain.aaa",
                EmailVerified = true,
                Password = Password
            });
        }

        public async Task DisposeAsync()
        {
            if (_user != null)
                await _managementApiClient.Users.DeleteAsync(_user.UserId);

            if (_connection != null)
                await _managementApiClient.Connections.DeleteAsync(_connection.Id);
        }

        [Fact]
        public async Task Passes_Token_Validation()
        {
            // Arrange
            var authenticationApiClient = new AuthenticationApiClient(GetVariable("AUTH0_AUTHENTICATION_API_URL"));

            // Act
            var authenticationResponse = await authenticationApiClient.GetTokenAsync(new ResourceOwnerTokenRequest
            {
                ClientId = GetVariable("AUTH0_CLIENT_ID"),
                ClientSecret = GetVariable("AUTH0_CLIENT_SECRET"),
                Realm = _connection.Name,
                Scope = "openid",
                Username = _user.Email,
                Password = Password

            });

            var idTokenValidation = new IdTokenRequirements($"https://{GetVariable("AUTH0_AUTHENTICATION_API_URL")}/", GetVariable("AUTH0_CLIENT_ID"), TimeSpan.FromMinutes(1));
            await idTokenValidation.AssertTokenMeetsRequirements(authenticationResponse.IdToken);
        }

        [Fact]
        public async Task Passes_Token_Validation_With_CNAME()
        {
            // Arrange
            var authenticationApiClient = new AuthenticationApiClient(GetVariable("BRUCKE_AUTHENTICATION_API_URL"));

            // Act
            var authenticationResponse = await authenticationApiClient.GetTokenAsync(new ResourceOwnerTokenRequest
            {
                ClientId = GetVariable("BRUCKE_CLIENT_ID"),
                ClientSecret = GetVariable("BRUCKE_CLIENT_SECRET"),
                Realm = GetVariable("BRUCKE_CONNECTION_NAME"),
                Scope = "openid",
                Username = GetVariable("BRUCKE_USERNAME"),
                Password = GetVariable("BRUCKE_PASSWORD")

            });

            var idTokenValidation = new IdTokenRequirements($"https://{GetVariable("BRUCKE_AUTHENTICATION_API_URL")}/", GetVariable("BRUCKE_CLIENT_ID"), TimeSpan.FromMinutes(1));
            await idTokenValidation.AssertTokenMeetsRequirements(authenticationResponse.IdToken);
        }

        [Fact]
        public async Task Fails_Token_Validation_With_Incorrect_Domain()
        {
            // Arrange
            var authenticationApiClient = new AuthenticationApiClient(GetVariable("AUTH0_AUTHENTICATION_API_URL"));

            // Act
            var authenticationResponse = await authenticationApiClient.GetTokenAsync(new ResourceOwnerTokenRequest
            {
                ClientId = GetVariable("AUTH0_CLIENT_ID"),
                ClientSecret = GetVariable("AUTH0_CLIENT_SECRET"),
                Realm = _connection.Name,
                Scope = "openid",
                Username = _user.Email,
                Password = Password

            });

            var idTokenValidation = new IdTokenRequirements("https://auth0.auth0.com/", GetVariable("AUTH0_CLIENT_ID"), TimeSpan.FromMinutes(1));

            // Assert
            authenticationResponse.IdToken.Should().NotBeNull();
            await Assert.ThrowsAsync<IdTokenValidationException>(() => idTokenValidation.AssertTokenMeetsRequirements(authenticationResponse.IdToken));
        }

        [Fact]
        public async Task Fails_Token_Validation_With_Incorrect_Audience()
        {
            // Arrange
            var authenticationApiClient = new AuthenticationApiClient(GetVariable("AUTH0_AUTHENTICATION_API_URL"));

            // Act
            var authenticationResponse = await authenticationApiClient.GetTokenAsync(new ResourceOwnerTokenRequest
            {
                ClientId = GetVariable("AUTH0_CLIENT_ID"),
                ClientSecret = GetVariable("AUTH0_CLIENT_SECRET"),
                Realm = _connection.Name,
                Scope = "openid",
                Username = _user.Email,
                Password = Password

            });

            var idTokenValidation = new IdTokenRequirements($"https://{GetVariable("AUTH0_AUTHENTICATION_API_URL")}/", "invalid_audience", TimeSpan.FromMinutes(1));

            // Assert
            authenticationResponse.IdToken.Should().NotBeNull();
            await Assert.ThrowsAsync<IdTokenValidationException>(() => idTokenValidation.AssertTokenMeetsRequirements(authenticationResponse.IdToken));
        }
    }
}