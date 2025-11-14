using System;
using System.Collections.Generic;
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
        private const double CardWidth = 180;
        private const double CardHeight = 120;
        private const double HorizontalSpacing = 60;
        private const double VerticalSpacing = 150;
        private const double StartX = 50;
        private const double StartY = 50;

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
                RootPersonText.Text = $"Current: {rootPerson.Name}";
            }

            DrawTree();
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
            var generations = BuildGenerationsList(rootPersonId);

            // Calculate positions for everyone
            var positions = CalculatePositions(generations);

            // Draw connections between people
            DrawConnections(positions, generations);

            // Draw person cards
            DrawPersonCards(positions);

            // Adjust canvas size
            AdjustCanvasSize(positions);
        }

        private List<List<int>> BuildGenerationsList(int startPersonId)
        {
            var generations = new List<List<int>>();
            var currentPerson = FamilyTree.GetPerson(startPersonId);

            if (currentPerson == null)
                return generations;

            // Start with the root person's generation (generation 0)
            // Include all siblings (same parents)
            var currentGeneration = GetSiblingsWithSpouses(startPersonId);
            generations.Add(currentGeneration);

            // Go up through ancestors (dad's lineage) - show ALL siblings at each level
            int currentId = startPersonId;
            while (true)
            {
                Person person = FamilyTree.GetPerson(currentId);
                if (person == null || !person.Dad.HasValue)
                    break;

                // Build parent generation - include dad's siblings
                var parentGen = GetSiblingsWithSpouses(person.Dad.Value);

                if (parentGen.Count > 0)
                {
                    generations.Insert(0, parentGen); // Insert at beginning (ancestors go on top)
                    currentId = person.Dad.Value; // Move up to dad
                }
                else
                {
                    break;
                }
            }

            // Go down through descendants (ONLY root person's children, not siblings' children)
            AddDescendants(startPersonId, generations);

            return generations;
        }

        private List<int> GetSiblingsWithSpouses(int personId)
        {
            var result = new List<int>();
            Person person = FamilyTree.GetPerson(personId);

            if (person == null)
                return result;

            var siblings = new List<int>();

            // Get all siblings from parents
            if (person.Dad.HasValue || person.Mom.HasValue)
            {
                // Get dad's children
                if (person.Dad.HasValue)
                {
                    Person dad = FamilyTree.GetPerson(person.Dad.Value);
                    if (dad != null)
                    {
                        siblings.AddRange(dad.Children);
                    }
                }

                // Get mom's children (union with dad's)
                if (person.Mom.HasValue)
                {
                    Person mom = FamilyTree.GetPerson(person.Mom.Value);
                    if (mom != null)
                    {
                        foreach (int childId in mom.Children)
                        {
                            if (!siblings.Contains(childId))
                                siblings.Add(childId);
                        }
                    }
                }
            }
            else
            {
                // No parents, just this person
                siblings.Add(personId);
            }

            // Add siblings and their spouses to result
            foreach (int siblingId in siblings)
            {
                if (!result.Contains(siblingId))
                {
                    result.Add(siblingId);

                    Person sibling = FamilyTree.GetPerson(siblingId);
                    if (sibling?.Spouse != null && !result.Contains(sibling.Spouse.Value))
                    {
                        result.Add(sibling.Spouse.Value);
                    }
                }
            }

            return result;
        }

        private void AddDescendants(int personId, List<List<int>> generations)
        {
            Person person = FamilyTree.GetPerson(personId);
            if (person == null || person.Children.Count == 0)
                return;

            // Build children generation - include all children and their spouses
            var childrenGen = new List<int>();

            foreach (int childId in person.Children)
            {
                if (!childrenGen.Contains(childId))
                {
                    childrenGen.Add(childId);

                    // Add child's spouse right next to them
                    Person child = FamilyTree.GetPerson(childId);
                    if (child?.Spouse != null && !childrenGen.Contains(child.Spouse.Value))
                    {
                        childrenGen.Add(child.Spouse.Value);
                    }
                }
            }

            if (childrenGen.Count > 0)
            {
                generations.Add(childrenGen);

                // Recursively add descendants from ALL children in this generation
                AddDescendantsFromGeneration(childrenGen, generations);
            }
        }

        private void AddDescendantsFromGeneration(List<int> currentGeneration, List<List<int>> generations)
        {
            var nextGen = new List<int>();
            var processedChildren = new HashSet<int>();

            // Collect all children from everyone in current generation
            foreach (int parentId in currentGeneration)
            {
                Person parent = FamilyTree.GetPerson(parentId);
                if (parent == null)
                    continue;

                foreach (int childId in parent.Children)
                {
                    if (!processedChildren.Contains(childId))
                    {
                        nextGen.Add(childId);
                        processedChildren.Add(childId);

                        // Add child's spouse
                        Person child = FamilyTree.GetPerson(childId);
                        if (child?.Spouse != null && !processedChildren.Contains(child.Spouse.Value))
                        {
                            nextGen.Add(child.Spouse.Value);
                            processedChildren.Add(child.Spouse.Value);
                        }
                    }
                }
            }

            if (nextGen.Count > 0)
            {
                generations.Add(nextGen);
                // Recursively process the next generation
                AddDescendantsFromGeneration(nextGen, generations);
            }
        }

        private Dictionary<int, Point> CalculatePositions(List<List<int>> generations)
        {
            var positions = new Dictionary<int, Point>();
            double currentY = StartY;

            foreach (var generation in generations)
            {
                double currentX = StartX;

                for (int i = 0; i < generation.Count; i++)
                {
                    int personId = generation[i];
                    positions[personId] = new Point(currentX, currentY);

                    currentX += CardWidth + HorizontalSpacing;
                }

                currentY += CardHeight + VerticalSpacing;
            }

            // Center children under parents (process from top to bottom)
            CenterChildrenUnderParents(positions, generations);

            return positions;
        }

        private void CenterChildrenUnderParents(Dictionary<int, Point> positions, List<List<int>> generations)
        {
            // Process from top generation down to ensure proper cascading
            for (int genIndex = 0; genIndex < generations.Count - 1; genIndex++)
            {
                var currentGen = generations[genIndex];
                var nextGen = generations[genIndex + 1];

                // Track which people in current generation have been processed as part of a couple
                var processed = new HashSet<int>();

                // Also track which children have been positioned
                var positionedChildren = new HashSet<int>();

                for (int i = 0; i < currentGen.Count; i++)
                {
                    int personId = currentGen[i];

                    if (processed.Contains(personId))
                        continue;

                    Person person = FamilyTree.GetPerson(personId);
                    if (person == null || person.Children.Count == 0)
                        continue;

                    // Check if this is a couple (spouse is next in the list)
                    bool isCoupleWithNext = false;
                    int? spouseId = null;

                    if (i + 1 < currentGen.Count && person.Spouse.HasValue && currentGen[i + 1] == person.Spouse.Value)
                    {
                        isCoupleWithNext = true;
                        spouseId = person.Spouse.Value;
                        processed.Add(spouseId.Value);
                    }

                    processed.Add(personId);

                    // Find this parent's ACTUAL children (not including their spouses who aren't children)
                    var children = person.Children.Where(cId => nextGen.Contains(cId) && !positionedChildren.Contains(cId)).ToList();
                    if (children.Count == 0)
                        continue;

                    // Calculate parent center point
                    Point personPos = positions[personId];
                    double parentCenterX;

                    if (isCoupleWithNext && spouseId.HasValue)
                    {
                        Point spousePos = positions[spouseId.Value];
                        parentCenterX = (personPos.X + spousePos.X + CardWidth) / 2;
                    }
                    else
                    {
                        parentCenterX = personPos.X + CardWidth / 2;
                    }

                    // Calculate total width needed for all children (including their spouses)
                    double totalChildrenWidth = 0;
                    var childInfo = new List<(int childId, int? spouseId, double width)>();

                    for (int j = 0; j < children.Count; j++)
                    {
                        int childId = children[j];
                        Person child = FamilyTree.GetPerson(childId);
                        int childIndex = nextGen.IndexOf(childId);

                        double width = CardWidth;
                        int? childSpouseId = null;

                        // Check if the next person in the list is this child's spouse
                        if (child?.Spouse != null && childIndex + 1 < nextGen.Count && nextGen[childIndex + 1] == child.Spouse.Value)
                        {
                            childSpouseId = child.Spouse.Value;
                            // Only include spouse width if there are multiple children
                            // If only 1 child, we center over the child only, not the child+spouse pair
                            if (children.Count > 1)
                            {
                                width = (CardWidth * 2) + HorizontalSpacing;
                            }
                            positionedChildren.Add(child.Spouse.Value); // Mark spouse as positioned
                        }

                        childInfo.Add((childId, childSpouseId, width));
                        totalChildrenWidth += width;

                        if (j < children.Count - 1)
                            totalChildrenWidth += HorizontalSpacing;

                        positionedChildren.Add(childId);
                    }

                    // Center children under parent(s)
                    double startX = parentCenterX - totalChildrenWidth / 2;
                    double childX = startX;

                    foreach (var (childId, childSpouseId, width) in childInfo)
                    {
                        positions[childId] = new Point(childX, positions[childId].Y);

                        if (childSpouseId.HasValue)
                        {
                            positions[childSpouseId.Value] = new Point(childX + CardWidth + HorizontalSpacing, positions[childSpouseId.Value].Y);
                        }

                        childX += width + HorizontalSpacing;
                    }
                }
            }

            // After all centering, shift everything right if any card went too far left
            double minX = positions.Values.Min(p => p.X);
            if (minX < StartX)
            {
                double shiftAmount = StartX - minX;
                var keys = positions.Keys.ToList();
                foreach (var key in keys)
                {
                    Point pos = positions[key];
                    positions[key] = new Point(pos.X + shiftAmount, pos.Y);
                }
            }
        }

        private void DrawConnections(Dictionary<int, Point> positions, List<List<int>> generations)
        {
            var drawnSpouses = new HashSet<(int, int)>();

            for (int genIndex = 0; genIndex < generations.Count; genIndex++)
            {
                var generation = generations[genIndex];

                for (int i = 0; i < generation.Count; i++)
                {
                    int personId = generation[i];
                    Person person = FamilyTree.GetPerson(personId);

                    if (person == null)
                        continue;

                    Point personPos = positions[personId];

                    // Draw spouse connection
                    if (i + 1 < generation.Count && person.Spouse.HasValue && generation[i + 1] == person.Spouse.Value)
                    {
                        var pair = (Math.Min(personId, person.Spouse.Value), Math.Max(personId, person.Spouse.Value));

                        if (!drawnSpouses.Contains(pair))
                        {
                            Point spousePos = positions[person.Spouse.Value];
                            double y = personPos.Y + CardHeight / 2;

                            Line spouseLine = new Line
                            {
                                X1 = personPos.X + CardWidth,
                                Y1 = y,
                                X2 = spousePos.X,
                                Y2 = y,
                                Stroke = new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                                StrokeThickness = 3
                            };
                            TreeCanvas.Children.Add(spouseLine);

                            drawnSpouses.Add(pair);

                            // Draw lines to children
                            if (person.Children.Count > 0 && genIndex + 1 < generations.Count)
                            {
                                DrawChildrenConnections(personId, person.Spouse.Value, positions, generations[genIndex + 1]);
                            }
                        }
                    }
                    else if (person.Children.Count > 0 && genIndex + 1 < generations.Count && !person.Spouse.HasValue)
                    {
                        // Single parent
                        DrawChildrenConnections(personId, null, positions, generations[genIndex + 1]);
                    }
                }
            }
        }

        private void DrawChildrenConnections(int parentId, int? spouseId, Dictionary<int, Point> positions, List<int> childGeneration)
        {
            Person parent = FamilyTree.GetPerson(parentId);
            if (parent == null)
                return;

            var children = parent.Children.Where(cId => childGeneration.Contains(cId)).ToList();
            if (children.Count == 0)
                return;

            Point parentPos = positions[parentId];
            double parentCenterX = parentPos.X + CardWidth / 2;
            double startY = parentPos.Y + CardHeight;
            double spouseLineY = parentPos.Y + CardHeight / 2;

            if (spouseId.HasValue)
            {
                Point spousePos = positions[spouseId.Value];
                parentCenterX = (parentPos.X + spousePos.X + CardWidth) / 2;
            }

            double verticalLineLength = VerticalSpacing / 2;

            if (spouseId.HasValue)
            {
                Line verticalLineToSpouse = new Line
                {
                    X1 = parentCenterX,
                    Y1 = spouseLineY,
                    X2 = parentCenterX,
                    Y2 = startY + verticalLineLength,
                    Stroke = new SolidColorBrush(Color.FromRgb(158, 158, 158)),
                    StrokeThickness = 2
                };
                TreeCanvas.Children.Add(verticalLineToSpouse);
            }
            else
            {
                Line verticalLine = new Line
                {
                    X1 = parentCenterX,
                    Y1 = startY,
                    X2 = parentCenterX,
                    Y2 = startY + verticalLineLength,
                    Stroke = new SolidColorBrush(Color.FromRgb(158, 158, 158)),
                    StrokeThickness = 2
                };
                TreeCanvas.Children.Add(verticalLine);
            }

            double leftChildX = positions[children[0]].X + CardWidth / 2;
            double rightChildX = positions[children[children.Count - 1]].X + CardWidth / 2;

            Line horizontalLine = new Line
            {
                X1 = leftChildX,
                Y1 = startY + verticalLineLength,
                X2 = rightChildX,
                Y2 = startY + verticalLineLength,
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
                    Y1 = startY + verticalLineLength,
                    X2 = childCenterX,
                    Y2 = childPos.Y,
                    Stroke = new SolidColorBrush(Color.FromRgb(158, 158, 158)),
                    StrokeThickness = 2
                };
                TreeCanvas.Children.Add(childLine);
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
                    FontSize = 10,
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
                if (rootPersonId == personId)
                {
                    NavigationService?.Navigate(new Profile(personId));
                }
                else
                {
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