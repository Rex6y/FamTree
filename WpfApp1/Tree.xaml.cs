using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfApp1;

namespace WpfApp1
{
    public partial class Tree : Page
    {
        private const double CardWidth = 180;
        private const double CardHeight = 120;
        private const double HorizontalSpacing = 80;
        private const double VerticalSpacing = 150;
        private double currentZoom = 1.0;
        private int rootPersonId;

        public Tree(int userId)
        {
            InitializeComponent();
            rootPersonId = userId;
            Loaded += Tree_Loaded;
        }

        private void Tree_Loaded(object sender, RoutedEventArgs e)
        {
            FamilyTree.Load();
            Person rootPerson = FamilyTree.GetPerson(rootPersonId);

            if (rootPerson != null)
            {
                RootPersonText.Text = $"Family Tree: {rootPerson.Name}";
            }

            DrawTree();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to previous page
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
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

            // Calculate positions for all nodes
            var nodePositions = new Dictionary<int, Point>();
            CalculateTreeLayout(rootPersonId, nodePositions);

            // DEBUG: Show generation assignments
            var generations = new Dictionary<int, int>();
            CalculateActualGenerations(rootPersonId, 0, generations, new HashSet<int>());

            string debugInfo = "Generation assignments:\n";
            foreach (var kvp in generations.OrderBy(x => x.Value))
            {
                Person p = FamilyTree.GetPerson(kvp.Key);
                debugInfo += $"Gen {kvp.Value}: {p?.Name} (ID: {kvp.Key})\n";
            }
            System.Diagnostics.Debug.WriteLine(debugInfo);

            // Draw connections first (so they appear behind cards)
            DrawConnections(nodePositions);

            // Draw person cards
            DrawPersonCards(nodePositions);

            // Adjust canvas size
            AdjustCanvasSize(nodePositions);
        }

        private void CalculateTreeLayout(int rootId, Dictionary<int, Point> positions)
        {
            // Calculate actual generation levels based on tree structure from root
            var generationLevels = new Dictionary<int, List<int>>();
            var actualGenerations = new Dictionary<int, int>();
            CalculateActualGenerations(rootId, 0, actualGenerations, new HashSet<int>());

            // Build generation lists using calculated generations
            foreach (var kvp in actualGenerations)
            {
                int personId = kvp.Key;
                int genLevel = kvp.Value;

                if (!generationLevels.ContainsKey(genLevel))
                    generationLevels[genLevel] = new List<int>();

                if (!generationLevels[genLevel].Contains(personId))
                    generationLevels[genLevel].Add(personId);
            }

            // Position each generation
            double currentY = 50;
            var generationXPositions = new Dictionary<int, double>(); // Track X position for each generation

            foreach (var generation in generationLevels.OrderBy(g => g.Key))
            {
                var peopleInGen = generation.Value;
                var familyUnits = BuildFamilyUnits(peopleInGen, generation.Key);

                // Calculate total width needed for this generation
                double totalWidth = CalculateFamilyUnitsWidth(familyUnits);
                double startX = 50;

                // Position each family unit
                double currentX = startX;
                foreach (var unit in familyUnits)
                {
                    if (unit.Count == 2)
                    {
                        // Couple
                        int leftId = unit[0];
                        int rightId = unit[1];

                        positions[leftId] = new Point(currentX, currentY);
                        positions[rightId] = new Point(currentX + CardWidth + HorizontalSpacing, currentY);
                        currentX += (CardWidth * 2) + HorizontalSpacing + (HorizontalSpacing * 2);
                    }
                    else
                    {
                        // Single person
                        positions[unit[0]] = new Point(currentX, currentY);
                        currentX += CardWidth + (HorizontalSpacing * 2);
                    }
                }

                currentY += CardHeight + VerticalSpacing;
            }

            // Adjust positions to center children under parents
            CenterChildrenUnderParents(positions, actualGenerations);
        }

