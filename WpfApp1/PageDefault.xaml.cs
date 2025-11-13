using System;
using System.Collections;
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
    /// Interaction logic for PageDefault.xaml
    /// </summary>
    public partial class PageDefault : Page
    {
        public PageDefault()
        {
            InitializeComponent();
            FamilyTree.Load();
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformSearch();
            }
        }

        private void PerformSearch()
        {
            string query = SearchBox.Text;
            NavigationService.Navigate(new PageSearchResults(query));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            SearchBox.Focus();
        }

        private void Create_Person(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PageCreate());
        }
    }
}