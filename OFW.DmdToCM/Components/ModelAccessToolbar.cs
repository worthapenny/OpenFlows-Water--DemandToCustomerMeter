using Haestad.Framework.Windows.Forms.Resources;
using Haestad.Support.Support;
using System.Drawing;
using System.Windows.Forms;

namespace OFW.DmdToCM.Components
{
    public class ModelAccessToolbar: ToolStrip
    {
        #region Constructor
        public ModelAccessToolbar()
        {
            InitializeComponents();
            InitializeVisually();
        }
        #endregion

        #region Private Methods
        private void InitializeComponents()
        {
            this.ToolStripButtonOpen = new ToolStripButton();
            this.toolStripButtonSelectScenario = new ToolStripButton();
            this.toolStripSeparator = new ToolStripSeparator();
            this.ToolStripButtonSaveAs = new ToolStripButton();

            this.Items.AddRange(new ToolStripItem[] {
                this.ToolStripButtonOpen,
                this.toolStripButtonSelectScenario,
                this.toolStripSeparator,
                this.ToolStripButtonSaveAs});
            this.Dock = DockStyle.None;
            this.Name = "ModelAccessToolbar";
            this.Text = "ModelAccessToolbar";

            // 
            // toolStripButtonOpen
            // 
            this.ToolStripButtonOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolStripButtonOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolStripButtonOpen.Name = "toolStripButtonOpen";
            this.ToolStripButtonOpen.Size = new System.Drawing.Size(20,20);
            this.ToolStripButtonOpen.Text = "Open";
            // 
            // toolStripButtonSelectScenario
            // 
            this.toolStripButtonSelectScenario.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonSelectScenario.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSelectScenario.Name = "toolStripButtonSelectScenario";
            this.toolStripButtonSelectScenario.Size = new System.Drawing.Size(20,20);
            this.toolStripButtonSelectScenario.Text = "Select Scenario";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(6, 27);
            // 
            // toolStripButtonSaveAs
            // 
            this.ToolStripButtonSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolStripButtonSaveAs.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolStripButtonSaveAs.Name = "toolStripButtonSaveAs";
            this.ToolStripButtonSaveAs.Size = new System.Drawing.Size(20,20);
            this.ToolStripButtonSaveAs.Text = "Save As";
        }
        private void InitializeVisually()
        {
            this.ToolStripButtonOpen.Image = ((Icon)GraphicResourceManager.Current[StandardGraphicResourceNames.Open])?.ToBitmap();
            this.toolStripButtonSelectScenario.Image = ((Icon)GraphicResourceManager.Current[StandardGraphicResourceNames.Scenario])?.ToBitmap();
            this.ToolStripButtonSaveAs.Image = ((Icon)GraphicResourceManager.Current[StandardGraphicResourceNames.SaveAs])?.ToBitmap();
        }
        #endregion

        #region Fields
        public ToolStripButton ToolStripButtonOpen;
        public ToolStripButton toolStripButtonSelectScenario;
        public ToolStripSeparator toolStripSeparator;
        public ToolStripButton ToolStripButtonSaveAs;
        #endregion
    }
}
