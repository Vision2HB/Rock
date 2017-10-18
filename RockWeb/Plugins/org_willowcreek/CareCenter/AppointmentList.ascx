<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AppointmentList.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.AppointmentList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-calendar"></i> <asp:Literal id="lPersonName" runat="server" /> Appointments</h1>
            </div>
            <div class="panel-body">

                <asp:Panel ID="pnlMainFilter" runat="server" cssClass="row margin-b-md">
                    <div class="col-md-12">
                        <Rock:RockDropDownList ID="ddlServiceAreas" runat="server" Label="Service Area" CssClass="input-width-lg" AutoPostBack="true" OnSelectedIndexChanged="ddlServiceAreas_SelectedIndexChanged" />
                    </div>
                </asp:Panel>

                <div class="grid grid-panel">

                    <Rock:GridFilter ID="gfAppointmentFilter" runat="server">
                        <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                        <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                        <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                        <Rock:SlidingDateRangePicker ID="drpAppointmentDate" runat="server" Label="Date Range" PreviewLocation="None" />
                    </Rock:GridFilter>

                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />

                    <Rock:Grid ID="gAppointments" runat="server" AllowSorting="true" RowItemText="Appointment" OnRowSelected="gAppointments_RowSelected" ExportSource="ColumnOutput" >
                        <Columns>
                            <Rock:RockBoundField DataField="TimeSlot.ServiceArea.Name" HeaderText="Service Area" SortExpression="TimeSlot.ServiceArea.Name" />
                            <Rock:RockBoundField DataField="PersonAlias.Person.FullName" HeaderText="Person" SortExpression="PersonAlias.Person.LastName,PersonAlias.Person.NickName" />
                            <Rock:RockTemplateFieldUnselected HeaderStyle-CssClass="grid-columncommand" ItemStyle-CssClass="grid-columncommand" ItemStyle-HorizontalAlign="Center" >
                                <ItemTemplate><a id="aProfileLink" runat="server" class='btn btn-default btn-sm' target="_blank"><i class='fa fa-user'></i></a></ItemTemplate>
                            </Rock:RockTemplateFieldUnselected>
                            <Rock:DateField DataField="AppointmentDate" HeaderText="Date" SortExpression="AppointmentDate" DataFormatString="{0:D}" />
                            <Rock:RockBoundField DataField="TimeSlot.DailyTitle" HeaderText="Time"  />
                            <Rock:RockTemplateField HeaderText="Status">
                                <ItemTemplate><asp:Literal ID="lStatus" runat="server" /></ItemTemplate>
                            </Rock:RockTemplateField>
                        </Columns>
                    </Rock:Grid>

                </div>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
