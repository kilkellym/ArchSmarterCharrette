namespace ArchSmarterCharrette
{
    [Transaction(TransactionMode.Manual)]
    public class RenderCommand : IExternalCommand
    {
        private static UI.RenderWindow _openWindow;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // Export the active view to a temporary PNG
                View activeView = doc.ActiveView;
                string exportedPath = Helpers.ViewExporter.ExportViewToTempPng(doc, activeView);

                // If the window is already open, update it with the fresh export
                if (_openWindow != null && _openWindow.IsLoaded)
                {
                    _openWindow.UpdateSourceImage(exportedPath);
                    _openWindow.Activate();
                    return Result.Succeeded;
                }

                // Open the render window as non-modal — user can continue working in Revit
                _openWindow = new UI.RenderWindow(exportedPath);
                _openWindow.Closed += (s, e) => _openWindow = null;
                _openWindow.Show();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                TaskDialog.Show("Charrette Error", ex.Message);
                return Result.Failed;
            }
        }

        internal static PushButtonData GetButtonData()
        {
            string buttonInternalName = "btnRenderCommand";
            string buttonTitle = "Render\nImage";

            Helpers.ButtonDataClass myButtonData = new Helpers.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Render_32,
                Properties.Resources.Render_16,
                Properties.Resources.RenderDark_32,
                Properties.Resources.RenderDark_16,
                "Export the active view and render it with AI using Google Gemini");

            return myButtonData.Data;
        }
    }
}
