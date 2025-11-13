using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

            SearchResultsPanel.Children.Clear();

            if (string.IsNullOrWhiteSpace(query))
            {
                SearchResultsPanel.Children.Add(new TextBlock
                {
                    Text = "Nhập tên để tìm kiếm...",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 50, 0, 0)
                });
                return;
            }
            var results = FamilyTree.SearchByName(query);

            if (results.Count == 0)
            {
                SearchResultsPanel.Children.Add(new TextBlock
                {
                    Text = "Không tìm thấy kết quả nào.",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 50, 0, 0)
                });
                return;
            }

            foreach (var r in results)
            {
                var item = CreateSearchResultItem(r.id, r.person);
                SearchResultsPanel.Children.Add(item);
            }
        }

        private UIElement CreateSearchResultItem(int id, Person p)
        {
            var item = new SearchResultItem(id, p);
            item.ItemClicked += (s, personId) =>
            {
                NavigationService.Navigate(new Tree(personId));
            };
            return item;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            RunSearch();
        }
        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RunSearch();
            }
        }
    }
}