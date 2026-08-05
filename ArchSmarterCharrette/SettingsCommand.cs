namespace ArchSmarterCharrette
{
    [Transaction(TransactionMode.Manual)]
    public class SettingsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var window = new UI.SettingsWindow();
                window.ShowDialog();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                TaskDialog.Show("Charrette Settings Error", ex.Message);
                return Result.Failed;
            }
        }

        internal static PushButtonData GetButtonData()
        {
            string buttonInternalName = "btnSettingsCommand";
            string buttonTitle = "Settings";

            Helpers.ButtonDataClass myButtonData = new Helpers.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Settings_32,
                Properties.Resources.Settings_16,
                Properties.Resources.SettingsDark_32,
                Properties.Resources.SettingsDark_16,
                "Configure Charrette settings (API key and model)");

            return myButtonData.Data;
        }
    }
}
