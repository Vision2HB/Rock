<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Intake.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.Intake" %>

<script>
    Sys.Application.add_load(function () {
        $('h4.js-service-checkbox').on('click', function () {
            var $cb = $(this).find('input:checkbox').first();
            if ($cb.prop('checked')) {

                var $cbFoodVisit = $('#<%= cbFoodVisit.ClientID %>');
                var $cbBreadVisit = $('#<%= cbBreadVisit.ClientID %>');
                var $cbClothingVisit = $('#<%= cbClothingVisit.ClientID %>');
                var $cbLimitedClothingVisit = $('#<%= cbLimitedClothingVisit.ClientID %>');

                if ($cb.attr('id') == $cbFoodVisit.attr('id')) {
                    $('.js-toggle-grace-visit').removeClass('active');
                    $('.js-toggle-courtesy-visit').removeClass('active');
                    $('.js-toggle-grace-visit').removeClass('btn-success');
                    $('.js-toggle-courtesy-visit').removeClass('btn-success');
                    $('.js-toggle-grace-visit').addClass('btn-default');
                    $('.js-toggle-courtesy-visit').addClass('btn-default');
                    $('.js-food-visit-type').val('')

                    if ($cbBreadVisit.prop('checked')) {
                        $cbBreadVisit.prop('checked', false);
                        $cbBreadVisit.closest('div.checkbox').siblings('div.rock-checkbox-icon').find('i').toggleClass('fa-square-o').toggleClass('fa-check-square');
                        $cbBreadVisit.closest('div.js-service-row').find('.js-sub-option').slideUp();
                    }
                }

                if ($cb.attr('id') == $cbBreadVisit.attr('id') ) {
                    $('.js-toggle-grace-visit').removeClass('active');
                    $('.js-toggle-courtesy-visit').removeClass('active');
                    $('.js-toggle-grace-visit').removeClass('btn-success');
                    $('.js-toggle-courtesy-visit').removeClass('btn-success');
                    $('.js-toggle-grace-visit').addClass('btn-default');
                    $('.js-toggle-courtesy-visit').addClass('btn-default');
                    $('.js-food-visit-type').val('')

                    if ($cbFoodVisit.prop('checked')) {
                        $cbFoodVisit.prop('checked', false);
                        $cbFoodVisit.closest('div.checkbox').siblings('div.rock-checkbox-icon').find('i').toggleClass('fa-square-o').toggleClass('fa-check-square');
                    }

                    $cbFoodVisit.closest('div.js-service-row').find('.js-sub-option').slideUp();
                }

                if ($cb.attr('id') == $cbClothingVisit.attr('id') && $cbLimitedClothingVisit.prop('checked') ) {
                    $cbLimitedClothingVisit.prop('checked', false);
                    $cbLimitedClothingVisit.closest('div.checkbox').siblings('div.rock-checkbox-icon').find('i').toggleClass('fa-square-o').toggleClass('fa-check-square');
                    $cbLimitedClothingVisit.closest('div.js-service-row').find('.js-sub-option').slideUp();
                }

                if ($cb.attr('id') == $cbLimitedClothingVisit.attr('id') && $cbClothingVisit.prop('checked') ) {
                    $cbClothingVisit.prop('checked', false);
                    $cbClothingVisit.closest('div.checkbox').siblings('div.rock-checkbox-icon').find('i').toggleClass('fa-square-o').toggleClass('fa-check-square');
                    $cbClothingVisit.closest('div.js-service-row').find('.js-sub-option').slideUp();
                }

                $(this).closest('div.js-service-row').find('.js-sub-option').slideDown();

            } else {
                $(this).closest('div.js-service-row').find('.js-sub-option').slideUp();
            }
        });

        $('.js-toggle-grace-visit').on('click', function ( e ) {
            e.preventDefault();
            $('.js-toggle-courtesy-visit').removeClass('active');
            $('.js-toggle-courtesy-visit').removeClass('btn-success');
            $('.js-toggle-courtesy-visit').addClass('btn-default');
            var $hf = $('.js-food-visit-type');
            if ($hf.val() != 'Grace') {
                $hf.val('Grace');
                $(this).addClass('active');
                $(this).addClass('btn-success');
                $(this).removeClass('btn-default');
            } else {
                $hf.val('');
                $(this).removeClass('active');
                $(this).removeClass('btn-success');
                $(this).addClass('btn-default');
            }
            if ( $hf.val() == '' ) {
                $(this).closest('div.js-service-row').find('.js-sub-option').slideUp();
            } else {
                $(this).closest('div.js-service-row').find('.js-sub-option').slideDown();

                var $cbFoodVisit = $('#<%= cbFoodVisit.ClientID %>');
                $cbFoodVisit.prop('checked', false);
                $cbFoodVisit.closest('div.checkbox').siblings('div.rock-checkbox-icon').find('i').addClass('fa-square-o').removeClass('fa-check-square');
            }

            var $cbBreadVisit = $('#<%= cbBreadVisit.ClientID %>');
            $cbBreadVisit.prop('checked', false);
            $cbBreadVisit.closest('div.checkbox').siblings('div.rock-checkbox-icon').find('i').addClass('fa-square-o').removeClass('fa-check-square');
            $cbBreadVisit.closest('div.js-service-row').find('.js-sub-option').slideUp();

            return false;
        });

        $('.js-toggle-courtesy-visit').on('click', function ( e ) {
            e.preventDefault();
            $('.js-toggle-grace-visit').removeClass('active');
            var $hf = $('.js-food-visit-type');
            if ($hf.val() != 'Courtesy') {
                $hf.val('Courtesy');
                $(this).addClass('active');
            } else {
                $hf.val('');
                $(this).removeClass('active');
            }
            if ($hf.val() == '') {
                $(this).closest('div.js-service-row').find('.js-sub-option').slideUp();
            } else {
                $(this).closest('div.js-service-row').find('.js-sub-option').slideDown();

                var $cbFoodVisit = $('#<%= cbFoodVisit.ClientID %>');
                $cbFoodVisit.prop('checked', false);
                $cbFoodVisit.closest('div.checkbox').siblings('div.rock-checkbox-icon').find('i').addClass('fa-square-o').removeClass('fa-check-square');
            }

            var $cbBreadVisit = $('#<%= cbBreadVisit.ClientID %>');
            $cbBreadVisit.prop('checked', false);
            $cbBreadVisit.closest('div.checkbox').siblings('div.rock-checkbox-icon').find('i').addClass('fa-square-o').removeClass('fa-check-square');
            $cbBreadVisit.closest('div.js-service-row').find('.js-sub-option').slideUp();

        });

    });
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContents" CssClass="panel panel-block" runat="server">
            <asp:HiddenField ID="hfVisitId" runat="server" />
            
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-users"></i> Intake</h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlFirstVisit" runat="server" LabelType="Danger" Text="First Visit" />
                </div>
            </div>

            <div class="panel-body">

                <Rock:NotificationBox ID="nbMessage" runat="server" />

                <asp:Panel ID="pnlDetails" runat="server">

                    <asp:ValidationSummary ID="valServiceAreaDetail" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                    <Rock:NotificationBox ID="nbValidationError" runat="server" NotificationBoxType="Danger" />

                    <h2><asp:Literal ID="lPersonName" runat="server" /> <small><asp:Literal ID="lFamilyMembers" runat="server" /></small></h2>

                    <Rock:NotificationBox ID="nbAppointmentMessage" runat="server" NotificationBoxType="Success" Heading="Future Appointment" />

                    <asp:Panel ID="pnlServiceAreas" runat="server">

                        <div class="row margin-b-lg">
                            <div class="col-sm-6">
                                <h4><Rock:RockCheckBox ID="cbPhotoIdValidated" runat="server" Text="Photo Id Validated" 
                                    UnSelectedIconCssClass="fa fa-fw fa-square-o fa-lg"
                                    SelectedIconCssClass="fa fa-fw fa-check-square fa-lg" /></h4>
                                <h4><Rock:RockCheckBox ID="cbAddressValidated" runat="server" Text="Proof of Address Validated"
                                    UnSelectedIconCssClass="fa fa-fw fa-square-o fa-lg"
                                    SelectedIconCssClass="fa fa-fw fa-check-square fa-lg" /></h4>
                                <h4><Rock:RockCheckBox ID="cbHomeless" runat="server" Text="Homeless"
                                    UnSelectedIconCssClass="fa fa-fw fa-square-o fa-lg"
                                    SelectedIconCssClass="fa fa-fw fa-check-square fa-lg" /></h4>
                            </div>
                            <div class="col-sm-6">
                                <Rock:NumberBox ID="nbPagerNumber" runat="server" Label="Pager Number" Required="true" />
                            </div>
                        </div>

                        <div class="panel panel-block">
                            <div class="panel-heading">
                                <h1 class="panel-title"><i class="fa fa-cutlery"></i> Food</h1>
                            </div>
                            <div class="panel-body">
                                <div class="row js-service-row">
                                    <div class="col-sm-4">
                                        <h4 class="js-service-checkbox"><Rock:RockCheckBox ID="cbFoodVisit" runat="server" Text="Food Visit" 
                                            UnSelectedIconCssClass="fa fa-fw fa-square-o fa-lg"
                                            SelectedIconCssClass="fa fa-fw fa-check-square fa-lg" /></h4>
                                        <div id="divFoodVisitOptions" runat="server" class="js-sub-option margin-l-lg" >
                                            <asp:PlaceHolder ID="phFoodVisitOptions" runat="server" />
                                        </div>
                                    </div>
                                    <div class="col-sm-8">
                                        <div class="row">
                                            <div class="col-sm-6">
                                                <asp:Literal ID="lFoodMessage" runat="server" />
                                            </div>
                                            <div class="col-sm-6 actions">
                                                <Rock:HiddenFieldWithClass ID="hfFoodVisitType" runat="server" CssClass="js-food-visit-type" />
                                                <asp:Label id="btnGraceVisit" runat="server" class="btn btn-default js-toggle-grace-visit">Grace Visit</asp:Label>
                                                <asp:label id="btnCourtesyVisit" runat="server" class="btn btn-default js-toggle-courtesy-visit" Visible="false">Courtesy Visit</asp:label>
                                            </div>
                                        </div>
                                        <div id="divFoodVisitCarOptions" runat="server" class="js-sub-option margin-t-sm">
                                            <Rock:RockTextBox ID="tbFoodVehicle" runat="server" Label="Vehicle Type and Color" CssClass="js-sub-option" />
                                            <Rock:RockTextBox ID="tbFoodInCarWith" runat="server" Label="In Car With" />
                                        </div>
                                    </div>
                                </div>
                                <div class="row js-service-row">
                                    <div class="col-sm-4">
                                        <h4 class="js-service-checkbox"><Rock:RockCheckBox ID="cbBreadVisit" runat="server" Text="Bread Visit" 
                                            UnSelectedIconCssClass="fa fa-fw fa-square-o fa-lg"
                                            SelectedIconCssClass="fa fa-fw fa-check-square fa-lg" /></h4>
                                        <div id="divBreadVisitOptions" runat="server" class="js-sub-option margin-l-lg" >
                                            <asp:PlaceHolder ID="phBreadVisitOptions" runat="server" />
                                        </div>
                                    </div>
                                    <div class="col-sm-8">
                                        <asp:Literal ID="lBreadMessage" runat="server" />
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="panel panel-block">
                            <div class="panel-heading">
                                <h1 class="panel-title"><i class="fa fa-tag"></i> Clothing</h1>
                            </div>
                            <div class="panel-body">
                                <div class="row js-service-row">
                                    <div class="col-sm-4">
                                        <h4 class="js-service-checkbox"><Rock:RockCheckBox ID="cbClothingVisit" runat="server" Text="Clothing Visit" 
                                            UnSelectedIconCssClass="fa fa-fw fa-square-o fa-lg"
                                            SelectedIconCssClass="fa fa-fw fa-check-square fa-lg" /></h4>
                                        <div id="divClothingVisitOptions" runat="server" class="js-sub-option margin-l-lg" >
                                            <asp:PlaceHolder ID="phClothingVisitOptions" runat="server" />
                                        </div>
                                    </div>
                                    <div class="col-sm-8">
                                        <asp:Literal ID="lClothingMessage" runat="server" />
                                    </div>
                                </div>
                                <div class="row js-service-row">
                                    <div class="col-sm-4">
                                        <h4 class="js-service-checkbox"><Rock:RockCheckBox ID="cbLimitedClothingVisit" runat="server" Text="Limited Clothing Visit" 
                                            UnSelectedIconCssClass="fa fa-fw fa-square-o fa-lg"
                                            SelectedIconCssClass="fa fa-fw fa-check-square fa-lg" /></h4>
                                        <div id="divLimitedClothingVisitOptions" runat="server" class="js-sub-option margin-l-lg" >
                                            <asp:PlaceHolder ID="phLimitedClothingVisitOptions" runat="server" />
                                        </div>
                                    </div>
                                    <div class="col-sm-8">
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="panel panel-block">
                            <div class="panel-heading">
                                <h1 class="panel-title"><i class="fa fa-heartbeat"></i> Care Team</h1>
                            </div>
                            <div class="panel-body">
                                <div class="row js-service-row">
                                    <div class="col-sm-4">
                                        <h4 class="js-service-checkbox"><Rock:RockCheckBox ID="cbCareTeamVisit" runat="server" Text="Care Team Visit" 
                                            UnSelectedIconCssClass="fa fa-fw fa-square-o fa-lg"
                                            SelectedIconCssClass="fa fa-fw fa-check-square fa-lg" /></h4>
                                        <div id="divCareTeamVisitOptions" runat="server" class="js-sub-option margin-l-lg" >
                                            <asp:PlaceHolder ID="phCareTeamVisitOptions" runat="server" />
                                        </div>
                                    </div>
                                    <div class="col-sm-8">
                                        <asp:Literal ID="lCareTeamMessage" runat="server" />
                                        <div id="divCareTeamNoteOptions" runat="server" class="js-sub-option margin-t-sm">
                                            <Rock:RockTextBox ID="tbCareTeamIntakeNote" runat="server" Label="Intake Note" TextMode="MultiLine" Rows="3" />
                                        </div>
                                    </div>
                                </div>
