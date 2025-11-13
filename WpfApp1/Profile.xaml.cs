using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfApp1;

namespace WpfApp1
{
    public partial class Profile : Page
    {
        private int personId;

        public Profile(int id)
        {
            InitializeComponent();
            personId = id;
            Loaded += Profile_Loaded;
        }

        private void Profile_Loaded(object sender, RoutedEventArgs e)
        {
            FamilyTree.Load();
            LoadPersonData();
        }

        private void LoadPersonData()
        {
            Person person = FamilyTree.GetPerson(personId);
            if (person == null)
            {
                MessageBox.Show("Person not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Set basic info
            PageTitle.Text = $"Profile: {person.Name}";
            NameText.Text = person.Name;
            GenderText.Text = person.Gender ? "Male" : "Female";
            BirthDateText.Text = person.BirthDate.ToString("MMMM dd, yyyy");
            GenerationText.Text = $"Generation {person.Generation}";

            // Set profile picture
            LoadProfilePicture(person);

            // Set family relationships
            if (person.Dad.HasValue)
            {
                Person dad = FamilyTree.GetPerson(person.Dad.Value);
                DadText.Text = dad != null ? dad.Name : "Unknown";
            }
            else
            {
                DadText.Text = "Not set";
            }

            if (person.Mom.HasValue)
            {
                Person mom = FamilyTree.GetPerson(person.Mom.Value);
                MomText.Text = mom != null ? mom.Name : "Unknown";
            }
            else
            {
                MomText.Text = "Not set";
            }

            if (person.Spouse.HasValue)
            {
                Person spouse = FamilyTree.GetPerson(person.Spouse.Value);
                SpouseText.Text = spouse != null ? spouse.Name : "Unknown";
            }
            else
            {
                SpouseText.Text = "Not set";
            }

            // Load children
            LoadChildren(person);
        }

        private void LoadProfilePicture(Person person)
        {
            ProfilePicBorder.Background = new SolidColorBrush(person.Gender ?
                Color.FromRgb(33, 150, 243) : Color.FromRgb(233, 30, 99));

            if (person.Pfp != null && person.Pfp.Length > 0)
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new MemoryStream(person.Pfp);
                    bitmap.EndInit();

                    ProfilePicBorder.Background = new ImageBrush
                    {
                        ImageSource = bitmap,
                        Stretch = Stretch.UniformToFill
                    };
                    ProfileInitial.Visibility = Visibility.Collapsed;
                }
                catch
                {
                    ProfileInitial.Text = person.Name.Length > 0 ? person.Name[0].ToString().ToUpper() : "?";
                    ProfileInitial.Visibility = Visibility.Visible;
                }
            }
            else
            {
                ProfileInitial.Text = person.Name.Length > 0 ? person.Name[0].ToString().ToUpper() : "?";
                ProfileInitial.Visibility = Visibility.Visible;
            }
        }

        private void LoadChildren(Person person)
        {
            ChildrenListBox.Items.Clear();
            foreach (int childId in person.Children)
            {
                Person child = FamilyTree.GetPerson(childId);
                if (child != null)
                {
                    var item = new ListBoxItem
                    {
                        Content = child.Name,
                        Tag = childId,
                        Cursor = System.Windows.Input.Cursors.Hand
                    };
                    item.MouseDoubleClick += ChildItem_DoubleClick;
                    ChildrenListBox.Items.Add(item);
                }
            }
        }

        private void ChildItem_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item && item.Tag is int childId)
            {
                NavigationService?.Navigate(new Profile(childId));
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void ChangePicture_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Title = "Select Profile Picture"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    byte[] imageData = File.ReadAllBytes(openFileDialog.FileName);
                    FamilyTree.updatePfp(personId, imageData);
                    Person person = FamilyTree.GetPerson(personId);
                    LoadProfilePicture(person);
                    MessageBox.Show("Profile picture updated!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RemovePicture_Click(object sender, RoutedEventArgs e)
        {
            Person person = FamilyTree.GetPerson(personId);
            FamilyTree.updatePfp(personId, null);
            LoadProfilePicture(person);
            MessageBox.Show("Profile picture removed!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SetDad_Click(object sender, RoutedEventArgs e)
        {
            var selector = new WindowSelector(true); // true = male only
            if (selector.ShowDialog() == true && selector.SelectedPersonId.HasValue)
            {
                int dadId = selector.SelectedPersonId.Value;
                FamilyTree.addFather(personId, dadId);
                LoadPersonData();
                MessageBox.Show($"{FamilyTree.GetPerson(dadId).Name} set as father!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
            }
        }

        private void SetMom_Click(object sender, RoutedEventArgs e)
        {
            var selector = new WindowSelector(false); // false = female only
            if (selector.ShowDialog() == true && selector.SelectedPersonId.HasValue)
            {
                int momId = selector.SelectedPersonId.Value;
                FamilyTree.addMother(personId, momId);
                LoadPersonData();
                MessageBox.Show($"{FamilyTree.GetPerson(momId).Name} set as mother!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            }
        }

        private void SetSpouse_Click(object sender, RoutedEventArgs e)
        {
            Person person = FamilyTree.GetPerson(personId);
            var selector = new WindowSelector(!person.Gender); // opposite gender
            if (selector.ShowDialog() == true && selector.SelectedPersonId.HasValue)
            {
                int spouseId = selector.SelectedPersonId.Value;
                FamilyTree.addSpouse(personId, spouseId);
                LoadPersonData();
                MessageBox.Show($"{FamilyTree.GetPerson(spouseId).Name} set as spouse!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            }
        }

        private void AddChild_Click(object sender, RoutedEventArgs e)
        {
            var selector = new WindowSelector(null);
            if (selector.ShowDialog() == true && selector.SelectedPersonId.HasValue)
            {
                int childId = selector.SelectedPersonId.Value;
                FamilyTree.addChildren(personId, childId);
                LoadPersonData();
                MessageBox.Show($"{FamilyTree.GetPerson(childId).Name} set as child!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            }
        }

        private void ViewTree_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Tree(personId));
        }

        private void DeletePerson_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete this person? This action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                FamilyTree.RemovePerson(personId);
                MessageBox.Show("Person deleted!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                if (NavigationService != null && NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
            }
        }
    }
}