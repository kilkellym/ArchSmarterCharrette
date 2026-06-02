namespace ReRender
{
    internal class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication app)
        {
            // 1. Create ribbon tab
            string tabName = "ArchSmarter";
            try
            {
                app.CreateRibbonTab(tabName);
            }
            catch (Exception)
            {
                Debug.Print("Tab already exists.");
            }

            // 2. Create ribbon panel
            RibbonPanel panel = Helpers.Utils.CreateRibbonPanel(app, tabName, "ReRender");

            // 3. Create button data instances
            PushButtonData btnRender = RenderCommand.GetButtonData();
            PushButtonData btnSettings = SettingsCommand.GetButtonData();

            // 4. Create buttons
            PushButton renderButton = panel.AddItem(btnRender) as PushButton;
            PushButton settingsButton = panel.AddItem(btnSettings) as PushButton;

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}