        private List<List<int>> BuildFamilyUnits(List<int> peopleInGen, int generation)
        {
            var units = new List<List<int>>();
            var processed = new HashSet<int>();

            foreach (int personId in peopleInGen)
            {
                if (processed.Contains(personId))
                    continue;

                Person person = FamilyTree.GetPerson(personId);
                if (person == null)
                    continue;

                var unit = new List<int> { personId };
                processed.Add(personId);

                // Check for spouse
                if (person.Spouse.HasValue && peopleInGen.Contains(person.Spouse.Value))
                {
                    // Determine order: male on left, female on right
                    Person spouse = FamilyTree.GetPerson(person.Spouse.Value);
                    if (spouse != null && !processed.Contains(person.Spouse.Value))
                    {
                        if (person.Gender) // person is male
                        {
                            unit = new List<int> { personId, person.Spouse.Value };
                        }
                        else // person is female
                        {
                            unit = new List<int> { person.Spouse.Value, personId };
                        }
                        processed.Add(person.Spouse.Value);
                    }
                }

                units.Add(unit);
            }

            return units;
        }

        private double CalculateFamilyUnitsWidth(List<List<int>> units)
        {
            double width = 0;
            foreach (var unit in units)
            {
                if (unit.Count == 2)
                {
                    width += (CardWidth * 2) + HorizontalSpacing + (HorizontalSpacing * 2);
                }
                else
                {
                    width += CardWidth + (HorizontalSpacing * 2);
                }
            }
            return width;
        }

        private void CenterChildrenUnderParents(Dictionary<int, Point> positions, Dictionary<int, int> generations)
        {
            // Group people by generation and process from top to bottom
            var genGroups = generations.GroupBy(g => g.Value).OrderBy(g => g.Key);

            foreach (var genGroup in genGroups)
            {
                foreach (var personKvp in genGroup)
                {
                    int parentId = personKvp.Key;
                    Person parent = FamilyTree.GetPerson(parentId);

                    if (parent == null || parent.Children.Count == 0)
                        continue;

                    // Get children that exist in the tree
                    var childrenIds = parent.Children.Where(cId => positions.ContainsKey(cId)).ToList();
                    if (childrenIds.Count == 0)
                        continue;

                    // Calculate parent center
                    Point parentPos = positions[parentId];
                    double parentCenterX;

                    if (parent.Spouse.HasValue && positions.ContainsKey(parent.Spouse.Value))
                    {
                        Point spousePos = positions[parent.Spouse.Value];
                        parentCenterX = (parentPos.X + spousePos.X + CardWidth) / 2;
                    }
                    else
                    {
                        parentCenterX = parentPos.X + CardWidth / 2;
                    }

                    // Calculate children's total width
                    double childrenWidth = 0;
                    var childrenWithSpouses = new Dictionary<int, int?>();

                    foreach (int childId in childrenIds)
                    {
                        childrenWithSpouses[childId] = null;
                        Person child = FamilyTree.GetPerson(childId);

                        if (child?.Spouse != null && positions.ContainsKey(child.Spouse.Value))
                        {
                            childrenWithSpouses[childId] = child.Spouse.Value;
                            childrenWidth += (CardWidth * 2) + HorizontalSpacing;
                        }
                        else
                        {
                            childrenWidth += CardWidth;
                        }

                        if (childId != childrenIds.Last())
                            childrenWidth += HorizontalSpacing * 2;
                    }

                    // Position children centered under parents
                    double startX = parentCenterX - childrenWidth / 2;
                    double currentX = startX;

                    foreach (int childId in childrenIds)
                    {
                        Point childPos = positions[childId];
                        positions[childId] = new Point(currentX, childPos.Y);

                        int? spouseId = childrenWithSpouses[childId];
                        if (spouseId.HasValue)
                        {
                            Point spousePos = positions[spouseId.Value];
                            positions[spouseId.Value] = new Point(currentX + CardWidth + HorizontalSpacing, spousePos.Y);
                            currentX += (CardWidth * 2) + HorizontalSpacing;
                        }
                        else
                        {
                            currentX += CardWidth;
                        }

                        if (childId != childrenIds.Last())
                            currentX += HorizontalSpacing * 2;
                    }
                }
            }
        }

