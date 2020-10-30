// <copyright>
// Copyright by BEMA Software Services
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.UI;
using RestSharp;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace RockWeb.Plugins.com_bemaservices.CustomBlocks.Reporting
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Blocked Email Report" )]
    [Category( "BEMA Services > Reporting" )]
    [Description( "A reporting block that displays blocked emails from Mailgun" )]

    [TextField( "Base URL", "Mailgun's base API URL. For US based domains, it's 'https://api.mailgun.net/v3', for EU based domains, it's 'https://api.eu.mailgun.net/v3'", true, "https://api.mailgun.net/v3", "", 0, AttributeKey.BaseUrl )]
    [TextField( "API Key", "", true, "", "", 1, AttributeKey.ApiKey )]
    public partial class BlockedEmailReport : RockBlock, ICustomGridColumns
    {
        #region Attribute Keys
        private static class AttributeKey
        {
            public const string BaseUrl = "BaseUrl";
            public const string ApiKey = "ApiKey";
        }
        #endregion

        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gList.GridRebind += gList_GridRebind;

            gList.DataKeyNames = new string[] { "Id" };
            gList.PersonIdField = "Id";

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                SetFilter();
                BindGrid();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "First Name", "First Name", tbFirstName.Text );
            rFilter.SaveUserPreference( "Last Name", "Last Name", tbLastName.Text );
            rFilter.SaveUserPreference( "Email", "Email", tbEmail.Text );
            rFilter.SaveUserPreference( "Domain", "Domain", tbDomain.Text );
            rFilter.SaveUserPreference( "Error Code", "Error Code", tbErrorCode.Text );
            rFilter.SaveUserPreference( "Type", "Type", cblTypeFilter.SelectedValues.AsDelimited( ";" ) );
            rFilter.SaveUserPreference( "Created Date Time", "Created Date Time", drpCreatedDateTime.DelimitedValues );

            BindGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {

            if ( e.Key == "First Name" )
            {
                return;
            }
            else if ( e.Key == "Last Name" )
            {
                return;
            }
            else if ( e.Key == "Email" )
            {
                return;
            }
            else if ( e.Key == "Domain" )
            {
                return;
            }
            else if ( e.Key == "Error Code" )
            {
                return;
            }
            else if ( e.Key == "Gender" )
            {
                e.Value = ResolveValues( e.Value, cblTypeFilter );
            }
            else if ( e.Key == "Created Date Time" )
            {
                e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
            }
            else
            {
                e.Value = string.Empty;
            }
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rFilter_ClearFilterClick( object sender, EventArgs e )
        {
            rFilter.DeleteUserPreferences();
            SetFilter();
        }

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

            SetFilter();
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gList_GridRebind( object sender, EventArgs e )
        {
            SetFilter();
            BindGrid();
        }

        #endregion

        #region Methods

        private string ResolveValues( string values, System.Web.UI.WebControls.CheckBoxList checkBoxList )
        {
            var resolvedValues = new List<string>();

            foreach ( string value in values.Split( ';' ) )
            {
                var item = checkBoxList.Items.FindByValue( value );
                if ( item != null )
                {
                    resolvedValues.Add( item.Text );
                }
            }

            return resolvedValues.AsDelimited( ", " );
        }

        private void SetFilter()
        {
            tbFirstName.Text = rFilter.GetUserPreference( "First Name" );
            tbLastName.Text = rFilter.GetUserPreference( "Last Name" );
            tbEmail.Text = rFilter.GetUserPreference( "Email" );
            tbDomain.Text = rFilter.GetUserPreference( "Domain" );
            tbErrorCode.Text = rFilter.GetUserPreference( "Error Code" );

            string typeValue = rFilter.GetUserPreference( "Type" );
            if ( !string.IsNullOrWhiteSpace( typeValue ) )
            {
                cblTypeFilter.SetValues( typeValue.Split( ';' ).ToList() );
            }
            else
            {
                cblTypeFilter.ClearSelection();
            }

            drpCreatedDateTime.DelimitedValues = rFilter.GetUserPreference( "Created Date Time" );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            RockContext rockContext = new RockContext();
            PersonService personService = new PersonService( rockContext );

            var basePersonQuery = personService.Queryable().AsNoTracking().Where( p => p.Email != null && p.Email != "" && p.IsEmailActive && p.EmailPreference != EmailPreference.DoNotEmail );
            var types = new List<string>();
            foreach ( var item in cblTypeFilter.SelectedValues )
            {
                types.Add( item );
            }

            var domainFilter = tbDomain.Text;
            var emailFilter = tbEmail.Text;
            var errorCodeFilter = tbErrorCode.Text;

            List<BlockedEmailEntry> blockedEmailEntries = new List<BlockedEmailEntry>();

            string baseUrl = GetAttributeValue( AttributeKey.BaseUrl );
            string apiKey = GetAttributeValue( AttributeKey.ApiKey );
            var errorMessages = new List<string>();
            List<DomainResponse> domains = new List<DomainResponse>();
            if ( !GetDomains( out domains, errorMessages, baseUrl, apiKey ) )
            {
                errorMessages.Add( "Unable to get Mailgun Domains." );
            }

            if ( domains != null )
            {
                foreach ( var domain in domains )
                {
                    if ( domainFilter.IsNullOrWhiteSpace() || domain.Name.Contains( domainFilter ) )
                    {
                        if ( !types.Any() || types.Contains( "Bounce" ) )
                        {
                            List<BounceResponse> bounces = new List<BounceResponse>();
                            if ( !GetBounces( out bounces, errorMessages, baseUrl, apiKey, domain.Name ) )
                            {
                                errorMessages.Add( "Unable to get Mailgun Bounces." );
                            }

                            if ( bounces != null )
                            {
                                foreach ( var bounce in bounces )
                                {
                                    if ( emailFilter.IsNullOrWhiteSpace() || bounce.Email.Contains( emailFilter ) )
                                    {
                                        if ( errorCodeFilter.IsNullOrWhiteSpace() || bounce.Error.Contains( errorCodeFilter ) )
                                        {
                                            var relatedPeople = basePersonQuery
                                                .Where( p => p.Email == bounce.Email )
                                                .ToList();

                                            foreach ( var relatedPerson in relatedPeople )
                                            {
                                                var blockedEmailEntry = new BlockedEmailEntry();
                                                blockedEmailEntry.Id = relatedPerson.Id;
                                                blockedEmailEntry.NickName = relatedPerson.NickName;
                                                blockedEmailEntry.LastName = relatedPerson.LastName;
                                                blockedEmailEntry.Email = bounce.Email;
                                                blockedEmailEntry.Domain = domain.Name;
                                                blockedEmailEntry.Type = "Bounce";
                                                blockedEmailEntry.ErrorCode = bounce.Error;
                                                blockedEmailEntry.CreatedDateTime = DateTime.Parse( bounce.CreatedDateTime.Replace( " UTC", "Z" ) );
                                                blockedEmailEntries.Add( blockedEmailEntry );
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if ( !types.Any() || types.Contains( "Unsubscribe" ) )
                        {
                            List<UnsubscribeResponse> unsubscribes = new List<UnsubscribeResponse>();
                            if ( !GetUnsubscribes( out unsubscribes, errorMessages, baseUrl, apiKey, domain.Name ) )
                            {
                                errorMessages.Add( "Unable to get Mailgun Unsubscribes." );
                            }

                            if ( unsubscribes != null )
                            {
                                foreach ( var unsubscribe in unsubscribes )
                                {
                                    if ( emailFilter.IsNullOrWhiteSpace() || unsubscribe.Email.Contains( emailFilter ) )
                                    {
                                        if ( errorCodeFilter.IsNullOrWhiteSpace() || unsubscribe.Error.Contains( errorCodeFilter ) )
                                        {
                                            var relatedPeople = basePersonQuery
                                                .Where( p => p.Email == unsubscribe.Email )
                                                .ToList();

                                            foreach ( var relatedPerson in relatedPeople )
                                            {
                                                var blockedEmailEntry = new BlockedEmailEntry();
                                                blockedEmailEntry.Id = relatedPerson.Id;
                                                blockedEmailEntry.NickName = relatedPerson.NickName;
                                                blockedEmailEntry.LastName = relatedPerson.LastName;
                                                blockedEmailEntry.Email = unsubscribe.Email;
                                                blockedEmailEntry.Domain = domain.Name;
                                                blockedEmailEntry.Type = "Unsubscribe";
                                                blockedEmailEntry.ErrorCode = unsubscribe.Error;
                                                blockedEmailEntry.CreatedDateTime = DateTime.Parse( unsubscribe.CreatedDateTime.Replace( " UTC", "Z" ) );
                                                blockedEmailEntries.Add( blockedEmailEntry );
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if ( !types.Any() || types.Contains( "Complaint" ) )
                        {
                            List<ComplaintResponse> complaints = new List<ComplaintResponse>();
                            if ( !GetComplaints( out complaints, errorMessages, baseUrl, apiKey, domain.Name ) )
                            {
                                errorMessages.Add( "Unable to get Mailgun Complaints." );
                            }

                            foreach ( var complaint in complaints )
                            {
                                if ( emailFilter.IsNullOrWhiteSpace() || complaint.Email.Contains( emailFilter ) )
                                {
                                    if ( errorCodeFilter.IsNullOrWhiteSpace() )
                                    {
                                        var relatedPeople = basePersonQuery
                                       .Where( p => p.Email == complaint.Email )
                                       .ToList();

                                        foreach ( var relatedPerson in relatedPeople )
                                        {
                                            var blockedEmailEntry = new BlockedEmailEntry();
                                            blockedEmailEntry.Id = relatedPerson.Id;
                                            blockedEmailEntry.NickName = relatedPerson.NickName;
                                            blockedEmailEntry.LastName = relatedPerson.LastName;
                                            blockedEmailEntry.Email = complaint.Email;
                                            blockedEmailEntry.Domain = domain.Name;
                                            blockedEmailEntry.Type = "Complaint";
                                            blockedEmailEntry.ErrorCode = "";
                                            blockedEmailEntry.CreatedDateTime = DateTime.Parse( complaint.CreatedDateTime.Replace( " UTC", "Z" ) );
                                            blockedEmailEntries.Add( blockedEmailEntry );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if ( errorMessages.Any() )
            {
                nbErrorMessages.Title = "Error</br>";
                nbErrorMessages.Text = errorMessages.AsDelimited( "</br>" );
                nbErrorMessages.Visible = true;
            }
            else
            {
                nbErrorMessages.Visible = false;
            }

            var qry = blockedEmailEntries.AsQueryable();

            // Filter by First Name
            string firstName = tbFirstName.Text;
            if ( !string.IsNullOrWhiteSpace( firstName ) )
            {
                qry = qry.Where( m =>
                    m.NickName.StartsWith( firstName ) );
            }

            // Filter by Last Name
            string lastName = tbLastName.Text;
            if ( !string.IsNullOrWhiteSpace( lastName ) )
            {
                qry = qry.Where( m => m.LastName.StartsWith( lastName ) );
            }

            // Filter by date range
            var dateRange = DateRangePicker.CalculateDateRangeFromDelimitedValues( drpCreatedDateTime.DelimitedValues );

            if ( dateRange.Start.HasValue )
            {
                qry = qry.Where( m =>
                    m.CreatedDateTime >= dateRange.Start.Value );
            }

            if ( dateRange.End.HasValue )
            {
                var end = dateRange.End.Value.AddHours( 23 ).AddMinutes( 59 ).AddSeconds( 59 );
                qry = qry.Where( m =>
                    m.CreatedDateTime < end );
            }

            // sort the query based on the column that was selected to be sorted
            var sortProperty = gList.SortProperty;
            if ( gList.AllowSorting && sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderByDescending( a => a.CreatedDateTime ).ThenBy( a => a.LastName ).ThenBy( a => a.NickName );
            }

            gList.EntityTypeId = EntityTypeCache.Get<Person>().Id;
            gList.Actions.ShowBulkUpdate = true;
            gList.Actions.ShowMergePerson = true;
            gList.Actions.ShowMergeTemplate = true;
            gList.SetLinqDataSource( qry );
            gList.DataBind();

        }

        /// <summary>
        /// Return a rest client.
        /// </summary>
        /// <returns>The rest client.</returns>
        private static RestClient RestClient( string baseUrl, string apiKey )
        {
            var restClient = new RestClient( baseUrl );
            var authorizationHeader = String.Format( "api:{0}", apiKey );
            var encodedHeader = Convert.ToBase64String( Encoding.UTF8.GetBytes( authorizationHeader ) );
            restClient.AddDefaultHeader( "Authorization", string.Format( "Basic {0}", encodedHeader ) );
            return restClient;
        }

        static bool GetDomains( out List<DomainResponse> getDomainResponse, List<string> errorMessages, string baseUrl, string apiKey )
        {
            getDomainResponse = null;
            RestClient restClient = RestClient( baseUrl, apiKey );
            RestRequest restRequest = new RestRequest( "/domains" );
            IRestResponse restResponse = restClient.Execute( restRequest );

            if ( restResponse.StatusCode == HttpStatusCode.Unauthorized )
            {
                errorMessages.Add( "Failed to authorize Mailgun. Please confirm your access token." );
                return false;
            }

            if ( restResponse.StatusCode != HttpStatusCode.OK )
            {
                errorMessages.Add( "Failed to get Mailgun Domains: " + restResponse.Content );
                return false;
            }

            var jObject = JObject.Parse( restResponse.Content );
            var items = jObject.GetValue( "items" ).ToString();
            getDomainResponse = JsonConvert.DeserializeObject<List<DomainResponse>>( items );
            if ( getDomainResponse == null )
            {
                errorMessages.Add( "Get Domains is not valid: " + restResponse.Content );
                return false;
            }

            return true;
        }

        static bool GetBounces( out List<BounceResponse> getBounceResponse, List<string> errorMessages, string baseUrl, string apiKey, string domain )
        {
            getBounceResponse = null;
            RestClient restClient = RestClient( baseUrl, apiKey );
            RestRequest restRequest = new RestRequest( string.Format( "/{0}/bounces", domain ) );
            IRestResponse restResponse = restClient.Execute( restRequest );

            if ( restResponse.StatusCode == HttpStatusCode.Unauthorized )
            {
                errorMessages.Add( "Failed to authorize Mailgun. Please confirm your access token." );
                return false;
            }

            if ( restResponse.StatusCode != HttpStatusCode.OK )
            {
                errorMessages.Add( "Failed to get Mailgun Bounces: " + restResponse.Content );
                return false;
            }


            var jObject = JObject.Parse( restResponse.Content );
            var items = jObject.GetValue( "items" ).ToString();
            getBounceResponse = JsonConvert.DeserializeObject<List<BounceResponse>>( items );
            if ( getBounceResponse == null )
            {
                errorMessages.Add( "Get Bounces is not valid: " + restResponse.Content );
                return false;
            }

            return true;
        }

        static bool GetUnsubscribes( out List<UnsubscribeResponse> getUnsubscribeResponse, List<string> errorMessages, string baseUrl, string apiKey, string domain )
        {
            getUnsubscribeResponse = null;
            RestClient restClient = RestClient( baseUrl, apiKey );
            RestRequest restRequest = new RestRequest( string.Format( "/{0}/unsubscribes", domain ) );
            IRestResponse restResponse = restClient.Execute( restRequest );

            if ( restResponse.StatusCode == HttpStatusCode.Unauthorized )
            {
                errorMessages.Add( "Failed to authorize Mailgun. Please confirm your access token." );
                return false;
            }

            if ( restResponse.StatusCode != HttpStatusCode.OK )
            {
                errorMessages.Add( "Failed to get Mailgun Unsubscribes: " + restResponse.Content );
                return false;
            }


            var jObject = JObject.Parse( restResponse.Content );
            var items = jObject.GetValue( "items" ).ToString();
            getUnsubscribeResponse = JsonConvert.DeserializeObject<List<UnsubscribeResponse>>( items );
            if ( getUnsubscribeResponse == null )
            {
                errorMessages.Add( "Get Unsubscribes is not valid: " + restResponse.Content );
                return false;
            }

            return true;
        }

        static bool GetComplaints( out List<ComplaintResponse> getComplaintResponse, List<string> errorMessages, string baseUrl, string apiKey, string domain )
        {
            getComplaintResponse = null;
            RestClient restClient = RestClient( baseUrl, apiKey );
            RestRequest restRequest = new RestRequest( string.Format( "/{0}/complaints", domain ) );
            IRestResponse restResponse = restClient.Execute( restRequest );

            if ( restResponse.StatusCode == HttpStatusCode.Unauthorized )
            {
                errorMessages.Add( "Failed to authorize Mailgun. Please confirm your access token." );
                return false;
            }

            if ( restResponse.StatusCode != HttpStatusCode.OK )
            {
                errorMessages.Add( "Failed to get Mailgun Complaints: " + restResponse.Content );
                return false;
            }


            var jObject = JObject.Parse( restResponse.Content );
            var items = jObject.GetValue( "items" ).ToString();
            getComplaintResponse = JsonConvert.DeserializeObject<List<ComplaintResponse>>( items );
            if ( getComplaintResponse == null )
            {
                errorMessages.Add( "Get Complaints is not valid: " + restResponse.Content );
                return false;
            }

            return true;
        }

        #endregion

        #region Helper Classes
        private class BlockedEmailEntry
        {
            public int Id { get; set; }
            public string NickName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Domain { get; set; }
            public string Type { get; set; }
            public string ErrorCode { get; set; }
            public DateTime CreatedDateTime { get; set; }
        }

        private class DomainResponse
        {
            [JsonProperty( "id" )]
            public string Id { get; set; }

            [JsonProperty( "name" )]
            public string Name { get; set; }
        }

        private class BounceResponse
        {
            [JsonProperty( "address" )]
            public string Email { get; set; }

            [JsonProperty( "code" )]
            public string Code { get; set; }

            [JsonProperty( "error" )]
            public string Error { get; set; }

            [JsonProperty( "created_at" )]
            public string CreatedDateTime { get; set; }
        }

        private class UnsubscribeResponse
        {
            [JsonProperty( "address" )]
            public string Email { get; set; }

            [JsonProperty( "tag" )]
            public string Error { get; set; }

            [JsonProperty( "created_at" )]
            public string CreatedDateTime { get; set; }
        }

        private class ComplaintResponse
        {
            [JsonProperty( "address" )]
            public string Email { get; set; }

            [JsonProperty( "created_at" )]
            public string CreatedDateTime { get; set; }
        }

        #endregion
    }


}