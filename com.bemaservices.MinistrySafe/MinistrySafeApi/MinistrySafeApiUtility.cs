﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using Rock.Web.Cache;
using com.bemaservices.MinistrySafe.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using System;

namespace com.bemaservices.MinistrySafe.MinistrySafeApi
{
    internal static class MinistrySafeApiUtility
    {
        #region Utilities        
        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static List<AttributeValue> GetSettings( RockContext rockContext )
        {
            var ministrySafeEntityType = EntityTypeCache.Get( typeof( com.bemaservices.MinistrySafe.MinistrySafe ) );
            if ( ministrySafeEntityType != null )
            {
                var service = new AttributeValueService( rockContext );
                return service.Queryable( "Attribute" )
                    .Where( v => v.Attribute.EntityTypeId == ministrySafeEntityType.Id )
                    .ToList();
            }

            return null;
        }

        /// <summary>
        /// Gets the setting value.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private static string GetSettingValue( List<AttributeValue> values, string key, bool encryptedValue = false )
        {
            string value = values
                .Where( v => v.AttributeKey == key )
                .Select( v => v.Value )
                .FirstOrDefault();
            if ( encryptedValue && !string.IsNullOrWhiteSpace( value ) )
            {
                try
                { value = Encryption.DecryptString( value ); }
                catch { }
            }

            return value;
        }

        /// <summary>
        /// Return a rest client.
        /// </summary>
        /// <returns>The rest client.</returns>
        private static RestClient RestClient()
        {
            string token = GlobalAttributesCache.Value( "MinistrySafeAPIToken" );
            var restClient = new RestClient( MinistrySafeConstants.MINISTRYSAFE_APISERVER );           

            restClient.AddDefaultHeader( "Authorization", string.Format( "Token token={0}", token ) );
            return restClient;
        }

        /// <summary>
        /// RestClient request to string for debugging purposes.
        /// </summary>
        /// <param name="restClient">The rest client.</param>
        /// <param name="restRequest">The rest request.</param>
        /// <returns>The RestClient Request in string format.</returns>
        // https://stackoverflow.com/questions/15683858/restsharp-print-raw-request-and-response-headers
        private static string RequestToString( RestClient restClient, RestRequest restRequest )
        {
            var requestToLog = new
            {
                resource = restRequest.Resource,
                // Parameters are custom anonymous objects in order to have the parameter type as a nice string
                // otherwise it will just show the enum value
                parameters = restRequest.Parameters.Select( parameter => new
                {
                    name = parameter.Name,
                    value = parameter.Value,
                    type = parameter.Type.ToString()
                } ),
                // ToString() here to have the method as a nice string otherwise it will just show the enum value
                method = restRequest.Method.ToString(),
                // This will generate the actual Uri used in the request
                uri = restClient.BuildUri( restRequest ),
            };
            return JsonConvert.SerializeObject( requestToLog );
        }

