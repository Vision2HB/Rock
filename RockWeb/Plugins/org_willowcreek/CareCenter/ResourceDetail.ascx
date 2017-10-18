<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ResourceDetail.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.ResourceDetail" %>

<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfResourceId" runat="server" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-bookmark"></i>
                    <asp:Literal ID="lTitle" runat="server" />
                </h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <div id="pnlEditDetails" runat="server">

                    <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbName" runat="server" Label="Name" />
                        </div>
                        <div class="col-md-3">
                            <Rock:RockRadioButtonList ID="rblOwner" runat="server" Label="Owner" Required="true" RepeatDirection="Horizontal" />
                        </div>
                        <div class="col-md-3">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" Text="Yes" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:RockTextBox ID="tbDescription" runat="server" Label="Description" TextMode="MultiLine" Rows="3" ValidateRequestMode="Disabled" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlResourceType" runat="server" Label="Resource Type" />
                            <Rock:UrlLinkBox ID="lbWebsite" runat="server" Label="Website" />
                            <Rock:AddressControl ID="acAddress" runat="server" Label="Address" ShowAddressLine2="false" ShowCounty="true" />
                            <Rock:GeoPicker ID="geopPoint" runat="server" DrawingMode="Point" Label="Point" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbReducedFeeProgram" runat="server" Label="Benevolence Counselor" Text="Yes" />
                            <Rock:RockCheckBox ID="cbSupportGroupsOfferred" runat="server" Label="Support Groups Offerred" Text="Yes" />
                            <Rock:RockCheckBox ID="cbSlidingFeeOfferred" runat="server" Label="Sliding Fee Offerred" Text="Yes" />
                            <Rock:RockCheckBox ID="cbWillowAttender" runat="server" Label="Willow Attender" Text="Yes" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <div class="row">
                                <div class="col-sm-6">
                                    <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                                </div>
                                <div class="col-sm-6">
                                    <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                                </div>
                            </div>
                            <Rock:DatePicker ID="dtpIntervieDate" runat="server" Label="Interview Date" />
                        </div>
                        <div class="col-md-6">
                            <Rock:EmailBox ID="ebEmail" runat="server" Label="EmailAddress" />
                            <div class="row">
                                <div class="col-sm-6">
                                    <Rock:PhoneNumberBox ID="pnbPhone" runat="server" Label="Phone Number" />
                                </div>
                                <div class="col-sm-6">
                                    <Rock:PhoneNumberBox ID="pnbMobilePhone" runat="server" Label="Mobile Phone Number" />
                                </div>
                            </div>
                        </div>
                    </div>

                    
                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>

                <fieldset id="fieldsetViewDetails" runat="server">

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <div class="row margin-b-md">
                        <div class="col-md-12">
                            <asp:Literal ID="lDescription" runat="server" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lDetails1" runat="server" />
                        </div>
                        <div class="col-md-6">
                            <asp:Literal ID="lDetails2" runat="server" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lDetails3" runat="server" />
                        </div>
                        <div class="col-md-6">
                            <asp:Literal ID="lDetails4" runat="server" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" Text="Edit" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnEdit_Click" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                    </div>

                </fieldset>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