        private void CalculateActualGenerations(int personId, int generation, Dictionary<int, int> generations, HashSet<int> visited)
        {
            Person person = FamilyTree.GetPerson(personId);
            if (person == null || visited.Contains(personId))
                return;

            visited.Add(personId);

            // Set this person's generation
            generations[personId] = generation;

            // CRITICAL: Set spouse to SAME generation immediately
            if (person.Spouse.HasValue)
            {
                generations[person.Spouse.Value] = generation;
            }

            // PATERNAL LINE PRIORITY: Recurse to DAD first (generation - 1)
            if (person.Dad.HasValue)
            {
                Person dad = FamilyTree.GetPerson(person.Dad.Value);
                if (dad != null && !visited.Contains(person.Dad.Value))
                {
                    CalculateActualGenerations(person.Dad.Value, generation - 1, generations, visited);
                }
            }

            // Don't automatically show mom's ancestors - only show mom herself
            if (person.Mom.HasValue)
            {
                Person mom = FamilyTree.GetPerson(person.Mom.Value);
                if (mom != null && !generations.ContainsKey(person.Mom.Value))
                {
                    generations[person.Mom.Value] = generation - 1;

                    // Add mom's spouse (the dad) if not already added
                    if (mom.Spouse.HasValue && !generations.ContainsKey(mom.Spouse.Value))
                    {
                        generations[mom.Spouse.Value] = generation - 1;
                    }
                }
            }

            // Recurse to children (generation + 1)
            foreach (int childId in person.Children)
            {
                if (!visited.Contains(childId))
                {
                    CalculateActualGenerations(childId, generation + 1, generations, visited);
                }
            }

            // Also process spouse's relationships
            if (person.Spouse.HasValue && !visited.Contains(person.Spouse.Value))
            {
                Person spouse = FamilyTree.GetPerson(person.Spouse.Value);
                if (spouse != null)
                {
                    visited.Add(person.Spouse.Value);

                    // Process spouse's children
                    foreach (int childId in spouse.Children)
                    {
                        if (!visited.Contains(childId))
                        {
                            CalculateActualGenerations(childId, generation + 1, generations, visited);
                        }
                    }
                }
            }
        }

        private void BuildGenerationLevels(int personId, Dictionary<int, List<int>> levels, HashSet<int> visited)
        {
            // This method is no longer used - replaced by CalculateActualGenerations
        }

        private void AdjustSpousePositions(Dictionary<int, Point> positions)
        {
            // This method is no longer needed - positioning happens in CalculateTreeLayout
        }

        private void PositionChildrenUnderParents(Dictionary<int, Point> positions, Dictionary<int, List<int>> generationLevels)
        {
            foreach (var generation in generationLevels.OrderBy(g => g.Key))
            {
                foreach (int personId in generation.Value)
                {
                    Person person = FamilyTree.GetPerson(personId);
                    if (person == null || person.Children.Count == 0)
                        continue;

                    // Get parent positions
                    Point parentPos = positions[personId];
                    Point? spousePos = null;

                    if (person.Spouse.HasValue && positions.ContainsKey(person.Spouse.Value))
                    {
                        spousePos = positions[person.Spouse.Value];
                    }

                    // Calculate center point between parents
                    double centerX;
                    if (spousePos.HasValue)
                    {
                        centerX = (parentPos.X + spousePos.Value.X + CardWidth) / 2;
                    }
                    else
                    {
                        centerX = parentPos.X + CardWidth / 2;
                    }

                    // Get all children that are actually in the tree
                    var childrenInTree = person.Children.Where(cId => positions.ContainsKey(cId)).ToList();

                    if (childrenInTree.Count == 0)
                        continue;

                    // Calculate total width needed for children
                    double childrenWidth = childrenInTree.Count * CardWidth + (childrenInTree.Count - 1) * HorizontalSpacing;
                    double startX = centerX - childrenWidth / 2;

                    // Position each child
                    for (int i = 0; i < childrenInTree.Count; i++)
                    {
                        int childId = childrenInTree[i];
                        double childX = startX + i * (CardWidth + HorizontalSpacing);

                        // Keep the Y position, just adjust X
                        Point currentPos = positions[childId];
                        positions[childId] = new Point(childX, currentPos.Y);

                        // If child has a spouse, adjust spouse position too
                        Person child = FamilyTree.GetPerson(childId);
                        if (child?.Spouse != null && positions.ContainsKey(child.Spouse.Value))
                        {
                            positions[child.Spouse.Value] = new Point(childX + CardWidth + HorizontalSpacing, currentPos.Y);
                        }
                    }
                }
            }
        }

