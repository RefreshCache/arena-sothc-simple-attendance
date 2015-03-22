namespace ArenaWeb.UserControls.Custom.SHEPHILLS.Core
{
	using System;
	using System.Xml;
	using System.Data;
	using System.Data.SqlClient;
	using System.Drawing;
	using System.Web;
    using System.Web.UI;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;

	using Arena.Core;
    using Arena.DataLayer.Core;
	using Arena.Exceptions;
	using Arena.Portal;
	using Arena.Portal.UI;
    using Arena.SmallGroup;
    using Arena.Utility;

	/// <summary>
	///		Summary description for SubscribedProfileList.
	/// </summary>
	public partial class Attendance : PortalControl
	{
        #region Module Settings

        [ListFromSqlSetting("Occurrence Type", "The occurrence type that should be used when adding new occurrences.", true, "", 
            "select t.occurrence_type_id, g.group_name + ' - ' + t.type_name from core_occurrence_type t inner join core_occurrence_type_group g on g.group_id = t.group_id order by g.group_name, t.type_order")]
        public string OccurrenceTypeSetting { get { return Setting("OccurrenceType", "", true); } }

        [TextSetting("Occurrence Name", "The occurrence name that should be used when adding new occurrences.", true)]
        public string OccurrenceNameSetting { get { return Setting("OccurrenceName", "", true); } }

        [PageSetting("Redirect Page", "The page that the user will be redirected to after entering attendance - should be the Group Details page.", false)]
        public string RedirectPageIDSetting { get { return Setting("RedirectPageID", "", false); } }

        #endregion

        #region Private Variables

        private OccurrenceData occurrenceData = new OccurrenceData();
        private Group group = null;

        #endregion

        #region Events

        public int defaultHeadCount = 0;
        public string defaultNote = "";

        protected void Page_Load(object sender, System.EventArgs e)
        {
            
            if (Page.IsPostBack)
            {
                if (ViewState["GroupID"] != null)
                    group = new Group((int)ViewState["GroupID"]);
            }
            else
            {
                group = Request.QueryString["group"] != null ? new Group(Convert.ToInt32(Request.QueryString["group"])) : null;
                if (group != null)
                {
                    if (group.ClusterType.AllowOccurrences)
                    {
                        if (group.Allowed(Arena.Security.OperationType.Edit, CurrentUser, CurrentPerson))
                        {
                            lGroupName.Text = group.Title;

                            Occurrence occurrence = null;
                            ShowAttendance(occurrence);

                            ShowOccurrences();

                            tbHeadCount.Text = defaultHeadCount.ToString();
                            tbNote.Text = defaultNote;

                            ViewState.Add("GroupID", group.GroupID);
                        }
                        else
                        {
                            ShowErrorMessage("You are not authorized to take attendance for the selected group.");
                            group = null;
                        }
                    }
                    else
                    {
                        ShowErrorMessage("The selected group does not support attendance.");
                        group = null;
                    }
                }
                else
                    ShowErrorMessage("Invalid group specified on query string");
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            GroupOccurrence occurrence = null;
            int headCount = Convert.ToInt32(tbHeadCount.Text);
            string note = tbNote.Text;

            if (ViewState["OccurrenceID"] != null)
            {
                occurrence = new GroupOccurrence((int)ViewState["OccurrenceID"]);
                if (occurrence.StartTime.Date != dtDate.SelectedDate)
                    occurrence = null;
            }

            if (occurrence == null)
            {
                occurrence = new GroupOccurrence();
                occurrence.GroupID = group.GroupID;
                occurrence.OccurrenceID = -1;
                occurrence.OccurrenceTypeID = Convert.ToInt32(OccurrenceTypeSetting);
                occurrence.Name = OccurrenceNameSetting;
                occurrence.StartTime = dtDate.SelectedDate;
                occurrence.EndTime = dtDate.SelectedDate;
                occurrence.HeadCount = headCount;
                occurrence.Description = note;
                occurrence.Save(CurrentUser.Identity.Name);
            }

            foreach (ListItem li in cblAttendees.Items)
            {
                OccurrenceAttendance attendance = new OccurrenceAttendance(occurrence.OccurrenceID, Convert.ToInt32(li.Value));
                attendance.OccurrenceID = occurrence.OccurrenceID;
                attendance.PersonID = Convert.ToInt32(li.Value);
                attendance.Attended = li.Selected;

                if (attendance.Attended)
                    attendance.Save(CurrentUser.Identity.Name);
                else
                {
                    if (attendance.OccurrenceAttendanceID != -1)
                    {
                        if (attendance.PersonID == -1 || attendance.Notes == string.Empty)
                            attendance.Delete();
                        else
                        attendance.Save(CurrentUser.Identity.Name);
                    }
                }
            }

            occurrence.HeadCount = headCount;
            occurrence.Description = note;
            occurrence.Save(CurrentUser.Identity.Name);

            occurrence = null;
            ShowAttendance(occurrence);

            ShowOccurrences();

            if (RedirectPageIDSetting != "")
            {
                string URLGroupID = Request.Params["group"];
                Response.Redirect(string.Format("default.aspx?page={0}&group={1}", RedirectPageIDSetting, URLGroupID));
            }

        }

        protected void lvOccurrences_ItemCommand(object sender, ListViewCommandEventArgs e)
        {
            Occurrence occurrence = new Occurrence(Convert.ToInt32(e.CommandArgument));

            switch (e.CommandName)
            {
                case "View":
                    ShowAttendance(occurrence);
                    break;

                case "Remove":
                    occurrence.Delete();
                    ShowOccurrences();
                    break;
            }
        }

        #endregion

        #region Private Methods

        private void ShowErrorMessage(string message)
        {
            pnlMessage.Controls.Clear();
            pnlMessage.Controls.Add(new LiteralControl(message));
            pnlMessage.Visible = true;
            pnlContent.Visible = false;
        }

        private void ShowAttendance(Occurrence occurrence)
        {

            cblAttendees.Items.Clear();

            if (occurrence != null)
            {
                dtDate.SelectedDate = occurrence.StartTime.Date;

                SqlDataReader rdr2 = occurrenceData.GetOccurrenceByID(occurrence.OccurrenceID);
                while (rdr2.Read())
                {
                    defaultHeadCount = Convert.ToInt32(rdr2["head_count"].ToString());
                    defaultNote = rdr2["occurrence_description"].ToString();
                }
                rdr2.Close();

                tbHeadCount.Text = defaultHeadCount.ToString();
                tbNote.Text = defaultNote;

                SqlDataReader rdr = occurrenceData.GetOccurrenceAttendanceByOccurrenceID(occurrence.OccurrenceID, -1);
                while (rdr.Read())
                {
                    ListItem li = new ListItem(rdr["person_name"].ToString(), rdr["person_id"].ToString());
                    li.Selected = (Boolean)rdr["attended"];
                    cblAttendees.Items.Add(li);
                }
                rdr.Close();

                if (ViewState["OccurrenceID"] != null)
                    ViewState["OccurrenceID"] = occurrence.OccurrenceID;
                else
                    ViewState.Add("OccurrenceID", occurrence.OccurrenceID);
            }
            else
            {
                dtDate.Text = string.Empty;

                if (group.Leader.PersonID != -1)
                    cblAttendees.Items.Add(new ListItem(group.Leader.FullName, group.Leader.PersonID.ToString()));

                foreach (GroupMember member in group.Members)
                    if (member.Active)
                        cblAttendees.Items.Add(new ListItem(member.FullName, member.PersonID.ToString()));

                if (ViewState["OccurrenceID"] != null)
                    ViewState.Remove("OccurrenceID");
            }
        }

        private void ShowOccurrences()
        {
            if (group != null)
            {
                //lvOccurrences.DataSource = occurrenceData.GetOccurrenceByGroupID(group.GroupID);
                //lvOccurrences.DataBind(); 
                
                SqlDataReader reader = occurrenceData.GetOccurrenceByGroupID(group.GroupID);
				DataTable resultTable = new DataTable();
        		resultTable.Load(reader);

				// sort results
				resultTable.DefaultView.Sort = "occurrence_start_time desc";
				
				// clear listview
				lvOccurrences.Items.Clear();
				
				if (resultTable.Rows.Count > 10)
				{
					for (int i = 10; i < resultTable.Rows.Count; i++)
					{
						resultTable.Rows[i].Delete();	
					}
				}
				
				lvOccurrences.DataSource = resultTable;
                lvOccurrences.DataBind();
            }
        }

        #endregion

        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}

		#endregion
    }
}
