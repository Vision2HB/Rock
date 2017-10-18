<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Dashboard.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.Dashboard" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><asp:Literal ID="lIcon" runat="server"></asp:Literal> Dashboard</h1>
            </div>
            <div class="panel-body">

                <div class="row margin-b-lg">
                    <div class="col-md-2">
                        <div class="form-horizontal label-sm">
                            <Rock:RockCheckBox ID="cbShowPending" runat="server" Text="Include Pending" AutoPostBack="true" OnCheckedChanged="cbFilter_CheckedChanged" />
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="form-horizontal label-sm">
                            <Rock:RockCheckBox ID="cbShowComplete" runat="server" Text="Include Completed" AutoPostBack="true" OnCheckedChanged="cbFilter_CheckedChanged" Help="Includes visits completed today." />
                        </div>
                    </div>
                </div>

                <div class="grid grid-panel">

                    <Rock:GridFilter ID="gFilter" runat="server">
                        <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                        <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                        <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                    </Rock:GridFilter>

                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />

                    <Rock:Grid ID="gVisits" runat="server" AllowSorting="true" OnRowSelected="gVisits_RowSelected" >
                        <Columns>
                            <Rock:SelectField />
                            <Rock:RockTemplateField HeaderText="Start Time" SortExpression="ActivatedDateTime">
                                <ItemTemplate><asp:Literal ID="lStartTime" runat="server" /></ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="WorkflowType.Name" HeaderText="Type" SortExpression="WorkflowType.Name" />
                            <Rock:RockTemplateField HeaderText="Guest Name">
                                <ItemTemplate><asp:Literal ID="lPrimaryContact" runat="server" /></ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateFieldUnselected HeaderStyle-CssClass="grid-columncommand" ItemStyle-CssClass="grid-columncommand" ItemStyle-HorizontalAlign="Center" >
                                <ItemTemplate><a id="aProfileLink" runat="server" class='btn btn-default btn-sm' target="_blank"><i class='fa fa-user'></i></a></ItemTemplate>
                            </Rock:RockTemplateFieldUnselected>
                            <Rock:RockTemplateField HeaderText="Pager" SortExpression="Pager">
                                <ItemTemplate><asp:Literal ID="lPager" runat="server" /></ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateField HeaderText="Language" >
                                <ItemTemplate><asp:Literal ID="lLanguage" runat="server" /></ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateField HeaderText="Status" SortExpression="Status">
                                <ItemTemplate><asp:Literal ID="lStatus" runat="server" /></ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateField HeaderText="First Visit" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate><asp:Literal ID="lFirstVisit" runat="server" /></ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateField HeaderText="Other Service Areas">
                                <ItemTemplate><asp:Literal ID="lOtherServiceAreas" runat="server" /></ItemTemplate>
                            </Rock:RockTemplateField>
                        </Columns>
                    </Rock:Grid>

                </div>

            </div>
        
        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgCancelWorkflow" runat="server" Title="Cancel" OnSaveClick="dlgCancelWorkflow_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="CancelWorkflow">
            <Content>
                <asp:HiddenField ID="hfWorkflowId" runat="server" />
                <Rock:RockDropDownList ID="ddlDlgCancelWorkflowReason" runat="server" Label="Cancel Reason" Required="true" ValidationGroup="CancelWorkflow" />
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
