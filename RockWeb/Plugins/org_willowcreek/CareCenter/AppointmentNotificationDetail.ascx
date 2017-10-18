<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AppointmentNotificationDetail.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.AppointmentNotificationDetail" %>

<asp:UpdatePanel ID="pnlAppointmentNotificationUpdatePanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfAppointmentNotificationId" runat="server" />
            
            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lIconHtml" runat="server"><i class="fa fa-comment-o"></i></asp:Literal>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </h1>
            </div>
            
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>

            <div class="panel-body">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div id="pnlEditDetails" runat="server">
                    <asp:ValidationSummary ID="valAppointmentNotificationDetail" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbName" runat="server" Label="Notification Name" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbFromName" runat="server" Label="From Name" />
                        </div>
                        <div class="col-md-6">
                            <Rock:EmailBox ID="tbFromEmail" runat="server" Label="From Email" />
                        </div>
                    </div>

                    <Rock:PanelWidget ID="pwAnnouncements" runat="server" Title="Announcement" Expanded="true" >
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbSendAnnouncement" runat="server" Label="Send Announcement" Text="Yes" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbAnnouncementSubject" runat="server" Label="Announcement Email Subject" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:CodeEditor ID="ceAnnouncementBody" runat="server" Label="Announcement Email Body" EditorMode="Lava" EditorTheme="Rock" EditorHeight="200" />
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="pwReminders" runat="server" Title="Reminders" Expanded="true" >
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbSendReminders" runat="server" Label="Send Reminders" Text="Yes" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbDaysAhead" runat="server" Label="Days Ahead" 
                                    Help="Comma-Delimited list of the number of days ahead of apointment that reminder(s) should be sent. For example a value of 10,5 will result in a reminder email getting sent 10 days prior and 5 days prior to the appointment." />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbReminderSubject" runat="server" Label="Reminder Email Subject" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:CodeEditor ID="ceReminderBody" runat="server" Label="Reminder Email Body" EditorMode="Lava" EditorTheme="Rock" EditorHeight="200" />
                            </div>
                        </div>
                    </Rock:PanelWidget>
                    
                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>

            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
