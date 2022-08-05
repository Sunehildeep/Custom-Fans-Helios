using Microsoft.Win32;
using OpenHardwareMonitor.Hardware;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.XPath;
using TsDotNetLib;

namespace CustomFans_Helios
{

    public partial class Form1 : Form
	{
		private System.Timers.Timer aTimer;
		private int[,] temps;
		private int[,] speed;
		private bool[,] fanSet;
		private XDocument _doc;
		private string _filename = Path.Combine(Application.StartupPath, "settings.xml");
		
		public const int WM_NCLBUTTONDOWN = 0xA1;
		public const int HT_CAPTION = 0x2;
		RegistryKey customFansStartup = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern bool ReleaseCapture();

		public Form1()
		{
			InitializeComponent();
			if (customFansStartup.GetValue("CustomFans-Helios") == null)
			{
				checkBox1.Checked = false;
			}
			else
			{
				checkBox1.Checked = true;
			}

			loadDataFromXml();
			fetchData();

			aTimer = new System.Timers.Timer(1000);
			aTimer.Elapsed += curveCheck;
			aTimer.AutoReset = true;
			aTimer.Enabled = true;
			notifyIcon1.BalloonTipText = "The app has been minimized to the system tray.";
			notifyIcon1.Text = "CustomFans-Helios";
		}

