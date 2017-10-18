<%@ Control Language="C#" AutoEventWireup="true" CodeFile="VisitList.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.VisitList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-users"></i> <asp:Literal ID="lPersonName" runat="server" /> Visits</h1>
            </div>
            <div class="panel-body">

               
                <asp:Panel ID="pnlMainFilter" runat="server" CssClass="row margin-b-md">
                    <div class="col-md-3">
                        <Rock:RockDropDownList ID="ddlServiceArea" runat="server" Label="Service Areas" CssClass="input-width-lg" AutoPostBack="true" OnSelectedIndexChanged="ddlFilter_SelectedIndexChanged" />
                    </div>
                    <div class="col-md-3">
                        <Rock:RockDropDownList ID="ddlPassportStatus" runat="server" Label="Passport Status" CssClass="input-width-md" AutoPostBack="true" OnSelectedIndexChanged="ddlFilter_SelectedIndexChanged" />
                    </div>
                </asp:Panel>

                <div class="grid grid-panel">

                    <Rock:GridFilter ID="gfVisitFilter" runat="server">
                        <Rock:NumberBox ID="nbPagerNumber" runat="server" Label="Pager Number" />
                        <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                        <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                        <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                        <Rock:SlidingDateRangePicker ID="drpVisitDate" runat="server" Label="Date Range" PreviewLocation="None" />
                    </Rock:GridFilter>

                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />

                    <Rock:Grid ID="gVisits" runat="server" AllowSorting="true" RowItemText="Visit" OnRowSelected="gVisits_RowSelected" >
                        <Columns>
                            <Rock:SelectField />
                            <Rock:DateTimeField DataField="VisitDate" HeaderText="Intake Date/Time" SortExpression="VisitDate" Visible="false" />
                            <Rock:TimeField DataField="VisitDate" HeaderText="Intake Time" SortExpression="VisitDate" />
                            <Rock:RockBoundField DataField="PersonAlias.Person" HeaderText="Primary Contact" SortExpression="PersonAlias.Person.LastName,PersonAlias.Person.NickName" />
                            <Rock:RockTemplateFieldUnselected HeaderStyle-CssClass="grid-columncommand" ItemStyle-CssClass="grid-columncommand" ItemStyle-HorizontalAlign="Center" >
                                <ItemTemplate><a id="aProfileLink" runat="server" class='btn btn-default btn-sm' target="_blank"><i class='fa fa-user'></i></a></ItemTemplate>
                            </Rock:RockTemplateFieldUnselected>
                            <Rock:RockTemplateField HeaderText="Status">
                                <ItemTemplate><asp:Literal ID="lStatus" runat="server" /></ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateField HeaderText="Service Areas" ItemStyle-CssClass="col-serviceareas">
                                <ItemTemplate><asp:Literal ID="lServiceAreas" runat="server" /></ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="PagerId" HeaderText="Pager" SortExpression="PagerId" />
                            <Rock:RockTemplateField HeaderText="First Visit" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate><asp:Literal ID="lFirstVisit" runat="server" /></ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateField HeaderText="Passport" SortExpression="PassportStatus"  ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" >
                                <ItemTemplate><asp:Literal ID="lPassport" runat="server" /></ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateField HeaderText="Notes" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" >
                                <ItemTemplate><asp:Literal ID="lNotes" runat="server" /></ItemTemplate>
                            </Rock:RockTemplateField>
                        </Columns>
                    </Rock:Grid>

                    <Rock:NotificationBox ID="nbResult" runat="server" Visible="false" CssClass="margin-b-none" Dismissable="true"></Rock:NotificationBox>

                </div>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
