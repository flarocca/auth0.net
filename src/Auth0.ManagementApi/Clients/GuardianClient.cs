﻿using Auth0.Core.Http;
using Auth0.ManagementApi.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Auth0.ManagementApi.Clients
{
    /// <summary>
    /// Interface with all the methods available for /guardian endpoints.
    /// </summary>
    public class GuardianClient : ClientBase
    {
        /// <summary>
        /// Creates a new instance of <see cref="GuardianClient"/>.
        /// </summary>
        /// <param name="connection">The <see cref="IApiConnection" /> which is used to communicate with the API.</param>
        public GuardianClient(IApiConnection connection)
            : base(connection)
        {
        }

        /// <summary>
        /// Generate an email with a link to start the Guardian enrollment process.
        /// </summary>
        /// <param name="request">
        /// The <see cref="CreateGuardianEnrollmentTicketRequest" /> containing the information about the user who should be enrolled.
        /// </param>
        /// <returns>A <see cref="CreateGuardianEnrollmentTicketResponse" /> with the details of the ticket that was created.</returns>
        public Task<CreateGuardianEnrollmentTicketResponse> CreateEnrollmentTicketAsync(CreateGuardianEnrollmentTicketRequest request)
        {
            return Connection
                .RunAsync<CreateGuardianEnrollmentTicketResponse>(HttpMethod.Post,
                "guardian/enrollments/ticket",
                request);
        }

        /// <summary>
        /// Deletes an enrollment.
        /// </summary>
        /// <param name="id">The ID of the enrollment to delete.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous delete operation.</returns>
        public Task DeleteEnrollmentAsync(string id)
        {
            return Connection
                .RunAsync<object>(HttpMethod.Delete,
                $"guardian/enrollments/{id}");
        }

        /// <summary>
        /// Retrieves an enrollment.
        /// </summary>
        /// <param name="id">The ID of the enrollment to retrieve.</param>
        /// <returns>A <see cref="GuardianEnrollment"/> containing details of the enrollment.</returns>
        public Task<GuardianEnrollment> GetEnrollmentAsync(string id)
        {
            return Connection
                .RunAsync<GuardianEnrollment>(HttpMethod.Get,
                $"guardian/enrollments/{id}");
        }

        /// <summary>
        /// Retrieves all factors. Useful to check factor enablement and trial status.
        /// </summary>
        /// <returns>List of <see cref="GuardianFactor" /> instances with the available factors.</returns>
        public Task<IList<GuardianFactor>> GetFactorsAsync()
        {
            return Connection
                .RunAsync<IList<GuardianFactor>>(HttpMethod.Get,
                "guardian/factors");
        }

        /// <summary>
        /// Retrieves enrollment and verification templates. You can use it to check the current values for your templates.
        /// </summary>
        /// <returns>A <see cref="GuardianSmsEnrollmentTemplates" /> containing the templates.</returns>
        public Task<GuardianSmsEnrollmentTemplates> GetSmsTemplatesAsync()
        {
            return Connection
                .RunAsync<GuardianSmsEnrollmentTemplates>(HttpMethod.Get,
                "guardian/factors/sms/templates");
        }

        /// <summary>
        /// Returns provider configuration for AWS SNS.
        /// </summary>
        /// <returns>A <see cref="GuardianSnsConfiguration" /> containing Amazon SNS configuration.</returns>
        public Task<GuardianSnsConfiguration> GetSnsConfigurationAsync()
        {
            return Connection
                .RunAsync<GuardianSnsConfiguration>(HttpMethod.Get,
                "guardian/factors/push-notification/providers/sns");
        }

        /// <summary>
        /// Returns configuration for the Guardian Twilio provider.
        /// </summary>
        /// <returns><see cref="GuardianTwilioConfiguration" /> with the Twilio configuration.</returns>
        public Task<GuardianTwilioConfiguration> GetTwilioConfigurationAsync()
        {
            return Connection
                .RunAsync<GuardianTwilioConfiguration>(HttpMethod.Get,
                "guardian/factors/sms/providers/twilio");
        }

        /// <summary>
        /// Enable or Disable a Guardian factor.
        /// </summary>
        /// <param name="request">The <see cref="UpdateGuardianFactorRequest" /> containing the details of the factor to update.</param>
        /// <returns>The <see cref="UpdateGuardianFactorResponse" /> indicating the status of the factor.</returns>
        public Task<UpdateGuardianFactorResponse> UpdateFactorAsync(UpdateGuardianFactorRequest request)
        {
            var name = request.Factor == GuardianFactorName.PushNotifications ? "push-notification" : "sms";

            return Connection
                .RunAsync<UpdateGuardianFactorResponse>(HttpMethod.Put,
                $"guardian/factors/{name}",
                new { enabled = request.IsEnabled });
        }

        /// <summary>
        /// Updates enrollment and verification templates. Useful to send custom messages on SMS enrollment and verification.
        /// </summary>
        /// <param name="templates">A <see cref="GuardianSmsEnrollmentTemplates" /> containing the updated templates.</param>
        /// <returns>A <see cref="GuardianSmsEnrollmentTemplates" /> containing the templates.</returns>
        public Task<GuardianSmsEnrollmentTemplates> UpdateSmsTemplatesAsync(GuardianSmsEnrollmentTemplates templates)
        {
            return Connection
                .RunAsync<GuardianSmsEnrollmentTemplates>(HttpMethod.Put,
                "guardian/factors/sms/templates",
                templates);
        }

        /// <summary>
        /// Configure the Guardian Twilio provider.
        /// </summary>
        /// <param name="request">
        /// The <see cref="UpdateGuardianTwilioConfigurationRequest" /> containing the configuration settings.
        /// </param>
        /// <returns>The <see cref="GuardianTwilioConfiguration" /> containing the updated configuration settings.</returns>
        public Task<GuardianTwilioConfiguration> UpdateTwilioConfigurationAsync(UpdateGuardianTwilioConfigurationRequest request)
        {
            return Connection
                .RunAsync<GuardianTwilioConfiguration>(HttpMethod.Put,
                "guardian/factors/sms/providers/twilio",
                request);
        }
    }
}