		private void Form1_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				ReleaseCapture();
				SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
			}
		}

		private void Form1_Resize(object sender, EventArgs e)
		{
			if (FormWindowState.Minimized == this.WindowState)
			{
				notifyIcon1.Visible = true;
				notifyIcon1.ShowBalloonTip(500);
				this.Hide();
			}

			else if (FormWindowState.Normal == this.WindowState)
			{
				notifyIcon1.Visible = false;
			}
		}

		private void loadDataFromXml()
		{
			try
			{
				_doc = XDocument.Load(_filename);
				_doc.Save(_filename);

				XElement node = _doc.XPathSelectElement("//Temps/GPU[1]");
				numericUpDown1.Value = Convert.ToDecimal(node.Value);
				node = _doc.XPathSelectElement("//Temps/GPU[2]");
				numericUpDown2.Value = Convert.ToDecimal(node.Value);
				node = _doc.XPathSelectElement("//Temps/GPU[3]");
				numericUpDown3.Value = Convert.ToDecimal(node.Value);
				node = _doc.XPathSelectElement("//Temps/GPU[4]");
				numericUpDown4.Value = Convert.ToDecimal(node.Value);
				node = _doc.XPathSelectElement("//Temps/GPU[5]");
				numericUpDown5.Value = Convert.ToDecimal(node.Value);

				node = _doc.XPathSelectElement("//Speed/GPU[1]");
				trackBar1.Value = Convert.ToInt32(node.Value);
				node = _doc.XPathSelectElement("//Speed/GPU[2]");
				trackBar2.Value = Convert.ToInt32(node.Value);
				node = _doc.XPathSelectElement("//Speed/GPU[3]");
				trackBar3.Value = Convert.ToInt32(node.Value);
				node = _doc.XPathSelectElement("//Speed/GPU[4]");
				trackBar4.Value = Convert.ToInt32(node.Value);
				node = _doc.XPathSelectElement("//Speed/GPU[5]");
				trackBar5.Value = Convert.ToInt32(node.Value);

				node = _doc.XPathSelectElement("//Temps/CPU[1]");
				numericUpDown10.Value = Convert.ToDecimal(node.Value);
				node = _doc.XPathSelectElement("//Temps/CPU[2]");
				numericUpDown9.Value = Convert.ToDecimal(node.Value);
				node = _doc.XPathSelectElement("//Temps/CPU[3]");
				numericUpDown8.Value = Convert.ToDecimal(node.Value);
				node = _doc.XPathSelectElement("//Temps/CPU[4]");
				numericUpDown7.Value = Convert.ToDecimal(node.Value);
				node = _doc.XPathSelectElement("//Temps/CPU[5]");
				numericUpDown6.Value = Convert.ToDecimal(node.Value);

				node = _doc.XPathSelectElement("//Speed/CPU[1]");
				trackBar10.Value = Convert.ToInt32(node.Value);
				node = _doc.XPathSelectElement("//Speed/CPU[2]");
				trackBar9.Value = Convert.ToInt32(node.Value);
				node = _doc.XPathSelectElement("//Speed/CPU[3]");
				trackBar8.Value = Convert.ToInt32(node.Value);
				node = _doc.XPathSelectElement("//Speed/CPU[4]");
				trackBar7.Value = Convert.ToInt32(node.Value);
				node = _doc.XPathSelectElement("//Speed/CPU[5]");
				trackBar6.Value = Convert.ToInt32(node.Value);

				node = _doc.XPathSelectElement("//Settings/Startup[1]");
				checkBox2.Checked = (bool) node;
			}
			catch { }
		}

		private void saveDataToXml()
		{
			try
			{
				_doc.XPathSelectElement("//Temps").RemoveAll();
				_doc.XPathSelectElement("//Speed").RemoveAll();
				_doc.XPathSelectElement("//Settings").RemoveAll();

				_doc.XPathSelectElement("//Temps").Add(new XElement("GPU", numericUpDown1.Value));
				_doc.XPathSelectElement("//Temps").Add(new XElement("GPU", numericUpDown2.Value));
				_doc.XPathSelectElement("//Temps").Add(new XElement("GPU", numericUpDown3.Value));
				_doc.XPathSelectElement("//Temps").Add(new XElement("GPU", numericUpDown4.Value));
				_doc.XPathSelectElement("//Temps").Add(new XElement("GPU", numericUpDown5.Value));

				_doc.XPathSelectElement("//Speed").Add(new XElement("GPU", trackBar1.Value));
				_doc.XPathSelectElement("//Speed").Add(new XElement("GPU", trackBar2.Value));
				_doc.XPathSelectElement("//Speed").Add(new XElement("GPU", trackBar3.Value));
				_doc.XPathSelectElement("//Speed").Add(new XElement("GPU", trackBar4.Value));
				_doc.XPathSelectElement("//Speed").Add(new XElement("GPU", trackBar5.Value));

				_doc.XPathSelectElement("//Temps").Add(new XElement("CPU", numericUpDown10.Value));
				_doc.XPathSelectElement("//Temps").Add(new XElement("CPU", numericUpDown9.Value));
				_doc.XPathSelectElement("//Temps").Add(new XElement("CPU", numericUpDown8.Value));
				_doc.XPathSelectElement("//Temps").Add(new XElement("CPU", numericUpDown7.Value));
				_doc.XPathSelectElement("//Temps").Add(new XElement("CPU", numericUpDown6.Value));

				_doc.XPathSelectElement("//Speed").Add(new XElement("CPU", trackBar10.Value));
				_doc.XPathSelectElement("//Speed").Add(new XElement("CPU", trackBar9.Value));
				_doc.XPathSelectElement("//Speed").Add(new XElement("CPU", trackBar8.Value));
				_doc.XPathSelectElement("//Speed").Add(new XElement("CPU", trackBar7.Value));
				_doc.XPathSelectElement("//Speed").Add(new XElement("CPU", trackBar6.Value));

				_doc.XPathSelectElement("//Settings").Add(new XElement("Startup", checkBox2.Checked));


				_doc.Save(_filename);
			}
			catch { }

		}

		private void Form1_Show(object sender, EventArgs e)
		{
			try
			{
				if (checkBox2.Checked) this.WindowState = FormWindowState.Minimized;

				else
				{
					string[] args = Environment.GetCommandLineArgs();

					if (args[1] == "-m") this.WindowState = FormWindowState.Minimized;
				}
			}
			catch { }

		}

		private void fetchData()
		{
			try
			{
				temps = new int[5, 2]
				{
				{ (int) numericUpDown10.Value, (int) numericUpDown1.Value },
				{ (int) numericUpDown9.Value, (int)numericUpDown2.Value },
				{ (int) numericUpDown8.Value, (int)numericUpDown3.Value },
				{ (int) numericUpDown7.Value, (int)numericUpDown4.Value },
				{ (int) numericUpDown6.Value, (int)numericUpDown5.Value }
				};

				speed = new int[5, 2]
				{
				{ trackBar10.Value*10, trackBar1.Value*10 },
				{ trackBar9.Value*10, trackBar2.Value*10 },
				{ trackBar8.Value*10, trackBar3.Value*10 },
				{ trackBar7.Value*10, trackBar4.Value*10 },
				{ trackBar6.Value*10, trackBar5.Value*10 }
				};

				fanSet = new bool[5, 2]
				{
				{false, false},
				{false, false},
				{false, false},
				{false, false},
				{false, false}
				};
			}
			catch { }
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			closeApp(e);
		}

		private async void closeApp(FormClosingEventArgs e = null)
		{
			if (TsDotNetLib.Registry.CheckLM("SOFTWARE\\OEM\\PredatorSense\\FanControl", "CurrentFanMode", 0u) == 0u)
			{
				if (e != null) e.Cancel = true;
				try
				{
					saveDataToXml();
					await set_single_custom_fan_state(true, 100u, "GPU");
					await set_single_custom_fan_state(true, 100u, "CPU");
				}
				catch (Exception e2)
				{
					MessageBox.Show(e2.ToString());
				}
				Environment.Exit(1);
			}
		}

		public static bool IsBetween(int MIN, int MAX, int x)
		{
			if (x >= MIN && x <= MAX) return true;
			else return false;
		}

		private void curveSet(int id, int type)
		{
			if (type == 0)
			{
				for (int i = 0; i < 5; i++)
				{
					if (i == id) fanSet[i, 1] = true;
					else fanSet[i, 1] = false;
				}
			}
			else if (type == 1)
			{
				for (int i = 0; i < 5; i++)
				{
					if (i == id) fanSet[i, 0] = true;
					else fanSet[i, 0] = false;
				}
			}
		}

		private void resetCurve()
		{
			for (int i = 0; i < 5; i++)
			{
				fanSet[i, 1] = false;
				fanSet[i, 0] = false;
			}

		}

		private bool isCurveSet(int id, int type)
		{
			if (type == 0)
			{
				for (int i = 0; i < 5; i++)
				{
					if (i != id) continue;
					if (fanSet[i, 1] == true) return true;
				}
			}
			else if (type == 1)
			{
				for (int i = 0; i < 5; i++)
				{
					if (i != id) continue;
					if (fanSet[i, 0] == true) return true;
				}
			}
			return false;
		}

		private async void curveCheck(Object source, ElapsedEventArgs e)
		{
			int cpu = 0;
			int gpu = 0;

			getTemp(ref cpu, ref gpu);
			label7.Invoke((Action)delegate
			{
				if (gpu != 0) label7.Text = Convert.ToString(gpu) + " c";
			});

			label8.Invoke((Action)delegate
			{
				if (cpu != 0) label8.Text = Convert.ToString(cpu) + " c";
			});

			if (TsDotNetLib.Registry.CheckLM("SOFTWARE\\OEM\\PredatorSense\\FanControl", "CurrentFanMode", 0u) == 0u)
			{
				if (gpu != 0)
				{
					for (int i = 0; i < 5; i++)
					{
						if (i == 4)
						{
							if (IsBetween(temps[i - 1, 1], temps[i, 1], gpu)) await set_single_custom_fan_state(false, (ulong)speed[i, 1], "GPU");
						}
						else if (i >= 1 && speed[i - 1, 1] > speed[i, 1] && !isCurveSet(i - 1, 0))
						{
							await set_single_custom_fan_state(false, (ulong)speed[i - 1, 1], "GPU");

							curveSet(i - 1, 0);
							break;
						}
						else if (IsBetween(temps[i, 1], temps[i + 1, 1], gpu) && !isCurveSet(i, 0))
						{
							await set_single_custom_fan_state(false, (ulong)speed[i, 1], "GPU");
							curveSet(i, 0);
							break;
						}
					}
				}
				if (cpu != 0)
				{
					for (int i = 0; i < 5; i++)
					{
						if (i == 4)
						{
							if (IsBetween(temps[i - 1, 0], temps[i, 0], cpu)) await set_single_custom_fan_state(false, (ulong)speed[i, 0], "CPU");
						}
						else if (i >= 1 && speed[i - 1, 0] > speed[i, 0] && !isCurveSet(i - 1, 1))
						{
							await set_single_custom_fan_state(false, (ulong)speed[i - 1, 0], "CPU");
							curveSet(i - 1, 1);
							break;
						}
						else if (IsBetween(temps[i, 0], temps[i + 1, 0], cpu) && !isCurveSet(i, 1))
						{
							await set_single_custom_fan_state(false, (ulong)speed[i, 0], "CPU");
							curveSet(i, 1);
							break;
						}

					}
				}
			}
		}


		private void getTemp(ref int cpu, ref int gpu)
		{
			Computer myComputer;
			myComputer = new Computer();
			myComputer.Open();
			myComputer.GPUEnabled = true;
			myComputer.CPUEnabled = true;
			foreach (var hardwareItem in myComputer.Hardware)
			{
				if (hardwareItem.HardwareType == HardwareType.GpuNvidia)
				{
					foreach (var sensor in hardwareItem.Sensors)
					{
						if (sensor.SensorType == SensorType.Temperature)
						{
							if (sensor.Value == null) continue;
							gpu = Convert.ToInt32(sensor.Value);
						}
					}
				}
				if (hardwareItem.HardwareType == HardwareType.CPU)
				{
					foreach (var sensor in hardwareItem.Sensors)
					{
						if (sensor.SensorType == SensorType.Temperature)
						{
							cpu = Convert.ToInt32(sensor.Value);
						}
					}
				}
			}
		}

		public static async Task<uint> WMISetGamingFanGroupBehavior(ulong intput)
		{
			try
			{
				NamedPipeClientStream cline_stream = new NamedPipeClientStream(".", "predatorsense_service_namedpipe", (PipeDirection)3);
				cline_stream.Connect();
				uint result = await Task.Run(delegate
				{
					IPCMethods.SendCommandByNamedPipe(cline_stream, 24, new object[1]
					{
						intput
					});
					((PipeStream)cline_stream).WaitForPipeDrain();
					byte[] array = new byte[9];
					((Stream)(object)cline_stream).Read(array, 0, array.Length);
					return BitConverter.ToUInt32(array, 5);
				}).ConfigureAwait(continueOnCapturedContext: false);
				((Stream)(object)cline_stream).Close();
				return result;
			}
			catch (Exception)
			{
				return uint.MaxValue;
			}
		}


		public static async Task<uint> WMISetGamingFanGroupSpeed(ulong intput)
		{
			try
			{
				NamedPipeClientStream cline_stream = new NamedPipeClientStream(".", "predatorsense_service_namedpipe", (PipeDirection)3);
				cline_stream.Connect();
				uint result = await Task.Run(delegate
				{
					IPCMethods.SendCommandByNamedPipe(cline_stream, 26, new object[1]
					{
						intput
					});
					((PipeStream)cline_stream).WaitForPipeDrain();
					byte[] array = new byte[9];
					((Stream)(object)cline_stream).Read(array, 0, array.Length);
					return BitConverter.ToUInt32(array, 5);
				}).ConfigureAwait(continueOnCapturedContext: false);
				((Stream)(object)cline_stream).Close();
				return result;
			}
			catch (Exception)
			{
				return uint.MaxValue;
			}
		}

		public static async Task<bool> set_single_custom_fan_state(bool auto, ulong percentage, string fan_group_type)
		{
			bool ret = true;
			ulong num = 0uL;
			try
			{
				switch (fan_group_type)
				{
					case "CPU":
						if (!auto)
						{
							num |= 0x30001;
							await WMISetGamingFanGroupBehavior(num);
							await WMISetGamingFanGroupSpeed(1 | (percentage << 8));
							return ret;
						}
						num |= 0x10001;
						await WMISetGamingFanGroupBehavior(num);
						return ret;
					case "GPU":
						if (!auto)
						{
							num |= 0xC00008;
							await WMISetGamingFanGroupBehavior(num);
							await WMISetGamingFanGroupSpeed(4 | (percentage << 8));

							return ret;
						}
						num |= 0x400008;
						await WMISetGamingFanGroupBehavior(num);
						return ret;
					default:
						return ret;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return false;
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			fetchData();
			resetCurve();
		}

		private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			this.Show();
			this.WindowState = FormWindowState.Normal;
		}

		private void button3_Click(object sender, EventArgs e)
		{
			WindowState = FormWindowState.Minimized;
		}

		private void button2_Click(object sender, EventArgs e)
		{
			closeApp();
		}

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
			if(checkBox1.Checked)
            {
				customFansStartup.SetValue("CustomFans-Helios", Application.ExecutablePath);
			}
			else
            {
				customFansStartup.DeleteValue("CustomFans-Helios", false);
			}
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
			saveDataToXml();
		}
    }
	
}
