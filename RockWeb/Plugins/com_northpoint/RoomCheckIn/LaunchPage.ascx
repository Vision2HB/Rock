<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LaunchPage.ascx.cs" Inherits="RockWeb.Plugins.com_northpoint.RoomCheckIn.LaunchPage" %>

<meta name="viewport" content="width=device-width, initial-scale=1">

<style>
</style>

<script type="text/javascript">

    function pageLoad() {
        $('.tenkey a.digit').click(function () {
            $phoneNumber = $("input[id$='tbPIN']");
            $phoneNumber.val($phoneNumber.val() + $(this).html());
        });
        $('.tenkey a.back').click(function () {
            $phoneNumber = $("input[id$='tbPIN']");
            $phoneNumber.val($phoneNumber.val().slice(0, -1));
        });
        $('.tenkey a.clear').click(function () {
            $phoneNumber = $("input[id$='tbPIN']");
            $phoneNumber.val('');
        });

        // set focus to the input unless on a touch device
        var isTouchDevice = 'ontouchstart' in document.documentElement;
        if (!isTouchDevice) {
            if ($('.checkin-phone-entry').length) {
                $('.checkin-phone-entry').focus();
            }
        }
    }
    
</script>

<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfNextPage" runat="server" />

         <div class="text-center">
            <h1>
                <%= _pageTitle %>
            </h1>
         </div>
        <asp:Panel ID="pnlCampus" runat="server" CssClass="container text-center" >
           
            <div class="btn-group-vertical text-center">
                <asp:Repeater ID="rptCampus" runat="server" >
                    <ItemTemplate><br />
                        <a href="/room-checkin/?CampusId=<%# Eval("Id") %>" class="btn btn-primary btn-lg"><%# Eval("Name") %></a>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

        </asp:Panel>

        <asp:Panel ID="pnlLinks" runat="server" CssClass="text-center" >
            <asp:Repeater ID="rptLinks" runat="server">
                <ItemTemplate>
                    <div class="col-sm-6">
                        <h3><%# Eval("Name")  %></h3>
                        <a href="<%# Eval("Url") %>"><i style="color: #428bca;" class="<%# Eval("FontAwesomeIconClass") %>"></i></a>
                    </div>
                </ItemTemplate>
                <FooterTemplate>
                    <div class="col-sm-6">
                        <h3>Family Registration</h3>
                        <Rock:BootstrapButton ID="btnRegistration" runat="server" OnClick="btnRegistration_Click" ><i style="color: #428bca;" class="fa fa-address-card fa-10x"></i></Rock:BootstrapButton>
                    </div>
                    <div class="col-sm-6">
                        <h3>Check-In Manager</h3>
                        <Rock:BootstrapButton ID="btnManager" runat="server" OnClick="btnManager_Click" ><i style="color: #428bca;" class="fa fa-chart-bar fa-10x"></i></Rock:BootstrapButton>
                    </div>
                </FooterTemplate>
            </asp:Repeater>
        </asp:Panel>

        <Rock:ModalDialog ID="mdlPIN" runat="server" OnSaveClick="mdlPIN_SaveClick" SaveButtonText="Login">
            <Content>
                <asp:Panel ID="pnlPIN" runat="server" CssClass="text-center">
                    <asp:HiddenField ID="hfPINnextUrl" runat="server" />
                    <div class="checkin-search-body col-sm-offset-3 col-sm-6 ">
                        <Rock:RockTextBox ID="tbPIN" CssClass="checkin-phone-entry" TextMode="Password" runat="server" Label="PIN" />

                        <div class="tenkey checkin-phone-keypad">
                            <div>
                                <a href="#" class="btn btn-default btn-lg btn-keypad digit">1</a>
                                <a href="#" class="btn btn-default btn-lg btn-keypad digit">2</a>
                                <a href="#" class="btn btn-default btn-lg btn-keypad digit">3</a>
                            </div>
                            <div>
                                <a href="#" class="btn btn-default btn-lg btn-keypad digit">4</a>
                                <a href="#" class="btn btn-default btn-lg btn-keypad digit">5</a>
                                <a href="#" class="btn btn-default btn-lg btn-keypad digit">6</a>
                            </div>
                            <div>
                                <a href="#" class="btn btn-default btn-lg btn-keypad digit">7</a>
                                <a href="#" class="btn btn-default btn-lg btn-keypad digit">8</a>
                                <a href="#" class="btn btn-default btn-lg btn-keypad digit">9</a>
                            </div>
                            <div>
                                <a href="#" class="btn btn-default btn-lg btn-keypad command clear">Clr</a>
                                <a href="#" class="btn btn-default btn-lg btn-keypad digit">0</a>
                                <a href="#" class="btn btn-default btn-lg btn-keypad command back"><i class="fas fa-backspace"></i></a>
                            </div>
                        </div>
                    </div>
                    <div class="col-sm-3"></div>
                </asp:Panel>
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalAlert ID="maWarning" runat="server" />


    </ContentTemplate>
</asp:UpdatePanel>

