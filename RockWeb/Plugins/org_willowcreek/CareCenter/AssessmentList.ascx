<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AssessmentList.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.AssessmentList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-check-square-o"></i> <asp:Literal ID="lPersonName" runat="server" /> Assessments</h1>
            </div>
            <div class="panel-body">

                <asp:Panel ID="pnlMainFilter" runat="server" CssClass="row margin-b-md">
                    <div class="col-md-3">
                        <Rock:RockDropDownList ID="ddlAssessmentType" runat="server" Label="Assessment Type" CssClass="input-width-lg" AutoPostBack="true" OnSelectedIndexChanged="ddlFilter_SelectedIndexChanged" />
                    </div>
                </asp:Panel>

                <div class="grid grid-panel">

                    <Rock:GridFilter ID="gfAssessmentFilter" runat="server">
                        <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                        <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                        <Rock:SlidingDateRangePicker ID="drpAssessmentDate" runat="server" Label="Date Range" PreviewLocation="None" />
                        <Rock:RockCheckBox ID="cbIncludeCompleted" runat="server" Label="Include Completed Assessments" Text="Yes" />
                    </Rock:GridFilter>

                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />

                    <Rock:Grid ID="gAssessments" runat="server" AllowSorting="true" RowItemText="Assessment" OnRowSelected="gAssessments_RowSelected" >
                        <Columns>
                            <Rock:DateTimeField DataField="AssessmentDate" HeaderText="Intake Date/Time" SortExpression="AssessmentDate" Visible="false" />
                            <Rock:TimeField DataField="AssessmentDate" HeaderText="Intake Time" SortExpression="AssessmentDate" />
                            <Rock:RockBoundField DataField="PersonAlias.Person" HeaderText="Primary Contact" SortExpression="PersonAlias.Person.LastName,PersonAlias.Person.NickName" />
                            <Rock:RockTemplateFieldUnselected HeaderStyle-CssClass="grid-columncommand" ItemStyle-CssClass="grid-columncommand" ItemStyle-HorizontalAlign="Center" >
                                <ItemTemplate><a id="aProfileLink" runat="server" class='btn btn-default btn-sm' target="_blank"><i class='fa fa-user'></i></a></ItemTemplate>
                            </Rock:RockTemplateFieldUnselected>
                            <Rock:RockTemplateField HeaderText="Assessment Types">
                                <ItemTemplate><asp:Literal ID="lAssessmentTypes" runat="server" /></ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="ApprovedByPersonAlias.Person" HeaderText="Approved By" SortExpression="ApprovedByPersonAlias.Person.LastName,ApprovedByPersonAlias.Person.NickName" />
                            <Rock:RockTemplateField HeaderText="First Assessment" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate><asp:Literal ID="lFirstAssessment" runat="server" /></ItemTemplate>
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
