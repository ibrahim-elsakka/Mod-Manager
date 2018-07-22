using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using MaterialSkin;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Mod_Manager
{
    public partial class Form1 : MaterialSkin.Controls.MaterialForm
    {
        public Form1()
        {
            InitializeComponent();
            ControlWriter controlWriter = new ControlWriter(richTextBox2);
            Console.SetOut(controlWriter);


            //  Process p = OtherUtils.GetParent(Process.GetCurrentProcess());
            //   Console.WriteLine(p.MainModule.FileName + "\t\t" + GetMD5(p.MainModule.FileName));
            //  Console.WriteLine(Process.GetCurrentProcess().GetParent().MainModule.FileName);

            Process parentProc = ProcessExtensions.ParentProcessUtilities.GetParentProcess();

            if (parentProc != null)
            {
                if (parentProc.MainModule.FileName.ToLower().Contains("explorer"))
                {
                    MessageBox.Show("Startup Error, Run the \"Mod Manager.exe\" file to start.");
                    Environment.Exit(1);
                    Application.Exit();
                }
            } 

            darkThemeCheckBox.Checked = Properties.Settings.Default.darktheme;
            systemThemeRadioButton.Checked = Properties.Settings.Default.windowstheme;
            DllInjectTextBox.Text = Properties.Settings.Default.dllpath;
            gameDirTextBox.Text = Properties.Settings.Default.gamepath;

            Gracias gracias = new Gracias();
            groupBox2.Controls.Add(gracias);
            gracias.Dock = DockStyle.Fill;
            gracias.BorderStyle = BorderStyle.None;
            gracias.AddCredit("1507768");
            gracias.AddCredit("1554884");
            gracias.AddCredit("2177635");
            gracias.AddCredit("530215");
            gracias.AddCredit("1551570");
            gracias.AddCredit("278477");
            gracias.AddCredit("308912");
            gracias.AddCredit("179910");
            gracias.AddCredit("2180346");
            gracias.AddCredit("284797");  //haunge
            gracias.AddCredit("300934"); //hatti
            gracias.AddCredit("827166"); //shell
            gracias.AddCredit("154771"); //cerrasso
            gracias.AddCredit("1633066"); //tormund
            gracias.AddCredit("729055"); //txt231
            gracias.AddCredit("1014627"); //robm1q

            materialLabel3.ForeColor = MaterialSkinManager.Instance.ColorScheme.AccentColor;

            moreinfobutt.Enabled = moreinfobutt.Visible = materialRaisedButton3.Enabled = materialRaisedButton3.Visible = false;
            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += (a, b) =>
             {
                 if (float.Parse(b.Result) > float.Parse(VersionNumberLable.Text.Substring(1).TrimEnd('a')))
                 {
                     Console.WriteLine(b.Result);
                     MessageBox.Show("Update required, visit coltonon.cc");
                     Environment.Exit(1);
                     Application.Exit();
                 }
             };
            wc.DownloadStringAsync(new Uri("https://coltonon.cc/cmods/version.dat"));



            //

            /*  ColorScheme scheme = new ColorScheme(
                  ColorTranslator.FromHtml("#482880"),    // Primary
                  ColorTranslator.FromHtml("#341d5b"),    // DarkPrimary
                  ColorTranslator.FromHtml("#845bcb"),    // Light
                  ColorTranslator.FromHtml("#ffbb40"),    // Accent
                  TextShade.WHITE, MaterialSkinManager.Instance.GetApplicationBackgroundColor());*/

            ReColor();


            materialListView1.BackColor =  moduleListView.BackColor = MaterialSkinManager.Instance.GetApplicationBackgroundColor();
            richTextBox1.BackColor = MaterialSkinManager.Instance.GetApplicationBackgroundColor();
            richTextBox2.BackColor = MaterialSkinManager.Instance.GetApplicationBackgroundColor();
            listView1.BackColor = MaterialSkinManager.Instance.GetApplicationBackgroundColor();
            gracias.BackColor = MaterialSkinManager.Instance.GetApplicationBackgroundColor();

            moduleListView.ForeColor = MaterialSkinManager.Instance.GetPrimaryTextColor();
            richTextBox1.ForeColor = MaterialSkinManager.Instance.GetPrimaryTextColor();
            richTextBox2.ForeColor = MaterialSkinManager.Instance.ColorScheme.AccentColor;
            gracias.ForeColor = MaterialSkinManager.Instance.GetPrimaryTextColor();
            groupBox1.ForeColor = groupBox2.ForeColor = MaterialSkinManager.Instance.GetPrimaryTextColor();
           VersionNumberLable.ForeColor = MaterialSkinManager.Instance.ColorScheme.AccentColor;

            materialListView1.Columns.Add("Mod", 150);
            materialListView1.Columns.Add("Description", 300);
            materialListView1.Columns.Add("State", 150);
            materialListView1.Columns[2].TextAlign = HorizontalAlignment.Right;
            materialListView1.Alignment = ListViewAlignment.SnapToGrid;
            CalcResize();



            moduleListView.Columns.Add("Module", 300);
            moduleListView.Columns.Add("Base Address", 150);
            moduleListView.Columns.Add("Path", -2);



            richTextBox2.Font = richTextBox1.Font = groupBox1.Font =  MaterialSkinManager.Instance.ROBOTO_REGULAR_11;

            ShowScrollBar(materialListView1.Handle.ToInt64(), (int)SB_HORZ, 0);
          //  ShowScrollBar(materialListView1.Handle, (int)SB_VERT, true);

            ReloadGameDir();
            ReloadMods();
            LoadModules();



        }

        


        public static String GetHash<T>(Stream stream) where T : HashAlgorithm
        {
            StringBuilder sb = new StringBuilder();

            MethodInfo create = typeof(T).GetMethod("Create", new Type[] { });
            using (T crypt = (T)create.Invoke(null, null))
            {
                byte[] hashBytes = crypt.ComputeHash(stream);
                foreach (byte bt in hashBytes)
                {
                    sb.Append(bt.ToString("x2"));
                }
            }
            return sb.ToString();
        }

        string GetMD5(string file)
        {
            using (FileStream fStream = File.OpenRead(file))
            {
                return GetHash<MD5>(fStream);
            }
        }


        Color SetHue(Color oldColor, int shift)
        {
           // GetBrightness();

            var temp = new HSV();
            temp.h = oldColor.GetHue() + shift;
            temp.s = oldColor.GetSaturation();
            temp.v = oldColor.GetBrightness();
            return ColorFromHSL(temp);
        }

        float getBrightness(Color c)
        { return (c.R * 0.299f + c.G * 0.587f + c.B * 0.114f) / 256f; }
        // A common triple float struct for both HSL & HSV
        // Actually this should be immutable and have a nice constructor!!
        public struct HSV { public float h; public float s; public float v; }

        // the Color Converter
        static public Color ColorFromHSL(HSV hsl)
        {
            if (hsl.s == 0)
            { int L = (int)hsl.v; return Color.FromArgb(255, L, L, L); }

            double min, max, h;
            h = hsl.h / 360d;

            max = hsl.v < 0.5d ? hsl.v * (1 + hsl.s) : (hsl.v + hsl.s) - (hsl.v * hsl.s);
            min = (hsl.v * 2d) - max;

            Color c = Color.FromArgb(255, (int)(255 * RGBChannelFromHue(min, max, h + 1 / 3d)),
                (int)(255 * RGBChannelFromHue(min, max, h)),
                (int)(255 * RGBChannelFromHue(min, max, h - 1 / 3d)));
            return c;
        }

        static double RGBChannelFromHue(double m1, double m2, double h)
        {
            h = (h + 1d) % 1d;
            if (h < 0) h += 1;
            if (h * 6 < 1) return m1 + (m2 - m1) * 6 * h;
            else if (h * 2 < 1) return m2;
            else if (h * 3 < 2) return m1 + (m2 - m1) * 6 * (2d / 3d - h);
            else return m1;

        }


        private void materialRaisedButton1_Click(object sender, EventArgs e)
        {
            ReloadMods();
        }

        private void ReloadMods()
        {
            WebClient client = new WebClient();
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(ds_ModList);
            client.DownloadStringAsync(new Uri("https://coltonon.cc/cmods/modlist.dat"));
        }

        private void ds_ModList(object sender, DownloadStringCompletedEventArgs e)
        {
            materialListView1.Items.Clear();
            configed_mods.Clear();
            string[] modnames = e.Result.Split('\n');
            foreach (string mod in modnames)
            {
                string[] moddetail = mod.Split(';');
                ListViewItem item = new ListViewItem(new string[] { moddetail[0], moddetail[1], "" });
                
                materialListView1.Items.Add(item);
            }
            // Verify state

            VerifyMods();

        }

        List<string> configed_mods = new List<string>();

        private void VerifyMods()
        {
            foreach (ListViewItem litem in materialListView1.Items)
           // for (int i = 0; i < materialListView1.Items.Count - 1; i++)
            {
                WebClient client = new WebClient();
                client.DownloadStringCompleted += (s, ev) =>
                {
                    // logLabel.Text = e.Error.Message;
                  //  try
                    {
                        bool? Verified = true;

                        string[] files = ev.Result.Split(';');
                        foreach (string filehash in files)
                        {
                            //  Console.WriteLine("check");
                            string[] filehashcombo = filehash.Split(':');
                            string filepath = gameDirTextBox.Text + "cmods\\" + litem.SubItems[0].Text + "\\" + filehashcombo[0];
                            if (File.Exists(filepath))
                            {
                                if (GetMD5(filepath).ToUpper() == filehashcombo[1])
                                {
                                    Console.WriteLine(filepath + "  Verified");
                                    Verified = true;
                                } else
                                {
                                    Console.WriteLine(filepath + "  Corrupt");
                                    Verified = false;
                                }
                            }
                            else
                            {
                                Console.WriteLine(filepath + "  Not Found");
                                Verified = null;
                            }
                        }

                        litem.SubItems[0].ForeColor = moddescription.ForeColor;
                        litem.SubItems[1].ForeColor = moddescription.ForeColor;

                        if (Verified == true)
                        {
                            litem.SubItems[2].Text = "Installed";
                            litem.SubItems[2].ForeColor = MaterialSkinManager.Instance.ColorScheme.LightPrimaryColor;
                            configed_mods.Add("cmods/" + litem.SubItems[0].Text+ "/" + files[0].Split(':')[0]);
                        }
                        if (Verified == false)
                        {
                            litem.SubItems[2].Text = "Outdated/Corrupt";
                            litem.SubItems[2].ForeColor = MaterialSkinManager.Instance.ColorScheme.AccentColor;
                        }
                        if (Verified == null)
                        {
                            litem.SubItems[2].Text = "Not Installed";
                            litem.SubItems[2].ForeColor = moddescription.ForeColor;

                        }
                    }
                  //  catch (Exception) { }
                      WriteConfig();

                };
                client.DownloadStringAsync(new Uri("https://coltonon.cc/cmods/" + litem.SubItems[0].Text + "/info.dat"));
            }
            
        }



        private void ReloadGameDir()
        {
           if (File.Exists(gameDirTextBox.Text + "starwarsbattlefrontii.exe"))
           { 
               gameDirCheckBox.Checked = true;
           }
           else
           {
               gameDirCheckBox.Checked = false;
           }
            

        }


        private void materialSingleLineTextField1_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.gamepath = gameDirTextBox.Text;
            Properties.Settings.Default.Save();
            ReloadGameDir();
        }

        private void materialRaisedButton4_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.FileName = "starwarsbattlefrontii.exe";
            openFileDialog.Filter = "Battlefront II |starwarsbattlefrontii.exe";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.gamepath = gameDirTextBox.Text = Path.GetDirectoryName(openFileDialog.FileName) + "\\";
                Properties.Settings.Default.Save();
            }
                //  ChooseFileDialog;
            }

        private void materialRaisedButton3_Click(object sender, EventArgs e)
        {
            // install/uninstall
            if (!gameDirCheckBox.Checked) return;
            if (materialListView1.SelectedItems.Count != 1) return;


            string tmp_gmadir = gameDirTextBox.Text + "cmods\\";
            string tmp_moddir = gameDirTextBox.Text + "cmods\\" + materialListView1.SelectedItems[0].SubItems[0].Text;
            //check proxied dll
            Console.WriteLine("Button Clicked");

            WebClient client = new WebClient();
            client.Headers["User-Agent"] =
                "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 6.0) " +
                "(compatible; MSIE 6.0; Windows NT 5.1; " +
                ".NET CLR 1.1.4322; .NET CLR 2.0.50727)";

            client.DownloadStringCompleted += (s, ev) =>
            {
                Console.WriteLine(gameDirTextBox.Text + "amd_ags_x64.dll");
                string[] outstr = ev.Result.Split(';'); // original:modified
                Console.WriteLine("Download Complete");

                //if an update occured
                if (File.Exists(gameDirTextBox.Text + "ori_amd_ags_x64.dll"))
                {
                    Console.WriteLine("Proxied DLL found, Checking MD5");
                    if (GetMD5(gameDirTextBox.Text + "ori_amd_ags_x64.dll").ToUpper() != outstr[0]) // original
                    {
                        Console.WriteLine("MD5 Mismatch, Deleting Proxied DLL");
                        File.Delete(gameDirTextBox.Text + "ori_amd_ags_x64.dll");
                    }
                }


                if (File.Exists(gameDirTextBox.Text + "amd_ags_x64.dll"))
                {
                    Console.WriteLine(GetMD5(gameDirTextBox.Text + "amd_ags_x64.dll").ToUpper());
                    Console.WriteLine(outstr[0]);

                    if (GetMD5(gameDirTextBox.Text + "amd_ags_x64.dll").ToUpper() == outstr[0]) // original
                    {
                        Console.WriteLine("Original DLL found");
                        Console.WriteLine("Duplicating Original DLL");
                        File.Copy(gameDirTextBox.Text + "amd_ags_x64.dll", gameDirTextBox.Text + "ori_amd_ags_x64.dll");
                        Console.WriteLine("Deleting Original DLL");
                        File.Delete(gameDirTextBox.Text + "amd_ags_x64.dll");
                        Console.WriteLine("Downloading Proxy File");
                        client.DownloadFile("https://coltonon.cc/cmods/amd_ags_x64.dll", gameDirTextBox.Text + "amd_ags_x64.dll");
                    } 

                    if (GetMD5(gameDirTextBox.Text + "amd_ags_x64.dll").ToUpper() == outstr[1]) // proxied
                    {
                        if (!Directory.Exists(tmp_gmadir))
                        {
                            Directory.CreateDirectory(tmp_gmadir);
                            Console.WriteLine("Creating Mod Dir");

                        }
                        if (!Directory.Exists(tmp_moddir))
                        {
                            Directory.CreateDirectory(tmp_moddir);
                            Console.WriteLine("Creating Dir for Mod");
                        }

                        WebClient wclist = new WebClient();
                        wclist.DownloadProgressChanged += (li, lo) =>
                        {
                            materialProgressBar1.Value = lo.ProgressPercentage;
                        };
                        wclist.DownloadStringCompleted += (li, lo) =>
                        {
                            string[] files = lo.Result.Split(';');
                            foreach (string filehash in files)
                            {
                                string file = filehash.Split(':')[0];
                                WebClient wcfile = new WebClient();
                                wcfile.DownloadProgressChanged += (fi, fo) => {
                                    materialProgressBar1.Value = fo.ProgressPercentage;
                                };
                                wcfile.DownloadFileCompleted += (fi, fo) =>
                                {
                                    Console.WriteLine("Finished Downloading File");
                                };
                                wcfile.DownloadFileAsync(new Uri("https://coltonon.cc/cmods/" + materialListView1.SelectedItems[0].SubItems[0].Text + "/" + file), tmp_moddir + "\\" + file);
                            }
                            
                            reloadshit = true;  
                        };
                        Console.WriteLine("https://coltonon.cc/cmods/" + materialListView1.SelectedItems[0].SubItems[0].Text + "/info.dat");

                        wclist.DownloadStringAsync(new Uri("https://coltonon.cc/cmods/" + materialListView1.SelectedItems[0].SubItems[0].Text + "/info.dat"));
                        // do shit here

                    }
                } else
                {
                    Console.WriteLine("No File");
                }

                
            };
            client.DownloadStringAsync(new Uri("https://coltonon.cc/cmods/proxied.dat"));
            testhit();
           // ReloadMods();
        }


        private void materialListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (materialListView1.SelectedItems.Count == 1)
            {
                moddescription.Text = materialListView1.SelectedItems[0].SubItems[1].Text;
                moreinfobutt.Enabled = moreinfobutt.Visible = materialRaisedButton3.Enabled = materialRaisedButton3.Visible = true;


     
            }
            else
            {
                moddescription.Text = "Click on a mod to view it's description";
                moreinfobutt.Enabled = moreinfobutt.Visible = materialRaisedButton3.Enabled = materialRaisedButton3.Visible = false;
            }
            CheckChecks();
        }


        private async void testhit()
        {
            materialProgressBar1.Value = 0;
            Console.WriteLine("Refreshing UI");
            await Task.Delay(TimeSpan.FromSeconds(3));
            ReloadMods();
        }


        private void CheckChecks()
        {
            ReColor();
            if (materialListView1.SelectedItems.Count == 1 && gameDirCheckBox.Checked)
            {
                if ((materialListView1.SelectedItems[0].SubItems[2].Text == "Installed") ||
                    (materialListView1.SelectedItems[0].SubItems[2].Text == "Outdated/Corrupt"))
                {
                    UninstallButton.Visible = UninstallButton.Enabled = true;
                    InstallButton.Visible = InstallButton.Enabled = false;
                } else
                {
                    UninstallButton.Visible = UninstallButton.Enabled = false;
                    InstallButton.Visible = InstallButton.Enabled = true;

                }
            } else
            {
                UninstallButton.Visible = UninstallButton.Enabled = false;
                InstallButton.Visible = InstallButton.Enabled = false;
            }


            if (moduleListView.SelectedItems.Count == 1 && ProcCheckBox.Checked)
            {
                EjectButton.Visible = true;
            } else
            {
                EjectButton.Visible = false;
            }


            Process[] procs = Process.GetProcessesByName("starwarsbattlefrontii");
            if (procs.Length < 1)
            {
                ProcCheckBox.Checked = false;
                DllReadyCheckBox.Checked = false;
                InejctButton.Visible = false;
                EjectButton.Visible = false;
                return;
            }
            else
            {
                ProcCheckBox.Checked = true;
                if (File.Exists(DllInjectTextBox.Text))
                {
                    if (DllInjectTextBox.Text.EndsWith(".dll"))
                    {
                        foreach (ListViewItem li in moduleListView.Items)
                        {
                            if (li.SubItems[2].Text == DllInjectTextBox.Text)
                            {
                                InejctButton.Visible = false;
                                DllReadyCheckBox.Checked = false;
                                return;
                            }
                        }
                        DllReadyCheckBox.Checked = true;
                        InejctButton.Visible = true;
                    }
                } else
                {
                  //  ProcCheckBox.Checked = false;
                    DllReadyCheckBox.Checked = false;
                    InejctButton.Visible = false;
                    return;
                }
            }
        }



        private void WriteConfig()
        {
            materialProgressBar1.Value = 0;
            foreach (var process in Process.GetProcessesByName("starwarsbattlefrontii"))
            {
               // MessageBox.Show("Cannot write config to running game", "Hold Up!", MessageBoxButtons.OK);
                return;
            }


            if (!gameDirCheckBox.Checked) return;
            if (!Directory.Exists(gameDirTextBox.Text + "cmods")) return;
            Console.WriteLine("Writing Config");
         //   if (File.Exists(gameDirTextBox.Text + "cmods\\mods.dat")) File.Delete(gameDirTextBox.Text + "cmods\\mods.dat");
            using (StreamWriter file =
            new StreamWriter(gameDirTextBox.Text + "cmods\\mods.dat"))
            {
                
                foreach (string mod in configed_mods)
                {
                    Console.WriteLine("Saving: " + mod);
                    file.WriteLine(mod);
                }
            }
           // configed_mods.Clear();

        }


        private static bool reloadshit = false;


        private void timer1_Tick(object sender, EventArgs e)
        {
            if (reloadshit)
            {
              //  ReloadMods();
                reloadshit = false;
            }
            CheckChecks();
        }

        private void UninstallButton_Click(object sender, EventArgs e)
        {
            if (!gameDirCheckBox.Checked) return;
            if (materialListView1.SelectedItems.Count != 1) return;
            string tmp_gmadir = gameDirTextBox.Text + "cmods\\";
            string tmp_moddir = gameDirTextBox.Text + "cmods\\" + materialListView1.SelectedItems[0].SubItems[0].Text;
            DeleteDirectory(tmp_moddir);
            ReloadMods();
        }

        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }

        private void gameDirCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ReloadMods();
        }

        private void materialLabel4_Click(object sender, EventArgs e)
        {
           Process.Start("https://coltonon.cc");
        }

        private void moreinfobutt_Click(object sender, EventArgs e)
        {
           GetModDetails(materialListView1.SelectedItems[0].Text);

        }

        private void materialRaisedButton2_Click(object sender, EventArgs e)
        {
            if (gameDirCheckBox.Checked)
            {
                Process.Start(gameDirTextBox.Text + "starwarsbattlefrontii.exe");
                Console.WriteLine("Launching Game");
            }
        }




        public void GetModDetails(string modName)
        {

            WebClient client = new WebClient();
            client.DownloadStringCompleted += (sender, args) =>
            {
                materialTabControl1.SelectedIndex = 1;
                richTextBox1.Text = args.Result;
                string[] textBoxLines = richTextBox1.Lines;
                for (int i = 0; i < textBoxLines.Length; i++)
                {
                    string line = textBoxLines[i];
         
                    if (line.StartsWith("#"))
                    {
                        richTextBox1.SelectionStart = richTextBox1.GetFirstCharIndexFromLine(i);
                        richTextBox1.SelectionLength = line.Length;
                        richTextBox1.Lines[i] = richTextBox1.Lines[i].Replace("#", "");
                        richTextBox1.SelectionFont = new Font(MaterialSkin.MaterialSkinManager.Instance.ROBOTO_REGULAR_11, FontStyle.Bold);
                        richTextBox1.SelectionIndent = 8;
                    }
                    else
                    {
                        richTextBox1.SelectionStart = richTextBox1.GetFirstCharIndexFromLine(i);
                        richTextBox1.SelectionLength = line.Length;

                        richTextBox1.SelectionFont = new Font(MaterialSkin.MaterialSkinManager.Instance.ROBOTO_REGULAR_11, FontStyle.Regular);
                    }
                }
                richTextBox1.SelectionLength = 0;//Unselect the selection


                var imageList = new ImageList();
                listView1.Items.Clear();
                foreach (Match item in Regex.Matches(richTextBox1.Text, @"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?"))
                {
                    if (!item.Value.EndsWith("png")) continue;
                   WebClient imageClient = new WebClient();
                    imageClient.DownloadDataCompleted += (o, eventArgs) =>
                    {
                        var bmp = getThumb(GetImageFromByteArray(eventArgs.Result));

                        imageList.Images.Add(item.Value, bmp);
                        
                        imageList.ImageSize = new Size(256, 256);
                        imageList.ColorDepth = ColorDepth.Depth32Bit;
                        listView1.LargeImageList = imageList;
                        var listViewItem = listView1.Items.Add(item.Value);
                        listViewItem.ImageKey = item.Value;
                    };
                    imageClient.Headers.Add("user-agent", "Test");

                    imageClient.DownloadDataAsync(new Uri(item.Value));


                }


                for (int i = 0; i < textBoxLines.Length; i++)
                {
                    string line = textBoxLines[i];
                    if (line.StartsWith("!"))
                    {
                        changeLine(i, " ");
                    }

                    if (line.Contains("#"))
                    {
                        changeLine(i, line.Trim('#'));
                    }
                }





            };
            client.DownloadStringAsync(new Uri("https://coltonon.cc/cmods/" + materialListView1.SelectedItems[0].Text + "/info.md"));
            // Process.Start("https://coltonon.cc/cmods/viewmod.php?mod=" + materialListView1.SelectedItems[0].Text);
        }

        private void materialRaisedButton3_Click_1(object sender, EventArgs e)
        {
            Process.Start("https://coltonon.cc/cmods/viewmod.php?mod=" + materialListView1.SelectedItems[0].Text);
        }

        private void tableLayoutPanel7_Paint(object sender, PaintEventArgs e)
        {

        }
        private static Bitmap GetImageFromByteArray(byte[] byteArray)
        {
            var imageConverter = new ImageConverter();
            var bm = (Bitmap)imageConverter.ConvertFrom(byteArray);
            if (bm != null && (bm.HorizontalResolution != (int)bm.HorizontalResolution ||
                               bm.VerticalResolution != (int)bm.VerticalResolution))
                bm.SetResolution((int)(bm.HorizontalResolution + 0.5f),
                    (int)(bm.VerticalResolution + 0.5f));
            return bm;
        }

        public Bitmap getThumb(Bitmap image)

        {
            int tw, th, tx, ty;
            int w = image.Width;
            int h = image.Height;
            double whRatio = (double)w / h;
            if (image.Width >= image.Height)
            {
                tw = 256;
                th = (int)(tw / whRatio);
            } else
            {
                th = 256;
                tw = (int)(th * whRatio);
            }
            tx = (256 - tw) / 2;
            ty = (256 - th) / 2;
            Bitmap thumb = new Bitmap(256, 256, PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(thumb);
            g.Clear(MaterialSkinManager.Instance.GetApplicationBackgroundColor());
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(image,
                new Rectangle(tx, ty, tw, th),
                new Rectangle(0, 0, w, h),
                GraphicsUnit.Pixel);
            return thumb;
        }

        private void materialListView1_ItemActivate(object sender, EventArgs e)
        {
            GetModDetails(materialListView1.SelectedItems[0].Text);
        }
        void changeLine(int line, string text)
        {
            RichTextBox RTB = richTextBox1;
            int s1 = RTB.GetFirstCharIndexFromLine(line);
            int s2 = line < RTB.Lines.Count() - 1 ?
                RTB.GetFirstCharIndexFromLine(line + 1) - 1 :
                RTB.Text.Length;
            RTB.Select(s1, s2 - s1);
            RTB.SelectedText = text;
        }

        private void materialRaisedButton3_Click_2(object sender, EventArgs e)
        {
            Process.Start("https://coltonon.cc/cmods/viewmod.php?mod=" + materialListView1.SelectedItems[0].Text);
        }

        private void materialListView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            CalcResize();
        }

        public void CalcResize()
        {
            materialListView1.Columns[1].Width = tableLayoutPanel1.Width - (materialListView1.Columns[0].Width + materialListView1.Columns[2].Width + 8);
            TabPage[] hiddenTabs = {tabPage5, tabPage4, tabPage7};
            for (int i = 0; i < hiddenTabs.Length; i++)
            {
                if (showAdvancedCheckBox.Checked)
                {
                    if (!materialTabSelector1.BaseTabControl.TabPages.Contains(hiddenTabs[i]))
                    {
                        materialTabSelector1.BaseTabControl.TabPages.Add(hiddenTabs[i]);
                    }
                }
                else
                {
                    if (materialTabSelector1.BaseTabControl.TabPages.Contains(hiddenTabs[i]))
                    {
                        materialTabSelector1.BaseTabControl.TabPages.Remove(hiddenTabs[i]);
                    }

                }
            }
        }

        [DllImport("user32")]
        private static extern long ShowScrollBar(long hwnd, long wBar, long bShow);
        long SB_HORZ = 0;
        long SB_VERT = 1;
        long SB_BOTH = 3;

        private void showAdvancedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CalcResize();
        }

        private void ReColor()
        {
            MaterialSkinManager.Instance.Theme = (Properties.Settings.Default.darktheme) ? MaterialSkinManager.Themes.DARK : MaterialSkinManager.Themes.LIGHT ;

            if (purpleThemeRadioButton.Checked)
            {
                ColorScheme schem2e = new ColorScheme(
                  ColorTranslator.FromHtml("#482880"),    // Primary
                  ColorTranslator.FromHtml("#341d5b"),    // DarkPrimary
                  ColorTranslator.FromHtml("#845bcb"),    // Light
                  ColorTranslator.FromHtml("#ffbb40"),    // Accent
                  TextShade.WHITE, MaterialSkinManager.Instance.GetApplicationBackgroundColor());
                if (MaterialSkinManager.Instance.ColorScheme != schem2e)
                {
                    MaterialSkinManager.Instance.ColorScheme = schem2e;
                }
                return;
            }


            TextShade tx = TextShade.WHITE;
            if (ThemeInfo.GetThemeColor().GetBrightness() > .5f)
            {
                tx = TextShade.BLACK;
            }
            ColorScheme scheme = new ColorScheme(
                ThemeInfo.GetThemeColor(),    // Primary
                ControlPaint.Dark(ThemeInfo.GetThemeColor(), .1f),    // DarkPrimary
                ControlPaint.Light(ThemeInfo.GetThemeColor(), .8f),    // Light
                SetHue(ControlPaint.Light(ThemeInfo.GetThemeColor(), .8f), 120),    // Accent
                tx, MaterialSkinManager.Instance.GetApplicationBackgroundColor());
            
            if (MaterialSkinManager.Instance.ColorScheme != scheme)
            {
                MaterialSkinManager.Instance.ColorScheme = scheme;
            }
        }

        private void toolStripContainer1_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel10_Paint(object sender, PaintEventArgs e)
        {

        }


        public void LoadModules()
        {
            Process[] procs = Process.GetProcessesByName("starwarsbattlefrontii");
            if (procs.Length < 1) return;
            Process proc = procs[0];
            List<ListViewItem> lvs = new List<ListViewItem>();
            foreach (ProcessModule module in proc.Modules)
            {
                if (KnownModules.Contains(module.ModuleName)) continue;
                ListViewItem li = new ListViewItem(module.ModuleName);
              //  Console.Write("\"" + module.ModuleName + "\", ");
               // li.SubItems.Add(module.ModuleName);
                li.SubItems.Add(module.BaseAddress.ToString("X"));
                li.SubItems.Add(module.FileName);
                lvs.Add(li);
            }
            bool rebuild = false;
            foreach (ListViewItem li in lvs)
            {
                if (!moduleListView.Items.Contains(li))
                {
                    rebuild = true;
                }
            }
            if (rebuild)
            {
                moduleListView.Items.Clear();
                foreach (ListViewItem li in lvs)
                {
                    moduleListView.Items.Add(li);
                }
            }
            
        }

        public static string[] KnownModules = { "starwarsbattlefrontii.exe", "WINMMBASE.dll", "ntdll.dll", "KERNEL32.DLL", "KERNELBASE.dll", "Activation64.dll", "CRYPT32.dll", "ucrtbase.dll", "MSASN1.dll", "WINTRUST.dll", "msvcrt.dll", "RPCRT4.dll", "advapi32.dll", "sechost.dll", "USER32.dll", "win32u.dll", "GDI32.dll", "MSVCP120.dll", "MSVCR120.dll", "gdi32full.dll", "msvcp_win.dll", "SHELL32.dll", "cfgmgr32.dll", "shcore.dll", "combase.dll", "bcryptPrimitives.dll", "windows.storage.dll", "shlwapi.dll", "kernel.appcore.dll", "powrprof.dll", "profapi.dll", "IMM32.DLL", "igo64.dll", "ole32.dll", "MSVCP140.dll", "VCRUNTIME140.dll", "WINMM.dll", "winmmbase.dll", "ntmarta.dll", "dbdata.dll", "WS2_32.dll", "uxtheme.dll", "CRYPTSP.dll", "rsaenh.dll", "bcrypt.dll", "CRYPTBASE.dll", "imagehlp.dll", "gpapi.dll", "cryptnet.dll", "IPHLPAPI.DLL", "WINNSI.DLL", "NSI.dll", "dbghelp.dll", "DINPUT8.dll", "GFSDK_Aftermath_Lib.x64.dll", "inputhost.dll", "CoreUIComponents.dll", "CoreMessaging.dll", "wintypes.dll", "MFPlat.DLL", "RTWorkQ.DLL", "OLEAUT32.dll", "PSAPI.DLL", "USP10.dll", "VERSION.dll", "WTSAPI32.dll", "amd_ags_x64.dll", "ori_amd_ags_x64.dll", "d3d11.dll", "dxgi.dll", "dbgcore.DLL", "dwmapi.dll", "urlmon.dll", "iertutil.dll", "mswsock.dll", "msiso.dll", "Engine.BuildInfo.dll", "d3d12.dll", "msvcp110_win.dll", "Setupapi.dll", "DEVOBJ.dll", "nvapi64.dll", "nvwgf2umx.dll", "nvspcap64.dll", "WINHTTP.dll", "SspiCli.dll", "dxilconv.dll", "clbcatq.dll", "sxs.dll", "AnselSDK64.dll", "nvcuda.dll", "nvfatbinaryLoader.dll", "DWrite.dll", "nvldumdx.dll", "NvCamera64.dll", "XINPUT9_1_0.dll", "HID.DLL", "WININET.dll", "WindowsCodecs.dll", "d3dcompiler_47_64.dll", "GFSDK_ShadowLib_DX11.win64.dll", "xinput1_4.dll", "USERENV.dll", "DPAPI.dll", "MSCTF.dll", "WINSTA.dll", "MMDevApi.dll", "PROPSYS.dll", "AUDIOSES.DLL", "AVRT.dll", "EAWebKit.dll", "DNSAPI.dll", "rasadhlp.dll", "fwpuclnt.dll", "wdmaud.drv", "ksuser.dll", "msacm32.drv", "MSACM32.dll", "midimap.dll", "dhcpcsvc6.DLL", "dhcpcsvc.DLL", "D3DCOMPILER_47.dll", };


        private void RefreshModules_Click(object sender, EventArgs e)
        {
            moduleListView.Items.Clear();
            LoadModules();
        }

        private void materialRaisedButton5_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = ".dll files | *.dll";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                DllInjectTextBox.Text = openFileDialog.FileName;
            }
        }

        private void InejctButton_Click(object sender, EventArgs e)
        {
            if (DllReadyCheckBox.Checked && ProcCheckBox.Checked)
            {
                Injector.doshit("starwarsbattlefrontii", DllInjectTextBox.Text);
            }
        }

        private void darkThemeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.darktheme = darkThemeCheckBox.Checked;
            Properties.Settings.Default.windowstheme = systemThemeRadioButton.Checked;
            Properties.Settings.Default.Save();

        }

        private void materialRaisedButton6_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
            Application.Restart();
        }

        private void darkThemeCheckBox_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.darktheme = darkThemeCheckBox.Checked;
            Properties.Settings.Default.windowstheme = systemThemeRadioButton.Checked;
            Properties.Settings.Default.Save();
            Application.Restart();
        }

        private void tableLayoutPanel13_Paint(object sender, PaintEventArgs e)
        {

        }

        private void DllInjectTextBox_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.dllpath = DllInjectTextBox.Text;
            Properties.Settings.Default.Save();
        }

        private void materialLabel3_Click(object sender, EventArgs e)
        {

        }

        private void materialLabel3_DoubleClick(object sender, EventArgs e)
        {
            Process.Start("https://coltonon.cc");
        }


        

    }

    public class ControlWriter : TextWriter
    {
        private Control textbox;
        public ControlWriter(Control textbox)
        {
            this.textbox = textbox;
        }

        public override void Write(char value)
        {
            textbox.Text += value;
        }

        public override void Write(string value)
        {
            textbox.Text += value;
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }



    }

    public class ThemeInfo
    {
        [DllImport("uxtheme.dll", EntryPoint = "#95")]
        public static extern uint GetImmersiveColorFromColorSetEx(uint dwImmersiveColorSet, uint dwImmersiveColorType, bool bIgnoreHighContrast, uint dwHighContrastCacheMode);
        [DllImport("uxtheme.dll", EntryPoint = "#96")]
        public static extern uint GetImmersiveColorTypeFromName(IntPtr pName);
        [DllImport("uxtheme.dll", EntryPoint = "#98")]
        public static extern int GetImmersiveUserColorSetPreference(bool bForceCheckRegistry, bool bSkipCheckOnFail);

        public static Color GetThemeColor()
        {
            var colorSetEx = GetImmersiveColorFromColorSetEx(
                (uint)GetImmersiveUserColorSetPreference(false, false),
                GetImmersiveColorTypeFromName(Marshal.StringToHGlobalUni("ImmersiveStartSelectionBackground")),
                false, 0);

            var colour = Color.FromArgb((byte)((0xFF000000 & colorSetEx) >> 24), (byte)(0x000000FF & colorSetEx),
                (byte)((0x0000FF00 & colorSetEx) >> 8), (byte)((0x00FF0000 & colorSetEx) >> 16));

            return colour;
        }
    }

    public static class ProcessExtensions
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct ParentProcessUtilities
        {
            // These members must match PROCESS_BASIC_INFORMATION
            internal IntPtr Reserved1;
            internal IntPtr PebBaseAddress;
            internal IntPtr Reserved2_0;
            internal IntPtr Reserved2_1;
            internal IntPtr UniqueProcessId;
            internal IntPtr InheritedFromUniqueProcessId;

            [DllImport("ntdll.dll")]
            private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref ParentProcessUtilities processInformation, int processInformationLength, out int returnLength);

            /// <summary>
            /// Gets the parent process of the current process.
            /// </summary>
            /// <returns>An instance of the Process class.</returns>
            public static Process GetParentProcess()
            {
                return GetParentProcess(Process.GetCurrentProcess().Handle);
            }

            /// <summary>
            /// Gets the parent process of specified process.
            /// </summary>
            /// <param name="id">The process id.</param>
            /// <returns>An instance of the Process class.</returns>
            public static Process GetParentProcess(int id)
            {
                Process process = Process.GetProcessById(id);
                return GetParentProcess(process.Handle);
            }

            /// <summary>
            /// Gets the parent process of a specified process.
            /// </summary>
            /// <param name="handle">The process handle.</param>
            /// <returns>An instance of the Process class.</returns>
            public static Process GetParentProcess(IntPtr handle)
            {
                ParentProcessUtilities pbi = new ParentProcessUtilities();
                int returnLength;
                int status = NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out returnLength);
                if (status != 0)
                    throw new Win32Exception(status);

                try
                {
                    return Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
                }
                catch (ArgumentException)
                {
                    // not found
                    return null;
                }
            }
        }
    }
}
