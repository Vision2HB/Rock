<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ServiceMetricsEntry5.ascx.cs" Inherits="com.bemadev.Blocks.Reporting.ServiceMetricsEntry5" %>



<asp:UpdatePanel ID="upnlContent" runat="server">
<ContentTemplate>

    <div class="panel panel-block">

        <div class="panel-heading">
            <h1 class="panel-title"><i class="fa fa-signal"></i> Metric Entry</h1>
        </div>

        <div class="panel-body">

            <asp:Panel ID="pnlSelection" runat="server">

                <h3><asp:Literal ID="lSelection" runat="server"></asp:Literal></h3>

                <asp:Repeater ID="rptrSelection" runat="server" OnItemCommand="rptrSelection_ItemCommand" >
                    <ItemTemplate>
                        <asp:LinkButton ID="lbSelection" runat="server" CommandName='<%# Eval("CommandName") %>'  CommandArgument='<%# Eval("CommandArg") %>' Text='<%# Eval("OptionText") %>' CssClass="btn btn-default btn-block" />
                    </ItemTemplate>
                </asp:Repeater>       

            </asp:Panel>

            <asp:Panel ID="pnlMetrics" runat="server" Visible="false">

                <div class="btn-group btn-group-justified margin-b-lg panel-settings-group" >
                    <Rock:ButtonDropDownList ID="bddlCampus" runat="server" OnSelectionChanged="bddl_SelectionChanged" />
                    <Rock:ButtonDropDownList ID="bddlWeekend" runat="server" OnSelectionChanged="bddl_SelectionChanged" />
                </div>

                <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="nbMetricsSaved" runat="server" Text="Metric Values Have Been Updated" NotificationBoxType="Success" Visible="false" />

                <div class="form-horizontal label-md" >
                    <div class="container">
                        <div class="row">
                            <div class="col-md-4">
                                <Rock:RockLiteral ID="tbHeader1" runat="server" Visible="true" Text=''></Rock:RockLiteral>
                            </div>
                            <div class="col-md-2">
                                <Rock:RockLiteral ID="tbHeader2" runat="server" Visible="false" Text=''></Rock:RockLiteral>
                            </div>
                            <div class="col-md-2">
                                <Rock:RockLiteral ID="tbHeader3" runat="server" Visible="false" Text=''></Rock:RockLiteral>
                            </div>
                            <div class="col-md-2">
                                <Rock:RockLiteral ID="tbHeader4" runat="server" Visible="false" Text=''></Rock:RockLiteral>
                            </div>
                            <div class="col-md-2">
                                <Rock:RockLiteral ID="tbHeader5" runat="server" Visible="false" Text=''></Rock:RockLiteral>
                            </div>
                        </div>
                        <asp:Repeater ID="rptrMetric" runat="server" OnItemDataBound="rptrMetric_ItemDataBound">
                            <ItemTemplate>
                            <div class="row">
                                <div class="col-md-4">
                                    <asp:HiddenField ID="hfServiceId" runat="server" Value='<%# Eval("ServiceId") %>' />
                                    <asp:HiddenField ID="hfMetricId1" runat="server" Value='<%# Eval("MetricId1") %>' />
                                    <Rock:NumberBox ID="nbMetricValue1" runat="server" NumberType="Double" Label='<%# Eval( "Name") %>' Visible='<%# Eval( "Visible1" ) %>' Text='<%# Eval( "Value1") %>' />
                                </div>
                                <div class="col-md-2">
                                    <asp:HiddenField ID="hfMetricId2" runat="server" Value='<%# Eval("MetricId2") %>' />
                                    <Rock:NumberBox ID="nbMetricValue2" runat="server" NumberType="Double" Visible='<%# Eval( "Visible2" ) %>' Text='<%# Eval( "Value2") %>' />
                                </div>
                                <div class="metricValueColumn3 col-md-2">
                                    <asp:HiddenField ID="hfMetricId3" runat="server" Value='<%# Eval("MetricId3") %>' />
                                    <Rock:NumberBox ID="nbMetricValue3" runat="server" NumberType="Double" Visible='<%# Eval( "Visible3" ) %>' Text='<%# Eval( "Value3") %>' />
                                </div>
                                <div class="col-md-2">
                                    <asp:HiddenField ID="hfMetricId4" runat="server" Value='<%# Eval("MetricId4") %>' />
                                    <Rock:NumberBox ID="nbMetricValue4" runat="server" NumberType="Double" Visible='<%# Eval( "Visible4" ) %>' Text='<%# Eval( "Value4") %>' />
                                </div>
                                <div class="col-md-2">
                                    <asp:HiddenField ID="hfMetricId5" runat="server" Value='<%# Eval("MetricId5") %>' />
                                    <Rock:NumberBox ID="nbMetricValue5" runat="server" NumberType="Double" Visible='<%# Eval( "Visible5" ) %>' Text='<%# Eval( "Value5") %>' />
                                </div>
                            </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </div>

                <Rock:RockTextBox ID="tbNote" runat="server" Label="Note" TextMode="MultiLine" Rows="4" />

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" AccessKey="s" ToolTip="Alt+s" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                </div>

            </asp:Panel>

        </div>

    </div>

</ContentTemplate>
</asp:UpdatePanel>
