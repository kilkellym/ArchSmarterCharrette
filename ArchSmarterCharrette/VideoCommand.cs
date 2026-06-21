namespace ArchSmarterCharrette
{
    [Transaction(TransactionMode.Manual)]
    public class VideoCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Data.VideoToolLauncher.Launch();
                return Result.Succeeded;
            }
            catch (Data.VideoGenerationException ex)
            {
                TaskDialog.Show("Charrette Video", ex.Message);
                return Result.Failed;
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
            string buttonInternalName = "btnVideoCommand";
            string buttonTitle = "Render\nVideo";

            Helpers.ButtonDataClass myButtonData = new Helpers.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "Generate a video from a rendered image");

            return myButtonData.Data;
        }
    }
}
