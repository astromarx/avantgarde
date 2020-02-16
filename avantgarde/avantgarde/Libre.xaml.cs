
using System;
using System.Collections.Generic;
using Windows.Foundation;

using System.ComponentModel;

using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Input.Inking;

using Windows.Devices.Input.Preview;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace avantgarde
{
    public partial class Libre : INotifyPropertyChanged
    {
        private Controller.GazeController controller;
        private DrawingModel drawingModel;

        private InkDrawingAttributes drawingAttributes = new InkDrawingAttributes();

        private String backgroundHex { get; set; }

        private List<Line> GridLines = new List<Line>();
        private int WIDTH { get; set; }
        private int HEIGHT { get; set; }

        public static Color colourSelection {get; set;}

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void getWindowAttributes()
        {
            WIDTH = (int)Window.Current.Bounds.Width;
            HEIGHT = (int)Window.Current.Bounds.Height;
        }
        public Libre()
        {
            colourSelection = Menus.ColourManager.defaultColour;
            getWindowAttributes();

            this.InitializeComponent();

            this.DataContext = this;

            backgroundHex = Colors.LightGreen.ToString();

            ui.goHomeButtonClicked += new EventHandler(goHomeButtonClicked);
            ui.drawStateChanged += new EventHandler(drawStateButtonClicked);
            ui.drawingPropertiesUpdated += new EventHandler(drawingPropertiesUpdated);
            ui.undoButtonClicked += new EventHandler(undoButtonClicked);
            ui.redoButtonClicked += new EventHandler(redoButtonClicked);
            ui.backgroundButtonClicked += new EventHandler(backgroundColourUpdated);
            ui.colourSelectionUpdated += new EventHandler(updateColourSelection);

            inkCanvas.InkPresenter.InputDeviceTypes =
                Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                Windows.UI.Core.CoreInputDeviceTypes.Pen |
                Windows.UI.Core.CoreInputDeviceTypes.Touch;
            DataContext = this;

            drawingModel = new DrawingModel(inkCanvas.InkPresenter.StrokeContainer);
            controller = new Controller.GazeController(this, GazeInputSourcePreview.GetForCurrentView(), drawingModel, ui);         
        }

        private Point ToCanvasPoint(Point point)
        {
            Double x = point.X;
            Double y = point.Y;
            y -= TopGrid.RowDefinitions[0].ActualHeight;
            return new Point(x, y);
        }
        
        private void goHomeButtonClicked(object sender, EventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
        private void redoButtonClicked(object sender, EventArgs e)
        {
            drawingModel.redo();
        }

        private void undoButtonClicked(object sender, EventArgs e)
        {
            drawingModel.undo();
        }

        private void drawStateButtonClicked(object sender, EventArgs e)
        {
            
        }

        private void drawingPropertiesUpdated(object sender, EventArgs e) {
            drawingAttributes = ui.getDrawingAttributes();
        }

        private void backgroundColourUpdated(object sender, EventArgs e)
        {
            backgroundHex = ui.getBackgroundHex();
            NotifyPropertyChanged();
        }

        private void updateColourSelection(object sender, EventArgs e) {
            colourSelection = ui.getColour();
            drawingAttributes.Color = colourSelection;
        }
    }
}
