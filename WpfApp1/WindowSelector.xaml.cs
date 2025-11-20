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
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for WindowSelector.xaml
    /// </summary>
    public partial class WindowSelector : Window
    {
        public int? SelectedPersonId { get; private set; }

        public WindowSelector(bool? genderFilter, int id, int mode)
        {
            InitializeComponent();
            LoadPeople(genderFilter, id, mode);
        }

        private void LoadPeople(bool? genderFilter, int personid, int mode)
        {
            var allPeople = FamilyTree.SearchByName("");
            var related = FamilyTree.getRelated(personid);
			allPeople.RemoveAll(x => related.Contains(x.id));
            if (mode==2) allPeople.RemoveAll(x => x.person.Dad.HasValue); // get fatherless
            else if (mode==3) allPeople.RemoveAll(x => x.person.Mom.HasValue); // get motherless
			if (genderFilter.HasValue)
            {
                allPeople = allPeople.Where(p => p.person.Gender == genderFilter.Value).ToList();
            }
            foreach (var (id, person) in allPeople.OrderBy(p => p.person.Name))
            {
                var item = new ListBoxItem
                {
                    Content = $"{person.Name} ({person.BirthDate:dd-MM-yyyy}) - {(person.Gender ? "Male" : "Female")}",
                    Tag = id
                };
                PeopleListBox.Items.Add(item);
            }
        }

		private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = SearchBox.Text.ToLower();

            foreach (ListBoxItem item in PeopleListBox.Items)
            {
                string content = item.Content.ToString().ToLower();
                item.Visibility = content.Contains(query) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            if (PeopleListBox.SelectedItem is ListBoxItem item)
            {
                SelectedPersonId = (int)item.Tag;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a person.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
