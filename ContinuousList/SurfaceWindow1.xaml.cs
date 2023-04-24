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
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace ContinuousList
{
    /// <summary>
    /// Interaction logic for SurfaceWindow1.xaml
    /// </summary>
    public partial class SurfaceWindow1 : SurfaceWindow, INotifyPropertyChanged
    {
        private CarouselPanel listPanel1 = null;
        private LoopingPanel listPanel2 = null;
        private CarouselPanel listPanel3 = null;

        private ObservableCollection<string> fullliste = new ObservableCollection<string>();

        private ObservableCollection<string> _datalist1 = new ObservableCollection<string>();
        private ObservableCollection<string> _datalist2 = new ObservableCollection<string>();
        private ObservableCollection<string> _datalist3 = new ObservableCollection<string>();

        public ObservableCollection<string> datalist1
        {
            get { return _datalist1; }
            set { _datalist1 = value; }
        }

        public ObservableCollection<string> datalist2
        {
            get { return _datalist2; }
            set { _datalist2 = value; }
        }

        public ObservableCollection<string> datalist3
        {
            get { return _datalist3; }
            set { _datalist3 = value; }
        }

        private Random random = new Random();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SurfaceWindow1()
        {
            InitializeComponent();

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            string publicFoldersPath = Environment.GetEnvironmentVariable("public");
            string publicImagesPath = publicFoldersPath + @"\Pictures\Sample Pictures";

            string[] list = System.IO.Directory.GetFiles(publicImagesPath, "*.jpg");

            foreach (string s in list)
            {
                fullliste.Add(s);
                if (_datalist1.Count < 10) { _datalist1.Add(s); }
                if (_datalist2.Count < 3) { _datalist2.Add(s); }
                if (_datalist3.Count < 20) { _datalist3.Add(s); }
            }

            this.DataContext = this;

            label1.Content = datalist1.Count.ToString();
            label2.Content = datalist2.Count.ToString();
            label3.Content = datalist3.Count.ToString();
        }

        /// <summary>
        /// Occurs when the window is about to close. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Remove handlers for window availability events
            RemoveWindowAvailabilityHandlers();
        }

        /// <summary>
        /// Adds handlers for window availability events.
        /// </summary>
        private void AddWindowAvailabilityHandlers()
        {
            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;
        }

        /// <summary>
        /// Removes handlers for window availability events.
        /// </summary>
        private void RemoveWindowAvailabilityHandlers()
        {
            // Unsubscribe from surface window availability events
            ApplicationServices.WindowInteractive -= OnWindowInteractive;
            ApplicationServices.WindowNoninteractive -= OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable -= OnWindowUnavailable;
        }

        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: enable audio, animations here
        }

        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: optionally enable animations here
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }

        //------------- LIST 1
        private void List1Add_Click(object sender, RoutedEventArgs e)
        {
            datalist1.Add(fullliste[random.Next(0, fullliste.Count)]);
            label1.Content = datalist1.Count.ToString();
        }

        private void List1Del_Click(object sender, RoutedEventArgs e)
        {
            if (datalist1.Count > 0)
            {
                datalist1.RemoveAt(listPanel1.activeIndex);
                label1.Content = datalist1.Count.ToString();
            }
        }

        private void List1Panel_Loaded(object sender, RoutedEventArgs e)
        {
            listPanel1 = sender as CarouselPanel;
        }

        //-------------  LIST 2 
        private void List2Add_Click(object sender, RoutedEventArgs e)
        {
            datalist2.Add(fullliste[random.Next(0, fullliste.Count)]);
            label2.Content = datalist2.Count.ToString();
        }

        private void List2Del_Click(object sender, RoutedEventArgs e)
        {
            if (datalist2.Count > 0)
            {
                datalist2.RemoveAt(0);
                label2.Content = datalist2.Count.ToString();
            }
        }

        private void List2Panel_Loaded(object sender, RoutedEventArgs e)
        {
            listPanel2 = sender as LoopingPanel;
        }

        //-------------  LIST 3 
        private void List3Add_Click(object sender, RoutedEventArgs e)
        {
            datalist3.Add(fullliste[random.Next(0, fullliste.Count)]);
            label3.Content = datalist3.Count.ToString();
            listPanel1.Refresh();
            list3.InvalidateVisual();
        }

        private void List3Del_Click(object sender, RoutedEventArgs e)
        {
            if (datalist3.Count > 0)
            {
                datalist3.RemoveAt(listPanel3.activeIndex);
                label3.Content = datalist3.Count.ToString();
                listPanel1.Refresh();
                list3.InvalidateVisual();
            }
        }

        private void List3Panel_Loaded(object sender, RoutedEventArgs e)
        {
            listPanel3 = sender as CarouselPanel;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}