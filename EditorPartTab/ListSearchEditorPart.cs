using System;
using System.Collections.Generic;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint.WebPartPages;
using EditorPartTab.TabEditorWebPart;
using EditorPartTab.ControlTemplates.Maxim.WebParts.ListSearch;

namespace EditorPartTab
{
    public class ListSearchEditorPart : EditorPart
    {
        // The deployment path of the user control.

        const string TabControlConfigurationPath = @"~/_CONTROLTEMPLATES/Maxim.WebParts.ListSearch/ListSearchEditor.ascx";


        //const string TabControlConfigurationPath = @"~/_FEATURES/Maxim.WebParts.ListSearch_Maxim.WebParts.ListSearch/ListSearch.WebPart/ListSearchEditorUserControl.ascx";
        // The user control object ID.
        const string UserControlID = "OperationsUserControl";

        // Declare a reference to the user control.
        EditorPartTab.ControlTemplates.Maxim.WebParts.ListSearch.ListSearchEditor configuratorControl;

        // Declare a reference to the Tab Editor Web Part.
        private TabEditorWebPart.TabEditorWebPart tabEditorWebPart;

        public ListSearchEditorPart()
        {
            this.Title = "Maxim List Search Configuration";
        }

        //public List<ListSearchData> TabList { get; set; }

        public string searchListName { get; set; }

        public ListSettings MSettings { get; set; }

        void Cancel_Click(object sender, EventArgs e)
        {
            // On cancel, roll back all the changes by restoring the original list.
            //if (this.configuratorControl.OriginalTabList != null)
            //{
            //    //this.TabList = this.configuratorControl.OriginalTabList;
                
            //    this.ApplyChanges();
            //}



            //reset
            //this.MSettings = this.configuratorControl.OriginalMaximList;
        }


        protected override void CreateChildControls()
        {
            // Get a reference to the Edit Tool pane.
            ToolPane pane = this.Zone as ToolPane;
            if (pane != null)
            {
                // Disable the validation on Cancel button of ToolPane.
                pane.Cancel.CausesValidation = false;
                pane.Cancel.Click += new EventHandler(Cancel_Click);
            }

            // Load the user control and add it to the controls collection of the editor part.
            //this.configuratorControl = this.Page.LoadControl(ListSearchEditorPart.TabControlConfigurationPath) as EditorPartTab.TabEditorWebPart.ListSearchEditorUserControl;
            this.configuratorControl = this.Page.LoadControl(ListSearchEditorPart.TabControlConfigurationPath) as EditorPartTab.ControlTemplates.Maxim.WebParts.ListSearch.ListSearchEditor;
            this.configuratorControl.ID = ListSearchEditorPart.UserControlID;
            this.Controls.Add(configuratorControl);
        }

        public override void SyncChanges()
        {
            // Sync with the new property changes here.
            EnsureChildControls();
            this.tabEditorWebPart = this.WebPartToEdit as TabEditorWebPart.TabEditorWebPart;
            this.MSettings = this.tabEditorWebPart.MaximList;
        }

        public override bool ApplyChanges()
        {
            this.tabEditorWebPart = this.WebPartToEdit as TabEditorWebPart.TabEditorWebPart;
            // Set the Web Part's TabList.
            //this.tabEditorWebPart.TabList = this.TabList;

            this.tabEditorWebPart.MaximList = this.MSettings;
            //this.tabEditorWebPart.searchListName = this.searchListName;

            // Call the Web Part's personalization dirty.
            this.tabEditorWebPart.SaveChanges();
            //tabEditorWebPart.CreateEditorParts();
            return true;
        }
    }
}
