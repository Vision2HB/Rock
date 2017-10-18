
<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AssessmentDashboard.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.AssessmentDashboard" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><asp:Literal ID="lIcon" runat="server"></asp:Literal> Dashboard</h1>
            </div>
            <div class="panel-body">

                <div class="row margin-b-lg">
                    <div class="col-md-3 col-sm-6">
                        <div class="form-horizontal label-sm">
                            <Rock:RockCheckBox ID="cbShowComplete" runat="server" Text="Include Completed" AutoPostBack="true" OnCheckedChanged="Filter_Changed" Help="Includes assessments completed today." />
                        </div>
                    </div>
                    <div class="col-md-4 col-sm-6">
                        <div class="form-horizontal label-lg">
                            <Rock:RockDropDownList ID="ddlAssessmentType" runat="server" Label="Type" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed" />
                        </div>
                    </div>
                    <div class="col-md-5 col-sm-12">
                        <div class="form-horizontal label-sm">
                            <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed" />
                        </div>
                    </div>
                </div>

                <div class="grid grid-panel">

                    <Rock:GridFilter ID="gFilter" runat="server">
                        <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                        <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                        <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                        <Rock:SlidingDateRangePicker ID="drpDate" runat="server" Label="Date Range" PreviewLocation="None" />
                        <asp:PlaceHolder ID="phAttributeFilters" runat="server" />
                    </Rock:GridFilter>

                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />

                    <Rock:Grid ID="gAssessments" runat="server" AllowSorting="true" OnRowSelected="gAssessments_RowSelected" RowItemText="Assessment" >
                        <Columns>
                            <Rock:RockBoundField DataField="Id" SortExpression="Id" HeaderText="Id" />
                            <Rock:RockTemplateField HeaderText="Started" SortExpression="ActivatedDateTime">
                                <ItemTemplate><asp:Literal ID="lStartTime" runat="server" /></ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="WorkflowType.Name" HeaderText="Type" SortExpression="WorkflowType.Name" />
                            <Rock:RockTemplateField HeaderText="Guest Name">
                                <ItemTemplate><asp:Literal ID="lPrimaryContact" runat="server" /></ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateFieldUnselected HeaderStyle-CssClass="grid-columncommand" ItemStyle-CssClass="grid-columncommand" ItemStyle-HorizontalAlign="Center" >
                                <ItemTemplate><a id="aProfileLink" runat="server" class='btn btn-sm' target="_blank"><i class='fa fa-user'></i></a></ItemTemplate>
                            </Rock:RockTemplateFieldUnselected>
                            <Rock:RockBoundField DataField="Status" HeaderText="Status" SortExpression="Status" />
                        </Columns>
                    </Rock:Grid>

                </div>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
