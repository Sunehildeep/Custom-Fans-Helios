using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace CustomFans_Helios
{
    static class Program
    {
        static Mutex mutex = new Mutex(true, "{8F6F0AC4-B9A1-45fd-A8CF-72F04E6BDE8F}");
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                if (!IsAdministrator())
                {
                    StartAsAdmin(Assembly.GetExecutingAssembly().Location);
                    return;
                }
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            else
            {
                MessageBox.Show("Error: Only one instance at a time!");
                Environment.Exit(1);
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs e)
        {
            string _resName = "CustomFans-Helios.libs." + new AssemblyName(e.Name).Name + ".dll";
            
            using (var _stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(_resName))
            {
                byte[] _data = new byte[_stream.Length];
                _stream.Read(_data, 0, _data.Length);
                return Assembly.Load(_data);
            }
        }
        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void StartAsAdmin(string fileName)
        {
            var proc = new Process
            {
                StartInfo =
                {
                    FileName = fileName,
                    UseShellExecute = true,
                    Verb = "runas"
                }
            };

            proc.Start();
        }
    }
}