        private void DrawConnections(Dictionary<int, Point> positions)
        {
            var drawnSpouses = new HashSet<(int, int)>();

            foreach (var kvp in positions)
            {
                int personId = kvp.Key;
                Person person = FamilyTree.GetPerson(personId);
                if (person == null)
                    continue;

                // Draw spouse connection (horizontal line between spouses)
                if (person.Spouse.HasValue && positions.ContainsKey(person.Spouse.Value))
                {
                    var pair = personId < person.Spouse.Value
                        ? (personId, person.Spouse.Value)
                        : (person.Spouse.Value, personId);

                    if (!drawnSpouses.Contains(pair))
                    {
                        Point pos1 = positions[personId];
                        Point pos2 = positions[person.Spouse.Value];

                        double y = pos1.Y + CardHeight / 2;
                        double x1 = pos1.X + CardWidth;
                        double x2 = pos2.X;

                        // Horizontal line connecting spouses
                        Line spouseLine = new Line
                        {
                            X1 = x1,
                            Y1 = y,
                            X2 = x2,
                            Y2 = y,
                            Stroke = new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                            StrokeThickness = 3
                        };
                        TreeCanvas.Children.Add(spouseLine);

                        // Draw vertical line down from center of spouse line
                        if (person.Children.Count > 0)
                        {
                            double centerX = (x1 + x2) / 2;
                            double verticalLineLength = VerticalSpacing / 2;

                            Line verticalLine = new Line
                            {
                                X1 = centerX,
                                Y1 = y,
                                X2 = centerX,
                                Y2 = y + verticalLineLength,
                                Stroke = new SolidColorBrush(Color.FromRgb(158, 158, 158)),
                                StrokeThickness = 2
                            };
                            TreeCanvas.Children.Add(verticalLine);

                            // Draw horizontal line connecting to all children
                            var childrenInTree = person.Children.Where(cId => positions.ContainsKey(cId)).ToList();
                            if (childrenInTree.Count > 0)
                            {
                                double leftMostChildX = positions[childrenInTree[0]].X + CardWidth / 2;
                                double rightMostChildX = positions[childrenInTree[childrenInTree.Count - 1]].X + CardWidth / 2;

                                Line horizontalLine = new Line
                                {
                                    X1 = leftMostChildX,
                                    Y1 = y + verticalLineLength,
                                    X2 = rightMostChildX,
                                    Y2 = y + verticalLineLength,
                                    Stroke = new SolidColorBrush(Color.FromRgb(158, 158, 158)),
                                    StrokeThickness = 2
                                };
                                TreeCanvas.Children.Add(horizontalLine);

                                // Draw vertical lines down to each child
                                foreach (int childId in childrenInTree)
                                {
                                    Point childPos = positions[childId];
                                    double childCenterX = childPos.X + CardWidth / 2;

                                    Line childLine = new Line
                                    {
                                        X1 = childCenterX,
                                        Y1 = y + verticalLineLength,
                                        X2 = childCenterX,
                                        Y2 = childPos.Y,
                                        Stroke = new SolidColorBrush(Color.FromRgb(158, 158, 158)),
                                        StrokeThickness = 2
                                    };
                                    TreeCanvas.Children.Add(childLine);
                                }
                            }
                        }

                        drawnSpouses.Add(pair);
                    }
                }
                else if (person.Children.Count > 0)
                {
                    // Single parent with children
                    Point parentPos = positions[personId];
                    double parentCenterX = parentPos.X + CardWidth / 2;
                    double parentBottomY = parentPos.Y + CardHeight;
                    double verticalLineLength = VerticalSpacing / 2;

                    // Vertical line down from parent
                    Line verticalLine = new Line
                    {
                        X1 = parentCenterX,
                        Y1 = parentBottomY,
                        X2 = parentCenterX,
                        Y2 = parentBottomY + verticalLineLength,
                        Stroke = new SolidColorBrush(Color.FromRgb(158, 158, 158)),
                        StrokeThickness = 2
                    };
                    TreeCanvas.Children.Add(verticalLine);

                    var childrenInTree = person.Children.Where(cId => positions.ContainsKey(cId)).ToList();
                    if (childrenInTree.Count > 0)
                    {
                        double leftMostChildX = positions[childrenInTree[0]].X + CardWidth / 2;
                        double rightMostChildX = positions[childrenInTree[childrenInTree.Count - 1]].X + CardWidth / 2;

                        // Horizontal line connecting all children
                        Line horizontalLine = new Line
                        {
                            X1 = leftMostChildX,
                            Y1 = parentBottomY + verticalLineLength,
                            X2 = rightMostChildX,
                            Y2 = parentBottomY + verticalLineLength,
                            Stroke = new SolidColorBrush(Color.FromRgb(158, 158, 158)),
                            StrokeThickness = 2
                        };
                        TreeCanvas.Children.Add(horizontalLine);

                        // Vertical lines to each child
                        foreach (int childId in childrenInTree)
                        {
                            Point childPos = positions[childId];
                            double childCenterX = childPos.X + CardWidth / 2;

                            Line childLine = new Line
                            {
                                X1 = childCenterX,
                                Y1 = parentBottomY + verticalLineLength,
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

                // Create card border
                Border card = new Border
                {
                    Width = CardWidth,
                    Height = CardHeight,
                    Style = person.Gender
                        ? (Style)FindResource("MaleCardStyle")
                        : (Style)FindResource("FemaleCardStyle"),
                    Cursor = System.Windows.Input.Cursors.Hand
                };

                // Add click event to card
                card.Tag = personId;
                card.MouseDown += Card_MouseDown;

                // Create card content
                Grid cardContent = new Grid();
                cardContent.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                cardContent.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // Profile picture or icon
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

                // Person info
                StackPanel info = new StackPanel
                {
                    Margin = new Thickness(5, 0, 5, 5)
                };

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
                    Text = person.BirthDate.ToString("yyyy-MM-dd"),
                    FontSize = 10,
                    Foreground = Brushes.Gray,
                    TextAlignment = TextAlignment.Center
                };

                // Highlight root person
                if (personId == rootPersonId)
                {
                    card.BorderThickness = new Thickness(4);
                    card.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 193, 7));
                }

                info.Children.Add(nameText);
                info.Children.Add(birthText);

                Grid.SetRow(info, 1);
                cardContent.Children.Add(info);

                card.Child = cardContent;

                // Position on canvas
                Canvas.SetLeft(card, position.X);
                Canvas.SetTop(card, position.Y);

                TreeCanvas.Children.Add(card);
            }
        }

        private void Card_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border card && card.Tag is int personId)
            {
                if (e.ClickCount == 2)
                {
                    // Double click - open profile
                    NavigationService?.Navigate(new Profile(personId));
                    e.Handled = true;
                }
                else if (e.ClickCount == 1)
                {
                    // Single click - re-center tree on this person
                    NavigationService?.Navigate(new Tree(personId));
                }
            }
        }

        private void AdjustCanvasSize(Dictionary<int, Point> positions)
        {
            if (positions.Count == 0)
                return;

            double maxX = positions.Values.Max(p => p.X) + CardWidth + 100;
            double maxY = positions.Values.Max(p => p.Y) + CardHeight + 100;

            TreeCanvas.Width = maxX;
            TreeCanvas.Height = maxY;
        }
    }
}