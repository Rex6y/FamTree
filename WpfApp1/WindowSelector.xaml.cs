using System;
using System.Collections;
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
            var p = FamilyTree.GetPerson(personid);
            if (mode == 1) allPeople.RemoveAll(x => x.person.Spouse.HasValue);
            else if (mode == 2) allPeople.RemoveAll(x => x.person.Dad.HasValue || x.person.BirthDate <= p.BirthDate);
            else if (mode == 3) allPeople.RemoveAll(x => x.person.Mom.HasValue || x.person.BirthDate <= p.BirthDate);
            else if (mode == 4) allPeople.RemoveAll(x => x.person.BirthDate >= p.BirthDate);

			if (genderFilter.HasValue)
            {
                allPeople = allPeople.Where(x => x.person.Gender == genderFilter.Value).ToList();
            }
            foreach (var (id, person) in allPeople.OrderBy(x => x.person.Name))
            {
                BitmapImage bitmapImage = null;
                if (person.Pfp != null && person.Pfp.Length > 0)
                {
                    try
                    {
                        bitmapImage = new BitmapImage();
                        using (var ms = new MemoryStream(person.Pfp))
                        {
                            bitmapImage.BeginInit();
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.StreamSource = ms;
                            bitmapImage.EndInit();
                        }
                        bitmapImage.Freeze();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Load lỗi: {ex.Message}");
                        bitmapImage = null;
                    }
                }

                var item = new ListBoxItem
                {
                    Content = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                {
                    new Image
                    {
                        Width = 40,
                        Height = 40,
                        Margin = new Thickness(0, 0, 10, 0),
                        Source = bitmapImage
                    },
                    new TextBlock
                    {
                        Text = $"{person.Name} ({person.BirthDate:dd/MM/yyyy}) - {(person.Gender ? "Nam" : "Nữ")}",
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
                    },
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

        private void PeopleListBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
    }
}
