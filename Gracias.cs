using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Net;
using System.Windows.Forms;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace Mod_Manager
{
    public partial class Gracias : ListView
    {
        private readonly ImageList _imageList = new ImageList();

        public Gracias()
        {
            View = View.LargeIcon;
            InitializeComponent();
            ItemActivate += Gracias_ItemActivate;
        }

        private void Gracias_ItemActivate(object sender, EventArgs e)
        {
            foreach (ListViewItem item in Items)
                if (item.Text == SelectedItems[0].Text)
                    Process.Start(item.Name);
        }

        public void AddCredit(string name)
        {
            try
            {
                name = "https://www.unknowncheats.me/forum/members/" + name + ".html";
                var client = new WebClient();
                client.DownloadStringCompleted += (sender, args) =>
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(args.Result);
                    var picElement = doc.GetElementbyId("user_avatar");
                    var imageUrl = picElement.GetAttributeValue("src", "");
                    var nameElement = doc.GetElementbyId("username_box");
                    var username = nameElement.ChildNodes[3].FirstChild.InnerText;
                    var imageClient = new WebClient();
                    imageClient.DownloadDataCompleted += (o, eventArgs) =>
                    {
                        var bmp = getThumb(GetImageFromByteArray(eventArgs.Result));
                        _imageList.Images.Add(username, bmp);
                        _imageList.ImageSize = new Size(96, 96);
                        _imageList.ColorDepth = ColorDepth.Depth32Bit;
                        LargeImageList = _imageList;
                        var listviewItem = new ListViewItem
                        {
                            Text = username,
                            Name = name,
                            ImageIndex = _imageList.Images.Count - 1
                        };
                        Items.Add(listviewItem);
                    };
                    imageClient.Headers.Add("user-agent", "Test");
                    imageClient.DownloadDataAsync(new Uri(imageUrl));
                };
                client.Headers.Add("user-agent", "Test");
                client.DownloadStringAsync(new Uri(name));
            }
            catch (Exception) { }
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
        private Bitmap getThumb(Bitmap image)

        {
            int tw, th, tx, ty;
            int w = image.Width;
            int h = image.Height;
            double whRatio = (double)w / h;
            if (image.Width >= image.Height)
            {
                tw = 256;
                th = (int)(tw / whRatio);
            }
            else
            {
                th = 256;
                tw = (int)(th * whRatio);
            }
            tx = (256 - tw) / 2;
            ty = (256 - th) / 2;
            Bitmap thumb = new Bitmap(256, 256, PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(thumb);
            g.Clear(this.BackColor);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(image,
                new Rectangle(tx, ty, tw, th),
                new Rectangle(0, 0, w, h),
                GraphicsUnit.Pixel);
            return thumb;
        }
    }
}