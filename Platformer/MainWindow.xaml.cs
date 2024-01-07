using Platformer.Classes;
using Platformer.UserControls;
using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Security.RightsManagement;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static Platformer.Classes.Entity;
using static System.Net.Mime.MediaTypeNames;

namespace Platformer {
    public partial class MainWindow : Window {
        public MainWindow() {
            //application initialization
            InitializeComponent();

            //since we are using pixel art sprites
            //SnapsToDevicePixels = true;
            //RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
            //RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

            Content = new MainMenu_UserControl();
        }
    }
}