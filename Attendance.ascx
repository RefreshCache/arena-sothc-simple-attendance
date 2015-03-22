<%@ Control Language="c#" Inherits="ArenaWeb.UserControls.Custom.SHEPHILLS.Core.Attendance" CodeFile="Attendance.ascx.cs" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>

<script language="javascript" type="text/javascript">

	function validateDelete(when, e)
	{
	    return confirm('Are you sure you want to delete the attendance for ' + when + '?');
	}
	
</script>

<asp:Panel ID="pnlMessage" runat="server" class="errorText" Visible="false"></asp:Panel>

<asp:UpdatePanel ID="pnlContent" runat="server">
    <ContentTemplate>
    
        <!--<h1>Enter New Attendance</h1>    -->
        <table style="width:100%" class="shep-attendance">
        <tr>
            <td valign="top" class="normalText">
                
                
                Group : <asp:Literal ID="lGroupName" runat="server"></asp:Literal><br />
                Date of Group <Arena:DateTextBox ID="dtDate" runat="server" EmptyValueMessage="Date of Group is Required" Format="Date" InvalidValueMessage="Date of Group is not a Valid Date" Required="true"></Arena:DateTextBox>

                <h4>Attendees</h4>
                <asp:CheckBoxList ID="cblAttendees" runat="server" RepeatLayout="Flow" RepeatColumns="1" RepeatDirection="Vertical" TextAlign="Right"></asp:CheckBoxList>
                <br />
                <table style="width:100%;">
                    <tr id="occAddSpace">
                        <td colspan="2">&nbsp;</td>
                    </tr>
                    <tr id="occHeadCount">
                        <td style="width:25%;">
                            <asp:Label id="HeadCountLabel" AssociatedControlId="tbHeadCount" Text="Head Count: " runat="server" />
                        </td>
                        <td style="width:75%;">
                            <asp:TextBox ID="tbHeadCount" runat="server" Width="35px"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="valHeadCountReq" runat="server" ControlToValidate="tbHeadCount" Display="Dynamic" EnableClientScript="true" CssClass="errorText" ValidationGroup="Save" ErrorMessage="Invalid value for head count" Text="*" />
                            <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ControlToValidate="tbHeadCount" Display="Dynamic" EnableClientScript="true" CssClass="errorText" ValidationGroup="Save" ValidationExpression="\d+" ErrorMessage="Invalid value for head count" Text="*" />
                        </td>
                    </tr>
                    <tr id="occNote">
                        <td style="vertical-align:top;">
                            <asp:Label id="noteLabel" AssociatedControlId="tbNote" Text="Note: " runat="server" />
                        </td>
                        <td>
                            <asp:TextBox ID="tbNote" runat="server" TextMode="MultiLine" width="95%" Rows="3" TextWrapping="Wrap"></asp:TextBox>
                        </td>
                    </tr>
                </table>
                <br />
                <asp:Button ID="btnSubmit" runat="server" Text="Submit Attendance" 
                    CssClass="smallText" onclick="btnSubmit_Click" />
                <div class="notice" style="border:solid 1px #999999;padding:5px;margin-top:10px">
                    Note: 
                    Use the group details page to update the members and their roles in your group.
                </div>
                
            </td>
            <td valign="top" class="normalText" style="padding-left:20px">
                <div style="width:200px;padding:5px">
                    <h4>Previous Attendance</h4>
                    <asp:ListView ID="lvOccurrences" runat="server" 
                        onitemcommand="lvOccurrences_ItemCommand">
                    <LayoutTemplate>
                        <ul class="occurrenceList">
                            <li id="itemPlaceHolder" runat="server"></li>
                        </ul>
                    </LayoutTemplate>
                    <ItemTemplate>
                        <li id="Li1" runat="server">
                            <asp:LinkButton ID="lbPrevious" runat="server" CausesValidation="false" CommandName="View" CommandArgument='<%#Eval("occurrence_id")%>' Text='<%# ((DateTime)Eval("occurrence_start_time")).ToShortDateString() %>' ></asp:LinkButton><asp:ImageButton ID="ibDelete" runat="server" CausesValidation="false" CommandName="Remove" CommandArgument='<%#Eval("occurrence_id")%>' ImageUrl="~/images/delete.gif" OnClientClick="return validateDelete(this.previousSibling.innerHTML, event);"  />
                        </li>
                    </ItemTemplate>
                    </asp:ListView>
                </div>
            </td>
        </tr>
        </table>

    </ContentTemplate>
</asp:UpdatePanel>
