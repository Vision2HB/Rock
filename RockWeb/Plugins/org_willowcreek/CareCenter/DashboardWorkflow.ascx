<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DashboardWorkflow.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.DashboardWorkflow" %>

<asp:HiddenField ID="hfDashboardPageId" runat="server" />
<div class="actions">
    <asp:LinkButton ID="btnDashboard" runat="server" Text="Dashboard" CssClass="btn btn-default" OnClick="btnDashboard_Click" />
    <asp:HyperLink ID="hlAddAppointment" runat="server" Text="Add Appointment" CssClass="btn btn-default" Target="_blank" Visible="false" />
</div>
