﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AutoConfigure.ascx.cs" Inherits="RockWeb.Plugins.com_northpoint.KioskTypes.AutoConfigure" %>

<%--<script>
    var getClientName = function ()
    {
        try
        {
            var client = device.uuid;
            if (typeof (client) === undefined) {
                __doPostBack("UseDNS", "");
            }
            else {
                __doPostBack("ClientName", client);
            }
            
        }
        catch (e)
        {
            __doPostBack("UseDNS", "");
        }
    }
</script>--%>

<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <asp:Panel runat="server" Visible="false" ID="pnlMain">
            <Rock:NotificationBox runat="server" NotificationBoxType="Danger" ID="nbNotFound" Visible="false">
                <h2>Sorry</h2>
                    There was a problem with the configuration of this kiosk. Please contact an administrator to resolve.
            </Rock:NotificationBox>
            <asp:Panel runat="server" ID="nbWait" CssClass="alert alert-info" Visible="false">
               
                <h2>Redirecting</h2>
                This page will redirect with preset Kiosk settings.
            </asp:Panel>
            <asp:Literal Text="" ID="ltKioskName" runat="server" />
        </asp:Panel>
        <asp:Panel ID="pnlManual" Visible="false" runat="server">
            <Rock:RockDropDownList runat="server" ID="ddlKioskType" Label="Kiosk Type" DataTextField="Name" DataValueField="Id" Required="true"></Rock:RockDropDownList>
            <Rock:RockDropDownList runat="server" ID="ddlKioskPrinter" Label="Printer" DataTextField="Name" DataValueField="Id"></Rock:RockDropDownList>
            <Rock:RockTextBox ID="tbKioskName" runat="server" Label="Kiosk Name" Required="true" />
        </asp:Panel>
        <asp:Panel ID="pnlKioskDropdown" Visible="false" runat="server">
            <Rock:RockDropDownList runat="server" ID="ddlKiosk" Label="Kiosk" DataTextField="Name" DataValueField="Id" Required="true" />
        </asp:Panel>
        <asp:Panel ID="pnlSelect" Visible="true" runat="server">
            <Rock:BootstrapButton ID="btnSelectKiosk" runat="server" OnClick="btnSelectKiosk_Click" CssClass="btn btn-lg btn-primary btn-block" Text="Continue" />
        </asp:Panel>
        
        
    </ContentTemplate>
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="Timer1" EventName="Tick" />
    </Triggers>
</asp:UpdatePanel>
<asp:Timer ID="Timer1" runat="server" Interval="30000" OnTick="Timer1_Tick">
</asp:Timer>
