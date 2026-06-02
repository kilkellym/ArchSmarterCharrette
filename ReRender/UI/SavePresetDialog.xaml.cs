using System.Windows;

namespace ReRender.UI
{
    public partial class SavePresetDialog : Window
    {
        public string PresetName => TxtPresetName.Text;

        public SavePresetDialog(string defaultName = "")
        {
            InitializeComponent();
            TxtPresetName.Text = defaultName;
            TxtPresetName.SelectAll();
            TxtPresetName.Focus();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtPresetName.Text))
                return;

            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