        /// <summary>
        /// RestClient response to string for debugging purposes.
        /// </summary>
        /// <param name="restResponse">The rest response.</param>
        /// <returns>The RestClient response in string format.</returns>
        // https://stackoverflow.com/questions/15683858/restsharp-print-raw-request-and-response-headers
        private static string ResponseToString( IRestResponse restResponse )
        {
            var responseToLog = new
            {
                statusCode = restResponse.StatusCode,
                content = restResponse.Content,
                headers = restResponse.Headers,
                // The Uri that actually responded (could be different from the requestUri if a redirection occurred)
                responseUri = restResponse.ResponseUri,
                errorMessage = restResponse.ErrorMessage,
            };

            return JsonConvert.SerializeObject( responseToLog );
        }
        #endregion
        #region Public Methods
        /// <summary>
        /// Gets the packages.
        /// </summary>
        /// <param name="getPackagesResponse">The get packages response.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>True/False value of whether the request was successfully sent or not.</returns>
        internal static bool GetUsers( out GetUsersResponse getUsersResponse, List<string> errorMessages )
        {
            getUsersResponse = null;
            RestClient restClient = RestClient();
            RestRequest restRequest = new RestRequest( MinistrySafeConstants.MINISTRYSAFE_USERS_URL );
            IRestResponse restResponse = restClient.Execute( restRequest );

            if ( restResponse.StatusCode == HttpStatusCode.Unauthorized )
            {
                errorMessages.Add( "Failed to authorize MinistrySafe. Please confirm your access token." );
                return false;
            }

            if ( restResponse.StatusCode != HttpStatusCode.OK )
            {
                errorMessages.Add( "Failed to get MinistrySafe Users: " + restResponse.Content );
                return false;
            }

            getUsersResponse = JsonConvert.DeserializeObject<GetUsersResponse>( restResponse.Content );
            if ( getUsersResponse == null )
            {
                errorMessages.Add( "Get Users is not valid: " + restResponse.Content );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates the candidate.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="createCandidateResponse">The create candidate response.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>True/False value of whether the request was successfully sent or not.</returns>
        internal static bool CreateUser(Rock.Model.Workflow workflow, Person person, int personAliasId, string userType, out CreateUserResponse createUserResponse, List<string> errorMessages )
        {
            createUserResponse = null;
            RestClient restClient = RestClient();
            RestRequest restRequest = new RestRequest( MinistrySafeConstants.MINISTRYSAFE_USERS_URL, Method.POST );
            restRequest.AddJsonBody( new
            {
                user = new CreateUserRequest()
                {
                    first_name = person.FirstName,
                    last_name = person.LastName,
                    email = person.Email,
                    external_id = workflow.Id.ToString(),
                    user_type = userType
                }
            } );

            IRestResponse restResponse = restClient.Execute( restRequest );

            if ( restResponse.StatusCode == HttpStatusCode.Unauthorized )
            {
                errorMessages.Add( "Invalid MinistrySafe access token. To Re-authenticate go to Admin Tools > System Settings > MinistrySafe. Click edit to change your access token." );
                return false;
            }

            if ( restResponse.StatusCode != HttpStatusCode.Created )
            {
                errorMessages.Add( "Failed to create MinistrySafe User: " + restResponse.Content );
                return false;
            }

            createUserResponse = JsonConvert.DeserializeObject<CreateUserResponse>( restResponse.Content );
            if ( createUserResponse == null )
            {
                errorMessages.Add( "Create User Response is not valid: " + restResponse.Content );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates the invitation.
        /// </summary>
        /// <param name="candidateId">The candidate identifier.</param>
        /// <param name="package">The package.</param>
        /// <param name="createInvitationResponse">The create invitation response.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>True/False value of whether the request was successfully sent or not.</returns>
        internal static bool AssignTraining( string candidateId, string surveyCode, out AssignTrainingResponse assignTrainingResponse, List<string> errorMessages )
        {
            assignTrainingResponse = null;
            RestClient restClient = RestClient();
            RestRequest restRequest = new RestRequest( String.Format( "{0}/{1}/assign_training", MinistrySafeConstants.MINISTRYSAFE_USERS_URL, candidateId ), Method.POST );
            restRequest.AddParameter( "survey_code", surveyCode );

            IRestResponse restResponse = restClient.Execute( restRequest );

            if ( restResponse.StatusCode == HttpStatusCode.Unauthorized )
            {
                errorMessages.Add( "Invalid MinistrySafe access token. To Re-authenticate go to Admin Tools > System Settings > MinistrySafe. Click edit to change your access token." );
                return false;
            }

            if ( restResponse.StatusCode != HttpStatusCode.Created )
            {
                errorMessages.Add( "Failed to assign MinistrySafe Training: " + restResponse.Content );
                return false;
            }

            assignTrainingResponse = JsonConvert.DeserializeObject<AssignTrainingResponse>( restResponse.Content );
            if ( assignTrainingResponse == null )
            {
                errorMessages.Add( "Assign Training Response is not valid: " + restResponse.Content );
                return false;
            }

            return true;
        }

        internal static bool ResendTraining( string candidateId, string surveyCode, out AssignTrainingResponse resendTrainingResponse, List<string> errorMessages )
        {
            resendTrainingResponse = null;
            RestClient restClient = RestClient();
            RestRequest restRequest = new RestRequest( String.Format( "{0}/{1}/resend_training", MinistrySafeConstants.MINISTRYSAFE_USERS_URL, candidateId ), Method.POST );
            restRequest.AddParameter( "survey_code", surveyCode );

            IRestResponse restResponse = restClient.Execute( restRequest );

            if ( restResponse.StatusCode == HttpStatusCode.Unauthorized )
            {
                errorMessages.Add( "Invalid MinistrySafe access token. To Re-authenticate go to Admin Tools > System Settings > MinistrySafe. Click edit to change your access token." );
                return false;
            }

            if ( restResponse.StatusCode != HttpStatusCode.Created )
            {
                errorMessages.Add( "Failed to resend MinistrySafe Training: " + restResponse.Content );
                return false;
            }

            resendTrainingResponse = JsonConvert.DeserializeObject<AssignTrainingResponse>( restResponse.Content );
            if ( resendTrainingResponse == null )
            {
                errorMessages.Add( "Assign Training Response is not valid: " + restResponse.Content );
                return false;
            }

            return true;
        }

        internal static bool GetTrainingForUser( string candidateId, out GetTrainingResponse getReportResponse, List<string> errorMessages )
        {
            getReportResponse = null;
            RestClient restClient = RestClient();
            RestRequest restRequest = new RestRequest( String.Format( "{0}/{1}/trainings", MinistrySafeConstants.MINISTRYSAFE_USERS_URL, candidateId ) );
            IRestResponse restResponse = restClient.Execute( restRequest );

            if ( restResponse.StatusCode == HttpStatusCode.Unauthorized )
            {
                errorMessages.Add( "Invalid MinistrySafe access token. To Re-authenticate go to Admin Tools > System Settings > MinistrySafe. Click edit to change your access token." );
                return false;
            }

            if ( restResponse.StatusCode != HttpStatusCode.OK )
            {
                errorMessages.Add( "Failed to get MinistrySafe Training: " + restResponse.Content );
                return false;
            }

            getReportResponse = JsonConvert.DeserializeObject<GetTrainingResponse>( restResponse.Content );
            if ( getReportResponse == null )
            {
                errorMessages.Add( "Get Training is not valid: " + restResponse.Content );
                return false;
            }

            return true;
        }
        #endregion
    }
}
