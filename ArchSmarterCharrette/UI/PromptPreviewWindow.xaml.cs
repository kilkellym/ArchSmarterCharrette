using System.Windows;

namespace ArchSmarterCharrette.UI
{
    public partial class PromptPreviewWindow : Window
    {
        public PromptPreviewWindow(string promptText)
        {
            InitializeComponent();
            TxtPrompt.Text = promptText;
        }

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(TxtPrompt.Text);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
