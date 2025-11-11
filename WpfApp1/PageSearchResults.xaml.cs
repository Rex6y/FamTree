using System;
using System.Collections.Generic;
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

        private UIElement CreateSearchResultItem(int id, Person p)
        {
            var btn = new Button
            {
                Content = $"{p.Name} - {p.BirthDate:dd/MM/yyyy}",
                Margin = new Thickness(0, 5, 0, 0),
                Height = 35,
                Tag = id
            };

            btn.Click += (s, e) =>
            {
                //NavigationService.Navigate(new PersonPage(id));
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
