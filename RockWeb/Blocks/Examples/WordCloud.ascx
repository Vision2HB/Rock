<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WordCloud.ascx.cs" Inherits="RockWeb.Blocks.Examples.WordCloud" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlView" runat="server">
            <asp:Panel Id="pnlSvgContainer" runat="server" CssClass="margin-all-md" />
            <Rock:RockTextBox ID="tbText" runat="server" Label="Words for the cloud" TextMode="MultiLine" Rows="10" />
            <asp:LinkButton Id="btnMakeCloud" CssClass="btn btn-primary" Text="Generate Cloud" runat="server" OnClick="btnMakeCloud_Click" />
            
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
