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
            LoadPersonData();
        }

        private void LoadPersonData()
        {
            Person person = FamilyTree.GetPerson(personId);
            if (person == null)
            {
                MessageBox.Show("Không tìm thấy!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Set basic info
            PageTitle.Text = $"Thông tin cá nhân: {person.Name}";
            NameText.Text = person.Name;
            GenderText.Text = person.Gender ? "Nam" : "Nữ";
            BirthDateText.Text = person.BirthDate.ToString("dd/MM/yyyy");
            GenerationText.Text = $"Thế hệ {person.Generation}";

            // Set profile picture
            LoadProfilePicture(person);

            // Set family relationships
            if (person.Dad.HasValue)
            {
                Person dad = FamilyTree.GetPerson(person.Dad.Value);
                DadText.Text = dad != null ? dad.Name : "Unknown";
				SetDadButton.Visibility = Visibility.Collapsed;
                DeleteDad.Visibility = Visibility.Visible;
			}
            else
            {
                DadText.Text = "Chưa đặt";
				SetDadButton.Visibility = Visibility.Visible;
				DeleteDad.Visibility = Visibility.Collapsed;
			}

            if (person.Mom.HasValue)
            {
                Person mom = FamilyTree.GetPerson(person.Mom.Value);
                MomText.Text = mom != null ? mom.Name : "Unknown";
				SetMomButton.Visibility = Visibility.Collapsed;
				DeleteMom.Visibility = Visibility.Visible;
			}
            else
            {
                MomText.Text = "Chưa đặt";
				SetMomButton.Visibility = Visibility.Visible;
				DeleteMom.Visibility = Visibility.Collapsed;
			}

            if (person.Spouse.HasValue)
            {
                Person spouse = FamilyTree.GetPerson(person.Spouse.Value);
                SpouseText.Text = spouse != null ? spouse.Name : "Unknown";
            }
            else
            {
                SpouseText.Text = "Chưa đặt";
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
					// Create main grid for the child item
					var grid = new Grid
					{
						Margin = new Thickness(5)
					};
					grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Profile pic
					grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Name
					grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Delete button

					// Profile picture
					var pfpBorder = new Border
					{
						Width = 40,
						Height = 40,
						CornerRadius = new CornerRadius(20),
						Background = new SolidColorBrush(child.Gender
							? Color.FromRgb(33, 150, 243)
							: Color.FromRgb(233, 30, 99)),
						Margin = new Thickness(0, 0, 10, 0),
						VerticalAlignment = VerticalAlignment.Center
					};

					if (child.Pfp != null && child.Pfp.Length > 0)
					{
						try
						{
							BitmapImage bitmap = new BitmapImage();
							bitmap.BeginInit();
							bitmap.StreamSource = new MemoryStream(child.Pfp);
							bitmap.EndInit();

							pfpBorder.Background = new ImageBrush
							{
								ImageSource = bitmap,
								Stretch = Stretch.UniformToFill
							};
						}
						catch
						{
							var initial = new TextBlock
							{
								Text = child.Name.Length > 0 ? child.Name[0].ToString().ToUpper() : "?",
								FontSize = 18,
								FontWeight = FontWeights.Bold,
								Foreground = Brushes.White,
								HorizontalAlignment = HorizontalAlignment.Center,
								VerticalAlignment = VerticalAlignment.Center
							};
							pfpBorder.Child = initial;
						}
					}
					else
					{
						var initial = new TextBlock
						{
							Text = child.Name.Length > 0 ? child.Name[0].ToString().ToUpper() : "?",
							FontSize = 18,
							FontWeight = FontWeights.Bold,
							Foreground = Brushes.White,
							HorizontalAlignment = HorizontalAlignment.Center,
							VerticalAlignment = VerticalAlignment.Center
						};
						pfpBorder.Child = initial;
					}

					Grid.SetColumn(pfpBorder, 0);
					grid.Children.Add(pfpBorder);

					// Child name (just text, no button)
					var nameText = new TextBlock
					{
						Text = child.Name,
						FontSize = 14,
						VerticalAlignment = VerticalAlignment.Center,
						Margin = new Thickness(0, 0, 10, 0)
					};
					Grid.SetColumn(nameText, 1);
					grid.Children.Add(nameText);

					// Delete button (right-aligned)
					var deleteButton = new Button
					{
						Content = "✕",
						Width = 30,
						Height = 30,
						Background = new SolidColorBrush(Color.FromRgb(244, 67, 54)),
						Foreground = Brushes.White,
						BorderThickness = new Thickness(0),
						Cursor = System.Windows.Input.Cursors.Hand,
						Tag = childId,
						FontWeight = FontWeights.Bold,
						FontSize = 16,
						VerticalAlignment = VerticalAlignment.Center
					};
					deleteButton.Click += DeleteChild_Click;
					Grid.SetColumn(deleteButton, 2);
					grid.Children.Add(deleteButton);

					// Create ListBoxItem with double-click handler
					var item = new ListBoxItem
					{
						Content = grid,
						Tag = childId,
						Cursor = System.Windows.Input.Cursors.Hand
					};
					item.MouseDoubleClick += ChildItem_DoubleClick;

					ChildrenListBox.Items.Add(item);
				}
			}
		}
		private void DeleteChild_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button button && button.Tag is int childId)
			{
				Person child = FamilyTree.GetPerson(childId);
				var result = MessageBox.Show(
					$"Xóa {child.Name} khỏi danh sách con?",
					"Xác nhận xóa",
					MessageBoxButton.YesNo,
					MessageBoxImage.Question
				);

				if (result == MessageBoxResult.Yes)
				{
					FamilyTree.deleteChildren(personId, childId);
					LoadPersonData();
					MessageBox.Show($"Đã xóa {child.Name} khỏi danh sách con !", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
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
                Title = "CHọn ảnh"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    byte[] imageData = File.ReadAllBytes(openFileDialog.FileName);
                    FamilyTree.updatePfp(personId, imageData);
                    Person person = FamilyTree.GetPerson(personId);
                    LoadProfilePicture(person);
                    MessageBox.Show("Chưa update ảnh!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Tải ảnh bị lỗi: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RemovePicture_Click(object sender, RoutedEventArgs e)
        {
            Person person = FamilyTree.GetPerson(personId);
            FamilyTree.updatePfp(personId, null);
            LoadProfilePicture(person);
            MessageBox.Show("Đã xóa ảnh!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SetDad_Click(object sender, RoutedEventArgs e)
        {
            var selector = new WindowSelector(true, personId, 1); // true = male only
            if (selector.ShowDialog() == true && selector.SelectedPersonId.HasValue)
            {
                int dadId = selector.SelectedPersonId.Value;
                FamilyTree.addFather(personId, dadId);
                LoadPersonData();
                MessageBox.Show($"Đã thêm {FamilyTree.GetPerson(dadId).Name} làm bố!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                
            }
        }

		private void SetMom_Click(object sender, RoutedEventArgs e)
        {
            var selector = new WindowSelector(false, personId, 1); // false = female only
            if (selector.ShowDialog() == true && selector.SelectedPersonId.HasValue)
            {
                int momId = selector.SelectedPersonId.Value;
                FamilyTree.addMother(personId, momId);
                LoadPersonData();
                MessageBox.Show($"Đã thêm {FamilyTree.GetPerson(momId).Name} làm mẹ!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

            }
        }
		private void DeleteParents_Click(object sender, RoutedEventArgs e)
		{
			var result = MessageBox.Show(
				"Bạn có chắc muốn xóa bố/mẹ?",
				"Xác nhận xóa",
				MessageBoxButton.YesNo,
				MessageBoxImage.Warning
			);
			if (result == MessageBoxResult.Yes)
			{
				FamilyTree.deleteParents(personId);
				LoadPersonData();
				MessageBox.Show("Đã xóa!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
			}

		}
		private void SetSpouse_Click(object sender, RoutedEventArgs e)
        {
            var person = FamilyTree.GetPerson(personId);
            if (person.Spouse != null)
            {
				MessageBox.Show("Đã có vợ/chồng,không thể thêm nữa!", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
			}
            var selector = new WindowSelector(!person.Gender, personId, 1); // opposite gender
            if (selector.ShowDialog() == true && selector.SelectedPersonId.HasValue)
            {
                int spouseId = selector.SelectedPersonId.Value;
                FamilyTree.addSpouse(personId, spouseId);
                LoadPersonData();
                MessageBox.Show($"Đã thêm {FamilyTree.GetPerson(spouseId).Name} làm vợ/chồng!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

            }
        }

        private void AddChild_Click(object sender, RoutedEventArgs e)
        {
            var person = FamilyTree.GetPerson(personId);
            int mode = person.Gender ? 2 : 3;
            var selector = new WindowSelector(null, personId, mode);
            if (selector.ShowDialog() == true && selector.SelectedPersonId.HasValue)
            {
                int childId = selector.SelectedPersonId.Value;
                FamilyTree.addChildren(personId, childId);
                LoadPersonData();
                MessageBox.Show($"Đã thêm  {FamilyTree.GetPerson(childId).Name} làm con!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

            }
        }

        private void ViewTree_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Tree(personId));
        }

        private void DeletePerson_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa thành viên này? Thao tác này không thể hoàn lại!!",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                FamilyTree.RemovePerson(personId);
                MessageBox.Show("Dã xóa thành viên!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                NavigationService.Navigate(new PageSearchResults(""));
            }
        }
    }
}