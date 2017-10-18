<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TimeSlotDetail.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.TimeSlotDetail" %>

<asp:UpdatePanel ID="upServiceAreaTimeSlot" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfServiceAreaId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-calendar"></i>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </h1>
            </div>

            <div class="panel-body">

                <asp:ValidationSummary ID="valTimeSlot" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="ServiceTimeSlotGroup" />
                <Rock:NotificationBox ID="nbTimeSlot" runat="server" NotificationBoxType="Danger" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbScheduleTitle" runat="server" Label="Title" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" Text="Yes" />
                    </div>
                </div>
                    
                <div class="row">

                    <div class="col-md-6">
                        <Rock:ScheduleBuilder ID="sbSchedule" runat="server" Label="Schedule" ShowDuration="true" ShowScheduleFriendlyTextAsToolTip="true" />
                        <Rock:RockDropDownList ID="ddlNotification" runat="server" Label="Notification" />
                        <Rock:RockTextBox ID="tbDailyTitle" runat="server" Label="Daily Title" />
                    </div>

                    <div class="col-md-6">

                        <Rock:RockCheckBox ID="cbAllowPublicRegistration" runat="server" Label="Allow Public Registration" Text="Yes" />

                        <div class="row">
                            <div class="col-xs-6">
                                <Rock:NumberBox ID="nbRegistrationLimit" runat="server" Label="Registration Limit" />
                            </div>
                            <div class="col-xs-6">
                                <Rock:NumberBox ID="nbWalkupLimit" runat="server" Label="Walk-Up Limit" />
                            </div>
                        </div>                            

                    </div>

                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
