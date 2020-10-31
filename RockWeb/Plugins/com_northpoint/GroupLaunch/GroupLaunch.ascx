<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupLaunch.ascx.cs" Inherits="RockWeb.Plugins.com_northpoint.GroupLaunch.GroupLaunch" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server">
            <div id="pnlGroupLaunch" runat="server" visible="false">
                <div class="panel panel-block">

                    <div class="panel-heading">
                        <h1 class="panel-title pull-left">
                            <i class="fa fa-clipboard-list"></i>
                            <asp:Literal ID="lHeading" runat="server" Text="Group Launch" />
                        </h1>

                        <div class="panel-labels">
                            <span runat="server" id="spanSyncLink" visible="false" data-toggle="popover" class="label label-info js-sync-popover" data-trigger="hover click focus" data-html="true" data-placement="left" data-delay="{&quot;hide&quot;: 1500}"><i class='fa fa-exchange'></i></span>&nbsp;           
                        </div>
                    </div>

                    <div class="panel-body">
                        <style>
                            .actions {
                                margin-top: 2rem;
                            }
                        </style>

                        <div id="pnlConnected" runat="server" visible="false">
                            <Rock:NotificationBox ID="FinalizedNotification" runat="server" CssClass="padding-top-md" NotificationBoxType="Info" Title="Please DO NOT Make Any Changes To This Group"
                                        Text="It is currently open for registration in GroupLaunch and will interfere with the synchronization between systems." Visible="false" />

                            <p>Status: <Rock:HighlightLabel ID="ConnectedHighlight" runat="server" LabelType="Success" Text="Connected" /></p>
                            <p>Name: <asp:Literal id="groupLaunchGroupName" runat="server" /></p>
                            <div class="actions">
                                <asp:HyperLink 
                                    ID="ManageGroupLaunchLink"
                                    runat="server"
                                    Text="Manage in GroupLaunch"
                                    Class="btn btn-primary"
                                    Target="_blank"
                                    >
                                </asp:HyperLink>
                                        
                                <asp:LinkButton ID="ReConnectButton" runat="server" Text="Re-Publish" CssClass="btn btn-primary" OnClick="btnConnect_Click" CausesValidation="false" Visible="false" />
                            </div>
                        </div>

                        <div id="pnlUnconnected" runat="server" visible="false">
                            Status: <Rock:HighlightLabel ID="UnconnectedHighlight" runat="server" LabelType="Default" Text="Unconnected" />   
                            <br /><br />
                            <div class="actions">
                                <asp:LinkButton ID="ConnectButton" runat="server" Text="Connect" CssClass="btn btn-primary" OnClick="btnConnect_Click" CausesValidation="false" />
                            </div>
                        </div>

                        <div id="pnlBuildConnection" runat="server" visibe="false">
                            <Rock:NotificationBox ID="nbWarning" runat="server" Dismissable="true" Visible="false" />
                            <Rock:RockDropDownList ID="ddlGroupCollections" runat="server" Label="Group Collections" Help="Pick the Group Collection" Required="true" />
                            <Rock:RockDropDownList ID="ddlGroupLeaderRole" runat="server" Label="Rock Leader Role" Help="If someone has the role of Leader in the Groups Reg system, what role do you want them to have in Rock?" Required="true" />
                            <Rock:RockTextBox ID="groupNameText" runat="server" Label="Group Name" Help="Name of the group in GroupLaunch" />

                            <Rock:ModalDialog ID="mdConfirmRePublish" runat="server" Title="Confirm GroupLaunch Remove and Create"
                                OnSaveClick="mdConfirmRePublish_SaveClick" OnCancelScript="clearActiveDialog();" SaveButtonText="Re-Publish">
                                <Content>
                                    <p>This will delete the existing group in GroupLaunch before re-creating with current Rock Group.</p>
                                    <p>Are you sure you want to re-publish to GroupLaunch?</p>
                                </Content>
                            </Rock:ModalDialog>

                            <div class="actions">
                                <asp:LinkButton ID="btnConnectPublish" runat="server" Text="Publish" CssClass="btn btn-primary" OnClick="btnPublish_Click" CausesValidation="false" />
                                <asp:LinkButton ID="btnRePublish" runat="server" Text="Re-Publish" CssClass="btn btn-primary" OnClick="btnRePublish_Click" Visible="false" />
                                <asp:LinkButton ID="btnCancelConnect" runat="server" Text="Cancel" CssClass="btn" OnClick="btnCancelConnect_Click" CausesValidation="false" />
                            </div>
                        </div>

                    </div>
                </div>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>