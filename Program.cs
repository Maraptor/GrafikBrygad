namespace GrafikBrygad
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // PerMonitorV2 jest ustawione w GrafikBrygad.csproj.
            // ApplicationConfiguration.Initialize() wykorzystuje
            // tę konfigurację przed utworzeniem pierwszego okna.
            ApplicationConfiguration.Initialize();

            Application.Run(
                new Form1());
        }
    }
}