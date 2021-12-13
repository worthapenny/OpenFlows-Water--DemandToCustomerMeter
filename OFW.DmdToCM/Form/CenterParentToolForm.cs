using System.Drawing;
using System.Windows.Forms;

namespace OFW.DmdToCM.Form
{
    public class CenterParentToolForm
    {
        #region Constructor
        public CenterParentToolForm(string title, System.Windows.Forms.Form parentForm, Control control, Size size)
        {
            this.title = title;
            this.parentForm = parentForm;
            this.control = control;
            this.size = size;
        }
        #endregion

        #region Public Methods
        public DialogResult ShowDialog()
        {
            var form = new System.Windows.Forms.Form();

            control.Dock = DockStyle.Fill;
            form.Controls.Add(control);
            form.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            form.Text = title;
            form.Size = size;

            form.StartPosition = FormStartPosition.Manual;

            if (parentForm != null)
            {
                form.Location = new Point(
                    parentForm.Left + parentForm.Width / 2 - form.Width / 2,
                    parentForm.Top + parentForm.Height / 2 - form.Height / 2);
            }

            return form.ShowDialog(parentForm);
        }
        #endregion

        #region Fields
        string title;
        System.Windows.Forms.Form parentForm;
        Control control;
        Size size;
        #endregion
    }
}
