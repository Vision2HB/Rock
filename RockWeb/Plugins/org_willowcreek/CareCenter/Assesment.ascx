<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Assesment.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.Assesment" %>

<script>

    Sys.Application.add_load(function () {

        $(".photo a").fluidbox();
        $('.js-email-status').tooltip({ html: true, container: 'body', delay: { show: 100, hide: 100 } });

        $('.js-assessment-checkbox').on('click', function () {

            var $cb = $(this).find('input:checkbox').first();
            if ($cb.prop('checked')) {
                $('#<%= nbApprovalPin.ClientID %>').val('');
                $('#<%= hfApprover.ClientID %>').val('');
                $('#<%= pnlApproval.ClientID %>').hide();
            }

            var $cbAutoRepair = $('#<%= cbAutoRepair.ClientID %>');
            var $cbAutoRepairWithBenevolence = $('#<%= cbAutoRepairWithBenevolence.ClientID %>');
            var $cbFinancial = $('#<%= cbFinancial.ClientID %>');
            var $cbHousingRepair = $('#<%= cbHousingRepair.ClientID %>');
            var $cbBike = $('#<%= cbBike.ClientID %>');
            var $cbComputer = $('#<%= cbComputer.ClientID %>');
            var $cbDental = $('#<%= cbDental.ClientID %>');
            var $cbVision = $('#<%= cbVision.ClientID %>');
            var $cbReceiveCar = $('#<%= cbReceiveCar.ClientID %>');
            var $cbPurchaseCar = $('#<%= cbPurchaseCar.ClientID %>');

            if ($cbAutoRepair.prop('checked') || $cbAutoRepairWithBenevolence.prop('checked')) {
                $('.js-assessment-cars').slideDown();
            } else {
                $('.js-assessment-cars').slideUp();
            }

            if ($cbReceiveCar.prop('checked')) {
                $('.js-assessment-receive-car').slideDown();
            } else {
                $('.js-assessment-receive-car').slideUp();
            }

            if ($cbFinancial.prop('checked')) {
                $('.js-assessment-financial').slideDown();
            } else {
                $('.js-assessment-financial').slideUp();
            }

            if ($cbHousingRepair.prop('checked')) {
                $('.js-assessment-housing').slideDown();
            } else {
                $('.js-assessment-housing').slideUp();
            }

            if ($cbDental.prop('checked') || $cbVision.prop('checked')) {
                var $divDental = $('.js-assessment-dental')
                var $divVision = $('.js-assessment-vision')
                if ($cbDental.prop('checked')) {
                    $divDental.slideDown();
                } else {
                    $divDental.slideUp();
                }
                if ($cbVision.prop('checked')) {
                    $divVision.slideDown();
                } else {
                    $divVision.slideUp();
                }
                $('.js-assessment-dentalvision').slideDown();
            } else {
                $('.js-assessment-dentalvision').slideUp();
            }
        });

    });
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block assessment-block">

            <asp:Panel ID="pnlSelectPerson" runat="server" Visible="false" CssClass="panel-body">
                <Rock:PersonPicker ID="ppPerson" runat="server" Label="Assessment Recipient" OnSelectPerson="ppPerson_SelectPerson" />
            </asp:Panel>

            <asp:Panel ID="pnlAssesmentDetails" runat="server">

                <ul class="nav navbar-nav contextsetter contextsetter-campus">
                    <li class="dropdown">

                        <a class="dropdown-toggle navbar-link" href="#" data-toggle="dropdown">
                            <asp:Literal ID="lCurrentSelection" runat="server" />
                            <b class="fa fa-caret-down"></b>
                        </a>

                        <ul id="ulDropdownMenu" runat="server" enableviewstate="false" class="dropdown-menu">
                            <asp:Repeater runat="server" ID="rptCampuses" OnItemCommand="rptCampuses_ItemCommand">
                                <ItemTemplate>
                                    <li>
                                        <asp:LinkButton ID="btnCampus" runat="server" Text='<%# Eval("Name") %>' CommandArgument='<%# Eval("Id") %>' />
                                    </li>
                                </ItemTemplate>
                            </asp:Repeater>
                        </ul>
                    </li>
                    
                </ul>

                <div class="personprofilebar-bio assessment">

                    <div id="divBio" runat="server" class="">

                        <div class="row">

                            <div class="col-sm-2 col-md-2 xs-text-center">
                                <div class="photo">
                                    <asp:Literal ID="lImage" runat="server" />
                                </div>
                            </div>

                            <div class="col-sm-9 col-md-9 xs-text-center">

                                <h1 class="title name">
                                    <asp:Literal ID="lName" runat="server" /></h1>

                                <Rock:PersonProfileBadgeList ID="blStatus" runat="server" />

                                <div class="summary">
                                    <div class="demographics">
                                        <asp:Literal ID="lAge" runat="server" />
                                        <asp:Literal ID="lGender" runat="server" /><br />
                                        <asp:Literal ID="lMaritalStatus" runat="server" />
                                        <asp:Literal ID="lAnniversary" runat="server" /><br />
                                        <asp:Literal ID="lGrade" runat="server" />
                                        <asp:Literal ID="lGraduation" runat="server" />
                                    </div>

                                    <div class="personcontact">
                                        <ul class="list-unstyled phonenumbers">
                                            <asp:Repeater ID="rptPhones" runat="server">
                                                <ItemTemplate>
                                                    <li data-value="<%# Eval("Number") %>"><%# FormatPhoneNumber( (bool)Eval("IsUnlisted"), Eval("CountryCode"), Eval("Number"), (int?)Eval("NumberTypeValueId") ?? 0, (bool)Eval("IsMessagingEnabled") ) %></li>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                        </ul>

                                        <div class="email">
                                            <asp:Literal ID="lEmail" runat="server" />
                                        </div>
                                    </div>
                                </div>

                            </div>

                        </div>

                    </div>

                    <div class="actions">
                        <asp:HyperLink ID="hlViewPerson" runat="server" CssClass="btn btn-default btn-sm" Target="_blank"><i class="fa fa-search"></i> View Person</asp:HyperLink>
                        <asp:HyperLink ID="hlEditPerson" runat="server" CssClass="btn btn-default btn-sm" Target="_blank"><i class="fa fa-pencil"></i> Edit Person</asp:HyperLink>
                        <asp:LinkButton ID="lbRefreshPerson" runat="server" CssClass="btn btn-default btn-sm" OnClick="lbRefreshPerson_Click" CausesValidation="false"><i class="fa fa-refresh"></i></asp:LinkButton>
                    </div>

                </div>

                <div class="personprofilebar-family">

                    <div class="persondetails-group js-persondetails-group">
                        <div class="row group-details">

                            <div class="col-sm-12 col-md-7 clearfix">
                                <ul class="groupmembers">
                                    <asp:Repeater ID="rptrMembers" runat="server">
                                        <ItemTemplate>
                                            <li class='<%# FormatPersonCssClass( (bool)Eval("Person.IsDeceased") ) %>'>
                                                <a href='<%# FormatPersonLink(Eval("PersonId").ToString()) %>'>
                                                    <div class="person-image" id="divPersonImage" runat="server"></div>
                                                    <div class="person-info">
                                                        <%# FormatPersonName(Eval("Person.NickName") as string, Eval("Person.LastName") as string) %></h4>
                                                            <small><%# FormatPersonDetails( Container.DataItem ) %></small>
                                                    </div>
                                                </a>
                                            </li>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </ul>
                            </div>

                            <div class="col-sm-6 col-md-2 addresses clearfix">
                                <ul class="list-unstyled margin-t-md">
                                    <asp:Repeater ID="rptrAddresses" runat="server">
                                        <ItemTemplate>
                                            <li class="address rollover-container clearfix">
                                                <h4><%# FormatAddressType(Eval("GroupLocationTypeValue.Value")) %></h4>
                                                <div class="address">
                                                    <%# Eval("Location.FormattedHtmlAddress") %>
                                                </div>
                                            </li>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </ul>
                            </div>

                            <div class="col-sm-6 col-md-3 clearfix">
                                <div class="pull-right">
                                    <asp:HyperLink ID="hlEditFamily" runat="server" CssClass="btn btn-default btn-sm" Target="_blank"><i class="fa fa-pencil"></i> Edit Family</asp:HyperLink>
                                    <asp:LinkButton ID="lbRefreshFamily" runat="server" CssClass="btn btn-default btn-sm" OnClick="lbRefreshFamily_Click" CausesValidation="false"><i class="fa fa-refresh"></i></asp:LinkButton>
                                </div>
                            </div>

                        </div>

                        <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                        <Rock:NotificationBox ID="nbValidationError" runat="server" NotificationBoxType="Danger" />

                        <asp:Panel ID="pnlGroupAttributes" runat="server" CssClass="margin-l-md js-group-attributes" Style="min-height: 22px;">
                            <div class="row">
                                <div class="col-sm-6 col-md-4">
                                    <Rock:RockLiteral ID="lPager" runat="server" Label="Pager Number" Visible="false" />
                                    <Rock:NumberBox ID="nbPager" runat="server" Label="Pager Number" Required="true" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-sm-6 col-md-4">
                                    <Rock:CurrencyBox ID="cbFamilyIncome" runat="server" Label="Family Monthly Income" AutoPostBack="true" OnTextChanged="cbFamilyIncome_TextChanged" />
                                </div>
                                <div class="col-sm-6 col-md-4">
                                    <Rock:RockDropDownList ID="ddlIncomeSource" runat="server" Label="Income Source" AutoPostBack="true" OnSelectedIndexChanged="ddlIncomeSource_SelectedIndexChanged" />
                                </div>
                                <div class="col-sm-6 col-md-4">
                                    <Rock:RockDropDownList ID="ddlChurchAttendance" runat="server" Label="Church Attendance" AutoPostBack="true" OnSelectedIndexChanged="ddlChurchAttendance_SelectedIndexChanged" />
                                </div>
                                <div class="col-sm-6 col-md-4">
                                    <Rock:RockDropDownList ID="ddlAttendanceFrequency" runat="server" Label="Attendance Frequency" AutoPostBack="true" OnSelectedIndexChanged="ddlAttendanceFrequency_SelectedIndexChanged" />
                                </div>
                                <div class="col-sm-6 col-md-4">
                                    <Rock:RockDropDownList ID="ddlDurationAttended" runat="server" Label="Duration Attended" AutoPostBack="true" OnSelectedIndexChanged="ddlDurationAttended_SelectedIndexChanged" />
                                </div>
                            </div>
                        </asp:Panel>

                    </div>

                </div>

                <asp:Panel ID="pnlTabs" runat="server" CssClass="pagetabs">

                    <ul class="nav nav-pills margin-b-md">
                        <li id="liAssessment" runat="server" class="active">
                            <asp:LinkButton ID="lbAssessment" runat="server" Text="Assessment" OnClick="lbTab_Click" CausesValidation="false" />
                        </li>
                        <li id="liBenevolence" runat="server">
                            <asp:LinkButton ID="lbBenevolence" runat="server" Text="Benevolence History" OnClick="lbTab_Click" CausesValidation="false" />
                        </li>
                        <li id="liTransportation" runat="server">
                            <asp:LinkButton ID="lbTransportation" runat="server" Text="Transportation Visits" OnClick="lbTab_Click" CausesValidation="false" />
                        </li>
                    </ul>

                    <asp:Panel ID="pnlAssessment" runat="server">

                        <div class="assessment-points margin-b-md">

                            <h4>Assessment Points</h4>

                            <Rock:NotificationBox ID="nbUnavailableAssessments" runat="server" NotificationBoxType="Warning" Visible="false" />

                            <div class="row assessment-options">
                                <asp:Panel ID="pnlBikeOption" runat="server" CssClass="col-xs-6 col-sm-4 col-md-2 margin-b-sm js-assessment-checkbox">
                                    <Rock:RockCheckBox ID="cbBike" runat="server" Text="Bike"
                                        UnSelectedIconCssClass="fa fa-fw fa-square-o fa-lg"
                                        SelectedIconCssClass="fa fa-fw fa-check-square fa-lg" />
                                </asp:Panel>
                                <asp:Panel ID="pnlComputerOption" runat="server" CssClass="col-xs-6 col-sm-4 col-md-2 margin-b-sm js-assessment-checkbox">
                                    <Rock:RockCheckBox ID="cbComputer" runat="server" Text="Computer"
                                        UnSelectedIconCssClass="fa fa-fw fa-square-o fa-lg"
                                        SelectedIconCssClass="fa fa-fw fa-check-square fa-lg" />
                                </asp:Panel>
                                <asp:Panel ID="pnlDentalOption" runat="server" CssClass="col-xs-6 col-sm-4 col-md-2 margin-b-sm js-assessment-checkbox">
                                    <Rock:RockCheckBox ID="cbDental" runat="server" Text="Dental"
                                        UnSelectedIconCssClass="fa fa-fw fa-square-o fa-lg"
                                        SelectedIconCssClass="fa fa-fw fa-check-square fa-lg" />
                                </asp:Panel>
                                <asp:Panel ID="pnlFinancialOption" runat="server" CssClass="col-xs-6 col-sm-4 col-md-2 margin-b-sm js-assessment-checkbox">
                                    <Rock:RockCheckBox ID="cbFinancial" runat="server" Text="Benevolence"
                                        UnSelectedIconCssClass="fa fa-fw fa-square-o fa-lg"
                                        SelectedIconCssClass="fa fa-fw fa-check-square fa-lg" />
                                </asp:Panel>
                                <asp:Panel ID="pnlHousingRepairOption" runat="server" CssClass="col-xs-6 col-sm-4 col-md-2 margin-b-sm js-assessment-checkbox">
                                    <Rock:RockCheckBox ID="cbHousingRepair" runat="server" Text="Housing Repair"
                                        UnSelectedIconCssClass="fa fa-fw fa-square-o fa-lg"
                                        SelectedIconCssClass="fa fa-fw fa-check-square fa-lg" />
                                </asp:Panel>
                                <asp:Panel ID="pnlVisionOption" runat="server" CssClass="col-xs-6 col-sm-4 col-md-2 margin-b-sm js-assessment-checkbox">
                                    <Rock:RockCheckBox ID="cbVision" runat="server" Text="Vision"
                                        UnSelectedIconCssClass="fa fa-fw fa-square-o fa-lg"
                                        SelectedIconCssClass="fa fa-fw fa-check-square fa-lg" />
                                </asp:Panel>
                                <asp:Panel ID="pnlAutoRepairOption" runat="server" CssClass="col-xs-6 col-sm-4 col-md-2 margin-b-sm js-assessment-checkbox">
                                    <Rock:RockCheckBox ID="cbAutoRepair" runat="server" Text="Auto Repair"
                                        UnSelectedIconCssClass="fa fa-fw fa-square-o fa-lg"
                                        SelectedIconCssClass="fa fa-fw fa-check-square fa-lg" />
                                </asp:Panel>
                                <asp:Panel ID="pnlAutoRepairWBenevolenceOption" runat="server" CssClass="col-xs-6 col-sm-4 col-md-2 margin-b-sm js-assessment-checkbox">
                                    <Rock:RockCheckBox ID="cbAutoRepairWithBenevolence" runat="server" Text="Auto Repair With Benevolence"
                                        UnSelectedIconCssClass="fa fa-fw fa-square-o fa-lg"
                                        SelectedIconCssClass="fa fa-fw fa-check-square fa-lg" />
                                </asp:Panel>
                                <asp:Panel ID="pnlPurchaseCarOption" runat="server" CssClass="col-xs-6 col-sm-4 col-md-2 margin-b-sm js-assessment-checkbox">
                                    <Rock:RockCheckBox ID="cbPurchaseCar" runat="server" Text="Purchase a Car"
                                        UnSelectedIconCssClass="fa fa-fw fa-square-o fa-lg"
                                        SelectedIconCssClass="fa fa-fw fa-check-square fa-lg" />
                                </asp:Panel>
                                <asp:Panel ID="pnlReceiveCarOption" runat="server" CssClass="col-xs-6 col-sm-4 col-md-2 margin-b-sm js-assessment-checkbox">
                                    <Rock:RockCheckBox ID="cbReceiveCar" runat="server" Text="Receive a Car"
                                        UnSelectedIconCssClass="fa fa-fw fa-square-o fa-lg"
                                        SelectedIconCssClass="fa fa-fw fa-check-square fa-lg" />
                                </asp:Panel>
                            </div>
                        </div>

                        <div id="divAssessmentFinancial" runat="server" class="panel panel-block js-assessment-financial">
                            <div class="panel-heading">
                                <h1 class="panel-title"><i class="fa fa-usd"></i>
                                    Benevolence</h1>
                            </div>
                            <div class="panel-body">
                                <Rock:RockTextBox ID="tbNatureFinancialRequest" runat="server" Label="Benevolence Request Description" TextMode="MultiLine" Rows="4" />
                            </div>
                        </div>

                        <div id="divAssessmentHousing" runat="server" class="panel panel-block js-assessment-housing">
                            <div class="panel-heading">
                                <h1 class="panel-title"><i class="fa fa-home"></i>
                                    Housing</h1>
                            </div>
                            <div class="panel-body">
                                <div class="row">
                                    <div class="col-sm-6">
                                        <Rock:RockRadioButtonList ID="rblHousingStatus" runat="server" Label="Housing Status" RepeatDirection="Horizontal">
                                            <asp:ListItem Value="Own" Text="Own" />
                                            <asp:ListItem Value="Rent" Text="Rent" />
                                        </Rock:RockRadioButtonList>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-sm-6">
                                        <Rock:RockRadioButtonList ID="rblPlanOnSelling" runat="server" Label="Do You Plan on Selling Your Home?" RepeatDirection="Horizontal">
                                            <asp:ListItem Value="True" Text="Yes" />
                                            <asp:ListItem Value="False" Text="No" />
                                        </Rock:RockRadioButtonList>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-sm-6">
                                        <Rock:RockRadioButtonList ID="rblMortgageCurrent" runat="server" Label="Current With Mortgage/Rent" RepeatDirection="Horizontal">
                                            <asp:ListItem Value="True" Text="Yes" />
                                            <asp:ListItem Value="False" Text="No" />
                                        </Rock:RockRadioButtonList>
                                    </div>
                                    <div class="col-sm-6">
                                        <Rock:NumberBox ID="nbMonthsBehind" runat="server" Label="Number of Months Behind" NumberType="Integer" MinimumValue="0" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-sm-6">
                                        <Rock:RockRadioButtonList ID="rblInForeclosure" runat="server" Label="In Foreclosure" RepeatDirection="Horizontal">
                                            <asp:ListItem Value="True" Text="Yes" />
                                            <asp:ListItem Value="False" Text="No" />
                                        </Rock:RockRadioButtonList>
                                    </div>
                                    <div class="col-sm-6">
                                        <Rock:CurrencyBox ID="cbHomeAmountOwed" runat="server" Label="Amount Owed" MinimumValue="0" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-sm-6">
                                        <Rock:RockRadioButtonList ID="rblCurrentWithUtilities" runat="server" Label="Current With Utilities" RepeatDirection="Horizontal">
                                            <asp:ListItem Value="True" Text="Yes" />
                                            <asp:ListItem Value="False" Text="No" />
                                        </Rock:RockRadioButtonList>
                                    </div>
                                    <div class="col-sm-6">
                                        <Rock:CurrencyBox ID="cbUtilitiesAmountOwed" runat="server" Label="Amount Owed" MinimumValue="0" />
                                    </div>
                                </div>
                            <Rock:RockTextBox ID="tbHousingConcerns" runat="server" Label="Housing Concerns" TextMode="MultiLine" Rows="3" />
                           </div>
                        </div>

                        <div id="divAssessmentDentalVision" runat="server" class="panel panel-block js-assessment-dentalvision">
                            <div class="panel-heading">
                                <h1 class="panel-title"><i class="fa fa-thumb-tack"></i>
                                    Dental/Vision</h1>
                            </div>
                            <div class="panel-body">
                                <div class="row">
                                    <div class="col-sm-6">
                                        <Rock:RockDropDownList ID="ddlInsuranceSituation" runat="server" Label="Insurance Situation" CssClass="input-width-lg" />
                                    </div>
                                    <div class="col-sm-6">
                                        <Rock:CurrencyBox ID="cbClinicScholarship" runat="server" Label="Clinic Scholarship" />
                                    </div>
                                </div>
                                <div id="divAssessmentDental" runat="server" class="js-assessment-dental">
                                    <Rock:RockTextBox ID="tbDentalConerns" runat="server" Label="Dental Concerns" TextMode="MultiLine" Rows="3" />
                                </div>
                                <div id="divAssessmentVision" runat="server" class="js-assessment-vision">
                                    <Rock:RockTextBox ID="tbVisionConcerns" runat="server" Label="Vision Concerns" TextMode="MultiLine" Rows="3" />
                                </div>
                            </div>
                        </div>

                        <div id="divAssessmentCars" runat="server" class="panel panel-block js-assessment-cars">
                            <div class="panel-heading">
                                <h1 class="panel-title"><i class="fa fa-car"></i>
                                    Car Repair</h1>
                            </div>
                            <div class="panel-body">
                                <div class="row">
                                    <div class="col-sm-4">
                                        <Rock:RockRadioButtonList ID="rblValidDriversLicense" runat="server" Label="Valid Driver's License" RepeatDirection="Horizontal">
                                            <asp:ListItem Value="True" Text="Yes" />
                                            <asp:ListItem Value="False" Text="No" />
                                        </Rock:RockRadioButtonList>
                                        <Rock:RockRadioButtonList ID="rblValidRegistration" runat="server" Label="Valid Registration" RepeatDirection="Horizontal">
                                            <asp:ListItem Value="True" Text="Yes" />
                                            <asp:ListItem Value="False" Text="No" />
                                        </Rock:RockRadioButtonList>
                                    </div>
                                    <div class="col-sm-4">
                                        <Rock:RockTextBox ID="tbAutoMake" runat="server" Label="Make" />
                                        <Rock:RockTextBox ID="tbAutoModel" runat="server" Label="Model" />
                                    </div>
                                    <div class="col-sm-4">
                                        <Rock:YearPicker ID="ypAutoYear" runat="server" Label="Year" />
                                        <Rock:RockTextBox ID="tbAutoMiles" runat="server" Label="Miles" />
                                    </div>
                                </div>
                                <Rock:RockTextBox ID="tbTransportationConcerns" runat="server" Label="Transportation Concerns" TextMode="MultiLine" Rows="3" />
                            </div>
                        </div>

                        <div id="divAssessmentReceiveCar" runat="server" class="panel panel-block js-assessment-receive-car">
                            <div class="panel-heading">
                                <h1 class="panel-title"><i class="fa fa-car"></i>
                                    Receive Car</h1>
                            </div>
                            <div class="panel-body">
                                <div class="row">
                                    <div class="col-sm-6">
                                        <Rock:RockRadioButtonList ID="rblValidDriversLicense2" runat="server" Label="Valid Driver's License" RepeatDirection="Horizontal">
                                            <asp:ListItem Value="True" Text="Yes" />
                                            <asp:ListItem Value="False" Text="No" />
                                        </Rock:RockRadioButtonList>
                                    </div>
                                    <div class="col-sm-6">
                                        <Rock:RockRadioButtonList ID="rblCurrentlyVolunteering" runat="server" Label="Currently Volunteering" RepeatDirection="Horizontal" >
                                            <asp:ListItem Value="True" Text="Yes" />
                                            <asp:ListItem Value="False" Text="No" />
                                        </Rock:RockRadioButtonList>
                                        <Rock:RockTextBox ID="tbServingArea" runat="server" Label="Serving Area"  />
                                    </div>
                                </div>
                                <Rock:RockTextBox ID="tbTransportationConcerns2" runat="server" Label="Transportation Concerns" TextMode="MultiLine" Rows="3" />
                            </div>
                        </div>

                        <asp:Panel ID="pnlApproval" runat="server" Visible="false">
                            <Rock:NotificationBox ID="nbApprovalRequired" runat="server" NotificationBoxType="Warning" Text="Leader approval required for xxxx." />
                            <Rock:NotificationBox ID="nbInvalidPin" runat="server" NotificationBoxType="Danger" Text="The Approval PIN is invalid." />
                            <div class="row">
                                <div class="col-sm-4">
                                    <div class="form-group">
                                        <label class="control-label">Approval PIN</label>
                                        <div class="control-wrapper form-inline">
                                            <Rock:RockTextBox ID="nbApprovalPin" runat="server" CssClass="input-width-sm" TextMode="Password" />
                                            <asp:LinkButton ID="lbApproval" runat="server" CssClass="btn btn-primary margin-l-sm" Text="Enter" OnClick="lbApproval_Click" />
                                            <asp:HiddenField ID="hfApprover" runat="server" />
                                        </div>
                                    </div>
                                </div>
                                <div class="col-sm-8">
                                    <Rock:RockLiteral ID="lApprovedBy" runat="server" Label="Approved By" Visible="false"></Rock:RockLiteral>
                                </div>
                            </div>
                        </asp:Panel>

                        <div class="actions margin-b-md">
                            <asp:LinkButton ID="lbSaveAssessment" runat="server" Text="Save Assessment" CssClass="btn btn-primary" OnClick="lbSaveAssessment_Click" />
                            <asp:LinkButton ID="lbCancelAssessment" runat="server" Text="Cancel" CssClass="btn btn-default" OnClick="lbCancelAssessment_Click" CausesValidation="false" />
                        </div>

                        <asp:ValidationSummary ID="ValSummary2" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                        <Rock:NotificationBox ID="nbValidationError2" runat="server" NotificationBoxType="Danger" />

                    </asp:Panel>

                    <asp:Panel ID="pnlBenevolence" runat="server" Visible="false">

                        <div class="panel panel-block">
                            <div class="panel-heading">
                                <h1 class="panel-title"><i class="fa fa-paste"></i> Benevolence History</h1>
                            </div>
                            <div class="panel-body">
                                <div class="grid grid-panel">
                                    <Rock:Grid ID="gBenevolence" runat="server" DisplayType="Full" RowItemText="Benevolence" >
                                        <Columns>
                                            <Rock:RockBoundField DataField="RequestDateTime" HeaderText="Date" DataFormatString="{0:d}" />
                                            <Rock:RockBoundField DataField="Campus.Name" HeaderText="Campus" />
                                            <Rock:RockTemplateField HeaderText="Name">
                                                <ItemTemplate>
                                                    <asp:Literal ID="lName" runat="server" />
                                                </ItemTemplate>
                                            </Rock:RockTemplateField>
                                            <Rock:RockBoundField DataField="GovernmentId" HeaderText="Government ID" ColumnPriority="DesktopLarge" />
                                            <Rock:RockBoundField DataField="RequestText" HeaderText="Request" />
                                            <Rock:PersonField DataField="CaseWorkerPersonAlias.Person" HeaderText="Case Worker" ColumnPriority="Tablet" />
                                            <Rock:RockBoundField DataField="ResultSummary" HeaderText="Result Summary" ColumnPriority="DesktopLarge" />
                                            <Rock:RockTemplateField HeaderText="Result Specifics" ColumnPriority="DesktopLarge">
                                                <ItemTemplate>
                                                    <asp:Literal ID="lResults" runat="server" />
                                                </ItemTemplate>
                                            </Rock:RockTemplateField>
                                            <Rock:RockTemplateField SortExpression="RequestStatusValue.Value" HeaderText="Status">
                                                <ItemTemplate>
                                                    <Rock:HighlightLabel ID="hlStatus" runat="server" />
                                                </ItemTemplate>
                                            </Rock:RockTemplateField>
                                            <Rock:CurrencyField DataField="TotalAmount" HeaderText="Total Amount" SortExpression="TotalAmount" />
                                        </Columns>
                                    </Rock:Grid>
                                </div>
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-4 col-md-offset-8 margin-t-md">
                                <asp:Panel ID="pnlSummary" runat="server" CssClass="panel panel-block">
                                    <div class="panel-heading">
                                        <h1 class="panel-title">Total Results</h1>
                                    </div>
                                    <div class="panel-body">
                                        <asp:PlaceHolder ID="phBenevolenceSummary" runat="server" />
                                    </div>
                                </asp:Panel>
                            </div>
                        </div>

                    </asp:Panel>

                    <asp:Panel ID="pnlTransportation" runat="server" Visible="false">

                        <div class="panel panel-block">
                            <div class="panel-heading">
                                <h1 class="panel-title"><i class="fa fa-car"></i> Transportation Visits</h1>
                            </div>
                            <div class="panel-body">
                                <div class="grid grid-panel">
                                    <Rock:Grid ID="gTransportation" runat="server" DisplayType="Full" RowItemText="Visit" >
                                        <Columns>
                                            <Rock:DateTimeField DataField="CreatedDateTime" HeaderText="Date" />
                                            <Rock:RockBoundField DataField="Assessment.PersonAlias.Person.FullName" HeaderText="Name" />
                                            <Rock:RockBoundField DataField="Type" HeaderText="Type" />
                                            <Rock:RockBoundField DataField="Status" HeaderText="Status" />
                                            <Rock:DateTimeField DataField="CompletedDateTime" HeaderText="Completed" />
                                        </Columns>
                                    </Rock:Grid>
                                </div>
                            </div>
                        </div>

                    </asp:Panel>

                </asp:Panel>

            </asp:Panel>

            <asp:Panel ID="pnlComplete" runat="server" Visible="false" CssClass="panel-body">

                <Rock:NotificationBox ID="nbCompleteMessage" runat="server" NotificationBoxType="Info" />

                <div class="actions">
                    <asp:LinkButton ID="lbClose" runat="server" Text="Home" CssClass="btn btn-primary" OnClick="lbClose_Click" />
                    <asp:LinkButton ID="lbPersonProfile" runat="server" CssClass="btn btn-secondary" OnClick="lbPersonProfile_Click" ><i class="fa fa-user"></i> Person Profile </asp:LinkButton>
                </div>

            </asp:Panel>


        </div>

    </ContentTemplate>
</asp:UpdatePanel>

