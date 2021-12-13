using Haestad.Framework.Application;
using Haestad.Framework.Windows.Forms.Forms;
using Haestad.Framework.Windows.Forms.Resources;
using Haestad.Support.Support;
using OFW.DmdToCM.FormModel;
using OpenFlows.Application;
using OpenFlows.Water.Application;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OFW.DmdToCM.Form
{
    public class DemandToCMParentForm : HaestadParentForm, IParentFormSurrogate
    {
        
        #region Constructor
        public DemandToCMParentForm(HaestadParentFormModel parentFormModel)
            : base(parentFormModel)
        {
            InitializeComponent();
        }
        #endregion

        #region Overridden Methods
        protected override void InitializeVisually()
        {
            this.Icon = (Icon)GraphicResourceManager.Current[StandardGraphicResourceNames.DemandAdjustments];
            this.Text = "Demand to Customer Meters";

            this.toolStripButtonBuildingPolygon.Image = ((Icon)GraphicResourceManager.Current[StandardGraphicResourceNames.ThiessenPolygon])?.ToBitmap();
            this.toolStripButtonValidate.Image = ((Icon)GraphicResourceManager.Current[StandardGraphicResourceNames.Validate])?.ToBitmap();
            this.toolStripButtonRun.Image = ((Icon)GraphicResourceManager.Current[StandardGraphicResourceNames.Compute])?.ToBitmap();

            EnableControl(false);
        }
        protected override void InitializeEvents()
        {
            App.ParentFormModel.ProjectEventChannel.ProjectOpened += (s, e) => ProjectOpened(true, e);
            App.ParentFormModel.ProjectEventChannel.ProjectClosed += (s, e) => ProjectOpened(false, e);

            this.modelAccessToolbar.ToolStripButtonSaveAs.Click += (s, e) => SaveProjectAs();
            this.modelAccessToolbar.toolStripButtonSelectScenario.Click += (s, e) => ShowScenarioSelectionDialog();
            this.modelAccessToolbar.ToolStripButtonOpen.Click += (s, e) => ButtonOpenClicked();

            this.toolStripButtonBuildingPolygon.Click += (s, e) => ShowBuildingPolygonSelectionDialog();
            this.toolStripButtonValidate.Click += (s, e) => DemandToCMFormModel.Validate();
            this.toolStripButtonRun.Click += (s, e) => DemandToCMFormModel.Run();
        }
        protected override void DisposeEvents()
        {
            App.ParentFormModel.ProjectEventChannel.ProjectOpened -= (s, e) => ProjectOpened(true, e);
            App.ParentFormModel.ProjectEventChannel.ProjectClosed -= (s, e) => ProjectOpened(false, e);
        }
        public override OpenFileDialog NewOpenFileDialog()
        {
            OpenFileDialog open = new OpenFileDialog();
            open.CheckFileExists = true;
            open.CheckPathExists = true;
            open.DefaultExt = App.ParentFormModel.ApplicationDescription.LeadFileExtension;
            open.Filter = (App.ParentFormModel.ApplicationDescription).MultiExtensionOpenFileFilter;
            open.ShowReadOnly = false;
            return open;
        }
        #endregion

        #region Private Methods
        private void ProjectOpened(bool isOpened, ProjectEventArgs e)
        {
            DemandToCMFormModel = isOpened
                ? new DemandToCMFormModel()
                : null;

            UpdateUI(isOpened, e);
            EnableControl(isOpened);
        }
        private void UpdateUI(bool projectOpened, ProjectEventArgs e)
        {

            Text = projectOpened
                ? $"Demand to Customer Meters | {e.Project.Label}"
                : $"Demand to Customer Meters";

            EnableControl(projectOpened);
        }
        private void EnableControl(bool enble)
        {
            this.modelAccessToolbar.toolStripButtonSelectScenario.Enabled = enble;
            this.modelAccessToolbar.ToolStripButtonSaveAs.Enabled = enble;

            this.toolStripButtonBuildingPolygon.Enabled = enble;
            this.toolStripButtonValidate.Enabled = enble;
            this.toolStripButtonRun.Enabled = enble;
        }
        
        private void ButtonOpenClicked()
        {
            if(App.WaterParentFormModel.CurrentProject != null)
                CloseCurrentFile();

            var open = NewOpenFileDialog();
            if (open.ShowDialog(this) == DialogResult.OK)
            {
                OpenFile(open.FileName);

                // Suppress any prompts to save and treat the project as if no changes were made.
                ((ProjectBase)App.ParentFormModel.CurrentProject).MakeClean();
            }
        }
        private void ShowScenarioSelectionDialog()
        {
            new CenterParentToolForm(
              "Scenario",
              FindForm(),
              WaterApplicationManager.GetInstance().ParentFormUIModel.ScenarioManagerProxy,
              new Size(350, 350)
              ).ShowDialog();

        }
        private void SaveProjectAs()
        {
            if (PromptSaveAs(ParentFormModel.CurrentProject) == DialogResult.OK)
            {
                var newProjectFullPath = ParentFormModel.CurrentProject.FullPath;
                //Log.Information($"Project is saved as at: {newProjectFullPath}");

                var mbox = MessageBox.Show(this, "Would you like to open the newly saved file in the main application?", "Open in Water[GEMS/CAD/OPS]", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (mbox == DialogResult.Yes)
                {
                    Process.Start(newProjectFullPath);
                }
            }
        }
        private void ShowBuildingPolygonSelectionDialog()
        {
            var openDialog = NewOpenFileDialog("Shapefiles|*.shp");
            if (openDialog.ShowDialog(this) == DialogResult.OK)
            {

            }
        }
        private OpenFileDialog NewOpenFileDialog(string filter /*= "Office Files|*.doc;*.xls;*.ppt"*/)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.CheckFileExists = true;
            open.CheckPathExists = true;
            open.Filter = filter;
            open.ShowReadOnly = false;
            return open;
        }
        #endregion

        #region Public Methods
        public void SetParentWindowHandle(long handle)
        {
            //
        }
        #endregion

        #region Private Properties
        private WaterApplicationManager App => (WaterApplicationManager)WaterApplicationManager.GetInstance();
        private DemandToCMFormModel DemandToCMFormModel { get; set; }
        #endregion

        #region Auto Generated Code

        private ToolStripButton toolStripButtonBuildingPolygon;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton toolStripButtonValidate;
        private ToolStripButton toolStripButtonRun;
        private Components.ModelAccessToolbar modelAccessToolbar;

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DemandToCMParentForm));
            this.modelAccessToolbar = new OFW.DmdToCM.Components.ModelAccessToolbar();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonBuildingPolygon = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonValidate = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonRun = new System.Windows.Forms.ToolStripButton();
            this.modelAccessToolbar.SuspendLayout();
            this.SuspendLayout();
            // 
            // modelAccessToolbar
            // 
            this.modelAccessToolbar.Dock = System.Windows.Forms.DockStyle.None;
            this.modelAccessToolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this.toolStripButtonBuildingPolygon,
            this.toolStripButtonValidate,
            this.toolStripButtonRun});
            this.modelAccessToolbar.Location = new System.Drawing.Point(9, 9);
            this.modelAccessToolbar.Name = "modelAccessToolbar";
            this.modelAccessToolbar.Size = new System.Drawing.Size(162, 25);
            this.modelAccessToolbar.TabIndex = 0;
            this.modelAccessToolbar.Text = "modelAccessToolbar1";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButtonBuildingPolygon
            // 
            this.toolStripButtonBuildingPolygon.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonBuildingPolygon.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonBuildingPolygon.Image")));
            this.toolStripButtonBuildingPolygon.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonBuildingPolygon.Name = "toolStripButtonBuildingPolygon";
            this.toolStripButtonBuildingPolygon.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonBuildingPolygon.Text = "Building / Parcel Polygon";
            // 
            // toolStripButtonValidate
            // 
            this.toolStripButtonValidate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonValidate.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonValidate.Image")));
            this.toolStripButtonValidate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonValidate.Name = "toolStripButtonValidate";
            this.toolStripButtonValidate.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonValidate.Text = "Validate";
            // 
            // toolStripButtonRun
            // 
            this.toolStripButtonRun.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonRun.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonRun.Image")));
            this.toolStripButtonRun.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonRun.Name = "toolStripButtonRun";
            this.toolStripButtonRun.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonRun.Text = "Run";
            // 
            // DemandToCMParentForm
            // 
            this.ClientSize = new System.Drawing.Size(556, 465);
            this.Controls.Add(this.modelAccessToolbar);
            this.Name = "DemandToCMParentForm";
            this.helpProviderHaestadForm.SetShowHelp(this, false);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.modelAccessToolbar.ResumeLayout(false);
            this.modelAccessToolbar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion
    }
}
