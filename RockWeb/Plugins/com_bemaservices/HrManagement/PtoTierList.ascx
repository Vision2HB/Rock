﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PtoTierList.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.HrManagement.PtoTierList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-plug"></i> Pto Teirs</h1>

                <div class="pull-right">
                    <asp:LinkButton ID="lbAddPtoTeir" runat="server" CssClass="btn btn-action btn-xs pull-right" OnClick="lbAddPtoTeir_Click" CausesValidation="false"><i class="fa fa-plus"></i></asp:LinkButton>
                </div>
            </div>
            <div class="panel-body">

                <div class="list-as-blocks clearfix">
                    <ul class="list-unstyled">
                        <asp:Repeater ID="rptPtoTeirs" runat="server">
                            <ItemTemplate>
                                <li>
                                    <asp:LinkButton ID="lbPtoTeir" runat="server" CommandArgument='<%# Eval("Id") %>' CommandName="Display">
                                        <i class='fa fa-clock'></i>
                                        <h3><%# Eval("Name") %> </h3>
                                    </asp:LinkButton>
                                </li>
                            </ItemTemplate>
                        </asp:Repeater>
                    </ul>
                </div>
            </div>
        </div>
        <script>
            $(".my-workflows .list-as-blocks li").on("click", function () {
                $(".my-workflows .list-as-blocks li").removeClass('active');
                $(this).addClass('active');
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
