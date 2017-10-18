using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using org.willowcreek.CareCenter;
using org.willowcreek.CareCenter.Model;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_willowcreek.CareCenter
{
    /// <summary>
    /// Block for starting an assessment.
    /// </summary>
    [DisplayName( "Assesment" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "Block for starting an assessment." )]

    [PersonBadgesField( "Badges", "The label badges to display in this block.", false, "", "", 0 )]
    [BooleanField( "Display Country Code", "When enabled prepends the country code to all phone numbers.", false, "", 1 )]
    [BooleanField( "Display Middle Name", "Display the middle name of the person.", false, "", 2 )]
    [LinkedPage( "Family Edit Page", "Page used to edit the members of the selected family.", true, "", "", 3 )]
    [LinkedPage( "Person Edit Page", "Page used to edit the person.", true, "", "", 4 )]
    [LinkedPage( "Home Page", "The page to return to after intake is completed.", true, "", "", 5 )]
    [LinkedPage( "Person Profile Page", "The page to navigate to if Person Profile option is selected.", true, "", "", 6 )]
    [LinkedPage( "Workflow Entry Page", "The page to return to after assessment is saved (only applies if assessment was started from a workflow).", true, "", "", 6 )]
    public partial class Assesment : PersonBlock
    {
        private int? _visitId;
        private int? _personId;
        private int? _familyId;
        private CampusCache _workingCampus;
        private string _activeTab;
        private Dictionary<string, string> _returnParams = null;

        private Person _person;
        private Group _family;

        #region Base Control Methods

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            _visitId = ViewState["VisitId"] as int?;
            _personId = ViewState["PersonId"] as int?;
            _familyId = ViewState["FamilyId"] as int?;
            _activeTab = ( ViewState["ActiveTab"] as string ) ?? "";

            int? workingCampusId = ViewState["WorkingCampusId"] as int?;
            if ( workingCampusId.HasValue )
            {
                _workingCampus = CampusCache.Read( workingCampusId.Value );
            }

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _visitId = PageParameter( "VisitId" ).AsIntegerOrNull();

            RockPage.AddCSSLink( ResolveRockUrl( "~/Styles/fluidbox.css" ) );
            RockPage.AddScriptLink( ResolveRockUrl( "~/Scripts/imagesloaded.min.js" ) );
            RockPage.AddScriptLink( ResolveRockUrl( "~/Scripts/jquery.fluidbox.min.js" ) );

            // Person object is loaded from context by PersonBlock
            using ( var rockContext = new RockContext() )
            {
                var workflowId = PageParameter( "WorkflowId" ).AsIntegerOrNull();
                if ( workflowId.HasValue )
                {
                    var workflow = new WorkflowService( rockContext ).Get( workflowId.Value );
                    if ( workflow != null )
                    {
                        _returnParams = new Dictionary<string, string>() { { "WorkflowTypeId", workflow.WorkflowTypeId.ToString() }, { "WorkflowId", workflow.Id.ToString() } };

                        if ( !_visitId.HasValue )
                        {
                            var visit = new VisitService( rockContext )
                                .Queryable().AsNoTracking()
                                .Where( v => v.Workflows.Any( w => w.Id == workflowId.Value ) )
                                .FirstOrDefault();
                            if ( visit != null )
                            {
                                if ( visit.PagerId.HasValue && visit.PagerId.Value > 0 )
                                {
                                    lPager.Text = visit.PagerId.Value.ToString();
                                    nbPager.Text = visit.PagerId.Value.ToString();
                                    lPager.Visible = true;
                                    nbPager.Visible = false;
                                }
                                _visitId = visit.Id;
                            }
                        }
                    }
                }
                else
                {
                    if ( _visitId.HasValue && _visitId.Value < 0 )
                    {
                        lPager.Visible = false;
                        nbPager.Visible = false;
                    }
                }

                _person = Person;
                if ( _personId.HasValue && ( _person == null || _person.Id != _personId.Value ) )
                {
                    _person = new PersonService( rockContext ).Get( _personId.Value );
                    _person.LoadAttributes( rockContext );
                }

                if ( _person != null )
                {
                    var families = _person.GetFamilies( rockContext );
                    if ( _familyId.HasValue )
                    {
                        _family = families.FirstOrDefault( f => f.Id == _familyId.Value );
                    }

                    if ( _family == null )
                    {
                        _family = families.OrderByDescending( g => g.IsActive ).FirstOrDefault();
                    }

                    if ( _family != null )
                    {
                        _family.LoadAttributes();
                    }

                    if ( _person.IsDeceased )
                    {
                        divBio.AddCssClass( "deceased" );
                    }

                    string badgeList = GetAttributeValue( "Badges" );
                    if ( !string.IsNullOrWhiteSpace( badgeList ) )
                    {
                        foreach ( string badgeGuid in badgeList.SplitDelimitedValues() )
                        {
                            Guid guid = badgeGuid.AsGuid();
                            if ( guid != Guid.Empty )
                            {
                                var personBadge = PersonBadgeCache.Read( guid, rockContext );
                                if ( personBadge != null )
                                {
                                    blStatus.PersonBadges.Add( personBadge );
                                }
                            }
                        }
                    }
                }
            }

            gBenevolence.DataKeyNames = new string[] { "Id" };
            gBenevolence.RowDataBound += gBenevolence_RowDataBound;
            gBenevolence.GridRebind += gBenevolence_GridRebind;
            gBenevolence.AllowSorting = false;
            gBenevolence.Actions.ShowAdd = false;
            gBenevolence.IsDeleteEnabled = false;

            gTransportation.DataKeyNames = new string[] { "Id" };
            gTransportation.GridRebind += gTransportation_GridRebind;
            gTransportation.AllowSorting = false;
            gTransportation.Actions.ShowAdd = false;
            gTransportation.IsDeleteEnabled = false;

            lbCancelAssessment.Visible =
                _returnParams != null &&
                GetAttributeValue( "WorkflowEntryPage" ).IsNotNullOrWhitespace();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbValidationError.Visible = false;
            nbValidationError2.Visible = false;
            nbInvalidPin.Visible = false;

            if ( !Page.IsPostBack )
            {
                int? tab = PageParameter( "Tab" ).AsIntegerOrNull();
                if ( tab.HasValue )
                {
                    switch ( tab.Value )
                    {
                        case 1:
                            _activeTab = "lbAssessment";
                            break;

                        case 2:
                            _activeTab = "lbBenevolence";
                            break;

                        case 3:
                            _activeTab = "lbTransportation";
                            break;
                    }
                }

                var campusEntityType = EntityTypeCache.Read( typeof( Campus ) );
                var campus = RockPage.GetCurrentContext( campusEntityType ) as Campus;
                if ( campus != null )
                {
                    _workingCampus = CampusCache.Read( campus.Id );
                }

                CheckSecurity();
                LoadDropdowns();
                ShowPerson();
            }
            else
            {
                if ( string.IsNullOrWhiteSpace( hfApprover.Value ) )
                {
                    lApprovedBy.Text = string.Empty;
                    lApprovedBy.Visible = false;
                }
            }

            LoadCampuses();
        }

        protected override void OnPreRender( EventArgs e )
        {
            bool showCarAssessment = cbAutoRepair.Checked || cbAutoRepairWithBenevolence.Checked;
            divAssessmentCars.Attributes["style"] = showCarAssessment ? "display:block" : "display:none";
            divAssessmentReceiveCar.Attributes["style"] = cbReceiveCar.Checked ? "display:block" : "display:none";

            divAssessmentFinancial.Attributes["style"] = cbFinancial.Checked ? "display:block" : "display:none";

            divAssessmentHousing.Attributes["style"] = cbHousingRepair.Checked ? "display:block" : "display:none";

            bool showDentalVisionAssessment = cbDental.Checked || cbVision.Checked;
            divAssessmentDentalVision.Attributes["style"] = showDentalVisionAssessment ? "display:block" : "display:none";
            divAssessmentDental.Attributes["style"] = cbDental.Checked ? "display:block" : "display:none";
            divAssessmentVision.Attributes["style"] = cbVision.Checked ? "display:block" : "display:none";

            base.OnPreRender( e );
        }

        protected override object SaveViewState()
        {
            ViewState["VisitId"] = _visitId;
            ViewState["PersonId"] = _person != null ? _person.Id : (int?)null;
            ViewState["FamilyId"] = _family != null ? _family.Id : (int?)null;
            ViewState["WorkingCampusId"] = _workingCampus != null ? _workingCampus.Id : (int?)null;
            ViewState["ActiveTab"] = _activeTab;

            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ItemCommand event of the rptCampuses control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptCampuses_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var campusId = e.CommandArgument.ToString();

            if ( campusId != null )
            {
                var campus = SetCampusContext( campusId.AsInteger(), true );
                lCurrentSelection.Text = campus.Name;

                _workingCampus = CampusCache.Read( campus.Id );
                SetAssessmentOptions();
            }
        }

        protected void ppPerson_SelectPerson( object sender, EventArgs e )
        {
            int? personId = ppPerson.PersonId;
            if ( personId.HasValue )
            {
                NavigateToCurrentPage( new Dictionary<string, string> { { "PersonId", personId.Value.ToString() } } );
            }
        }

        protected void lbRefreshFamily_Click( object sender, EventArgs e )
        {
            SetFamily();
        }

        protected void rptrMembers_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var groupMember = e.Item.DataItem as GroupMember;
                if ( groupMember != null && groupMember.Person != null )
                {
                    Person fm = groupMember.Person;

                    // very similar code in EditFamily.ascx.cs
                    HtmlControl divPersonImage = e.Item.FindControl( "divPersonImage" ) as HtmlControl;
                    if ( divPersonImage != null )
                    {
                        divPersonImage.Style.Add( "background-image", @String.Format( @"url({0})", Person.GetPersonPhotoUrl( fm ) + "&width=65" ) );
                        divPersonImage.Style.Add( "background-size", "cover" );
                        divPersonImage.Style.Add( "background-position", "50%" );
                    }
                }
            }
        }

        protected void ddlIncomeSource_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetAssessmentOptions();
        }

        protected void cbFamilyIncome_TextChanged( object sender, EventArgs e )
        {
            SetAssessmentOptions();
        }

        protected void ddlChurchAttendance_SelectedIndexChanged( object sender, EventArgs e )
        {
            bool attendsWillow = ShowDuration();
            ddlDurationAttended.Visible = attendsWillow;
            ddlAttendanceFrequency.Visible = attendsWillow;
            SetAssessmentOptions();
        }

        protected void ddlDurationAttended_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetAssessmentOptions();
        }

        protected void ddlAttendanceFrequency_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetAssessmentOptions();
        }

        protected void lbTab_Click( object sender, EventArgs e )
        {
            LinkButton lb = sender as LinkButton;
            if ( lb != null )
            {
                _activeTab = lb.ID;
                ShowTab();
            }
        }

        protected void lbSaveAssessment_Click( object sender, EventArgs e )
        {
            if ( ValidAssessment() )
            {
                if ( !RequiresApproval() )
                {
                    SaveAssessment();
                }
            }
            else
            {
                pnlApproval.Visible = false;
            }
        }

        protected void lbCancelAssessment_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "WorkflowEntryPage", _returnParams );
        }

        protected void lbApproval_Click( object sender, EventArgs e )
        {
            var pinAuth = AuthenticationContainer.GetComponent( typeof( Rock.Security.Authentication.PINAuthentication ).FullName );
            var rockContext = new Rock.Data.RockContext();
            var userLoginService = new UserLoginService( rockContext );
            var userLogin = userLoginService.GetByUserName( nbApprovalPin.Text );

            if ( userLogin != null && userLogin.EntityTypeId.HasValue )
            {
                // make sure this is a PIN auth user login
                var userLoginEntityType = EntityTypeCache.Read( userLogin.EntityTypeId.Value );
                if ( userLoginEntityType != null && userLoginEntityType.Id == pinAuth.EntityType.Id )
                {
                    if ( pinAuth != null && pinAuth.IsActive )
                    {
                        // should always return true, but just in case
                        if ( pinAuth.Authenticate( userLogin, null ) &&
                            ( userLogin.IsConfirmed ?? true ) &&
                            !( userLogin.IsLockedOut ?? false ) )
                        {
                            hfApprover.Value = userLogin.Person.PrimaryAliasId.ToString();
                            lApprovedBy.Text = string.Format( "{0} {1} {2}", userLogin.Person.FullName, RockDateTime.Now.ToShortDateString(), RockDateTime.Now.ToShortTimeString() );
                            lApprovedBy.Visible = true;
                            return;
                        }
                    }
                }
            }

            nbInvalidPin.Visible = true;
        }

        protected void lbClose_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "HomePage" );
        }

        public void gBenevolence_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                BenevolenceRequest benevolenceRequest = e.Row.DataItem as BenevolenceRequest;
                if ( benevolenceRequest != null )
                {
                    Literal lName = e.Row.FindControl( "lName" ) as Literal;
                    if ( lName != null )
                    {
                        if ( benevolenceRequest.RequestedByPersonAlias != null )
                        {
                            lName.Text = benevolenceRequest.RequestedByPersonAlias.Person.FullName ?? string.Empty;
                        }
                        else
                        {
                            lName.Text = string.Format( "{0} {1}", benevolenceRequest.FirstName, benevolenceRequest.LastName );
                        }
                    }

                    Literal lResults = e.Row.FindControl( "lResults" ) as Literal;
                    if ( lResults != null )
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.Append( "<div class='col-md-12'>" );
                        foreach ( BenevolenceResult result in benevolenceRequest.BenevolenceResults )
                        {
                            if ( result.Amount != null )
                            {
                                stringBuilder.Append( string.Format( "<div class='row'>{0} ({1}{2:0.00})</div>", result.ResultTypeValue, GlobalAttributesCache.Value( "CurrencySymbol" ), result.Amount ) );
                            }
                            else
                            {
                                stringBuilder.Append( string.Format( "<div class='row'>{0}</div>", result.ResultTypeValue ) );
                            }
                        }

                        stringBuilder.Append( "</div>" );
                        lResults.Text = stringBuilder.ToString();
                    }

                    HighlightLabel hlStatus = e.Row.FindControl( "hlStatus" ) as HighlightLabel;
                    if ( hlStatus != null )
                    {
                        switch ( benevolenceRequest.RequestStatusValue.Value )
                        {
                            case "Approved":
                                hlStatus.Text = "Approved";
                                hlStatus.LabelType = LabelType.Success;
                                return;
                            case "Denied":
                                hlStatus.Text = "Denied";
                                hlStatus.LabelType = LabelType.Danger;
                                return;
                            case "Pending":
                                hlStatus.Text = "Pending";
                                hlStatus.LabelType = LabelType.Default;
                                return;
                        }
                    }
                }
            }
        }

        private void gBenevolence_GridRebind( object sender, EventArgs e )
        {
            BindBenevolence();
        }

        private void gTransportation_GridRebind( object sender, EventArgs e )
        {
            BindTransportation();
        }

        protected void lbPersonProfile_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "PersonProfilePage", new Dictionary<string, string> { { "PersonId", _person.Id.ToString() } } );
        }

        protected void lbRefreshPerson_Click( object sender, EventArgs e )
        {
            ShowPerson();
        }
        #endregion

        #region Methods

        protected Campus SetCampusContext( int campusId, bool refreshPage = false )
        {
            var campus = new CampusService( new RockContext() ).Get( campusId );
            if ( campus == null )
            {
                // clear the current campus context
                campus = new Campus()
                {
                    Name = GetAttributeValue( "NoCampusText" ),
                    Guid = Guid.Empty
                };
            }

            // set context and refresh below with the correct query string if needed
            RockPage.SetContextCookie( campus, true, false );

            return campus;
        }

        private void LoadCampuses()
        {
            var campusEntityType = EntityTypeCache.Read( typeof( Campus ) );
            var currentCampus = RockPage.GetCurrentContext( campusEntityType ) as Campus;
            if ( currentCampus != null )
            {
                lCurrentSelection.Text = currentCampus.Name;
            }
            else
            {
                lCurrentSelection.Text = "Select Working Campus";
            }
            var campusList = CampusCache.All( true );
            rptCampuses.DataSource = campusList;
            rptCampuses.DataBind();
        }

        private void CheckSecurity()
        {
            var wtGuids = new List<Guid>();
            wtGuids.Add( org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_BIKE.AsGuid() );
            wtGuids.Add( org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_COMPUTER.AsGuid() );
            wtGuids.Add( org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_DENTAL.AsGuid() );
            wtGuids.Add( org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_FINANCIAL.AsGuid() );
            wtGuids.Add( org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_HOUSING_REPAIR.AsGuid() );
            wtGuids.Add( org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_VISION.AsGuid() );
            wtGuids.Add( org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_AUTO_REPAIR.AsGuid() );
            wtGuids.Add( org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_AUTO_REPAIR_WITH_BENEVOLENCE.AsGuid() );
            wtGuids.Add( org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_PURCHASE_CAR.AsGuid() );
            wtGuids.Add( org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_RECEIVE_A_CAR.AsGuid() );
            var workflowTypes = new WorkflowTypeService( new RockContext() )
                .Queryable().AsNoTracking()
                .Where( t => wtGuids.Contains( t.Guid ) )
                .ToList();

            CheckSecurity( workflowTypes, pnlBikeOption, org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_BIKE.AsGuid() );
            CheckSecurity( workflowTypes, pnlComputerOption, org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_COMPUTER.AsGuid() );
            CheckSecurity( workflowTypes, pnlDentalOption, org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_DENTAL.AsGuid() );
            CheckSecurity( workflowTypes, pnlFinancialOption, org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_FINANCIAL.AsGuid() );
            CheckSecurity( workflowTypes, pnlHousingRepairOption, org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_HOUSING_REPAIR.AsGuid() );
            CheckSecurity( workflowTypes, pnlVisionOption, org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_VISION.AsGuid() );
            CheckSecurity( workflowTypes, pnlAutoRepairOption, org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_AUTO_REPAIR.AsGuid() );
            CheckSecurity( workflowTypes, pnlAutoRepairWBenevolenceOption, org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_AUTO_REPAIR_WITH_BENEVOLENCE.AsGuid() );
            CheckSecurity( workflowTypes, pnlPurchaseCarOption, org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_PURCHASE_CAR.AsGuid() );
            CheckSecurity( workflowTypes, pnlReceiveCarOption, org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_RECEIVE_A_CAR.AsGuid() );
        }

        private void CheckSecurity( List<WorkflowType> workflowTypes, Panel pnl, Guid guid )
        {
            var workflowType = workflowTypes.FirstOrDefault( t => t.Guid.Equals( guid ) );
            bool isAuthorized = workflowType != null && workflowType.IsAuthorized( Authorization.VIEW, CurrentPerson );
            pnl.Visible = isAuthorized;
        }

        private void LoadDropdowns()
        {
            ddlIncomeSource.BindToDefinedType( DefinedTypeCache.Read( org.willowcreek.CareCenter.SystemGuid.DefinedType.FAMILY_INCOME_SOURCE.AsGuid() ), true );
            ddlChurchAttendance.BindToDefinedType( DefinedTypeCache.Read( org.willowcreek.CareCenter.SystemGuid.DefinedType.FAMILY_CHURCH_STATUS.AsGuid() ), false );
            ddlDurationAttended.BindToDefinedType( DefinedTypeCache.Read( org.willowcreek.CareCenter.SystemGuid.DefinedType.FAMILY_DURATION_ATTENDED.AsGuid() ), true );
            ddlAttendanceFrequency.BindToDefinedType( DefinedTypeCache.Read( org.willowcreek.CareCenter.SystemGuid.DefinedType.FAMILY_ATTENDANCE_FREQUENCY.AsGuid() ), true );
            ddlInsuranceSituation.BindToDefinedType( DefinedTypeCache.Read( org.willowcreek.CareCenter.SystemGuid.DefinedType.INSURANCE_STATUS.AsGuid() ), true );
        }

        private void ShowPerson()
        {
            if ( _person != null && _person.Id != 0 )
            {
                pnlSelectPerson.Visible = false;
                pnlAssesmentDetails.Visible = true;
                pnlComplete.Visible = false;

                SetPersonName();

                // Setup Image
                string imgTag = Rock.Model.Person.GetPersonPhotoImageTag( _person, 200, 200 );
                if ( _person.PhotoId.HasValue )
                {
                    lImage.Text = string.Format( "<a href='{0}'>{1}</a>", _person.PhotoUrl, imgTag );
                }
                else
                {
                    lImage.Text = imgTag;
                }

                if ( _person.BirthDate.HasValue )
                {
                    lAge.Text = string.Format( "{0}<small>({1})</small><br/>", _person.FormatAge(), ( _person.BirthYear.HasValue && _person.BirthYear != DateTime.MinValue.Year ) ? Person.BirthDate.Value.ToShortDateString() : Person.BirthDate.Value.ToMonthDayString() );
                }

                lGender.Text = _person.Gender.ToString();

                if ( _person.GraduationYear.HasValue && _person.HasGraduated.HasValue )
                {
                    lGraduation.Text = string.Format(
                        "<small>({0} {1})</small>",
                        _person.HasGraduated.Value ? "Graduated " : "Graduates ",
                        _person.GraduationYear.Value );
                }

                lGrade.Text = _person.GradeFormatted;

                lMaritalStatus.Text = _person.MaritalStatusValueId.DefinedValue();
                if ( _person.AnniversaryDate.HasValue )
                {
                    lAnniversary.Text = string.Format( "{0} yrs <small>({1})</small>", _person.AnniversaryDate.Value.Age(), _person.AnniversaryDate.Value.ToMonthDayString() );
                }

                if ( _person.PhoneNumbers != null )
                {
                    rptPhones.DataSource = _person.PhoneNumbers.ToList();
                    rptPhones.DataBind();
                }

                lEmail.Text = _person.GetEmailTag( ResolveRockUrl( "/" ) );

                SetFamily();
            }
            else
            {
                pnlSelectPerson.Visible = true;
                pnlAssesmentDetails.Visible = false;
                pnlComplete.Visible = false;
            }
        }

        private void SetPersonName()
        {
            // Get the Display Name.
            string nameText;

            if ( GetAttributeValue( "DisplayMiddleName" ).AsBoolean() && !String.IsNullOrWhiteSpace( _person.MiddleName ) )
            {
                nameText = string.Format( "<span class='first-word'>{0}</span> <span class='middlename'>{1}</span> <span class='lastname'>{2}</span>", _person.NickName, Person.MiddleName, Person.LastName );
            }
            else
            {
                nameText = string.Format( "<span class='first-word'>{0}</span> <span class='lastname'>{1}</span>", _person.NickName, _person.LastName );
            }


            // Add First Name if different from NickName.
            if ( _person.NickName != _person.FirstName )
            {
                if ( !string.IsNullOrWhiteSpace( _person.FirstName ) )
                {
                    nameText += string.Format( " <span class='firstname'>({0})</span>", _person.FirstName );
                }
            }

            // Add Suffix.
            if ( _person.SuffixValueId.HasValue )
            {
                var suffix = DefinedValueCache.Read( _person.SuffixValueId.Value );
                if ( suffix != null )
                {
                    nameText += " " + suffix.Value;
                }
            }

            // Add Previous Names. 
            using ( var rockContext = new RockContext() )
            {
                var previousNames = _person.GetPreviousNames( rockContext ).Select( a => a.LastName );

                if ( previousNames.Any() )
                {
                    nameText += string.Format( Environment.NewLine + "<span class='previous-names'>(Previous Names: {0})</span>", previousNames.ToList().AsDelimited( ", " ) );
                }
            }

            lName.Text = nameText;
        }

        /// <summary>
        /// Formats the phone number.
        /// </summary>
        /// <param name="unlisted">if set to <c>true</c> [unlisted].</param>
        /// <param name="countryCode">The country code.</param>
        /// <param name="number">The number.</param>
        /// <param name="phoneNumberTypeId">The phone number type identifier.</param>
        /// <returns></returns>
        protected string FormatPhoneNumber( bool unlisted, object countryCode, object number, int phoneNumberTypeId, bool smsEnabled = false )
        {
            string formattedNumber = "Unlisted";

            string cc = countryCode as string ?? string.Empty;
            string n = number as string ?? string.Empty;

            if ( !unlisted )
            {
                if ( GetAttributeValue( "DisplayCountryCode" ).AsBoolean() )
                {
                    formattedNumber = PhoneNumber.FormattedNumber( cc, n, true );
                }
                else
                {
                    formattedNumber = PhoneNumber.FormattedNumber( cc, n );
                }
            }

            // if the page is being loaded locally then add the tel:// link
            if ( RockPage.IsMobileRequest )
            {
                formattedNumber = string.Format( "<a href=\"tel://{0}\">{1}</a>", n, formattedNumber );
            }

            var phoneType = DefinedValueCache.Read( phoneNumberTypeId );
            if ( phoneType != null )
            {
                if ( smsEnabled )
                {
                    formattedNumber = string.Format( "{0} <small>{1} <i class='fa fa-comments'></i></small>", formattedNumber, phoneType.Value );
                }
                else
                {
                    formattedNumber = string.Format( "{0} <small>{1}</small>", formattedNumber, phoneType.Value );
                }
            }

            return formattedNumber;
        }

        private void SetFamily()
        {
            if ( _family != null && _family.Id != 0 )
            {
                var pageParams = new Dictionary<string, string>();
                pageParams.Add( "PersonId", Person.Id.ToString() );
                pageParams.Add( "GroupId", _family.Id.ToString() );
                hlViewPerson.NavigateUrl = LinkedPageUrl( "PersonProfilePage", pageParams );
                hlEditPerson.NavigateUrl = LinkedPageUrl( "PersonEditPage", pageParams );
                hlEditFamily.NavigateUrl = LinkedPageUrl( "FamilyEditPage", pageParams );

                using ( var rockContext = new RockContext() )
                {
                    var members = new GroupMemberService( rockContext ).Queryable( "GroupRole,Person", true )
                        .Where( m =>
                            m.GroupId == _family.Id &&
                            m.PersonId != _person.Id )
                        .OrderBy( m => m.GroupRole.Order )
                        .ToList();

                    var orderedMembers = new List<GroupMember>();

                    // Add adult males
                    orderedMembers.AddRange( members
                        .Where( m => m.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) &&
                            m.Person.Gender == Gender.Male )
                        .OrderByDescending( m => m.Person.Age ) );

                    // Add adult females
                    orderedMembers.AddRange( members
                        .Where( m => m.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) &&
                            m.Person.Gender != Gender.Male )
                        .OrderByDescending( m => m.Person.Age ) );

                    // Add non-adults
                    orderedMembers.AddRange( members
                        .Where( m => !m.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) )
                        .OrderByDescending( m => m.Person.Age ) );

                    rptrMembers.ItemDataBound += rptrMembers_ItemDataBound;
                    rptrMembers.DataSource = orderedMembers;
                    rptrMembers.DataBind();

                    rptrAddresses.DataSource = new GroupLocationService( rockContext ).Queryable( "Location,GroupLocationTypeValue" )
                        .Where( l => l.GroupId == _family.Id )
                        .OrderBy( l => l.GroupLocationTypeValue.Order )
                        .ToList();
                    rptrAddresses.DataBind();
                }

                cbFamilyIncome.Text = GetFamilyAttributeValue( org.willowcreek.CareCenter.SystemGuid.Attribute.FAMILY_HOUSEHOLD_INCOME, false );
                ddlIncomeSource.SetValue( GetFamilyAttributeValue( org.willowcreek.CareCenter.SystemGuid.Attribute.FAMILY_INCOME_SOURCE, true ) );
                ddlChurchAttendance.SetValue( GetFamilyAttributeValue( org.willowcreek.CareCenter.SystemGuid.Attribute.FAMILY_CHURCH_STATUS, true ) );
                ddlDurationAttended.SetValue( GetFamilyAttributeValue( org.willowcreek.CareCenter.SystemGuid.Attribute.FAMILY_DURATION_ATTENDED, true ) );
                ddlAttendanceFrequency.SetValue( GetFamilyAttributeValue( org.willowcreek.CareCenter.SystemGuid.Attribute.FAMILY_ATTENDANCE_FREQUENCY, true ) );
                cbClinicScholarship.Text = GetFamilyAttributeValue( org.willowcreek.CareCenter.SystemGuid.Attribute.FAMILY_CLINIC_SCHOLARSHIP_AMOUNT, false );

                bool attendsWillow = ShowDuration();
                ddlDurationAttended.Visible = attendsWillow;
                ddlAttendanceFrequency.Visible = attendsWillow;

                pnlAssesmentDetails.Visible = true;
                SetAssessmentOptions();

                ShowTab();
            }

        }

        private string GetFamilyAttributeValue( string attributeGuid, bool isDefinedValue )
        {
            var attr = AttributeCache.Read( attributeGuid.AsGuid() );
            if ( attr != null )
            {
                string value = _family.GetAttributeValue( attr.Key );
                if ( !isDefinedValue )
                {
                    return value;
                }

                var dv = DefinedValueCache.Read( value.AsGuid() );
                if ( dv != null )
                {
                    return dv.Id.ToString();
                }
            }

            return string.Empty;
        }

        private bool ShowDuration()
        {
            int? dvId = ddlChurchAttendance.SelectedValueAsInt();
            if ( dvId.HasValue )
            {
                var dv = DefinedValueCache.Read( dvId.Value );
                if ( dv != null && dv.Guid == org.willowcreek.CareCenter.SystemGuid.DefinedValue.CHURCHSTATUS_ATTENDS_WILLOW.AsGuid() )
                {
                    return true;
                }
            }

            return false;
        }

        private void ShowTab()
        {
            liAssessment.RemoveCssClass( "active" );
            pnlAssessment.Visible = false;

            liBenevolence.RemoveCssClass( "active" );
            pnlBenevolence.Visible = false;

            liTransportation.RemoveCssClass( "active" );
            pnlTransportation.Visible = false;

            switch ( _activeTab ?? string.Empty )
            {
                case "lbAssessment":
                    {
                        liAssessment.AddCssClass( "active" );
                        pnlAssessment.Visible = true;
                        break;
                    }

                case "lbBenevolence":
                    {
                        liBenevolence.AddCssClass( "active" );
                        pnlBenevolence.Visible = true;
                        BindBenevolence();
                        break;
                    }

                case "lbTransportation":
                    {
                        liTransportation.AddCssClass( "active" );
                        pnlTransportation.Visible = true;
                        BindTransportation();
                        break;
                    }

                default:
                    {
                        liAssessment.AddCssClass( "active" );
                        pnlAssessment.Visible = true;
                        break;
                    }
            }
        }

        private void SetAssessmentOptions()
        {
            // Get information about person/family
            bool attendsWillow = DefinedValueSelected( ddlChurchAttendance, org.willowcreek.CareCenter.SystemGuid.DefinedValue.CHURCHSTATUS_ATTENDS_WILLOW );
            bool attendsFourYrs = attendsWillow && DefinedValueSelected( ddlDurationAttended, org.willowcreek.CareCenter.SystemGuid.DefinedValue.ATTENDED_DURATION_FOUR_PLUS_YRS );
            bool attendsOneYr = attendsWillow && ( attendsFourYrs || DefinedValueSelected( ddlDurationAttended, org.willowcreek.CareCenter.SystemGuid.DefinedValue.ATTENDED_DURATION_ONE_THREE_YRS ) );
            bool attendsSixMos = attendsWillow && ( attendsOneYr || DefinedValueSelected( ddlDurationAttended, org.willowcreek.CareCenter.SystemGuid.DefinedValue.ATTENDED_DURATION_SIX_ELEVEN_MOS ) );
            bool attendsRegularly = ddlAttendanceFrequency.SelectedValue.IsNotNullOrWhitespace() && !DefinedValueSelected( ddlAttendanceFrequency, org.willowcreek.CareCenter.SystemGuid.DefinedValue.FAMILY_ATTENDANCE_FREQUENCY_OCCASIONALLY );
            bool unemployed = DefinedValueSelected( ddlIncomeSource, org.willowcreek.CareCenter.SystemGuid.DefinedValue.INCOMESOURCE_UNEMPLOYED );

            IncomeStatus incomeStatus = _person.GetIncomeStatus( cbFamilyIncome.Text.AsDecimalOrNull(), attendsWillow );

            var autoRepairStatus = _person.GetAutoRepairStatus();
            bool maxAutoRepairs = attendsWillow ? autoRepairStatus.AutoRepairsLastYear >= 2 : autoRepairStatus.AutoRepairsLastYear >= 1;

            bool sameCampus = false;
            if ( attendsWillow )
            {
                var personCampus = _person.GetCampus();
                if ( _workingCampus != null && personCampus != null )
                {
                    sameCampus = _workingCampus.Id == personCampus.Id;
                    if ( !sameCampus )
                    {
                        var similiarCampuses = new List<string> { "CDL", "SBR" };
                        sameCampus = ( similiarCampuses.Contains( _workingCampus.ShortCode ) && similiarCampuses.Contains( personCampus.ShortCode ) );
                    }
                }
            }

            DateTime? bikeDate = null;
            var bikeAttribute = AttributeCache.Read( org.willowcreek.CareCenter.SystemGuid.Attribute.PERSON_BIKE_RECEPTION.AsGuid() );
            if ( bikeAttribute != null )
            {
                bikeDate = _person.GetAttributeValue( bikeAttribute.Key ).AsDateTime();
            }

            DateTime? computerDate = null;
            var computerAttribute = AttributeCache.Read( org.willowcreek.CareCenter.SystemGuid.Attribute.PERSON_COMPUTER_RECEPTION.AsGuid() );
            if ( computerAttribute != null )
            {
                computerDate = _person.GetAttributeValue( computerAttribute.Key ).AsDateTime();
            }

            bool isAdult = _person.Age.HasValue && Person.Age.Value >= 18;

            var reasons = new List<string>();

            // Validate Bike
            if ( pnlBikeOption.Visible )
            {
                var msgs = new List<string>();
                AddMessage( incomeStatus != IncomeStatus.Meets, msgs, "family does not meet the income level requirements" );
                if ( bikeDate.HasValue )
                {
                    AddMessage( true, msgs, string.Format( "person already received a bike on {0}", bikeDate.Value.ToShortDateString() ) );
                }
                CheckMessages( msgs, reasons, cbBike, "Bike" );

            }

            // Validate Computer
            if ( pnlComputerOption.Visible )
            {
                var msgs = new List<string>();
                AddMessage( incomeStatus != IncomeStatus.Meets, msgs, "family does not meet the income level requirements" );
                if ( computerDate.HasValue )
                {
                    AddMessage( true, msgs, string.Format( "person already received a computer on {0}", computerDate.Value.ToShortDateString() ) );
                }
                CheckMessages( msgs, reasons, cbComputer, "Computer" );
            }

            // Validate Dental
            if ( pnlDentalOption.Visible )
            {
                var msgs = new List<string>();
                AddMessage( incomeStatus != IncomeStatus.Meets, msgs, "family does not meet the income level requirements" );
                AddMessage( !isAdult, msgs, "person is not an adult" );
                CheckMessages( msgs, reasons, cbDental, "Dental" );
            }

            // Validate Benevolence
            if ( pnlFinancialOption.Visible )
            {
                var msgs = new List<string>();
                AddMessage( incomeStatus != IncomeStatus.Meets, msgs, "family does not meet the income level requirements" );
                AddMessage( !attendsSixMos, msgs, "family has not been attending Willow for at least six months" );
                AddMessage( !sameCampus, msgs, "family attends a different campus" );
                AddMessage( !attendsRegularly, msgs, "family does not attend regularly" );
                CheckMessages( msgs, reasons, cbFinancial, "Benevolence" );
            }

            // Validate Housing Repair
            if ( pnlHousingRepairOption.Visible )
            {
                var msgs = new List<string>();
                AddMessage( incomeStatus != IncomeStatus.Meets, msgs, "family does not meet the income level requirements" );
                CheckMessages( msgs, reasons, cbHousingRepair, "Housing Repair" );
            }

            // Validate Vision
            if ( pnlVisionOption.Visible )
            {
                var msgs = new List<string>();
                AddMessage( incomeStatus != IncomeStatus.Meets, msgs, "family does not meet the income level requirements" );
                AddMessage( !isAdult, msgs, "person is not an adult" );
                CheckMessages( msgs, reasons, cbVision, "Vision" );
            }

            // Validate Auto Repair
            if ( pnlAutoRepairOption.Visible )
            {
                var msgs = new List<string>();
                AddMessage( incomeStatus != IncomeStatus.Meets, msgs, "family does not meet the income level requirements" );
                AddMessage( maxAutoRepairs, msgs, "family has already had the maximum number of repairs in the last year (2 for Willow attendees, 1 for non-attendees)." );
                CheckMessages( msgs, reasons, cbAutoRepair, "Auto Repair" );
            }

            // Validate Auto Repair with Benevolence
            if ( pnlAutoRepairWBenevolenceOption.Visible )
            {
                var msgs = new List<string>();
                AddMessage( incomeStatus != IncomeStatus.Meets, msgs, "family does not meet the income level requirements" );
                AddMessage( !attendsSixMos, msgs, "family has not been attending Willow for at least six months" );
                AddMessage( !sameCampus, msgs, "family attends a different campus" );
                AddMessage( !attendsRegularly, msgs, "family does not attend regularly" );
                AddMessage( maxAutoRepairs, msgs, "family has already had the maximum number of repairs in the last year (2 for Willow attendees, 1 for non-attendees)." );
                CheckMessages( msgs, reasons, cbAutoRepairWithBenevolence, "Auto Repair with Benevolence" );
            }

            // Validate Purchase a Car
            if ( pnlPurchaseCarOption.Visible )
            {
                var msgs = new List<string>();
                AddMessage( incomeStatus != IncomeStatus.Meets, msgs, "family does not meet the income level requirements" );
                CheckMessages( msgs, reasons, cbPurchaseCar, "Purchase a Car" );
            }

            // Validate Receive a Car
            if ( pnlReceiveCarOption.Visible )
            {
                var msgs = new List<string>();
                AddMessage( incomeStatus != IncomeStatus.Meets, msgs, "family does not meet the income level requirements" );
                AddMessage( !attendsOneYr, msgs, "family has not been attending Willow for at least a year" );
                AddMessage( !sameCampus, msgs, "family attends a different campus" );
                AddMessage( unemployed, msgs, "person is unemployed" );
                if ( autoRepairStatus.ReceivedAutoDate.HasValue )
                {
                    AddMessage( true, msgs, string.Format( "person already received a car on {0}", autoRepairStatus.ReceivedAutoDate.Value.ToShortDateString() ) );
                }
                CheckMessages( msgs, reasons, cbReceiveCar, "Receive a Car" );
            }

            if ( reasons.Any() )
            {
                nbUnavailableAssessments.Text = string.Format( "<p>Note: One or more of the assessments are not available for the following reasons:</p><ul><li>{0}</li></ul>", reasons.AsDelimited( "</li><li>" ) );
                nbUnavailableAssessments.Visible = true;
            }
            else
            {
                nbUnavailableAssessments.Visible = false;
            }

            if ( !cbAutoRepair.Enabled ) { cbAutoRepair.Checked = false; }
            if ( !cbAutoRepairWithBenevolence.Enabled ) { cbAutoRepairWithBenevolence.Checked = false; }
            if ( !cbFinancial.Enabled ) { cbFinancial.Checked = false; }
            if ( !cbHousingRepair.Enabled ) { cbHousingRepair.Checked = false; }
            if ( !cbBike.Enabled ) { cbBike.Checked = false; }
            if ( !cbComputer.Enabled ) { cbComputer.Checked = false; }
            if ( !cbDental.Enabled ) { cbDental.Checked = false; }
            if ( !cbVision.Enabled) { cbVision.Checked = false; }
            if ( !cbReceiveCar.Enabled ) { cbReceiveCar.Checked = false; }
            if ( !cbPurchaseCar.Enabled ) { cbPurchaseCar.Checked = false; }
        }

        private void AddMessage( bool condition, List<string> messages, string message )
        {
            if ( condition && message.IsNotNullOrWhitespace() )
            {
                messages.Add( message );
            }
        }

        private void CheckMessages( List<string> messages, List<string> reasons, RockCheckBox cb, string area )
        {
            if ( messages.Any() )
            {
                cb.Enabled = false;

                var reason = new StringBuilder();
                reason.AppendFormat( "The <em>{0}</em> assessment is not available because ", area );
                if ( messages.Count > 1 )
                {
                    reason.Append( messages.Take( messages.Count - 1 ).ToList().AsDelimited( ", " ) );
                    reason.Append( ", and " );
                    reason.Append( messages.Last() );
                }
                else
                {
                    reason.Append( messages.First() );
                }

                reason.Append( "." );
                reasons.Add( reason.ToString() );
            }
            else
            {
                cb.Enabled = true;
            }
        }

        private void BindBenevolence()
        {
            phBenevolenceSummary.Controls.Clear();

            gBenevolence.Visible = true;

            var rockContext = new RockContext();
            var qryFamilyMembers = _person.GetFamilyMembers( true, rockContext );
            var qry = new BenevolenceRequestService( rockContext )
                .Queryable( "BenevolenceResults,RequestedByPersonAlias,RequestedByPersonAlias.Person,CaseWorkerPersonAlias,CaseWorkerPersonAlias.Person" )
                .AsNoTracking()
                .Where( a => a.RequestedByPersonAliasId.HasValue && qryFamilyMembers.Any( b => b.PersonId == a.RequestedByPersonAlias.PersonId ) )
                .OrderByDescending( a => a.RequestDateTime ).ThenByDescending( a => a.Id );

            var list = qry.ToList();

            gBenevolence.DataSource = list;
            gBenevolence.DataBind();

            // Builds the Totals section
            var definedTypeCache = DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.BENEVOLENCE_RESULT_TYPE ) );
            Dictionary<string, decimal> resultTotals = new Dictionary<string, decimal>();
            decimal grandTotal = 0;
            foreach ( BenevolenceRequest request in list )
            {
                foreach ( BenevolenceResult result in request.BenevolenceResults )
                {
                    if ( result.Amount != null )
                    {
                        if ( resultTotals.ContainsKey( result.ResultTypeValue.Value ) )
                        {
                            resultTotals[result.ResultTypeValue.Value] += result.Amount.Value;
                        }
                        else
                        {
                            resultTotals.Add( result.ResultTypeValue.Value, result.Amount.Value );
                        }

                        grandTotal += result.Amount.Value;
                    }
                }
            }

            foreach ( KeyValuePair<string, decimal> keyValuePair in resultTotals )
            {
                phBenevolenceSummary.Controls.Add( new LiteralControl( string.Format( "<div class='row'><div class='col-xs-8'>{0}: </div><div class='col-xs-4 text-right'>{1}{2:#,##0.00}</div></div>", keyValuePair.Key, GlobalAttributesCache.Value( "CurrencySymbol" ), keyValuePair.Value ) ) );
            }

            phBenevolenceSummary.Controls.Add( new LiteralControl( string.Format( "<div class='row'><div class='col-xs-8'><b>Total: </div><div class='col-xs-4 text-right'>{0}{1:#,##0.00}</b></div></div>", GlobalAttributesCache.Value( "CurrencySymbol" ), grandTotal ) ) );
        }

        private void BindTransportation()
        {
            var transportationWorkflowGuids = new List<Guid>();
            transportationWorkflowGuids.Add( org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_AUTO_REPAIR.AsGuid() );
            //transportationWorkflowGuids.Add( org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_AUTO_REPAIR_APPT.AsGuid() );
            transportationWorkflowGuids.Add( org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_AUTO_REPAIR_WITH_BENEVOLENCE.AsGuid() );
            transportationWorkflowGuids.Add( org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_PURCHASE_CAR.AsGuid() );
            transportationWorkflowGuids.Add( org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_RECEIVE_A_CAR.AsGuid() );

            var rockContext = new RockContext();
            var qryFamilyMembers = _person.GetFamilyMembers( true, rockContext );

            var qryFamilyAssessments = new AssessmentService( rockContext )
                .Queryable().AsNoTracking()
                .Where( a =>
                    qryFamilyMembers.Any( b => b.PersonId == a.PersonAlias.PersonId ) &&
                    a.Workflows.Any( w => transportationWorkflowGuids.Contains( w.WorkflowType.Guid ) ) );

            var workflows = qryFamilyAssessments
                .SelectMany( a => a.Workflows.Where( w => transportationWorkflowGuids.Contains( w.WorkflowType.Guid ) ) )
                .Select( w => new
                {
                    w.Id,
                    w.Guid,
                    w.CreatedDateTime,
                    Type = w.WorkflowType.Name,
                    w.Status,
                    w.CompletedDateTime,
                    Assessment = qryFamilyAssessments.Where( a => a.Workflows.Any( aw => aw.Id == w.Id ) ).FirstOrDefault()
                } )
                .ToList();

            gTransportation.DataSource = workflows;
            gTransportation.DataBind();
        }

        private bool DefinedValueSelected( DropDownList ddl, string systemGuid )
        {
            var definedValue = DefinedValueCache.Read( systemGuid.AsGuid() );
            if ( definedValue != null )
            {
                int? selectedId = ddl.SelectedValueAsInt();
                return ( selectedId.HasValue && selectedId.Value == definedValue.Id );
            }
            return false;
        }

        /// <summary>
        /// Formats the type of the address.
        /// </summary>
        /// <param name="addressType">Type of the address.</param>
        /// <returns></returns>
        protected string FormatAddressType( object addressType )
        {
            string type = addressType != null ? addressType.ToString() : "Unknown";
            return type.EndsWith( "Address", StringComparison.CurrentCultureIgnoreCase ) ? type : type + " Address";
        }

        /// <summary>
        /// Formats the person link...keeping the subpage route on the person link.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns>a link to the profile page of the given person</returns>
        protected string FormatPersonLink( string personId )
        {
            return ResolveRockUrl( string.Format( "~/Assessment/{0}", personId ) );
        }

        /// <summary>
        /// Formats the name of the person.
        /// </summary>
        /// <param name="nickName">Name of the nick.</param>
        /// <param name="lastName">The last name.</param>
        /// <returns></returns>
        protected string FormatPersonName( string nickName, string lastName )
        {
            if ( Person != null && Person.LastName != lastName )
            {
                return string.Format( "{0} {1}", nickName, lastName );
            }

            return nickName;
        }

        /// <summary>
        /// Formats the person CSS class.
        /// </summary>
        /// <param name="isDeceased">The is deceased.</param>
        /// <returns></returns>
        protected string FormatPersonCssClass( bool isDeceased )
        {
            return isDeceased ? "member deceased" : "member";
        }

        /// <summary>
        /// Formats the details.
        /// </summary>
        /// <param name="dataitem">The dataitem.</param>
        /// <returns></returns>
        protected string FormatPersonDetails( object dataitem )
        {
            var gm = dataitem as GroupMember;
            if ( gm != null )
            {
                return gm.Person.FormatAge( true );
            }
            return string.Empty;
        }

        private bool ValidAssessment()
        {
            var errorMessages = new List<String>();
            if ( _person == null || _family == null )
            {
                errorMessages.Add( "Could not determine the current person and/or family. Please refresh this page and start over." );
            }
            else
            {
                if ( string.IsNullOrWhiteSpace( cbFamilyIncome.Text ) )
                {
                    errorMessages.Add( "A Family Monthly Income amount is required. Enter 0 if family has no income." );
                }
                var dv = DefinedValueCache.Read( ddlChurchAttendance.SelectedValueAsInt() ?? 0 );
                if ( dv == null )
                {
                    errorMessages.Add( "Church Attendance is required." );
                }
                else
                {
                    if ( dv.Guid == org.willowcreek.CareCenter.SystemGuid.DefinedValue.CHURCHSTATUS_ATTENDS_WILLOW.AsGuid() )
                    {
                        int? duration = ddlDurationAttended.SelectedValueAsInt();
                        if ( !duration.HasValue )
                        {
                            errorMessages.Add( "If family attends Willow, the Duration Attended is required." );
                        }
                        int? attendanceFrequency = ddlAttendanceFrequency.SelectedValueAsInt();
                        if ( !duration.HasValue )
                        {
                            errorMessages.Add( "If family attends Willow, the Attendance Frequency is required." );
                        }
                    }
                }

                bool optionSelected = false;

                if ( cbFinancial.Checked )
                {
                    optionSelected = true;

                    if ( string.IsNullOrWhiteSpace( tbNatureFinancialRequest.Text ) )
                    {
                        errorMessages.Add( "Benevolence Request Description is required." );
                    }
                }

                if ( cbHousingRepair.Checked )
                {
                    optionSelected = true;

                    if ( string.IsNullOrWhiteSpace( rblHousingStatus.SelectedValue ) )
                    {
                        errorMessages.Add( "Housing Status option is required." );
                    }
                    else
                    {
                        if ( rblHousingStatus.SelectedValue == "Rent")
                        {
                            errorMessages.Add( "Housing Repair is not available when renting." );
                        }
                    }

                    bool? planOnSelling = rblPlanOnSelling.SelectedValue.AsBooleanOrNull();
                    if ( !planOnSelling.HasValue )
                    {
                        errorMessages.Add( "Do You Plan on Selling Your Home option required." );
                    }
                    else
                    {
                        if ( planOnSelling.Value )
                        {
                            errorMessages.Add( "Housing Repair is not available if planning to sell home." );
                        }
                    }

                    bool? mortgageCurrent = rblMortgageCurrent.SelectedValue.AsBooleanOrNull();
                    if ( !mortgageCurrent.HasValue )
                    {
                        errorMessages.Add( "Current With Mortgage/Rent option required." );
                    }
                    else
                    {
                        if ( !mortgageCurrent.Value && string.IsNullOrWhiteSpace( nbMonthsBehind.Text ) )
                        {
                            errorMessages.Add( "Number of Months Behind is required when Mortgage/Rent is not current." );
                        }
                    }

                    bool? inForeclosure = rblInForeclosure.SelectedValue.AsBooleanOrNull();
                    if ( !inForeclosure.HasValue )
                    {
                        errorMessages.Add( "In Foreclosure option is required." );
                    }
                    else
                    {
                        if ( inForeclosure.Value && string.IsNullOrWhiteSpace( cbHomeAmountOwed.Text ) )
                        {
                            errorMessages.Add( "Amount Owed is required when In Foreclosure." );
                        }
                    }

                    bool? utilitiesCurrent = rblCurrentWithUtilities.SelectedValue.AsBooleanOrNull();
                    if ( !utilitiesCurrent.HasValue )
                    {
                        errorMessages.Add( "Current With Utilities option is required." );
                    }
                    else
                    {
                        if ( !utilitiesCurrent.Value && string.IsNullOrWhiteSpace( cbUtilitiesAmountOwed.Text ) )
                        {
                            errorMessages.Add( "Amount Owed is required when Utilities are not current." );
                        }
                    }
                }

                if ( cbDental.Checked || cbVision.Checked )
                {
                    optionSelected = true;
                    if ( !ddlInsuranceSituation.SelectedValueAsInt().HasValue )
                    {
                        errorMessages.Add( "Insurance Situation is required." );
                    }
                    else
                    {
                        bool noInsurance = DefinedValueSelected( ddlInsuranceSituation, org.willowcreek.CareCenter.SystemGuid.DefinedValue.INSURANCE_NONE );
                        noInsurance = noInsurance || DefinedValueSelected( ddlInsuranceSituation, org.willowcreek.CareCenter.SystemGuid.DefinedValue.INSURANCE_MEDICAID );

                        if ( cbDental.Checked )
                        {
                            if ( !noInsurance )
                            {
                                errorMessages.Add( "Dental is only allowed if person has Medicaid or no insurance." );
                            }
                            else
                            {
                                if ( string.IsNullOrWhiteSpace( tbDentalConerns.Text ) )
                                {
                                    errorMessages.Add( "Dental Concerns is required." );
                                }
                            }
                        }

                        if ( cbVision.Checked )
                        {
                            if ( !noInsurance )
                            {
                                errorMessages.Add( "Vision is only allowed if person has Medicaid or no insurance." );
                            }
                            else
                            {
                                if ( string.IsNullOrWhiteSpace( tbVisionConcerns.Text ) )
                                {
                                    errorMessages.Add( "Vision Concerns is required." );
                                }
                            }
                        }
                    }
                }

                if ( cbAutoRepair.Checked || cbAutoRepairWithBenevolence.Checked )
                {
                    optionSelected = true;

                    bool? validLicense = rblValidDriversLicense.SelectedValue.AsBooleanOrNull();
                    if ( !validLicense.HasValue )
                    {
                        errorMessages.Add( "Valid Driver's License option is required." );
                    }

                    bool? validRegistration = rblValidRegistration.SelectedValue.AsBooleanOrNull();
                    if ( !validLicense.HasValue )
                    {
                        errorMessages.Add( "Valid Registration option is required." );
                    }

                    if ( tbAutoMake.Text.IsNullOrWhiteSpace() )
                    {
                        errorMessages.Add( "Auto Make is required." );
                    }
                    if ( tbAutoModel.Text.IsNullOrWhiteSpace() )
                    {
                        errorMessages.Add( "Auto Model is required." );
                    }
                    if ( ypAutoYear.Text.IsNullOrWhiteSpace() )
                    {
                        errorMessages.Add( "Auto Year is required." );
                    }
                    if ( tbAutoMiles.Text.IsNullOrWhiteSpace() )
                    {
                        errorMessages.Add( "Auto Miles is required." );
                    }

                    if ( tbTransportationConcerns.Text.IsNullOrWhiteSpace() )
                    {
                        errorMessages.Add( "Transportation Concerns is required." );
                    }

                    if ( cbAutoRepairWithBenevolence.Checked && !cbFinancial.Checked )
                    {
                        errorMessages.Add( "The Auto Repair with Benevolence Assessment also requires the Benevolence assessment." );
                    }
                }

                if ( cbReceiveCar.Checked )
                {
                    optionSelected = true;

                    bool? validLicense = rblValidDriversLicense2.SelectedValue.AsBooleanOrNull();
                    if ( !validLicense.HasValue )
                    {
                        errorMessages.Add( "Valid Driver's License option is required." );
                    }

                    if ( rblCurrentlyVolunteering.SelectedValue.AsBoolean() && tbServingArea.Text.IsNullOrWhiteSpace() )
                    {
                        errorMessages.Add( "If currently volunteering, the Service Area is required." );
                    }

                    if ( tbTransportationConcerns2.Text.IsNullOrWhiteSpace() )
                    {
                        errorMessages.Add( "Transportation Concerns is required." );
                    }

                    if ( !cbFinancial.Checked )
                    {
                        errorMessages.Add( "The Receive a Car Assessment also requires the Benevolence assessment." );
                    }

                }

                if ( cbBike.Checked || cbComputer.Checked || cbPurchaseCar.Checked )
                {
                    optionSelected = true;
                }

                if ( !optionSelected )
                {
                    errorMessages.Add( "One or more Assessment Points needs to be selected." );
                }
            }

            if ( errorMessages.Any() )
            {
                nbValidationError.Text = string.Format( "Please Correct the Following<ul><li>{0}</li></ul>", errorMessages.AsDelimited( "</li><li>" ) );
                nbValidationError.Visible = true;
                nbValidationError2.Text = nbValidationError.Text;
                nbValidationError2.Visible = true;
                return false;
            }

            return true;

        }

        public bool RequiresApproval()
        {
            var needLeaderApproval = new List<string>();
            if ( cbAutoRepairWithBenevolence.Checked ) { needLeaderApproval.Add( "Auto Repair With Benevolence" ); }
            if ( cbFinancial.Checked ) { needLeaderApproval.Add( "Benevolence" ); }
            if ( cbHousingRepair.Checked ) { needLeaderApproval.Add( "Housing Repair" ); }
            if ( cbReceiveCar.Checked ) { needLeaderApproval.Add( "Receive a Car" ); }
            if ( cbPurchaseCar.Checked ) { needLeaderApproval.Add( "Purchase a Car" ); }

            pnlApproval.Visible = needLeaderApproval.Any();

            if ( !needLeaderApproval.Any() )
            {
                return false;
            }

            nbApprovalRequired.Text = "Leader approval is required for " + needLeaderApproval.AsDelimited( " and " ) + ".";

            return needLeaderApproval.Any() && string.IsNullOrWhiteSpace( hfApprover.Value );
        }

        public void SaveAssessment()
        {
            if ( _person != null && _family != null )
            {
                using ( var rockContext = new RockContext() )
                {

                    SetFamilyAttributeValue( org.willowcreek.CareCenter.SystemGuid.Attribute.FAMILY_HOUSEHOLD_INCOME, false, cbFamilyIncome.Text );
                    SetFamilyAttributeValue( org.willowcreek.CareCenter.SystemGuid.Attribute.FAMILY_INCOME_SOURCE, true, ddlIncomeSource.SelectedValue );
                    SetFamilyAttributeValue( org.willowcreek.CareCenter.SystemGuid.Attribute.FAMILY_CHURCH_STATUS, true, ddlChurchAttendance.SelectedValue );
                    SetFamilyAttributeValue( org.willowcreek.CareCenter.SystemGuid.Attribute.FAMILY_DURATION_ATTENDED, true, ddlDurationAttended.SelectedValue );
                    SetFamilyAttributeValue( org.willowcreek.CareCenter.SystemGuid.Attribute.FAMILY_ATTENDANCE_FREQUENCY, true, ddlAttendanceFrequency.SelectedValue );

                    var assessmentService = new AssessmentService( rockContext );
                    var assessment = new Assessment();
                    Workflow visitWorkflow = null;
                    if ( _visitId.HasValue )
                    {
                        if ( _visitId.Value > 0 )
                        {
                            assessment.VisitId = _visitId;
                        }
                    }
                    else
                    {
                        var visit = new Visit();
                        visit.VisitDate = RockDateTime.Now;
                        visit.PassportStatus = PassportStatus.NotPrinted;
                        visit.PersonAliasId = _person.PrimaryAliasId.Value;
                        visit.Status = VisitStatus.Active;
                        visit.CancelReasonValueId = null;
                        visit.PhotoIdValidated = false;
                        visit.ProofOfAddressValidated = false;
                        visit.IsHomeless = false;
                        visit.PagerId = nbPager.Text.AsIntegerOrNull();

                        var serviceArea = new ServiceAreaService( rockContext ).Get( org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_CARE_TEAM.AsGuid() );
                        if ( serviceArea != null && serviceArea.WorkflowType != null )
                        {
                            visitWorkflow = ActivateAndAddWorkflow( rockContext, serviceArea.WorkflowType.Guid.ToString(),
                            _person, _family, null, null );
                            visitWorkflow.Status = "Waiting";
                            visit.Workflows.Add( visitWorkflow );
                        }

                        assessment.Visit = visit;
                    }
                    assessment.AssessmentDate = RockDateTime.Now;
                    assessment.PersonAliasId = _person.PrimaryAliasId.Value;
                    assessmentService.Add( assessment );

                    var workflowTypeService = new WorkflowTypeService( rockContext );
                    var workflowTypes = workflowTypeService.Queryable().AsNoTracking().ToList();

                    var workflows = new List<Workflow>();
                    var selectedWorkflowTypes = new List<string>();

                    if ( cbAutoRepair.Checked )
                    {
                        var workflow = ActivateAndAddWorkflow( rockContext,
                            org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_AUTO_REPAIR,
                            _person, _family, workflows, selectedWorkflowTypes );

                        workflow.SetAttributeValue( "ValidDriversLicense", rblValidDriversLicense.SelectedValue );
                        workflow.SetAttributeValue( "ValidRegistration", rblValidRegistration.SelectedValue );
                        workflow.SetAttributeValue( "Make", tbAutoMake.Text );
                        workflow.SetAttributeValue( "Model", tbAutoModel.Text );
                        workflow.SetAttributeValue( "Year", ypAutoYear.Text );
                        workflow.SetAttributeValue( "Miles", tbAutoMiles.Text.AsNumeric() );
                        workflow.SetAttributeValue( "TransportationConcerns", tbTransportationConcerns.Text );
                    }

                    if ( cbAutoRepairWithBenevolence.Checked )
                    {
                        var workflow = ActivateAndAddWorkflow( rockContext,
                            org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_AUTO_REPAIR_WITH_BENEVOLENCE,
                            _person, _family, workflows, selectedWorkflowTypes );

                        workflow.SetAttributeValue( "ValidDriversLicense", rblValidDriversLicense.SelectedValue );
                        workflow.SetAttributeValue( "ValidRegistration", rblValidRegistration.SelectedValue );
                        workflow.SetAttributeValue( "Make", tbAutoMake.Text );
                        workflow.SetAttributeValue( "Model", tbAutoModel.Text );
                        workflow.SetAttributeValue( "Year", ypAutoYear.Text );
                        workflow.SetAttributeValue( "Miles", tbAutoMiles.Text.AsNumeric() );
                        workflow.SetAttributeValue( "TransportationConcerns", tbTransportationConcerns.Text );
                    }

                    if ( cbFinancial.Checked )
                    {
                        var workflow = ActivateAndAddWorkflow( rockContext,
                            org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_FINANCIAL,
                            _person, _family, workflows, selectedWorkflowTypes );

                        workflow.SetAttributeValue( "NatureOfRequest", tbNatureFinancialRequest.Text );
                    }

                    if ( cbHousingRepair.Checked )
                    {
                        var workflow = ActivateAndAddWorkflow( rockContext,
                            org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_HOUSING_REPAIR,
                            _person, _family, workflows, selectedWorkflowTypes );

                        workflow.SetAttributeValue( "HousingStatus", rblHousingStatus.SelectedValue );
                        workflow.SetAttributeValue( "PlanOnSelling", rblPlanOnSelling.SelectedValue );
                        workflow.SetAttributeValue( "MortgageCurrent", rblMortgageCurrent.SelectedValue );
                        workflow.SetAttributeValue( "MonthsBehind", nbMonthsBehind.Text );
                        workflow.SetAttributeValue( "InForeclosure", rblInForeclosure.SelectedValue );
                        workflow.SetAttributeValue( "AmountOwed", cbHomeAmountOwed.Text );
                        workflow.SetAttributeValue( "CurrentWithUtilities", rblCurrentWithUtilities.SelectedValue );
                        workflow.SetAttributeValue( "UtilitiesAmountOwed", cbUtilitiesAmountOwed.Text );
                        workflow.SetAttributeValue( "HousingConcerns", tbHousingConcerns.Text );

                    }

                    if ( cbReceiveCar.Checked )
                    {
                        var workflow = ActivateAndAddWorkflow( rockContext,
                            org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_RECEIVE_A_CAR,
                            _person, _family, workflows, selectedWorkflowTypes );

                        workflow.SetAttributeValue( "ValidDriversLicense", rblValidDriversLicense2.SelectedValue );
                        workflow.SetAttributeValue( "CurrentlyVolunteering", rblCurrentlyVolunteering.SelectedValue );
                        workflow.SetAttributeValue( "ServingArea", tbServingArea.Text );
                        workflow.SetAttributeValue( "TransportationConcerns", tbTransportationConcerns2.Text );
                    }

                    if ( cbBike.Checked )
                    {
                        var workflow = ActivateAndAddWorkflow( rockContext,
                            org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_BIKE,
                            _person, _family, workflows, selectedWorkflowTypes );
                    }

                    if ( cbComputer.Checked )
                    {
                        var workflow = ActivateAndAddWorkflow( rockContext,
                            org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_COMPUTER,
                            _person, _family, workflows, selectedWorkflowTypes );
                    }

                    if ( cbDental.Checked || cbVision.Checked )
                    {
                        SetFamilyAttributeValue( org.willowcreek.CareCenter.SystemGuid.Attribute.FAMILY_CLINIC_SCHOLARSHIP_AMOUNT, false, cbClinicScholarship.Text );

                        var clinicWorkflows = new List<Workflow>();
                        var spouse = _person.GetSpouse( rockContext );

                        if ( cbVision.Checked )
                        {
                            var workflow = ActivateAndAddWorkflow( rockContext,
                                org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_VISION,
                                _person, _family, workflows, selectedWorkflowTypes );

                            workflow.SetAttributeValue( "InsuranceSituation", ddlInsuranceSituation.SelectedValue );
                            workflow.SetAttributeValue( "ClinicScholarship", cbClinicScholarship.Text );
                            workflow.SetAttributeValue( "Concerns", tbVisionConcerns.Text );

                            if ( spouse != null )
                            {
                                var spouseWorkflow = ActivateAndAddWorkflow( rockContext,
                                    org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_VISION,
                                    spouse, _family, workflows, selectedWorkflowTypes );

                                spouseWorkflow.SetAttributeValue( "InsuranceSituation", ddlInsuranceSituation.SelectedValue );
                                spouseWorkflow.SetAttributeValue( "ClinicScholarship", cbClinicScholarship.Text );
                                spouseWorkflow.SetAttributeValue( "Concerns", tbVisionConcerns.Text );
                            }
                        }

                        if ( cbDental.Checked )
                        {
                            var workflow = ActivateAndAddWorkflow( rockContext,
                                org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_DENTAL,
                                _person, _family, workflows, selectedWorkflowTypes );

                            workflow.SetAttributeValue( "InsuranceSituation", ddlInsuranceSituation.SelectedValue );
                            workflow.SetAttributeValue( "ClinicScholarship", cbClinicScholarship.Text );
                            workflow.SetAttributeValue( "Concerns", tbDentalConerns.Text );

                            if ( spouse != null )
                            {
                                var spouseWorkflow = ActivateAndAddWorkflow( rockContext,
                                    org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_DENTAL,
                                    spouse, _family, workflows, selectedWorkflowTypes );

                                spouseWorkflow.SetAttributeValue( "InsuranceSituation", ddlInsuranceSituation.SelectedValue );
                                spouseWorkflow.SetAttributeValue( "ClinicScholarship", cbClinicScholarship.Text );
                                spouseWorkflow.SetAttributeValue( "Concerns", tbDentalConerns.Text );
                            }
                        }
                    }

                    if ( cbPurchaseCar.Checked )
                    {
                        var workflow = ActivateAndAddWorkflow( rockContext,
                            org.willowcreek.CareCenter.SystemGuid.WorkflowType.WORKFLOWTYPE_ASSESSMENT_PURCHASE_CAR,
                            _person, _family, workflows, selectedWorkflowTypes );
                    }

                    foreach ( var workflow in workflows.OrderByDescending( w => w.WorkflowTypeCache.Order ) )
                    {
                        assessment.Workflows.Add( workflow );
                    }

                    rockContext.WrapTransaction( () =>
                    {
                        rockContext.SaveChanges();
                        _family.SaveAttributeValues( rockContext );
                    } );

                    // Process workflows
                    List<string> workflowErrors;
                    if ( visitWorkflow != null )
                    {
                        new Rock.Model.WorkflowService( rockContext ).Process( visitWorkflow, out workflowErrors );
                    }
                    foreach( var workflow in workflows )
                    {
                        new Rock.Model.WorkflowService( rockContext ).Process( workflow, out workflowErrors );
                    }

                    string url = string.Empty;
                    if ( _returnParams != null )
                    {
                        url = LinkedPageUrl( "WorkflowEntryPage", _returnParams );
                    }

                    if ( url.IsNotNullOrWhitespace() )
                    {
                        Response.Redirect( url, false );
                    }
                    else
                    { 
                        var notes = new StringBuilder();
                        notes.AppendLine( "<h2>Assessment Saved</h2><p>Depending on the type of assessment there may be more steps required.<p>" );

                        foreach ( var workflowType in selectedWorkflowTypes )
                        {
                            notes.AppendFormat( "<h3>{0}</h3>", workflowType );
                            notes.AppendLine();
                        }

                        nbCompleteMessage.Text = notes.ToString();

                        pnlSelectPerson.Visible = false;
                        pnlAssesmentDetails.Visible = false;
                        pnlComplete.Visible = true;
                    }

                }
            }
        }

        private void SetFamilyAttributeValue( string attributeGuid, bool isDefinedValue, string value )
        {
            var attr = AttributeCache.Read( attributeGuid.AsGuid() );
            if ( attr != null )
            {
                if ( isDefinedValue )
                {
                    var dvId = value.AsIntegerOrNull();
                    if ( dvId.HasValue )
                    {
                        var dv = DefinedValueCache.Read( dvId.Value );
                        if ( dv != null )
                        {
                            value = dv.Guid.ToString();
                        }
                    }
                }

                _family.SetAttributeValue( attr.Key, value );
            }
        }

        private Workflow ActivateAndAddWorkflow( RockContext rockContext, string workflowTypeGuid, Person person, 
            Group family, List<Workflow> workflows, List<string> selectedWorkflowTypes )
        {
            var workflowType = WorkflowTypeCache.Read( workflowTypeGuid.AsGuid() );
            if ( workflowType != null )
            {
                if ( selectedWorkflowTypes != null )
                {
                    selectedWorkflowTypes.Add( workflowType.Name );
                }

                var workflowName = string.Format( "{0}: {1}", person.FullName, workflowType.Name );

                Workflow workflow = Rock.Model.Workflow.Activate( workflowType, person.FullName, rockContext );
                if ( workflow != null )
                {
                    if ( workflows != null )
                    {
                        workflows.Add( workflow );
                    }

                    workflow.Status = "Pending";
                    workflow.Guid = Guid.NewGuid();
                    workflow.SetAttributeValue( "Person", person.PrimaryAlias.Guid.ToString() );
                    workflow.SetAttributeValue( "Family", family.Guid.ToString() );
                    workflow.SetAttributeValue( "PagerId", nbPager.Text );
                    if ( _workingCampus != null )
                    {
                        workflow.SetAttributeValue( "Campus", _workingCampus.Guid.ToString() );
                    }

                    return workflow;
                }
            }

            return null;
        }

        #endregion

    }
}