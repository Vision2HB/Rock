using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_willowcreek.CareCenter
{
    /// <summary>
    /// Care Center Search Blok
    /// </summary>
    [DisplayName( "Search" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "Care Center search block" )]

    [TextField( "Search Caption", "Caption to display at the top of search panel", false, "Find A Guest", "", 0 )]
    [TextField( "Results Caption", "Caption to display at the top of search results", false, "Results", "", 1 )]
    [LinkedPage( "Select Page", "Page to navigate to after selecting a person", false, "", "", 2 )]
    [LinkedPage( "Person Profile Page", "Page to navigate to for viewing profile", false, Rock.SystemGuid.Page.PERSON_PROFILE_PERSON_PAGES, "", 3 )]
    [LinkedPage( "Add Family Page", "Page to navigate to when adding a new family", false, Rock.SystemGuid.Page.NEW_FAMILY, "", 4)]

    public partial class Search : Rock.Web.UI.RockBlock
    {

        private RockContext _rockContext;

        #region Properties

        protected bool AdvancedSearch
        {
            get
            {
                return hfAdvancedSearch.Value.AsBoolean();
            }

            set
            {
                hfAdvancedSearch.Value = value.ToString().ToLower();
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            tbFirstName.Attributes.Add( "autofocus", "autofocus" );

            gPeople.DataKeyNames = new string[] { "Id" };
            gPeople.GridRebind += gPeople_GridRebind;
            gPeople.RowDataBound += gPeople_RowDataBound;
            gPeople.PersonIdField = "Id";
            gPeople.Actions.ShowAdd = false;
            gPeople.Actions.ShowCommunicate = false;
            gPeople.Actions.ShowBulkUpdate = false;
            gPeople.Actions.ShowExcelExport = false;
            gPeople.Actions.ShowMergeTemplate = false;

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbWarning.Visible = false;

            if ( !Page.IsPostBack )
            {
                this.Page.Form.DefaultButton = btnSearch.UniqueID;

                tbFirstName.Text = PageParameter( "FirstName" );
                tbLastName.Text = PageParameter( "LastName" );
                if ( !string.IsNullOrWhiteSpace( tbFirstName.Text ) || !string.IsNullOrWhiteSpace( tbLastName.Text ) )
                {
                    FindPeople();
                }
                else
                {
                    ShowView();
                }
            }
        }

        protected override void OnPreRender( EventArgs e )
        {
            pnlAdvancedSearch.Style["display"] = AdvancedSearch ? "block" : "none";
            lAdvancedSearchLink.Text = AdvancedSearch ? "Hide Advanced Search" : "Show Advanced Search";
            base.OnPreRender( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowView();
        }

        protected void btnSearch_Click( object sender, EventArgs e )
        {
            FindPeople();
        }

        protected void btnClear_Click( object sender, EventArgs e )
        {
            tbFirstName.Text = string.Empty;
            tbLastName.Text = string.Empty;
            dpDOB.SelectedDate = null;

            tbStreet.Text = string.Empty;
            tbCity.Text = string.Empty;
            tbPostalCode.Text = string.Empty;
            tbPhone.Text = string.Empty;

            ShowView();
        }

        protected void btnAddFamily_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "AddFamilyPage" );
        }

        private void gPeople_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var person = e.Row.DataItem as Person;
                var lLocation = e.Row.FindControl( "lHomeLocation" ) as Literal;
                if ( person != null && lLocation != null )
                {
                    var homeLoc = person.GetHomeLocation( _rockContext );
                    if ( homeLoc != null )
                    {
                        lLocation.Text = homeLoc.GetFullStreetAddress().ConvertCrLfToHtmlBr();
                    }
                }
            }
        }

        private void gPeople_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGrid();
        }

        protected void gPeople_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "SelectPage", new Dictionary<string, string> { { "PersonId", e.RowKeyId.ToString() } } );
        }

        #endregion

        #region Methods

        protected void ShowView()
        {
            lSearchHeading.Text = GetAttributeValue( "SearchCaption" );
            lResultsHeading.Text = GetAttributeValue( "ResultsCaption" );

            btnAddFamily.Visible = false;
            nbOtherMatches.Visible = false;
            pnlResults.Visible = false;
        }

        protected void FindPeople()
        {
            BindGrid();
        }

        protected void BindGrid()
        {
            bool criteriaEntered = false;

            _rockContext = new RockContext();
            var personService = new PersonService( _rockContext );
            var peopleQry = personService.Queryable().AsNoTracking();
            var previousNamesQry = new PersonPreviousNameService( _rockContext ).Queryable().AsNoTracking();

            // FirstName    
            string firstName = tbFirstName.Text.TrimEnd();
            if ( !string.IsNullOrWhiteSpace( firstName ) )
            {
                criteriaEntered = true;
                peopleQry = peopleQry.Where( p =>
                    p.FirstName.StartsWith( firstName ) ||
                    p.NickName.StartsWith( firstName ) );
            }

            // LastName
            string lastName = tbLastName.Text.TrimEnd();
            if ( !string.IsNullOrWhiteSpace( lastName ) )
            {
                criteriaEntered = true;
                peopleQry = peopleQry.Where( p =>
                    p.LastName.StartsWith( lastName ) ||
                    previousNamesQry.Any( a => a.PersonAlias.PersonId == p.Id && a.LastName.StartsWith( lastName ) ) );
            }

            // Date of birth
            var dob = dpDOB.SelectedDate;
            if ( dob.HasValue )
            {
                criteriaEntered = true;

                peopleQry = peopleQry.Where( p =>
                    p.BirthDate == dob.Value );
            }

            // Phone
            Regex phoneRgx = new Regex( @"[^\d]" );
            string phoneDigits = phoneRgx.Replace( tbPhone.Text, "" );
            if ( phoneDigits.Length > 0 )
            {
                criteriaEntered = true;

                var phoneNumberQry = new PhoneNumberService( _rockContext )
                    .Queryable().AsNoTracking()
                    .Where( n => n.Number.Contains( phoneDigits ) )
                    .Select( n => n.PersonId );

                peopleQry = peopleQry.Where( p =>
                    phoneNumberQry.Contains( p.Id ) );
            }

            // Address
            string street = tbStreet.Text.TrimEnd();
            string city = tbCity.Text.TrimEnd();
            string postalCode = tbPostalCode.Text.TrimEnd();

            if ( !string.IsNullOrWhiteSpace( street ) ||
                !string.IsNullOrWhiteSpace( city ) ||
                !string.IsNullOrWhiteSpace( postalCode ) )
            {
                Guid groupTypefamilyGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
                var familyGroupType = GroupTypeCache.Read( groupTypefamilyGuid );
                if ( familyGroupType != null )
                {
                    criteriaEntered = true;

                    var locationQry = new LocationService( _rockContext )
                    .Queryable().AsNoTracking();

                    if ( !string.IsNullOrWhiteSpace( street ) )
                    {
                        locationQry = locationQry.Where( l =>
                            l.Street1.Contains( street ) );
                    }
                    if ( !string.IsNullOrWhiteSpace( city ) )
                    {
                        locationQry = locationQry.Where( l =>
                            l.City.Contains( city ) );
                    }
                    if ( !string.IsNullOrWhiteSpace( postalCode ) )
                    {
                        locationQry = locationQry.Where( l =>
                            l.PostalCode.Contains( postalCode ) );
                    }

                    var personIds = locationQry
                        .SelectMany( l => l.GroupLocations )
                        .Where( gl => gl.Group.GroupTypeId == familyGroupType.Id )
                        .SelectMany( g => g.Group.Members )
                        .Select( m => m.PersonId );

                    peopleQry = peopleQry.Where( p =>
                        personIds.Contains( p.Id ) );
                }
            }

            SortProperty sortProperty = gPeople.SortProperty;
            if ( sortProperty != null )
            {
                peopleQry = peopleQry.Sort( sortProperty );
            }
            else
            {
                peopleQry = peopleQry.OrderBy( p => p.LastName ).ThenBy( p => p.FirstName );
            }

            if ( criteriaEntered )
            {
                var personList = peopleQry.ToList();
                if ( personList.Count == 1 )
                {
                    NavigateToLinkedPage( "SelectPage", new Dictionary<string, string> { { "PersonId", personList.First().Id.ToString() } } );
                }
                else
                {
                    gPeople.DataSource = personList;
                    gPeople.DataBind();

                    var similiarNames = GetSimiliarNames( _rockContext, personService, firstName, lastName, personList.Select( p => p.Id ).ToList() );
                    if ( similiarNames != null && similiarNames.Any() )
                    {
                        var hyperlinks = new List<string>();
                        foreach ( string similiarName in similiarNames )
                        {
                            var names = similiarName.Split( '|' );
                            if ( names.Length == 2 )
                            {
                                var pageRef = CurrentPageReference;
                                pageRef.Parameters["FirstName"] = names[0];
                                pageRef.Parameters["LastName"] = names[1];
                                hyperlinks.Add( string.Format( "<a href='{0}'>{1} {2}</a>", pageRef.BuildUrl(), names[0], names[1] ) );
                            }
                        }
                        string altNames = string.Join( ", ", hyperlinks );
                        nbOtherMatches.Text = string.Format( "Other Possible Matches: {0}", altNames );
                        nbOtherMatches.Visible = true;
                    }
                }

                btnAddFamily.Visible = true;
                pnlResults.Visible = true;
            }

            else
            {
                nbWarning.Text = "Please select at least one criteria before searching.";
                nbWarning.Visible = true;
                btnAddFamily.Visible = false;
                pnlResults.Visible = false;
            }

        }

        private List<string> GetSimiliarNames( RockContext rockContext, PersonService personService, string firstName, string lastName, List<int> excludeIds )
        {
            nbOtherMatches.Visible = false;

            if ( !string.IsNullOrWhiteSpace( firstName ) && !string.IsNullOrWhiteSpace( lastName ) )
            {
                var metaphones = rockContext.Metaphones;

                string ln1 = string.Empty;
                string ln2 = string.Empty;
                Rock.Utility.DoubleMetaphone.doubleMetaphone( lastName, ref ln1, ref ln2 );
                ln1 = ln1 ?? string.Empty;
                ln2 = ln2 ?? string.Empty;

                var lastNames =  metaphones
                    .Where( m =>
                        ( ln1 != "" && ( m.Metaphone1 == ln1 || m.Metaphone2 == ln1 ) ) ||
                        ( ln2 != "" && ( m.Metaphone1 == ln2 || m.Metaphone2 == ln2 ) ) )
                    .Select( m => m.Name )
                    .Distinct()
                    .ToList();

                if ( lastNames.Any() )
                {
                    string fn1 = string.Empty;
                    string fn2 = string.Empty;
                    Rock.Utility.DoubleMetaphone.doubleMetaphone( firstName, ref fn1, ref fn2 );
                    fn1 = fn1 ?? string.Empty;
                    fn2 = fn2 ?? string.Empty;

                    var firstNames = metaphones
                        .Where( m =>
                            ( fn1 != "" && ( m.Metaphone1 == fn1 || m.Metaphone2 == fn1 ) ) ||
                            ( fn2 != "" && ( m.Metaphone1 == fn2 || m.Metaphone2 == fn2 ) ) )
                        .Select( m => m.Name )
                        .Distinct()
                        .ToList();

                    if ( firstNames.Any() )
                    {
                        return personService.Queryable().AsNoTracking()
                        .Where( p => !excludeIds.Contains( p.Id ) &&
                            lastNames.Contains( p.LastName ) &&
                            ( firstNames.Contains( p.FirstName ) || firstNames.Contains( p.NickName ) ) )
                        .OrderBy( p => p.LastName )
                        .ThenBy( p => p.NickName )
                        .Select( p => p.NickName + "|" + p.LastName )
                        .Distinct()
                        .ToList();
                    }
                }
            }

            return null;

        }
        #endregion

    }
}