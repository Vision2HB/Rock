<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EvacuationReport.ascx.cs" Inherits="RockWeb.Plugins.com_northpoint.RoomCheckIn.EvacuationReport" %>

<style>
    @media screen {
        .printonly {
            display: none;
        }
    }

    @media print {
        .pg {
            page-break-before: always;
        }

        .noprint {
            display: none;
        }

        .printonly {
            display: block;
        }
    }

    .evacTitle {
        font-size: 20px;
        font-weight: bold;
        margin-top: 10px;
        margin-bottom: 10px;
        width: 500px;
    }

    .evacSummary {
        font-size: 16px;
    }

    .evacTotal {
        font-size: 16px;
        font-weight: normal;
    }

    .evacGrade {
        font-size: 20px;
        font-weight: bold;
        margin-top: 10px;
        margin-bottom: 10px;
        width: 500px;
    }

    .evacSmallGroup {
        font-size: 16px;
        font-weight: bold;
        margin-left: 50px;
        margin-top: 10px;
        margin-bottom: 10px;
        width: 500px;
    }

    .evacChild {
        font-size: 14px;
        margin-left: 100px;
        width: 380px;
        border-bottom: 1px solid silver;
    }

    .evacId {
        width: 80px;
    }

    .evacName {
        width: 500px;
    }
</style>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
       
        <div class="panel-body">
            <div class="row noprint">
                <div class="col-sm-2">
                    <a href="javascript:window.history.back()" class="btn btn-default btn-block">< Back</a>
                </div>
                <div class="col-sm-3">
                    <a href="javascript:window.location.reload()" class="btn btn-primary btn-block">Refresh Report</a>
                </div>
                <div class="col-sm-2 col-sm-offset-5">
                    <a href="#" onclick="printCustom()" class="btn btn-primary btn-block">Print</a>
                </div>
            </div>

            <asp:Literal ID="lHtml" runat="server" />
        </div>

    </ContentTemplate>
</asp:UpdatePanel>

<script>
    function printCustom() {
        if (navigator.userAgent.match(/(iPhone|iPod|iPad|Android)/)) {
            var page = location.href;
            cordova.plugins.printer.print(page, 'EvacReport.html');
        } else {
            window.print();
        }
    }
</script>
