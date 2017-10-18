<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Resources.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.Resources" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:PanelWidget ID="pwSelected" runat="server" Expanded="true">

            <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
            <Rock:NotificationBox ID="nbEmailMessage" runat="server" Visible="false" />

            <div class="row">
                <div class="col-sm-9">
                    <Rock:RockControlWrapper ID="rcwSelected" runat="server" Label="Selected Resources">
                        <asp:Repeater ID="rptrSelected" runat="server" Visible="false" >
                            <HeaderTemplate>
                                <ul class="list-unstyled">
                            </HeaderTemplate>
                            <ItemTemplate>
                                <li>
                                    <asp:LinkButton ID="lbClear" runat="server" CommandArgument='<%# Eval("Key") %>' CommandName="Delete" CausesValidation="false"><i class="fa fa-times"></i></asp:LinkButton>
                                    <%# Eval("Value") %>
                                </li>
                            </ItemTemplate>
                            <FooterTemplate>
                                </ul>
                            </FooterTemplate>
                        </asp:Repeater>
                        <asp:Literal ID="lNone" runat="server" Text="None" Visible="false" />
                    </Rock:RockControlWrapper>
                </div>
                <div class="col-sm-3">
                    <Rock:RockDropDownList ID="ddlBenevolence" runat="server" Label="Benevolence" AutoPostBack="true" Required="true" OnSelectedIndexChanged="ddlBenevolence_SelectedIndexChanged"
                        Help="Does this referral include Benevolence?" >
                        <asp:ListItem Text="" Value="" />
                        <asp:ListItem Text="No" Value="False" />
                        <asp:ListItem Text="Yes" Value="True" />
                    </Rock:RockDropDownList>
                </div>
            </div>

            <div class="actions pull-right">
                <asp:LinkButton 
                    ID="lbReturn" runat="server" Text="Return" CssClass="btn btn-default" Visible="true" CausesValidation="false" OnClick="lbReturn_Click" />&nbsp;<asp:HyperLink 
                    ID="hlPrint" runat="server" Text="Print" CssClass="btn btn-primary print-resources" Target="_blank" />&nbsp;<asp:LinkButton 
                    ID="btnEmail" runat="server" Text="Email" CssClass="btn btn-primary" OnClick="btnEmail_Click" />
            </div>

        </Rock:PanelWidget>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-bookmark"></i> Find Resources</h1>
                <asp:LinkButton ID="lbShowFilter" runat="server" CssClass="pull-right" OnClick="lbShowFilter_Click">Show Additional Criteria <i class="fa fa-chevron-down"></i></asp:LinkButton>
            </div>

            <div class="panel-body">

                <div class="margin-b-md">

                    <asp:Panel ID="pnlMainSearch" runat="server" CssClass="row">
                        <div class="col-sm-6">
                            <asp:Literal ID="lSearchLeftTitle" runat="server" />
                            <asp:PlaceHolder ID="phMainSearchLeft" runat="server" />
                        </div>
                        <div class="col-sm-6">
                            <asp:Literal ID="lSearchRightTitle" runat="server" />
                            <asp:PlaceHolder ID="phMainSearchRight" runat="server" />
                        </div>
                    </asp:Panel>

                    <asp:Panel ID="pnlFilterSearch" runat="server" Visible="false">
                        <hr />
                        <asp:PlaceHolder ID="phSecondarySearch" runat="server" />
                    </asp:Panel>

                    <asp:LinkButton ID="lbSearch" runat="server" Text="Find Resources" CssClass="btn btn-primary" OnClick="lbSearch_Click" CausesValidation="false" />
                    <asp:LinkButton ID="lbClearSearch" runat="server" Text="Clear" CssClass="btn btn-default" OnClick="lbClearSearch_Click" CausesValidation="false" />

                </div>

                <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Warning" Title="Criteria Required" Text="<p>Please enter some criteria before searching.</p>" Visible="false" />
                <Rock:NotificationBox ID="nbNoResults" runat="server" NotificationBoxType="Warning" Title="No Results" Text="<p>There were not any results that matched your criteria.</p>" Visible="false" />
                
                <asp:Repeater ID="rptrResults" runat="server" Visible="false" >
                    <ItemTemplate>
                        <div class='panel panel-default'>
                            <div class='panel-heading'>
                                <strong><%# FormatHeader( Container.DataItem ) %></strong>
                                <span class='pull-right'>
                                    <asp:LinkButton ID="lbSelect" runat="server" CssClass="btn btn-xs btn-default" CausesValidation="false" CommandArgument='<%# Eval("Id") %>' CommandName="Select" Text="Unselected" />
                                    <asp:LinkButton ID="lbUnselect" runat="server" CssClass="btn btn-xs btn-success" CausesValidation="false" CommandArgument='<%# Eval("Id") %>' CommandName="Unselect" Text="Selected" />
                                    <asp:LinkButton ID="lbDetails" runat="server" CssClass="btn btn-xs btn-default" CausesValidation="false" CommandArgument='<%# Eval("Id") %>' CommandName="Details" Text="Details" />
                                </span>
                            </div>
                            <div class='panel-body'>
                                <%# FormatContent( Container.DataItem ) %>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

            </div>
        
        </asp:Panel>

        <Rock:ModalDialog ID="dlgDetails" runat="server" Title="Details" >
            <Content>
                <asp:HiddenField ID="hfResourceId" runat="server" />
                <asp:Literal ID="lContent" runat="server" />
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
