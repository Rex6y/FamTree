using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for PageCreate.xaml
    /// </summary>
    public partial class PageCreate : Page
    {
        public PageCreate()
        {
            InitializeComponent();
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
        private void Pfp_click(object sender, MouseButtonEventArgs e)
        {
            // Create and configure the OpenFileDialog
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*"
            };

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                // Load the selected image into the Image control
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(openFileDialog.FileName, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                Pfp.Source = bitmap; // set the Image control source
            }
        }

        private byte[]? ImageToByteArray(Image imgControl)
        {
            if (imgControl.Source == null)
                return null;

            var bitmapSource = imgControl.Source as BitmapSource;
            if (bitmapSource == null)
                return null;

            using (var stream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(stream);
                return stream.ToArray();
            }
        }
        private void CreatePerson_Click(object sender, RoutedEventArgs e)
        {
            string Name = FullName.Text.Trim();
            if (string.IsNullOrEmpty(Name))
            {
                MessageBox.Show("Vui lòng nhập họ và tên.");
                return;
            }

            if (GenderMale.IsChecked != true && GenderFemale.IsChecked != true)
            {
                MessageBox.Show("Vui lòng chọn giới tính.");
                return;
            }
            bool Gender = GenderMale.IsChecked == true;

            if (!Birthdate.SelectedDate.HasValue)
            {
                MessageBox.Show("Vui lòng chọn ngày sinh.");
                return;
            }
            DateTime birthDate = Birthdate.SelectedDate.Value;

            byte[]? imageData = null;
            if (Pfp.Source != null)
            {
                imageData = ImageToByteArray(Pfp);
            }

            int id = FamilyTree.AddPerson(Name, Gender, birthDate, 0, null, null, null, imageData);
            NavigationService.GoBack();
            return;
        }
    }
}
