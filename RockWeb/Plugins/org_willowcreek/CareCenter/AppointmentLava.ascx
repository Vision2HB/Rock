<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AppointmentLava.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.AppointmentLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Literal ID="lResults" runat="server" />

        <asp:Literal ID="lDebug" runat="server" />

    </ContentTemplate>
</asp:UpdatePanel>