<%--                                <div class="row js-service-row">
                                    <div class="col-sm-4">
                                        <h4 class="js-service-checkbox"><Rock:RockCheckBox ID="cbResourceVisit" runat="server" Text="Resource Visit" 
                                            UnSelectedIconCssClass="fa fa-fw fa-square-o fa-lg"
                                            SelectedIconCssClass="fa fa-fw fa-check-square fa-lg" /></h4>
                                        <div id="divResourceVisitOptions" runat="server" class="js-sub-option margin-l-lg" >
                                            <asp:PlaceHolder ID="phResourceVisitOptions" runat="server" />
                                        </div>
                                    </div>
                                    <div class="col-sm-8">
                                        <asp:Literal ID="lResourceMessage" runat="server" />
                                    </div>
                                </div>--%>
                            </div>
                        </div>

                        <div class="panel panel-block">
                            <div class="panel-heading">
                                <h1 class="panel-title"><i class="fa fa-clipboard"></i> Long-term Solutions</h1>
                            </div>
                            <div class="panel-body">
                                <div class="row js-service-row">
                                    <div class="col-sm-4">
                                        <h4 class="js-service-checkbox"><Rock:RockCheckBox ID="cbLegal" runat="server" Text="Legal" 
                                            UnSelectedIconCssClass="fa fa-fw fa-square-o fa-lg"
                                            SelectedIconCssClass="fa fa-fw fa-check-square fa-lg" /></h4>
                                        <div id="divLegalOptions" runat="server" class="js-sub-option margin-l-lg" >
                                            <asp:PlaceHolder ID="phLegalOptions" runat="server" />
                                        </div>
                                    </div>
                                    <div class="col-sm-8">
                                        <asp:Literal ID="lLegalMessage" runat="server" />
                                    </div>
                                </div>
                                <div class="row js-service-row">
                                    <div class="col-sm-4">
                                        <h4 class="js-service-checkbox"><Rock:RockCheckBox ID="cbLegalImmigration" runat="server" Text="Legal (Immigration)" 
                                            UnSelectedIconCssClass="fa fa-fw fa-square-o fa-lg"
                                            SelectedIconCssClass="fa fa-fw fa-check-square fa-lg" /></h4>
                                        <div id="divLegalImmigrationOptions" runat="server" class="js-sub-option margin-l-lg" >
                                            <asp:PlaceHolder ID="phLegalImmigrationOptions" runat="server" />
                                        </div>
                                    </div>
                                    <div class="col-sm-8">
                                        <asp:Literal ID="lLegalImmigrationMessage" runat="server" />
                                    </div>
                                </div>
                                <div class="row js-service-row">
                                    <div class="col-sm-4">
                                        <h4 class="js-service-checkbox"><Rock:RockCheckBox ID="cbEmployment" runat="server" Text="Employment" 
                                            UnSelectedIconCssClass="fa fa-fw fa-square-o fa-lg"
                                            SelectedIconCssClass="fa fa-fw fa-check-square fa-lg" /></h4>
										<div id="divEmploymentOptions" runat="server" class="js-sub-option margin-l-lg" >
											<asp:PlaceHolder ID="phEmploymentOptions" runat="server" />
										</div>
                                    </div>
                                    <div class="col-sm-8">
                                        <asp:Literal ID="lEmploymentMessage" runat="server" />
                                    </div>
                                </div>
                                <div class="row js-service-row">
                                    <div class="col-sm-4">
                                        <h4 class="js-service-checkbox"><Rock:RockCheckBox ID="cbFinancialCoaching" runat="server" Text="Financial Coaching" 
                                            UnSelectedIconCssClass="fa fa-fw fa-square-o fa-lg"
                                            SelectedIconCssClass="fa fa-fw fa-check-square fa-lg" /></h4>
										<div id="divFinancialCoachingOptions" runat="server" class="js-sub-option margin-l-lg" >
											<asp:PlaceHolder ID="phFinancialCoachingOptions" runat="server" />
										</div>
                                    </div>
                                    <div class="col-sm-8">
                                        <asp:Literal ID="lFinancialCoachingMessage" runat="server" />
                                    </div>
                                </div>
