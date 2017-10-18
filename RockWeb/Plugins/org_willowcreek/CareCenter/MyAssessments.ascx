<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MyAssessments.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.MyAssessments" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
       
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-check-square-o"></i> Assessments</h1>
            </div>

            <div class="panel-body">
                <ul>
                    <asp:Repeater ID="rptAssessments" runat="server">
                        <ItemTemplate>
                            <li>
                                <asp:LinkButton ID="lbAssessment" runat="server" CommandArgument='<%# Eval("Id") %>' CommandName="Display">
                                    <%# ((DateTime)Eval("AssessmentDate")).ToShortDateString() %>: <%# Eval("Person") %> - <%# Eval("Workflows") %>
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
