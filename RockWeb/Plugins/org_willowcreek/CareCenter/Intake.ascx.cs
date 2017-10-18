using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using org.willowcreek.CareCenter;
using org.willowcreek.CareCenter.Model;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_willowcreek.CareCenter
{
    /// <summary>
    /// Care Center block for new visitors.
    /// </summary>
    [DisplayName( "Intake Form" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "Care Center block for new visitors." )]

    [LinkedPage( "Person Page", "The page to return to after intake is cancelled.", true, "", "", 0 )]
    [LinkedPage( "Search Page", "The page to return to after intake is completed.", true, "", "", 1 )]
    [LinkedPage( "Appointment Page", "The page to navigate to if an appointment is being scheduled at end of Intake.", true, "", "", 2 )]
    [BooleanField( "Show Family Members", "Should the person's family member names be displayed?", false, "", 3 )]
    [BooleanField( "Allow Courtesy Visit", "Should the courtesy food option be available?", false, "", 4)]

    public partial class Intake : Rock.Web.UI.RockBlock
    {
        #region Properties

        private Visit Visit { get; set; }
        private Person Person { get; set; }
        private Group Family { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["Visit"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                Visit = new Visit();
            }
            else
            {
                Visit = JsonConvert.DeserializeObject<Visit>( json );
            }

            var rockContext = new RockContext();

            int? personId = ViewState["PersonId"] as int?;
            if ( personId.HasValue && personId.Value > 0 )
            {
                Person = new PersonService( rockContext ).Get( personId.Value );
                Person.LoadAttributes( rockContext );
            }

            int? familyId = ViewState["FamilyId"] as int?;
            if ( familyId.HasValue && familyId.Value > 0 )
            {
                Family = new GroupService( rockContext ).Get( familyId.Value );
                Family.LoadAttributes( rockContext );
            }

            BuildDynamicControls( false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Intake_BlockUpdated; ;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbMessage.Visible = false;
            nbValidationError.Visible = false;
            nbValidationError2.Visible = false;

            if ( !Page.IsPostBack )
            {
                ShowDetail(
                    PageParameter( "VisitId" ).AsIntegerOrNull(),
                    PageParameter( "PersonId" ).AsIntegerOrNull(),
                    PageParameter( "FamilyId" ).AsIntegerOrNull() );

                if ( Person != null )
                {
                    var today = RockDateTime.Today;
                    var now = RockDateTime.Now;
                    var appts = new List<WorkflowAppointment>();
                    foreach( var appt in new WorkflowAppointmentService( new RockContext() ).Queryable().AsNoTracking()
                        .Where( a =>
                            a.PersonAlias != null &&
                            a.PersonAlias.PersonId == Person.Id &&
                            a.AppointmentDate >= today && 
                            a.TimeSlot != null &&
                            a.TimeSlot.Schedule != null )
                        .ToList() )
                    {
                        if ( appt.AppointmentDateTime >= now )
                        {
                            appts.Add( appt );
                        }
                    }
                    if ( appts.Any() )
                    {
                        var apptMsgs = new List<string>();
                        foreach ( var appt in appts )
                        {
                            if ( appt.AppointmentDate == today )
                            {
                                apptMsgs.Add( string.Format( "{0} today at {1}",
                                    appt.Workflow.WorkflowTypeCache.Name, appt.TimeSlot.DailyTitle ) );
                            }
                            else
                            {
                                apptMsgs.Add( string.Format( "{0} on {1} at {2}",
                                    appt.Workflow.WorkflowTypeCache.Name, appt.AppointmentDate.ToLongDateString(), appt.TimeSlot.DailyTitle ) );
                            }
                        }
                        nbAppointmentMessage.Text = string.Format( "{0} has an upcoming appointment for {1}.", Person.NickName, apptMsgs.AsDelimited( " and " ) );
                        nbAppointmentMessage.Visible = true;
                    }
                    else
                    {
                        nbAppointmentMessage.Visible = false;
                    }
                }

            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            // Food
            bool showFoodOptions = cbFoodVisit.Checked || !string.IsNullOrWhiteSpace( hfFoodVisitType.Value );
            divFoodVisitOptions.Attributes["style"] = showFoodOptions ? "display:block" : "display:none";
            divFoodVisitCarOptions.Attributes["style"] = showFoodOptions ? "display:block" : "display:none";
            if ( hfFoodVisitType.Value == "Grace" )
            {
                btnGraceVisit.CssClass = "btn btn-success js-toggle-grace-visit active";
            }
            else
            {
                btnGraceVisit.CssClass = "btn btn-default js-toggle-grace-visit";
            }
            if ( hfFoodVisitType.Value == "Courtesy" )
            {
                btnCourtesyVisit.CssClass = "btn btn-success js-toggle-courtesy-visit active";
            }
            else
            {
                btnCourtesyVisit.CssClass = "btn btn-default js-toggle-courtesy-visit";
            }

            divBreadVisitOptions.Attributes["style"] = cbBreadVisit.Checked ? "display:block" : "display:none";
            divClothingVisitOptions.Attributes["style"] = cbClothingVisit.Checked ? "display:block" : "display:none";
            divLimitedClothingVisitOptions.Attributes["style"] = cbLimitedClothingVisit.Checked ? "display:block" : "display:none";
            divCareTeamVisitOptions.Attributes["style"] = cbCareTeamVisit.Checked ? "display:block" : "display:none";
            divCareTeamNoteOptions.Attributes["style"] = cbCareTeamVisit.Checked ? "display:block" : "display:none";
            //divResourceVisitOptions.Attributes["style"] = cbResourceVisit.Checked ? "display:block" : "display:none";
            divLegalOptions.Attributes["style"] = cbLegal.Checked ? "display:block" : "display:none";
            divLegalImmigrationOptions.Attributes["style"] = cbLegalImmigration.Checked ? "display:block" : "display:none";
            divEmploymentOptions.Attributes["style"] = cbEmployment.Checked ? "display:block" : "display:none";
            divFinancialCoachingOptions.Attributes["style"] = cbFinancialCoaching.Checked ? "display:block" : "display:none";
            //divTaxPrepOptions.Attributes["style"] = cbTaxPrep.Checked ? "display:block" : "display:none";
            //divResponsePastorOptions.Attributes["style"] = cbResponsePastor.Checked ? "display:block" : "display:none";
            //divResponsePastorSubOptions.Attributes["style"] = cbResponsePastor.Checked ? "display:block" : "display:none";
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["Visit"] = JsonConvert.SerializeObject( Visit, Formatting.None, jsonSetting );
            ViewState["PersonId"] = Person != null ? Person.Id : (int?)null;
            ViewState["FamilyId"] = Family != null ? Family.Id : (int?)null;

            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the Intake control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Intake_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail(
                PageParameter( "VisitId" ).AsIntegerOrNull(),
                PageParameter( "PersonId" ).AsIntegerOrNull(),
                PageParameter( "FamilyId" ).AsIntegerOrNull() );
        }

        /// <summary>
        /// Handles the Click event of the lbComplete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbComplete_Click( object sender, EventArgs e )
        {
            if ( ValidateVisit() )
            {
                SaveVisit();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            if ( Person != null )
            {
                NavigateToLinkedPage( "PersonPage", new Dictionary<string, string> { { "PersonId", Person.Id.ToString() } } );
            }
            else
            {
                NavigateToLinkedPage( "SearchPage" );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbClose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbClose_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "SearchPage" );
        }

        /// <summary>
        /// Handles the Click event of the lbScheduleAppointment control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbScheduleAppointment_Click( object sender, EventArgs e )
        {
            if ( Person != null )
            {
                NavigateToLinkedPage( "AppointmentPage", new Dictionary<string, string> { { "PersonId", Person.Id.ToString() } } );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="visitId">The financial visit identifier.</param>
        public void ShowDetail( int? visitId, int? personId, int? familyId )
        {
            using ( var rockContext = new RockContext() )
            {
                var visitService = new VisitService( rockContext );
                var personService = new PersonService( rockContext );
                var serviceAreaService = new ServiceAreaService( rockContext );

                var serviceAreas = serviceAreaService.Queryable().AsNoTracking().ToList();

                // If Visit was specified, try to load it
                if ( visitId.HasValue )
                {
                    Visit = visitService.Get( visitId.Value );
                }

                // If don't have visit yet, check for an existing visit for selected person
                if ( Visit == null && personId.HasValue )
                {
                    Visit = visitService.GetCurrentVisit( personId.Value );
                }

                // If visit exists...
                if ( Visit != null )
                {
                    // ...set the Person and Family from the visit     
                    if ( Visit.PersonAlias != null )
                    {
                        Person = Visit.PersonAlias.Person;
                    }
                }
                else
                {
                    // ...otherwise create a new visit
                    Visit = new Visit();
                    Visit.VisitDate = RockDateTime.Now;
                }

                // If person wasn't loaded from visit, try to get the person by id
                if ( Person == null && personId.HasValue )
                {
                    Person = personService.Get( personId.Value );
                    if ( Person != null && Person.PrimaryAliasId.HasValue )
                    {
                        Visit.PersonAliasId = Person.PrimaryAliasId.Value;
                    }
                }

                // If family wasn't loaded from visit, try to get the family by id
                if ( Family == null )
                {
                    if ( familyId.HasValue )
                    {
                        Family = new GroupService( rockContext ).Get( familyId.Value );
                    }

                    // If a specific family id was not selected, get the first family from person
                    if ( Family == null )
                    {
                        // TODO: Update to new GetFamily method.
                        Family = personService.GetFamilies( Person.Id ).FirstOrDefault();
                    }
                }

                if ( Visit != null && Person != null && Family != null )
                {
                    hlFirstVisit.Visible = Visit.FirstVisit();

                    lPersonName.Text = Person.FullName;

                    if ( GetAttributeValue( "ShowFamilyMembers" ).AsBoolean() )
                    {
                        lFamilyMembers.Visible = true;
                        lFamilyMembers.Text = personService.GetFamilyMembers( Family, Person.Id, false ).Select( p => p.Person.NickName ).ToList().AsDelimited( ", " );
                    }
                    else
                    {
                        lFamilyMembers.Visible = false;
                    }

                    cbPhotoIdValidated.Checked = Visit.PhotoIdValidated;
                    cbAddressValidated.Checked = Visit.ProofOfAddressValidated;
                    cbHomeless.Checked = Visit.IsHomeless ?? false;
                    nbPagerNumber.Text = Visit.PagerId.ToString();

                    bool existingFood = CheckForExisting( serviceAreas, org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_FOOD, cbFoodVisit, lFoodMessage );
                    bool existingBread = CheckForExisting( serviceAreas, org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_BREAD, cbBreadVisit, lFoodMessage );
                    bool existingClothing = CheckForExisting( serviceAreas, org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_CLOTHING, cbClothingVisit, lClothingMessage );
                    bool existingLimitedClothing = CheckForExisting( serviceAreas, org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_LIMITED_CLOTHING, cbLimitedClothingVisit, lClothingMessage );
                    //bool existingCareTeam = CheckForExisting( serviceAreas, org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_CARE_TEAM, cbCareTeamVisit, lCareTeamMessage );
                    //bool existingResource = CheckForExisting( serviceAreas, org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_RESOURCE, cbResourceVisit, lResourceMessage );
                    bool existingLegal = CheckForExisting( serviceAreas, org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_LEGAL, cbLegal, lLegalMessage );
                    bool existingLegalImmigration = CheckForExisting( serviceAreas, org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_LEGAL_IMMIGRATION, cbLegalImmigration, lLegalImmigrationMessage );
                    bool existingEmployment = CheckForExisting( serviceAreas, org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_EMPLOYMENT, cbEmployment, lEmploymentMessage );
                    bool existingFinancialCoaching = CheckForExisting( serviceAreas, org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_FINANCIAL_COACHING, cbFinancialCoaching, lFinancialCoachingMessage );

                    if ( existingFood || existingBread )
                    {
                        cbFoodVisit.Enabled = false;
                        cbBreadVisit.Enabled = false;
                        btnGraceVisit.Visible = false;
                        btnCourtesyVisit.Visible = false;
                    }
                    else
                    {
                        SetFoodOptions();
                    }

                    if ( existingClothing || existingLimitedClothing )
                    {
                        cbClothingVisit.Enabled = false;
                        cbLimitedClothingVisit.Enabled = false;
                    }
                    else
                    {
                        SetClothingOptions();
                    }

                    var now = RockDateTime.Now;

                    var careTeamBanned = Person.GetBannedDateForServiceArea( org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_CARE_TEAM );
                    if ( careTeamBanned.HasValue && careTeamBanned.Value > now )
                    {
                        cbCareTeamVisit.Enabled = false;
                        lCareTeamMessage.Text = string.Format( "The Care Team Visit is currently not an option today as {0} has been banned from Care Team Visits until {1}.",
                            Person.NickName, careTeamBanned.Value.ToShortDateString() );
                    }

                    var legalBanned = Person.GetBannedDateForServiceArea( org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_LEGAL );
                    if ( legalBanned.HasValue && legalBanned.Value > now )
                    {
                        cbLegal.Enabled = false;
                        lLegalMessage.Text = string.Format( "The Legal option is currently not an option today as {0} has been banned from Legal until {1}.",
                            Person.NickName, legalBanned.Value.ToShortDateString() );
                    }

                    var legalImmigrationBanned = Person.GetBannedDateForServiceArea( org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_LEGAL_IMMIGRATION );
                    if ( legalImmigrationBanned.HasValue && legalImmigrationBanned.Value > now )
                    {
                        cbLegalImmigration.Enabled = false;
                        lLegalImmigrationMessage.Text = string.Format( "The Legal (Immigration) option is currently not an option today as {0} has been banned from Legal (Immigration) until {1}.",
                            Person.NickName, legalImmigrationBanned.Value.ToShortDateString() );
                    }

                    var employmentBanned = Person.GetBannedDateForServiceArea( org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_EMPLOYMENT );
                    if ( employmentBanned.HasValue && employmentBanned.Value > now )
                    {
                        cbEmployment.Enabled = false;
                        lEmploymentMessage.Text = string.Format( "The Employment option is currently not an option today as {0} has been banned from Employment until {1}.",
                            Person.NickName, employmentBanned.Value.ToShortDateString() );
                    }

                    var financialCoachingBanned = Person.GetBannedDateForServiceArea( org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_FINANCIAL_COACHING );
                    if ( financialCoachingBanned.HasValue && financialCoachingBanned.Value > now )
                    {
                        cbFinancialCoaching.Enabled = false;
                        lFinancialCoachingMessage.Text = string.Format( "The Financial Coaching option is currently not an option today as {0} has been banned from Financial Coaching until {1}.",
                            Person.NickName, financialCoachingBanned.Value.ToShortDateString() );
                    }


                    //if ( !existingResource )
                    //{
                    //    SetResourceOptions();
                    //}

                    //SetResponsePastorOptions();
                }
                else
                {
                    ShowWarning( "Could not find a Visit record using provided selection!" );
                    pnlDetails.Visible = false;
                }
            }

            BuildDynamicControls( true );
        }

        private void BuildDynamicControls( bool setValues )
        {
            BuildOptions( phFoodVisitOptions, org.willowcreek.CareCenter.SystemGuid.DefinedType.INTAKE_OPTIONS_FOOD );
            BuildOptions( phBreadVisitOptions, org.willowcreek.CareCenter.SystemGuid.DefinedType.INTAKE_OPTIONS_BREAD );
            BuildOptions( phClothingVisitOptions, org.willowcreek.CareCenter.SystemGuid.DefinedType.INTAKE_OPTIONS_CLOTHING );
            BuildOptions( phLimitedClothingVisitOptions, org.willowcreek.CareCenter.SystemGuid.DefinedType.INTAKE_OPTIONS_LIMITED_CLOTHING );
            BuildOptions( phCareTeamVisitOptions, org.willowcreek.CareCenter.SystemGuid.DefinedType.INTAKE_OPTIONS_CARE_TEAM );
            //BuildOptions( phResourceVisitOptions, org.willowcreek.CareCenter.SystemGuid.DefinedType.INTAKE_OPTIONS_RESOURCE );
            BuildOptions( phLegalOptions, org.willowcreek.CareCenter.SystemGuid.DefinedType.INTAKE_OPTIONS_LEGAL );
            BuildOptions( phLegalImmigrationOptions, org.willowcreek.CareCenter.SystemGuid.DefinedType.INTAKE_OPTIONS_LEGAL_IMMIGRATION );
            BuildOptions( phEmploymentOptions, org.willowcreek.CareCenter.SystemGuid.DefinedType.INTAKE_OPTIONS_EMPLOYMENT );
            BuildOptions( phFinancialCoachingOptions, org.willowcreek.CareCenter.SystemGuid.DefinedType.INTAKE_OPTIONS_FINANCIAL_COACHING );
            //BuildOptions( phTaxPrepOptions, org.willowcreek.CareCenter.SystemGuid.DefinedType.INTAKE_OPTIONS_TAX_PREP );
            //BuildOptions( phResponsePastorOptions, org.willowcreek.CareCenter.SystemGuid.DefinedType.INTAKE_OPTIONS_RESPONSE_PASTOR );
        }

        private void BuildOptions( PlaceHolder placeHolder, string definedTypeGuid )
        {
            placeHolder.Controls.Clear();
            var definedType = DefinedTypeCache.Read( definedTypeGuid.AsGuid() );
            if ( definedType != null )
            {
                foreach( var option in definedType.DefinedValues )
                {
                    var cb = new RockCheckBox();
                    cb.ID = string.Format( "cbOption_{0}", option.Id );
                    cb.Text = option.Value;
                    cb.SelectedIconCssClass = "fa fa-fw fa-check-square fa-lg";
                    cb.UnSelectedIconCssClass = "fa fa-fw fa-square-o fa-lg";
                    placeHolder.Controls.Add( cb );
                }
            }
        }

        private bool OptionSelected( PlaceHolder placeHolder, string definedValueGuid )
        {
            var dv = DefinedValueCache.Read( definedValueGuid.AsGuid() );
            if ( dv != null )
            {
                var cb = placeHolder.FindControl( string.Format( "cbOption_{0}", dv.Id ) ) as RockCheckBox;
                if ( cb != null )
                {
                    return cb.Checked;
                }
            }

            return false;
        }

        private bool CheckForExisting( List<ServiceArea> serviceAreas, string serviceAreaGuid, CheckBox cb, Literal literal )
        {
            bool hasWorkflow = false;

            Guid? guid = serviceAreaGuid.AsGuidOrNull();
            if ( guid.HasValue )
            {
                var serviceArea = serviceAreas.Where( s => s.Guid == guid.Value && s.WorkflowTypeId.HasValue ).FirstOrDefault();
                if ( serviceArea != null )
                {
                    hasWorkflow = Visit.Workflows != null && Visit.Workflows.Any( w => w.WorkflowTypeId == serviceArea.WorkflowTypeId.Value && w.Status != org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_CANCELLED );
                    if ( hasWorkflow )
                    {
                        literal.Text = string.Format( "A <em>{0}</em> Intake has already been done today for {1}.", serviceArea.Name, Person.NickName );
                        cb.Enabled = false;
                    }
                }
            }

            return hasWorkflow;
        }


        private void SetFoodOptions()
        {
            var foodVisitStatus = Person.GetFoodVisitStatus();
            if ( foodVisitStatus != null )
            {
                if ( foodVisitStatus.IsBanned )
                {
                    cbFoodVisit.Enabled = false;
                    cbBreadVisit.Enabled = false;
                    btnGraceVisit.Visible = false;
                    btnCourtesyVisit.Visible = false;

                    if ( foodVisitStatus.BanExpireDate.HasValue )
                    {
                        lFoodMessage.Text = string.Format( "The food visit is currently not an option today as the {0} family has been banned from food visits until {1}.",
                            Person.LastName, foodVisitStatus.BanExpireDate.Value.ToShortDateString() );
                    }
                    else
                    {
                        lFoodMessage.Text = string.Format( "The food visit is currently not an option today as the {0} family has been banned from food visits.",
                            Person.LastName );
                    }
                }
                else
                {

                    if ( !foodVisitStatus.GraceVisitAllowed )
                    {
                        btnGraceVisit.AddCssClass( "disabled" );
                    }

                    if ( !foodVisitStatus.CourtesyVisitAllowed )
                    {
                        btnCourtesyVisit.AddCssClass( "disabled" );
                    }

                    if ( !foodVisitStatus.FoodVisitAllowed )
                    {
                        cbFoodVisit.Enabled = false;

                        if ( foodVisitStatus.LastFoodVisit.HasValue )
                        {
                            lFoodMessage.Text = string.Format( "The food visit is currently not an option today as the {0} family had a food visit {1} ({2}).",
                                Person.LastName, foodVisitStatus.LastFoodVisit.Value.ToElapsedString( false, false ), foodVisitStatus.LastFoodVisit.Value.ToShortDateString() );
                        }
                        else
                        {
                            lFoodMessage.Text = string.Format( "The food visit is currently not an option today for the {0} family.", Person.LastName );
                        }
                    }

                    if ( !foodVisitStatus.BreadVisitAllowed )
                    {
                        cbBreadVisit.Enabled = foodVisitStatus.BreadVisitAllowed;
                        if ( foodVisitStatus.LastBreadVisit.HasValue )
                        {
                            lBreadMessage.Text = string.Format( "The bread visit is currently not an option today as the {0} family had a bread visit {1} ({1}).",
                                foodVisitStatus.LastBreadVisit.Value.ToShortDateString() );
                        }
                        else
                        {
                            lFoodMessage.Text = string.Format( "The bread visit is currently not an option today for the {0} family.", Person.LastName );
                        }
                    }
                }
            }
            else
            {
                cbFoodVisit.Enabled = false;
                btnGraceVisit.AddCssClass( "disabled" );
                btnCourtesyVisit.AddCssClass( "disabled" );
                cbBreadVisit.Enabled = false;
            }
        }

        private void SetClothingOptions()
        {
            var clothingVisitStatus = Person.GetClothingVisitStatus();
            if ( clothingVisitStatus != null )
            {
                if ( clothingVisitStatus.IsBanned )
                {
                    cbClothingVisit.Enabled = false;
                    cbLimitedClothingVisit.Enabled = false;

                    if ( clothingVisitStatus.BanExpireDate.HasValue )
                    {
                        lClothingMessage.Text = string.Format( "The clothing visit is currently not an option today as the {0} family has been banned from clothing visits until {1}.",
                            Person.LastName, clothingVisitStatus.BanExpireDate.Value.ToShortDateString() );
                    }
                    else
                    {
                        lClothingMessage.Text = string.Format( "The clothing visit is currently not an option today as the {0} family has been banned from clothing visits.",
                            Person.LastName );
                    }
                }
                else
                {
                    cbClothingVisit.Enabled = clothingVisitStatus.ClothingLevel == ClothingLevel.Full;
                    cbLimitedClothingVisit.Enabled = clothingVisitStatus.ClothingLevel == ClothingLevel.Full || clothingVisitStatus.ClothingLevel == ClothingLevel.Limited;
                }
            }
            else
            {
                cbClothingVisit.Enabled = false;
                cbLimitedClothingVisit.Enabled = false;
            }

        }

        //private void SetResourceOptions()
        //{
        //    var resourceVisitStatus = Person.GetResourceVisitStatus();
        //    if ( resourceVisitStatus != null )
        //    {
        //        if ( resourceVisitStatus.IsBanned )
        //        {
        //            cbResourceVisit.Enabled = false;

        //            if ( resourceVisitStatus.BanExpireDate.HasValue )
        //            {
        //                lResourceMessage.Text = string.Format( "The resource visit is currently not an option today as the {0} family has been banned from resource visits until {1}.",
        //                    Person.LastName, resourceVisitStatus.BanExpireDate.Value.ToShortDateString() );
        //            }
        //            else
        //            {
        //                lResourceMessage.Text = string.Format( "The resource visit is currently not an option today as the {0} family has been banned from resource visits.",
        //                    Person.LastName );
        //            }
        //        }

        //        else if ( !resourceVisitStatus.ResourceVisitAllowed )
        //        {
        //            cbResourceVisit.Enabled = false;

        //            if ( resourceVisitStatus.FoodVisits > 0 )
        //            {
        //                lResourceMessage.Text = string.Format( "The resource visit is currently not an option today as the {0} family has only had {1} {2}, and not the required {3} visits.",
        //                    Person.LastName, resourceVisitStatus.FoodVisits, "visit".PluralizeIf( resourceVisitStatus.FoodVisits > 1 ), resourceVisitStatus.FoodVisitThreshold );
        //            }
        //            else
        //            {
        //                lResourceMessage.Text = string.Format( "The resource visit is currently not an option today as the {0} family has not had any food visits.",
        //                    Person.LastName );
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Sets the response pastor options.
        /// </summary>
        //private void SetResponsePastorOptions()
        //{
        //    var languages = DefinedTypeCache.Read( org.willowcreek.CareCenter.SystemGuid.DefinedType.PREFERRED_LANGUAGE.AsGuid() );
        //    rblResponsePastorLanguage.BindToDefinedType( languages );

        //    var natureOfCase = DefinedTypeCache.Read( org.willowcreek.CareCenter.SystemGuid.DefinedType.RESPONSE_PASTOR_NATURE_OF_CASE.AsGuid() );
        //    cblNatureOfCase.BindToDefinedType( natureOfCase );
        //}

        /// <summary>
        /// Validates the visit.
        /// </summary>
        /// <returns></returns>
        private bool ValidateVisit()
        {
            var errorMessages = new List<String>();

            if ( Visit == null || Person == null || Family == null )
            {
                errorMessages.Add( "Could not determine the current visit information for person and/or family. Please refresh this page and start over." );
            }
            else
            {
                if ( !cbPhotoIdValidated.Checked || !( cbAddressValidated.Checked || cbHomeless.Checked ) )
                {
                    if ( cbFoodVisit.Checked )
                    {
                        errorMessages.Add( "The Photo Id and Proof of Address must be validated (or Homeless option selected) for a Full Food Visit. If person does not have these items, they may be eligible for a Grace Visit." );
                    }
                    if ( cbCareTeamVisit.Checked )
                    {
                        errorMessages.Add( "Photo Id and Proof of Address must be validated (or Homeless option selected) for a Care Team Visit" );
                    }
                }

                if ( ( cbFoodVisit.Checked || !string.IsNullOrWhiteSpace( hfFoodVisitType.Value ) ) && !OptionSelected( phFoodVisitOptions, org.willowcreek.CareCenter.SystemGuid.DefinedValue.INTAKE_FOOD_OPTION_USDA_FORM_SIGNED ) )
                {
                    errorMessages.Add( "A Signed USDA Form is required for a Food Visit" );
                }

                if ( cbFoodVisit.Checked ||
                    cbBreadVisit.Checked ||
                    cbClothingVisit.Checked ||
                    cbLimitedClothingVisit.Checked ||
                    cbCareTeamVisit.Checked ||
                    cbLegal.Checked ||
                    cbLegalImmigration.Checked ||
                    cbEmployment.Checked ||
                    cbFinancialCoaching.Checked ||
                    !string.IsNullOrWhiteSpace( hfFoodVisitType.Value ) )
                {
                }
                else
                {
                    errorMessages.Add( "Please select at least one Intake option." );
                }

            }

            if ( errorMessages.Any() )
            {
                ShowValidationMessage( errorMessages );
                return false;
            }
            
            return true;
        }

        private void ShowValidationMessage( List<string> errorMessages )
        {
            string msg = string.Format( "Please Correct the Following<ul><li>{0}</li></ul>", errorMessages.AsDelimited( "</li><li>" ) );
            nbValidationError.Text = msg;
            nbValidationError.Visible = true;
            nbValidationError2.Text = msg;
            nbValidationError2.Visible = true;
        }

        /// <summary>
        /// Saves the visit.
        /// </summary>
        private void SaveVisit()
        {
            if ( Person != null && Family != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var visitService = new VisitService( rockContext );
                    var serviceAreaService = new ServiceAreaService( rockContext );

                    var serviceAreas = serviceAreaService.Queryable().AsNoTracking().ToList();

                    if ( Visit.Id > 0 )
                    {
                        Visit = visitService.Get( Visit.Id );
                    }
                    else
                    {
                        Visit = new Visit();
                        Visit.VisitDate = RockDateTime.Now;
                        Visit.PassportStatus = PassportStatus.NotPrinted;
                        visitService.Add( Visit );
                    }

                    Visit.PersonAliasId = Person.PrimaryAliasId.Value;
                    Visit.Status = VisitStatus.Active;
                    Visit.CancelReasonValueId = null;
                    Visit.PhotoIdValidated = cbPhotoIdValidated.Checked;
                    Visit.ProofOfAddressValidated = cbAddressValidated.Checked;
                    Visit.IsHomeless = cbHomeless.Checked;
                    Visit.PagerId = nbPagerNumber.Text.AsIntegerOrNull();

                    if ( !Visit.IsValid )
                    {
                        ShowValidationMessage( Visit.ValidationResults.Select( r => r.ErrorMessage ).ToList() );
                        return;
                    }

                    var workflows = new List<Workflow>();
                    var selectedServiceAreas = new List<ServiceArea>();

                    if ( Family.Attributes == null )
                    {
                        Family.LoadAttributes( rockContext );
                    }

                    // Food Visit Workflow
                    if ( cbFoodVisit.Checked || !string.IsNullOrWhiteSpace( hfFoodVisitType.Value ) )
                    {
                        var workflow = ActivateAndAddWorkflow( rockContext, serviceAreas, 
                            phFoodVisitOptions,
                            org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_FOOD, 
                            Person, Family, workflows, selectedServiceAreas );

                        workflow.SetAttributeValue( "VisitType", string.IsNullOrWhiteSpace( hfFoodVisitType.Value ) ? "Normal" : hfFoodVisitType.Value );
                        workflow.SetAttributeValue( "CarMakeModel", tbFoodVehicle.Text );
                        workflow.SetAttributeValue( "InCarWith", tbFoodInCarWith.Text );
                    }

                    // Bread Visit Workflow
                    if ( cbBreadVisit.Checked )
                    {
                        var workflow = ActivateAndAddWorkflow( rockContext, serviceAreas,
                            phBreadVisitOptions,
                            org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_BREAD,
                            Person, Family, workflows, selectedServiceAreas );
                    }

                    // Clothing Visit Workflow
                    if ( cbClothingVisit.Checked )
                    {
                        var workflow = ActivateAndAddWorkflow( rockContext, serviceAreas,
                            phClothingVisitOptions,
                            org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_CLOTHING,
                            Person, Family, workflows, selectedServiceAreas );

                        bool setScholarship = OptionSelected( phClothingVisitOptions, org.willowcreek.CareCenter.SystemGuid.DefinedValue.CLOTHING_CARIS );
                        setScholarship = setScholarship || OptionSelected( phClothingVisitOptions, org.willowcreek.CareCenter.SystemGuid.DefinedValue.CLOTHING_SAFE_FAMILY );

                        if ( setScholarship )
                        {
                            var attr = AttributeCache.Read( org.willowcreek.CareCenter.SystemGuid.Attribute.FAMILY_CLOTHING_SCHOLARSHIP_AMOUNT.AsGuid() );
                            if ( attr != null )
                            {
                                decimal? scholarshipAmount = Family.GetAttributeValue( attr.Key ).AsDecimalOrNull();
                                if ( !scholarshipAmount.HasValue || scholarshipAmount.Value < 10.0M )
                                {
                                    Family.SetAttributeValue( attr.Key, "10.00" );
                                    Family.SaveAttributeValues( rockContext );
                                }
                            }
                        }

                    }

                    // Limited Clothing Visit Workflow
                    if ( cbLimitedClothingVisit.Checked )
                    {
                        var workflow = ActivateAndAddWorkflow( rockContext, serviceAreas,
                            phLimitedClothingVisitOptions,
                            org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_LIMITED_CLOTHING,
                            Person, Family, workflows, selectedServiceAreas );
                    }

                    // Care Team Visit Workflow
                    if ( cbCareTeamVisit.Checked )
                    {
                        var workflow = ActivateAndAddWorkflow( rockContext, serviceAreas,
                            phCareTeamVisitOptions,
                            org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_CARE_TEAM,
                            Person, Family, workflows, selectedServiceAreas );

                        workflow.SetAttributeValue( "IntakeNote", tbCareTeamIntakeNote.Text );
                    }

                    //// Resource Visit Workflow
                    //if ( cbResourceVisit.Checked )
                    //{
                    //    var workflow = ActivateAndAddWorkflow( rockContext, serviceAreas,
                    //        phResourceVisitOptions,
                    //        org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_RESOURCE,
                    //        Person, Family, workflows, selectedServiceAreas );
                    //}

                    // Legal Workflow
                    if ( cbLegal.Checked )
                    {
                        var workflow = ActivateAndAddWorkflow( rockContext, serviceAreas,
                            phLegalOptions,
                            org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_LEGAL,
                            Person, Family, workflows, selectedServiceAreas );
                    }

                    // Legal Immigration
                    if ( cbLegalImmigration.Checked )
                    {
                        var workflow = ActivateAndAddWorkflow( rockContext, serviceAreas,
                            phLegalImmigrationOptions,
                            org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_LEGAL_IMMIGRATION,
                            Person, Family, workflows, selectedServiceAreas );
                    }

                    // Employment Workflow
                    if ( cbEmployment.Checked )
                    {
                        var workflow = ActivateAndAddWorkflow( rockContext, serviceAreas,
                            phEmploymentOptions,
                            org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_EMPLOYMENT,
                            Person, Family, workflows, selectedServiceAreas );
                    }

                    // Financial Coaching Workflow
                    if ( cbFinancialCoaching.Checked )
                    {
                        var workflow = ActivateAndAddWorkflow( rockContext, serviceAreas,
                            phFinancialCoachingOptions,
                            org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_FINANCIAL_COACHING,
                            Person, Family, workflows, selectedServiceAreas );
                    }

                    Visit.PassportStatus = selectedServiceAreas.Any( a => a.UsesPassport ) ? 
                        PassportStatus.NotPrinted : 
                        Visit.Id > 0 ? 
                            Visit.PassportStatus : 
                            PassportStatus.NotNeeded;

                    Guid? nextWorkflowGuid = null;
                    Workflow firstWorkflow = null;
                    foreach ( var workflow in workflows.OrderByDescending( w => w.WorkflowTypeCache.Order ) )
                    {
                        Visit.Workflows.Add( workflow );

                        firstWorkflow = workflow;
                        if ( nextWorkflowGuid.HasValue )
                        {
                            workflow.SetAttributeValue( "NextWorkflow", nextWorkflowGuid.Value.ToString() );
                        }
                        nextWorkflowGuid = workflow.Guid;
                    }

                    if ( firstWorkflow != null )
                    {
                        firstWorkflow.Status = "Waiting";
                    }

                    // Save Information
                    rockContext.WrapTransaction( () =>
                    {
                        rockContext.SaveChanges();

                        foreach ( var workflow in workflows )
                        {
                            workflow.SaveAttributeValues( rockContext );
                        }
                    } );

                    if ( firstWorkflow != null )
                    {
                        List<string> workflowErrors;
                        new Rock.Model.WorkflowService( rockContext ).Process( firstWorkflow, out workflowErrors );
                    }

                    Visit = visitService.Get( Visit.Id );
                    if ( Visit != null )
                    {
                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );
                        mergeFields.Add( "Visit", Visit );

                        var notes = new StringBuilder();
                        notes.AppendLine( "<h2>Intake Complete</h2>" );

                        foreach ( var serviceArea in selectedServiceAreas )
                        {
                            string msg = string.Empty;
                            if ( !string.IsNullOrWhiteSpace( serviceArea.IntakeLava ) )
                            {
                                msg = serviceArea.IntakeLava.ResolveMergeFields( mergeFields );
                            }

                            notes.AppendFormat( "<h3>{0}</h3>{1}", serviceArea.Name, msg );
                            notes.AppendLine();
                        }

                        nbCompleteMessage.Text = notes.ToString();
                    }

                    pnlServiceAreas.Visible = false;
                    pnlComplete.Visible = true;
                }
            }
            else
            {
                ShowError( "Sorry, could not determine the correct person and/or family. You will need to restart this intake." );
            }
        }

        /// <summary>
        /// Activates and adds the workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="serviceAreas">The service areas.</param>
        /// <param name="optionsPlaceHolder">The options place holder.</param>
        /// <param name="serviceAreaGuid">The system unique identifier.</param>
        /// <param name="person">The person.</param>
        /// <param name="family">The family.</param>
        /// <param name="workflows">The workflows.</param>
        /// <param name="selectedServiceAreas">The selected service areas.</param>
        /// <returns></returns>
        private Workflow ActivateAndAddWorkflow( RockContext rockContext, List<ServiceArea> serviceAreas, 
            PlaceHolder optionsPlaceHolder, string serviceAreaGuid, 
            Person person, Group family, List<Workflow> workflows, List<ServiceArea> selectedServiceAreas )
        {
            var guid = serviceAreaGuid.AsGuid();
            var serviceArea = serviceAreas
                .Where( s => s.Guid.Equals( guid ) )
                .FirstOrDefault();

            if ( serviceArea != null && serviceArea.WorkflowTypeId.HasValue )
            {
                selectedServiceAreas.Add( serviceArea );

                var workflowType = WorkflowTypeCache.Read( serviceArea.WorkflowTypeId.Value );
                if ( workflowType != null )
                {
                    var workflowName = string.Format( "{0}: {1}", person.FullName, serviceArea.Name );

                    var workflow = Rock.Model.Workflow.Activate( workflowType, person.FullName, rockContext );
                    if ( workflow != null )
                    {
                        workflows.Add( workflow );

                        workflow.Status = "Pending";
                        workflow.Guid = Guid.NewGuid();

                        var options = new List<string>();
                        foreach ( var cb in optionsPlaceHolder.ControlsOfTypeRecursive<RockCheckBox>() )
                        {
                            if ( cb.Checked )
                            {
                                var dv = DefinedValueCache.Read( cb.ID.Substring( 9 ).AsInteger() );
                                if ( dv != null )
                                {
                                    options.Add( dv.Value );
                                    workflow.SetAttributeValue( dv.Value.Replace(" ", ""), true.ToString() );
                                }
                            }
                        }

                        workflow.SetAttributeValue( "Person", person.PrimaryAlias.Guid.ToString() );
                        workflow.SetAttributeValue( "Family", family.Guid.ToString() );
                        workflow.SetAttributeValue( "PagerId", nbPagerNumber.Text );
                        workflow.SetAttributeValue( "IntakeOptions", options.AsDelimited( ", " ) );

                        return workflow;
                    }
                }
            }


            return null;
        }

        /// <summary>
        /// Sets the workflow attribute value.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        private void SetWorkflowAttributeValue( Workflow workflow, string key, string value )
        {
            if ( workflow.AttributeValues != null )
            {
                if ( workflow.AttributeValues.ContainsKey( key ) )
                {
                    workflow.AttributeValues[key].Value = value;
                }
            }
        }

        private void ShowWarning( string message )
        {
            ShowMessage( "Warning", message, NotificationBoxType.Warning );
        }

        private void ShowError( string message )
        {
            ShowMessage( "Error", message, NotificationBoxType.Danger );
        }

        private void ShowMessage( string title, string message, NotificationBoxType notificationType )
        {
            nbMessage.NotificationBoxType = notificationType;
            nbMessage.Title = title;
            nbMessage.Text = message;
            nbMessage.Visible = true;
        }

        #endregion

    }
}