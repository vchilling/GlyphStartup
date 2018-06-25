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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GlyphStartup
{
    /// <summary>
    /// Interaction logic for InfoBox.xaml
    /// </summary>
    public partial class InfoBox : UserControl
    {
        public event EventHandler GallerySelected;
        public InfoBox()
        {
            InitializeComponent();
            lstGalleries.SelectionChanged += new SelectionChangedEventHandler(lstGalleries_SelectionChanged);
        }

        void lstGalleries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EventHandler handler = GallerySelected;
            EventGallerySelectedArgs eData = new EventGallerySelectedArgs();
            eData.SelectedGallery = (Gallery)lstGalleries.SelectedValue;
            if (handler != null)
                handler(sender, eData);
        }

        public class EventGallerySelectedArgs : EventArgs
        {
            public Gallery SelectedGallery { get; set; }
        }

        public bool Visible { get; set; }

        
    }
}
