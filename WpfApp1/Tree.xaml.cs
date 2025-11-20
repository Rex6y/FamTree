using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp1
{
    public partial class Tree : Page
    {
        private const int CardWidth = 180;
        private const int CardHeight = 150;
        private const int HorizontalSpacing = 60;
        private const int VerticalSpacing = 150;
        private const double StartX = 60;
        private const double StartY = 50;

        private double currentZoom = 1.0;
        private int rootPersonId;
        private List<List<int>> gens = new List<List<int>>();
        private Dictionary<int, int> maxWidth = new Dictionary<int, int>();

        bool calcMode = false;
        int select1 = -1;
        int select2 = -1;

        bool search = false;
        public Tree(int userId, double? zoom = null)
        {
            InitializeComponent();
            if (zoom.HasValue) currentZoom = zoom.Value;
            rootPersonId = userId;
            Loaded += Tree_Loaded;
        }

        private void Tree_Loaded(object sender, RoutedEventArgs e)
        {
            FamilyTree.Load();
            Person rootPerson = FamilyTree.GetPerson(rootPersonId);

            if (rootPerson != null)
            {
                RootPersonText.Text = $"Current: {rootPerson.Name}";
            }

            DrawTree();
            ApplyZoom();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.Navigate(new PageSearchResults(""));
                while (NavigationService.CanGoBack)
                {
                    NavigationService.RemoveBackEntry();
                }
            }
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            currentZoom *= 1.2;
            ApplyZoom();
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            currentZoom /= 1.2;
            ApplyZoom();
        }

        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            currentZoom = 1.0;
            ApplyZoom();
        }

        private void ApplyZoom()
        {
            TreeCanvas.LayoutTransform = new ScaleTransform(currentZoom, currentZoom);
        }

        private void DrawTree()
        {
            TreeCanvas.Children.Clear();

            Person rootPerson = FamilyTree.GetPerson(rootPersonId);
            if (rootPerson == null)
            {
                MessageBox.Show("Person not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Build the tree structure (generations with their people)
            gens = BuildGenerationsList(rootPersonId);
            calcMaxWidth(gens[0][0]);
            if (gens[0].Count > 1) { maxWidth[gens[0][1]] = maxWidth[gens[0][0]]; }

            // Calculate positions for everyone
            var positions = new Dictionary<int, Point>();
            calcPos(gens[0][0], 0, StartY, new HashSet<int>(), positions);

            // Draw connections between people
            DrawLines(positions);

            // Draw person cards
            DrawPersonCards(positions);

            // Adjust canvas size
            AdjustCanvasSize(positions);
        }

        private void calcMaxWidth(int personId)
        {
			var person = FamilyTree.GetPerson(personId);
            
			if (person.Spouse.HasValue) //default value
			{
				maxWidth[person.Spouse.Value] = 2;
				maxWidth[personId] = 2;
			} 
            //edge case only having 1 child
            else if (FamilyTree.siblingsCount(personId) == 1) maxWidth[personId] = person.Dad.HasValue ? maxWidth[person.Dad.Value] : maxWidth[person.Mom.Value];
			else maxWidth[personId] = 1;

            if (person.Children.Count > 0)
            {
                int total = 0;
                foreach (int child in person.Children)
                {
                    if (!gens.Any(gen => gen.Contains(child))) continue;
                    calcMaxWidth(child);
                    total += maxWidth[child];
                }
                maxWidth[personId] = Math.Max(total, maxWidth[personId]);
            }
        }

		private void calcPos(int personId, long offset, double currentY, HashSet<int> processed, Dictionary<int, Point> positions)
		{
			if (processed.Contains(personId)) return;
			var person = FamilyTree.GetPerson(personId);
			long currMaxWidth = maxWidth[personId] * CardWidth + (maxWidth[personId] - 1) * HorizontalSpacing;

            if (person.Spouse.HasValue)
            {
                double x = offset + StartX + (currMaxWidth - (2 * CardWidth + HorizontalSpacing)) / 2;
                positions[personId] = new Point(x, currentY);
                x = x + CardWidth + HorizontalSpacing;
                positions[person.Spouse.Value] = new Point(x, currentY);
                processed.Add(person.Spouse.Value);
            }
            else
            {
                double x = offset + StartX + (currMaxWidth - CardWidth) / 2;
                positions[personId] = new Point(x, currentY);
            }
			processed.Add(personId);
			foreach (int child in person.Children)
			{
				if (!gens.Any(gen => gen.Contains(child))) continue;
				var c = FamilyTree.GetPerson(child);
				long childMaxWidth = maxWidth[child] * CardWidth + (maxWidth[child] - 1) * HorizontalSpacing;
				calcPos(child, offset, currentY + CardHeight + VerticalSpacing, processed, positions);
				offset += childMaxWidth + HorizontalSpacing;
			}
		}

		private List<List<int>> BuildGenerationsList(int personId)
        {
            var generations = new List<List<int>>();
            var person = FamilyTree.GetPerson(personId);
            if (person == null) return generations;
			
            //Get Ancestors
			int? current = personId;
            while (current != null)
            {
                person = FamilyTree.GetPerson(current.Value);
                int? parent = person.Dad.HasValue ? person.Dad : person.Mom;
                if (parent.HasValue)
                {
                    List<int> Gen = GetSiblingsWithSpouses(parent.Value);
                    if (Gen.Count > 0) generations.Insert(0, Gen);
                }
                else 
                { 
                    if (person.Spouse.HasValue)
                    {
                        if (person.Gender) generations.Insert(0, new List<int> { current.Value, person.Spouse.Value });
                        else generations.Insert(0, new List<int> { person.Spouse.Value, current.Value });
					}
                    else generations.Insert(0, new List<int> { current.Value });
				}
				current = parent;
            }

			//Get Descendants
			TraverseDown(new List<int> { personId }, generations);

			//Update Generation
			for (int i=0; i<generations.Count; i++)
            {
                var generation = generations[i];
                foreach (var id in generation) FamilyTree.dynamicGen(id, i);
            }

            return generations;
        }
        private List<int> GetSiblingsWithSpouses(int personId)
        {
            var siblings = new List<int>();
            var person = FamilyTree.GetPerson(personId);
            if (person==null) return siblings;

            foreach (int child in person.Children)
            {
                var c = FamilyTree.GetPerson(child);
				
				if (c.Spouse.HasValue)
                {
                    if (c.Gender)
                    {
                        siblings.Add(child);
						siblings.Add(c.Spouse.Value);
					} else
                    {
						siblings.Add(c.Spouse.Value);
                        siblings.Add(child);
					}
                } else siblings.Add(child);

            }
            return siblings;
        }

        private void TraverseDown(List<int> currentGen, List<List<int>> generations)
        {
			var newGen = new List<int>();
			foreach (int parent in currentGen)
            {
                var person = FamilyTree.GetPerson(parent);
                foreach (var child in person.Children)
                {
                    if (!newGen.Contains(child))
                    {
                        var c = FamilyTree.GetPerson(child);
                        if (c.Spouse.HasValue)
                        {
                            if (c.Gender)
                            {
                                newGen.Add(child);
                                newGen.Add(c.Spouse.Value);
                            }
                            else
                            {
                                newGen.Add(c.Spouse.Value);
                                newGen.Add(child);
                            }
                        } else newGen.Add(child);
					}
                }
            }
            if (newGen.Count>0)
            {
				generations.Add(newGen);
                TraverseDown(newGen, generations);
			}
        }

        private void DrawLines(Dictionary<int, Point> positions)
        {
			var drawn = new HashSet<int>();
            for  (int i=0; i<gens.Count; i++)
            {
                foreach (var p in gens[i])
                {
                    if (drawn.Contains(p)) continue;
                    drawn.Add(p);
                    var person = FamilyTree.GetPerson(p);
                    Line? spouseLine = null;
                    double childLineX = positions[p].X + CardWidth / 2;

                    if (person.Spouse.HasValue)
                    {
                        int spouse = person.Spouse.Value;
                        childLineX = (positions[p].X + positions[spouse].X + CardWidth) / 2;
						drawn.Add(person.Spouse.Value);
                        spouseLine = new Line
                        {
							X1 = positions[p].X + CardWidth,
							Y1 = positions[p].Y + CardHeight / 2,
							X2 = positions[spouse].X,
							Y2 = positions[p].Y + CardHeight / 2,
							Stroke = new SolidColorBrush(Color.FromRgb(244, 67, 54)),
							StrokeThickness = 3
						};
                    }
                    if (spouseLine != null) TreeCanvas.Children.Add(spouseLine);
                    if (person.Children.Count > 0 && i+1 < gens.Count)
                    {
						var children = person.Children.Where(cId => gens[i + 1].Contains(cId)).ToList();
						if (children.Count == 0) continue;

						double startY = positions[p].Y + CardHeight;
                        double vertLineLength = VerticalSpacing / 2;
                        Line vertSpouseLine = new Line
                        {
                            X1 = childLineX,
                            Y1 = positions[p].Y + CardHeight / 2,
                            X2 = childLineX,
                            Y2 = startY + vertLineLength,
							Stroke = new SolidColorBrush(Color.FromRgb(158, 158, 158)),
							StrokeThickness = 2
						};
                        TreeCanvas.Children.Add(vertSpouseLine);

						double leftChildX = positions[children[0]].X + CardWidth / 2;
						double rightChildX = Math.Max(positions[children[children.Count - 1]].X + CardWidth / 2, childLineX);

						Line horizontalLine = new Line
						{
							X1 = leftChildX,
							Y1 = startY + vertLineLength,
							X2 = rightChildX,
							Y2 = startY + vertLineLength,
							Stroke = new SolidColorBrush(Color.FromRgb(158, 158, 158)),
							StrokeThickness = 2
						};
						TreeCanvas.Children.Add(horizontalLine);

						foreach (int childId in children)
						{
							Point childPos = positions[childId];
							double childCenterX = childPos.X + CardWidth / 2;

							Line childLine = new Line
							{
								X1 = childCenterX,
								Y1 = startY + vertLineLength,
								X2 = childCenterX,
								Y2 = childPos.Y,
								Stroke = new SolidColorBrush(Color.FromRgb(158, 158, 158)),
								StrokeThickness = 2
							};
							TreeCanvas.Children.Add(childLine);
						}
					}
                }
            }
		}

        private void DrawPersonCards(Dictionary<int, Point> positions)
        {
            foreach (var kvp in positions)
            {
                int personId = kvp.Key;
                Point position = kvp.Value;
                Person person = FamilyTree.GetPerson(personId);

                if (person == null)
                    continue;

                Border card = new Border
                {
                    Width = CardWidth,
                    Height = CardHeight,
                    Style = person.Gender ? (Style)FindResource("MaleCardStyle") : (Style)FindResource("FemaleCardStyle"),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = personId
                };

                card.MouseDown += Card_MouseDown;

                Grid cardContent = new Grid();
                cardContent.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                cardContent.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                Border imageBorder = new Border
                {
                    Width = 60,
                    Height = 60,
                    CornerRadius = new CornerRadius(30),
                    Background = new SolidColorBrush(person.Gender ? Color.FromRgb(33, 150, 243) : Color.FromRgb(233, 30, 99)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 0)
                };

                if (person.Pfp != null && person.Pfp.Length > 0)
                {
                    try
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = new System.IO.MemoryStream(person.Pfp);
                        bitmap.EndInit();

                        imageBorder.Background = new ImageBrush
                        {
                            ImageSource = bitmap,
                            Stretch = Stretch.UniformToFill
                        };
                    }
                    catch { }
                }
                else
                {
                    TextBlock initial = new TextBlock
                    {
                        Text = person.Name.Length > 0 ? person.Name[0].ToString().ToUpper() : "?",
                        FontSize = 28,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    imageBorder.Child = initial;
                }

                Grid.SetRow(imageBorder, 0);
                cardContent.Children.Add(imageBorder);

                StackPanel info = new StackPanel { Margin = new Thickness(5, 0, 5, 5) };

                TextBlock nameText = new TextBlock
                {
                    Text = person.Name,
                    FontWeight = FontWeights.Bold,
                    FontSize = 13,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 2, 0, 0)
                };

                TextBlock birthText = new TextBlock
                {
                    Text = person.BirthDate.ToString("dd-MM-yyyy"),
                    FontSize = 12,
                    Foreground = Brushes.Gray,
                    TextAlignment = TextAlignment.Center
                };

				TextBlock genText = new TextBlock
				{
					Text = $"Generation: {person.Generation}",
					FontSize = 12,
					Foreground = Brushes.Gray,
					TextAlignment = TextAlignment.Center
				};

				if (personId == rootPersonId)
                {
                    card.BorderThickness = new Thickness(4);
                    card.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 193, 7));
                }

                info.Children.Add(nameText);
                info.Children.Add(birthText);
                info.Children.Add(genText);

				Grid.SetRow(info, 1);
                cardContent.Children.Add(info);

                card.Child = cardContent;

                Canvas.SetLeft(card, position.X);
                Canvas.SetTop(card, position.Y);

                TreeCanvas.Children.Add(card);
            }
        }

        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border card && card.Tag is int personId)
            {
                if (calcMode == false)
                {
					if (rootPersonId == personId)
					{
						NavigationService?.Navigate(new Profile(personId));
					}
					else
					{
						NavigationService?.Navigate(new Tree(personId, currentZoom));
					}
				}
                else
                {
                    if (select1 == -1 && select2 != personId) select1 = personId;
                    else if (select2 == -1 && select1 != personId) select2 = personId;
                    else if (select1 == personId)
                    {
                        resetBorder(select1);
                        select1 = -1;
                    }
					else if (select2 == personId)
					{
						resetBorder(select2);
						select2 = -1;
					}
					updateDis();
                }
            }
        }

        private void AdjustCanvasSize(Dictionary<int, Point> positions)
        {
            if (positions.Count == 0)
                return;

            double maxX = positions.Values.Max(p => p.X) + CardWidth + StartX;
            double maxY = positions.Values.Max(p => p.Y) + CardHeight + 60;

            TreeCanvas.Width = maxX;
            TreeCanvas.Height = maxY;
        }

		public int BloodlineDiff(int id1, int id2)
		{
			if (id1 == id2)
				return 0;

			// BFS from person1 to find person2
			var queue = new Queue<(int id, int distance)>();
			var ascendants = new HashSet<int>();
			var visited = new HashSet<int>();
			queue.Enqueue((id1, 0));
			visited.Add(id1);
            ascendants.Add(id1);

			while (queue.Count > 0)
			{
				var (currentId, distance) = queue.Dequeue();
                if (!gens.Any(gen => gen.Contains(currentId))) continue;

				if (currentId == id2) return distance;

				var current = FamilyTree.GetPerson(currentId);
				if (ascendants.Contains(currentId))
				{
					if (current.Dad.HasValue && !visited.Contains(current.Dad.Value))
                    {
                        queue.Enqueue((current.Dad.Value, distance + 1));
                        visited.Add(current.Dad.Value);
                        ascendants.Add(current.Dad.Value);
                    }
                    if (current.Mom.HasValue && !visited.Contains(current.Mom.Value))
                    {
						queue.Enqueue((current.Mom.Value, distance + 1));
						visited.Add(current.Mom.Value);
						ascendants.Add(current.Mom.Value);
					}
				}

                if (current.Dad.HasValue)
                {
                    var dad = FamilyTree.GetPerson(current.Dad.Value);
                    foreach (var c in dad.Children)
                    {
						if (visited.Contains(c)) continue;
						queue.Enqueue((c, distance + 1));
						visited.Add(c);
					}
                } else if (current.Mom.HasValue)
                {
					var mom = FamilyTree.GetPerson(current.Mom.Value);
					foreach (var c in mom.Children)
					{
						if (visited.Contains(c)) continue;
						queue.Enqueue((c, distance + 1));
						visited.Add(c);
					}
				}
                foreach (int c in current.Children)
                {
                    if (visited.Contains(c)) continue;
                    queue.Enqueue((c, distance + 1));
                    visited.Add(c);
                }
                
			}

			return -1; // No relation found
		}

		private void CalcDis(object sender, RoutedEventArgs e)
		{
            if (calcMode == false)
            {
                calcMode = true;
                DistanceText.Visibility = Visibility.Visible;
                updateDis();
                CalcBloodDiff.Background = new SolidColorBrush(Color.FromRgb(166, 66, 36));
			}
            else
            {
				DistanceText.Visibility = Visibility.Collapsed;
				if (select1 != -1)
                {
                    resetBorder(select1);
				}
				if (select2 != -1)
				{
                    resetBorder(select2);
				}
                select1 = select2 = -1;
                calcMode = false;
				CalcBloodDiff.Background = new SolidColorBrush(Color.FromRgb(235, 94, 52));
			}
		}
        private void resetBorder(int id)
        {
			var card = TreeCanvas.Children.OfType<Border>().FirstOrDefault(c => (int)c.Tag == id);
			if (id == rootPersonId) card.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 193, 7));
			else
			{
				var person = FamilyTree.GetPerson(id);
				card.BorderBrush = new SolidColorBrush(person.Gender ? Color.FromRgb(33, 150, 243) : Color.FromRgb(233, 30, 99));
			}
		}
        private void updateDis()
        {
            int distance = 0;
            if (select1 != -1 && select2 != -1) distance = BloodlineDiff(select1, select2);
            if (distance != -1) DistanceText.Text = $"Khoảng cách: {distance}";
            else DistanceText.Text = "Khoảng cách: Khác dòng máu";

            if (select1 != -1)
            {
				var card = TreeCanvas.Children.OfType<Border>().FirstOrDefault(c => (int)c.Tag == select1);
				card.BorderBrush = new SolidColorBrush(Color.FromRgb(52, 235, 171));
			}
			if (select2 != -1)
			{
				var card = TreeCanvas.Children.OfType<Border>().FirstOrDefault(c => (int)c.Tag == select2);
				card.BorderBrush = new SolidColorBrush(Color.FromRgb(52, 235, 171));
			}
		}

		private void FilterOption(object sender, RoutedEventArgs e)
		{
            if (search == false)
            {
                search = true;
                SearchPanel.Visibility = Visibility.Visible;
                genBox.Items.Clear();
                for (int i = 0; i<gens.Count; i++) genBox.Items.Add(i);
            }
            else
            {
                search = false;
                SearchPanel.Visibility = Visibility.Collapsed;
            }
		}

		private void SearchPanel_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
				RunSearch();
		}
		private void RunSearch()
		{
			string query = FullName.Text.Trim();
            bool? gender = GenderMale.IsChecked == true ? true : GenderFemale.IsChecked == true ? false : null; 
            int? genSel = genBox.SelectedItem != null ? (int)genBox.SelectedItem : null;

			var allIds = gens.SelectMany(g => g).ToList();
			var results = allIds
	        .Where(id =>
	        {
		    var p = FamilyTree.GetPerson(id);
		    return (string.IsNullOrEmpty(query) || p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) &&
			   (gender == null || p.Gender == gender.Value) &&
			   (genSel == null || p.Generation == genSel.Value);
	        }).ToList();

			SearchResultsPanel.Children.Clear();
			if (results.Count == 0)
			{
				SearchResultsPanel.Children.Add(new TextBlock
				{
					Text = "Không tìm thấy kết quả nào.",
					FontSize = 14,
					Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153)),
					HorizontalAlignment = HorizontalAlignment.Center,
					Margin = new Thickness(0, 0, 0, 0)
				});
				return;
			}

			foreach (var r in results)
			{
				var item = CreateSearchResultItem(r, FamilyTree.GetPerson(r));
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
		private void RadioButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			RadioButton rb = sender as RadioButton;
			if (rb.IsChecked == true)
			{
				rb.IsChecked = false;
				e.Handled = true;
			}
		}
	}
}