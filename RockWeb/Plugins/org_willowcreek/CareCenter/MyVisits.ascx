<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MyVisits.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.MyVisits" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
       
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-calendar-plus-o"></i> Visits</h1>
            </div>

            <div class="panel-body">
                <ul>
                    <asp:Repeater ID="rptVisits" runat="server">
                        <ItemTemplate>
                            <li>
                                <asp:LinkButton ID="lbVisit" runat="server" CommandArgument='<%# Eval("Id") %>' CommandName="Display">
                                    <%# ((DateTime)Eval("VisitDate")).ToShortDateString() %>: <%# Eval("Person") %> - <%# Eval("Workflows") %>
                                </asp:LinkButton>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>

                <div class="actions">
                    <asp:LinkButton ID="lbShowAll" runat="server" Text="Show All" CssClass="btn btn-link pull-right" OnClick="lbShowAll_Click" />
                </div>

            </div>
        
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
