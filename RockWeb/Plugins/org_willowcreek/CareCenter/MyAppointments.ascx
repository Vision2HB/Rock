<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MyAppointments.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.MyAppointments" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
       
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-calendar"></i> Appointments</h1>
            </div>

            <div class="panel-body">
                    <ul>
                        <asp:Repeater ID="rptAppointments" runat="server">
                            <ItemTemplate>
                                <li>
                                    <asp:LinkButton ID="lbAppointment" runat="server" CommandArgument='<%# Eval("Id") %>' CommandName="Display">
                                        <%# ((DateTime)Eval("AppointmentDate")).ToShortDateString() %>: <%# Eval("Person") %> - <%# Eval("Workflow") %>
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
