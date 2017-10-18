using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using org.willowcreek.CareCenter.Model;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_willowcreek.CareCenter
{
    /// <summary>
    /// Care Center block for viewing list of appointments for a specific persons family.
    /// </summary>
    [DisplayName( "Passport Lava" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "Care Center block for printing the Passport." )]

    [CodeEditorField( "Lava Template", "The Lava template to use for the passport.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 500, true, @"
<style>
    .food-box {
        float: right;
        width: 200px;
        height: 40px;
        margin: 5px;
        border: 1px solid rgba(0, 0, 0, .2);
    }
</style>

<div class='clearfix'>
    <div class='pull-right'>
        <a href='#' class='btn btn-primary hidden-print' onClick='window.print();'><i class='fa fa-print'></i> Print Passport(s)</a> 
    </div>
</div>

{% for visit in Visits %}

    <div style='page-break-after:always'>
        <div class='row margin-b-xl'>
            <div class='col-md-6'>
                <div class='pull-left'>
                    <img src='/Themes/CareCenter/Assets/Images/logo.png' width='300px' />
                </div>
            </div>
            <div class='col-md-6 text-right'>
                <h3>Guest Passport</h3>
                Intake Day: <strong>{{ visit.VisitDate | Date:'M/d/yyyy' }} @ {{ visit.VisitDate | Date:'hh:mm:ss tt' }}</strong><br/>
                Pager #: <strong><u>{{ visit.PagerId }}</u></strong><br/>
                {% if visit.IsFirstVisit %}
                    <span class='label label-info'>First Visit</span>
                {% endif %}
            </div>
        </div>

        <h3>{{ visit.PersonAlias.Person.FullName }}</h3>
        <hr>
        <p>Primary Language: <strong>{{ visit.PersonAlias.Person | Attribute:'PreferredLanguage' }}</strong></p>

        {% assign groupMember = visit.PersonAlias.Person | Groups:""10"" | First %}
        {% assign familySize = groupMember.Group.Members | Size %}
        {% assign children = visit.PersonAlias.Person | Children %}

        {% assign wfs = visit.Workflows | Where:'WorkflowTypeId',39 %}
        {% if wfs != empty %}

            {% assign diapers = false %}
            {% assign birthday = false %}
            {% assign diaperAgeWeeks = 'Global' | Attribute:'DiaperAge' %}
            {% assign birthdayDaySpan = 'Global' | Attribute:'BirthdayCakeAvailableTimespan' %}

            {% for familyMember in groupMember.Group.Members %}
                {% assign birthdate = familyMember.Person.BirthDate %} 
                {% if birthdate %}

                    {% assign daysOld = birthdate | DateDiff:'Now','d' %}
                    {% assign weeksOld = daysOld | DividedBy:7 %}
                    {% if weeksOld <= diaperAgeWeeks %}
                        {% assign diapers = true %}
                    {% endif %}

                    {% assign daysToBirthday = familyMember.Person.DaysToBirthday %}
                    {% assign daysSinceBirthday = 365 | Minus:daysToBirthday %}
                    {% if daysToBirthday <= birthdayDaySpan or daysSinceBirthday <= birthdayDaySpan %}
                        {% assign birthday = true %}
                    {% endif %}

                {% endif %}
            {% endfor %}

            {% assign wf = wfs | First %}
            <hr>
            <h3>{{ wf.WorkflowType.Name }}:</h3>
            <div class='row'>
                <div class='col-sm-4'>
                    Family Size: <strong>{{ familySize }}</strong><br/>
                    Diapers: <strong>{% if diapers == true %}Yes{% else %}No{% endif %}</strong>
                </div>
                <div class='col-sm-4'>
                    Car Make/Model: <strong>{{ wf | Attribute:'CarMakeModel' }}</strong><br/>
                    In Car With: <strong>{{ wf | Attribute:'InCarWith' }}</strong><br/>
                </div>
                <div class='col-sm-4 clearfix'>
                    {% if birthday == true %}<i class='fa fa-birthday-cake fa-3x'></i>{% endif %}
                    <div class='food-box btn-{% if familySize > 7 %}warning{% else %}{% if familySize > 3 %}danger{% else %}info{% endif %}{% endif %}'></div>
                </div>
            </div>

        {% endif %}

        {% assign wfs = visit.Workflows | Where:'WorkflowTypeId',42 %}
        {% if wfs != empty %}
            {% assign wf = wfs | First %}
            <hr>
            <h3>{{ wf.WorkflowType.Name }}:</h3>
        {% endif %}

        {% assign wfs = visit.Workflows | Where:'WorkflowTypeId',35 %}
        {% if wfs != empty %}
            {% assign wf = wfs | First %}
            <hr>
            <h3>{{ wf.WorkflowType.Name }}:</h3>
            <table style='border-collapse:collapse;'>
                <thead>
                    <tr>
                        <th style='border:1px solid #cccccc;padding:5px'>Child's Name</th>
                        <th style='border:1px solid #cccccc;padding:5px'>Gender</th>
                        <th style='border:1px solid #cccccc;padding:5px'>Age</th>
                        <th style='border:1px solid #cccccc;padding:5px'>Coat</th>
                   </tr>
                </thead>
                <tbody>
                {% for child in children %}
                    {% assign coatValid = 'Yes' %}
                    {% assign coatDate = child | Attribute:'CoatReception','RawValue' %}
                    {% if coatDate and coatDate != empty and coatDate != '' %}
                        {% if coatDate | DateDiff:'Now','Y' < 1 %}
                            {% assign coatValid = 'No' %}
                        {% endif %}
                    {% endif %}
                    <tr>
                        <td style='border:1px solid #cccccc;padding:5px'>{{ child.FullName }}</td>
                        <td style='border:1px solid #cccccc;padding:5px'>{{ child.Gender }}</td>
                        <td style='border:1px solid #cccccc;padding:5px'>{{ child.Age }}</td>
                        <td style='border:1px solid #cccccc;padding:5px'>{{ coatValid }}</td>
                   </tr>
                {% endfor %}
                </tbody>
            </table>            
        {% endif %}

        {% assign wfs = visit.Workflows | Where:'WorkflowTypeId',38 %}
        {% if wfs != empty %}
            {% assign wf = wfs | First %}
            <hr>
            <h3>{{ wf.WorkflowType.Name }}:</h3>
            <p>This is a limited clothing visit. This guest is allowed up to 10 clothing items.</p>
        {% endif %}

    </div>

{% endfor %}
", "", 0 )]
    [BooleanField( "Enable Debug", "Shows the merge fields available for the Lava", order: 1 )]
    public partial class PassportLava : RockBlock
    {

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
                DisplayResults();
            }
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
            DisplayResults();
        }

        #endregion

        #region Methods

        private void DisplayResults()
        {
            var visitIds = PageParameter( "VisitIds" ).SplitDelimitedValues().AsIntegerList();
            if ( visitIds != null && visitIds.Any() )
            {
                using ( var rockContext = new RockContext() )
                {
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );

                    var visitService = new VisitService( rockContext );
                    var visits = new VisitService( rockContext ).Queryable()
                        .Where( v => visitIds.Contains( v.Id ) )
                        .ToList();

                    if ( visits != null && visits.Any() )
                    {
                        mergeFields.Add( "Visits", visits );

                        foreach ( var visit in visits )
                        {
                            if ( visit.PassportStatus != PassportStatus.Printed )
                            {
                                visit.PassportStatus = PassportStatus.Printed;
                                rockContext.SaveChanges();

                                visitService.UpdateWorkflowPassportStatus( visit.Id );
                            }
                        }
                    }

                    var template = GetAttributeValue( "LavaTemplate" );
                    lResults.Text = template.ResolveMergeFields( mergeFields );

                    // show debug info
                    if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && UserCanEdit )
                    {
                        lDebug.Visible = true;
                        lDebug.Text = mergeFields.lavaDebugInfo();
                    }
                }
            }
        }

        #endregion

    }
}