<%--                                    <div class="row js-service-row">
                                    <div class="col-sm-12">
                                        <h4 class="js-service-checkbox"><Rock:RockCheckBox ID="cbTaxPrep" runat="server" Text="Tax Prep (VITA)" 
                                            UnSelectedIconCssClass="fa fa-fw fa-square-o fa-lg"
                                            SelectedIconCssClass="fa fa-fw fa-check-square fa-lg" /></h4>
										<div id="divTaxPrepOptions" runat="server" class="js-sub-option margin-l-lg" >
											<asp:PlaceHolder ID="phTaxPrepOptions" runat="server" />
										</div>
                                    </div>
                                </div>--%>
                            </div>
                        </div>

<%--                            <div class="panel panel-block">
                            <div class="panel-heading">
                                <h1 class="panel-title"><i class="fa fa-clipboard"></i> Response Pastor</h1>
                            </div>
                            <div class="panel-body">
                                <div class="row js-service-row">
                                    <div class="col-sm-12">
                                        <h4 class="js-service-checkbox"><Rock:RockCheckBox ID="cbResponsePastor" runat="server" Text="Response Pastor" 
                                            UnSelectedIconCssClass="fa fa-fw fa-square-o fa-lg"
                                            SelectedIconCssClass="fa fa-fw fa-check-square fa-lg" /></h4>
										<div id="divResponsePastorOptions" runat="server" class="js-sub-option margin-l-lg" >
											<asp:PlaceHolder ID="phResponsePastorOptions" runat="server" />
										</div>
                                        <div id="divResponsePastorSubOptions" runat="server" class="js-sub-option" >
                                            <Rock:RockRadioButtonList ID="rblResponsePastorLanguage" runat="server" Label="Language" RepeatDirection="Horizontal" />
                                            <Rock:RockTextBox ID="tbResponsePastorSummary" runat="server" Label="Summary Notes" TextMode="MultiLine" Rows="4" />
                                            <Rock:RockCheckBoxList ID="cblNatureOfCase" runat="server" Label="Nature of Case" RepeatDirection="Vertical" RepeatColumns="3" CssClass="row" />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>--%>

                        <div class="actions margin-b-lg">
                            <asp:LinkButton ID="lbComplete" runat="server" Text="Complete" CssClass="btn btn-primary" OnClick="lbComplete_Click" />
                            <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-default" CausesValidation="false" OnClick="lbCancel_Click" />
                        </div>

                        <asp:ValidationSummary ID="valServiceAreaDetail2" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                        <Rock:NotificationBox ID="nbValidationError2" runat="server" NotificationBoxType="Danger" />

                    </asp:Panel>

                    <asp:Panel ID="pnlComplete" runat="server" Visible="false">

                        <Rock:NotificationBox ID="nbCompleteMessage" runat="server" NotificationBoxType="Info" />

                        <div class="actions">
                            <asp:LinkButton ID="lbClose" runat="server" Text="Close" CssClass="btn btn-primary" OnClick="lbClose_Click" />
                            <asp:LinkButton ID="lbScheduleAppointment" runat="server" Text="Schedule an Appointment" CssClass="btn btn-link" OnClick="lbScheduleAppointment_Click" />
                        </div>

                    </asp:Panel>

                </asp:Panel>

            </div>


        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
