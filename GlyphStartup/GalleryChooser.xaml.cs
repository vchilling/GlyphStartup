using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace GlyphStartup
{
    /// <summary>
    /// Interaction logic for GalleryChooser.xaml
    /// </summary>
    public partial class GalleryChooser : Window
    {
        private string galeriesRemoteUrL = "d:\\galleries\\galleryList.json";

        public GalleryChooser()
        {
            InitializeComponent();
            galleryList.GallerySelected += new EventHandler(galleryList_GallerySelected);            
            String json = File.ReadAllText(@galeriesRemoteUrL);
            List<Gallery> galleriesFromJson = JsonConvert.DeserializeObject<List<Gallery>>(json);            
            galleryList.DataContext = galleriesFromJson;

        }

        void galleryList_GallerySelected(object sender, EventArgs e)
        {
            MainWindow galleryForm = new MainWindow(((EventGallerySelectedArgs)e).SelectedGallery);
            galleryForm.Show();
            this.Hide();

        }
    }
}
