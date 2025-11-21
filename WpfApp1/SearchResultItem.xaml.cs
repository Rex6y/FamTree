using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfApp1
{
    public partial class SearchResultItem : UserControl
    {
        public int PersonId { get; set; }
        public event EventHandler<int> ItemClicked;

        public SearchResultItem(int id, Person person)
        {
            InitializeComponent();
            PersonId = id;

            UserNameText.Text = person.Name;
            UserBirthDateText.Text = $"Ngày sinh: {person.BirthDate:dd/MM/yyyy}";

            if (person.Pfp != null && person.Pfp.Length > 0)
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    using (var ms = new MemoryStream(person.Pfp))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = ms;
                        bitmap.EndInit();
                    }
                    bitmap.Freeze();

                    var imageBrush = new ImageBrush(bitmap)
                    {
                        Stretch = Stretch.UniformToFill
                    };
                    AvatarEllipse.Fill = imageBrush;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Load lỗi: {ex.Message}");
                }
            }

            ActionButton.Click += ActionButton_Click;
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            ItemClicked?.Invoke(this, PersonId);
        }
    }
}