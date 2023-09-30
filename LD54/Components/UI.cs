using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Panels;

public class UI : SyncScript
{
    public static UI Instance { get; private set; }

    public UIElement StartButton { get; private set; }
    public UIElement SpecialBar { get; private set; }
    public UIElement SpaceBar { get; private set; }
    public TextBlock ScoreText { get; private set; }
    public TextBlock TitleText { get; private set; }
    public UIElement HelpText { get; private set; }
    public TextBlock TestText { get; private set; }

    public override void Update()
    {
        StartButton.Visibility = SessionManager.Instance.IsPlaying ? Visibility.Hidden : Visibility.Visible;
        HelpText.Visibility = SessionManager.Instance.IsPlaying ? Visibility.Hidden : Visibility.Visible;
        TitleText.Visibility = SessionManager.Instance.IsPlaying ? Visibility.Hidden : Visibility.Visible;
        TitleText.Text = SessionManager.Instance.State == SessionManager.ManagerState.GameOver ? "Game Over" : "LD54";

        ScoreText.Text = GarbageFactory.Instance == null ? "0" : GarbageFactory.Instance.DestroyedCount.ToString();
        SpecialBar.Height = Dozer.Instance == null ? 500 : Dozer.Instance.SpecialRatio * 500;
        SpaceBar.Height = GarbageFactory.Instance == null ? 500 : GarbageFactory.Instance.SpaceRatio * 500;

        TestText.Text = Program.Game.UpdateTime.FramePerSecond.ToString();
    }

    public static Entity Create()
    {
        var game = Program.Game;

        var font = game.Content.Load<SpriteFont>("StrideDefaultFont");
        var grid = new Grid() { Name = "RootGrid" };
        var page = new UIPage() { RootElement = grid };
        var text = new TextBlock
        {
            Text = "Start Game",
            TextColor = Color.White,
            TextSize = 32,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Font = font
        };
        var button = new ColorButton()
        {
            Name = "StartButton",
            Size = new Vector3(250, 80, 0),
            BackgroundColor = Color.DarkGray,
            Content = text,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var specialBar = new Border()
        {
            BackgroundColor = Color.Blue,
            VerticalAlignment = VerticalAlignment.Bottom,
            Height = 500
        };
        var specialBorder = new Border()
        {
            BackgroundColor = Color.Black,
            BorderColor = Color.White,
            BorderThickness = new Thickness(5, 5, 5, 5),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Height = 510,
            Width = 70,
            Margin = new Thickness(35, 0, 0, 0),
            Content = specialBar
        };

        var spaceBar = new Border()
        {
            BackgroundColor = Color.Red,
            VerticalAlignment = VerticalAlignment.Bottom,
            Height = 500
        };
        var spaceBorder = new Border()
        {
            BackgroundColor = Color.Black,
            BorderColor = Color.White,
            BorderThickness = new Thickness(5, 5, 5, 5),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Height = 510,
            Width = 70,
            Margin = new Thickness(0, 0, 35, 0),
            Content = spaceBar
        };

        var scoreText = new TextBlock
        {
            Text = "0",
            TextColor = Color.White,
            TextSize = 40,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Font = font
        };
        var scoreBorder = new Border()
        {
            BackgroundColor = Color.DarkGray,
            BorderColor = Color.White,
            BorderThickness = new Thickness(5, 5, 5, 5),
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 0),
            Height = 60,
            Width = 120,
            Content = scoreText
        };

        button.Click += (s, e) =>
        {
            SessionManager.Instance.StartSession();
        };

        var titleText = new TextBlock
        {
            Text = "Game Over",
            TextColor = Color.White,
            TextSize = 150,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Font = font,
            Margin = new Thickness(0, 140, 0, 0),
            Visibility = Visibility.Hidden,
        };

        var helpText = new TextBlock
        {
            Text = "Push the Garbage off before you run out of Space" + System.Environment.NewLine +
                   "Press WASD or ARROW KEYS to MOVE" + System.Environment.NewLine +
                   "The left bar is your SPECIAL, activate using SPACE KEY" + System.Environment.NewLine +
                   "The right bar is the remaining SPACE, when it is full the GAME ENDS",
            TextColor = Color.White,
            TextSize = 22,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            Font = font,
            Margin = new Thickness(0, 0, 0, 0)
        };

        var testText = new TextBlock
        {
            Text = "",
            TextColor = Color.Red,
            TextSize = 12,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Font = font,
            Visibility = System.Diagnostics.Debugger.IsAttached ? Visibility.Visible : Visibility.Hidden,
        };

        grid.Children.Add(button);
        grid.Children.Add(specialBorder);
        grid.Children.Add(spaceBorder);
        grid.Children.Add(scoreBorder);
        grid.Children.Add(titleText);
        grid.Children.Add(helpText);
        grid.Children.Add(testText);

        Instance = new()
        {
            StartButton = button,
            SpecialBar = specialBar,
            SpaceBar = spaceBar,
            ScoreText = scoreText,
            TitleText = titleText,
            HelpText = helpText,
            TestText = testText,
        };

        return new Entity("UI") { new UIComponent() { Page = page }, Instance };
    }

    public class ColorButton : Button
    {
        protected override void OnTouchDown(TouchEventArgs args)
        {
            base.OnTouchDown(args);

            BackgroundColor = Color.Black;
        }
        protected override void OnTouchUp(TouchEventArgs args)
        {
            base.OnTouchUp(args);

            BackgroundColor = Color.DarkGray;
        }
    }
}