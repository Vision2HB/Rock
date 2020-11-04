<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DefaultGroupSelect.ascx.cs" Inherits="RockWeb.Plugins.com_northpoint.RoomCheckIn.DefaultGroupSelect" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('a.btn-checkin-select').click(function () {
            $(this).siblings().attr('onclick', 'return false;');
        });
    });
</script>

<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>
    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="checkin-header">
        <h1>
            <Rock:RockLiteral ID="lHeader" runat="server" Text="" />
        </h1>
    </div>
                
    <div class="checkin-body">
        
        <div class="checkin-scroll-panel">
            <div class="scroller">

                <div class="control-group checkin-body-container">
                    <label class="control-label">
                        <Rock:RockLiteral ID="lSelectLocation" runat="server" />
                    </label>
                    <Rock:RockLiteral ID="lBody" runat="server" />
                    
                    
                    
                </div>
            </div>
        </div>
    </div>

     <div class="checkin-footer">   
        <div class="checkin-actions">
            <asp:LinkButton CssClass="btn btn-default" ID="lbBack" runat="server" OnClick="lbBack_Click" Text="Back" />
            <asp:LinkButton CssClass="btn btn-default btn-cancel" ID="lbCancel" runat="server" OnClick="lbCancel_Click" Text="Cancel" />
            <asp:LinkButton CssClass="btn btn-primary btn-pull-right" ID="lbSelect" runat="server" OnClick="lbSelect_Click" Text="Next" data-loading-text="Loading..." />
            
        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
