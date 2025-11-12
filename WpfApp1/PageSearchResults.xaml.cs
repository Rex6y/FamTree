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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for PageSearchResults.xaml
    /// </summary>
    public partial class PageSearchResults : Page
    {
        public PageSearchResults(string query)
        {
            InitializeComponent();
            SearchBox.Text = query;
            RunSearch();
        }

        private void RunSearch()
        {
            string query = SearchBox.Text.Trim();
            var results = FamilyTree.SearchByName(query);

            SearchResultsPanel.Children.Clear();

            if (results.Count == 0)
            {
                SearchResultsPanel.Children.Add(
                    new TextBlock { Text = "Không tìm thấy.", Margin = new Thickness(5) }
                );
                return;
            }

            foreach (var r in results)
            {
                var item = CreateSearchResultItem(r.id, r.person);
                SearchResultsPanel.Children.Add(item);
            }

        }

        private UIElement CreateSearchResultItem(int id, Person p) {
            var mainPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5)
            };
            var img = new Image
            {
                Width = 50,
                Height = 50,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Top
            };

            if (p.Pfp != null && p.Pfp.Length > 0)
            {
                BitmapImage bitmap = new BitmapImage();
                using (var ms = new MemoryStream(p.Pfp))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                }
                bitmap.Freeze();
                img.Source = bitmap;
            }
            var textPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center
            };
            textPanel.Children.Add(new TextBlock
            {
                Text = p.Name,
                FontWeight = FontWeights.Bold,
                FontSize = 14
            });
            textPanel.Children.Add(new TextBlock
            {
                Text = $"Ngày sinh: {p.BirthDate:dd/MM/yyyy}",
                FontSize = 12
            });
            mainPanel.Children.Add(img);
            mainPanel.Children.Add(textPanel);
            var btn = new Button
            {
                Content = mainPanel,
                Margin = new Thickness(0, 5, 0, 0),
                Height = 70,
                Tag = id,
                HorizontalContentAlignment = HorizontalAlignment.Left
            };

            btn.Click += (s, e) =>
            {
                // NavigationService.Navigate(new PersonPage(id));
            };

            return btn;
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            RunSearch();
        }
    }
}
