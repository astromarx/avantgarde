﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace avantgarde.Menus
{
    public sealed partial class ColourManager : UserControl, INotifyPropertyChanged
    {
        public bool selectingBackground = false;
        public bool themeChoosing {get;set;}

        public bool autoSwitchTurnOn = false;

        public Color backgroundSelection;
        public Color selection { get; set; }
        public int brightness { get; set; }
        public int opacity { get; set; }
        public int colourProfile { get; set; }
        public String colourName { get; set; }
        public String selectionHex { get; set; }
        public static Color defaultColour { get; set; }

        public String backgroundHex;

        private int DEFAULT_BRIGHTNESS = 5;

        ColourPickerData colourPickerData = new ColourPickerData();

        public String[] prevColours { get; set;}

        private int[,,] newcolorList = new int[6, 5, 2] {                   // int [lines of button,one button, color on the button]
            {{2,7 },{1,8},{ 1,1},{1,6 },{11,4 } },
            {{ 5,9},{ 1,8},{ 10,7},{ 11,4},{12,2 } },
            {{1,8 },{11,6 },{10,4 },{9,2 },{ 7,2} },
            {{6,9 },{5,8 },{ 1,8},{ 1,6},{ 11,8} },
            {{11,5},{ 10,3},{11,4 },{10,3 },{10,0 } },
            {{11,3 },{9,2 },{6,3 },{ 4,8},{3,9 } }

            };

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler updateColourSelection;
        public event EventHandler colourManagerClosed;
        public event EventHandler backgroundSelectionChanged;

        public string[,] getPresetColorTheme()
        {
            string[,] result = new string[6, 5];
            for(int i = 0; i < 6; i++)
            {
                for(int j = 0; j < 5; j++)
                {
                    result[i, j] = "FF" + colourPickerData.getColourHex(newcolorList[i,j,0],newcolorList[i,j,1]);
                }
            }
            return result;
        }
        private void NotifyPropertyChanged(String propertyName = "")
        { 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void updateColour(int profile, int brightness) {
            if (selectingBackground) {
                backgroundSelection = hexToColor("FF" + colourPickerData.getColourHex(profile, brightness));
                backgroundSelectionChanged?.Invoke(this, EventArgs.Empty);
                if (ColourPickerMenu.IsOpen) { ColourPickerMenu.IsOpen = false; }
                colourManagerClosed?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                selection = hexToColor("FF" + colourPickerData.getColourHex(profile, brightness));
                selectionHex = selection.ToString();
                this.brightness = brightness;
                this.colourProfile = profile;
            }
            updateColourName();
            updateColourSelection?.Invoke(this, EventArgs.Empty);
            NotifyPropertyChanged();
        }

        public Color getColour() {
            return selection;
        }

        public Color getBackgroundColour() {
            return backgroundSelection;
        }

        public ColourManager()
        {
            backgroundSelection = Colors.Lavender;
            brightness = 5;
            opacity = 100;
            colourProfile = 6;
            this.InitializeComponent();
            DataContext = colourPickerData.getColourPickerData();
            prevColours = colourPickerData.getDefaultPrevColours();
            colourPickerData.setColourDataTemp();
            defaultColour = hexToColor(getOpacityHex() + colourPickerData.getColourHex(colourProfile, brightness));
            selection = defaultColour;
            selectionHex = selection.ToString();
            updateColourName();
        }

        public void openMenu() {
            prevColours = colourPickerData.getDefaultPrevColours();
            NotifyPropertyChanged();
            if (!ColourPickerMenu.IsOpen) { ColourPickerMenu.IsOpen = true; }
            themeChoosing = false;
        }

        private void updateColourName() {
            colourName = ColorHelper.ToDisplayName(selection);
            Debug.WriteLine(colourName);
        }

        public Color hexToColor(string hex)
        {
            if (hex.IndexOf('#') != -1)
                hex = hex.Replace("#", "");

            byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));

            return Color.FromArgb(a, r, g, b);
        }


        private String getOpacityHex() {
            int newOpacity = (int)(opacity * 0.01 * 255);
            String newOpacityStr = Convert.ToString(newOpacity, 16);

            if (newOpacityStr.Length == 1)
            {
                newOpacityStr = newOpacityStr.Insert(0, "0");
            }

            return newOpacityStr;
        }

        


        private void updateOpacity() {
            selection = hexToColor(getOpacityHex() + selection.ToString().Substring(3));
        }

        private void increaseBrightness(object sender, RoutedEventArgs e)
        {
            if (brightness == 10)
            {
                return;
            }
            brightness++;
            updateSelection();
        }

        private void decreaseBrightness(object sender, RoutedEventArgs e)
        {
            if (brightness == 0)
            {
                return;
            }
            brightness--;
            updateSelection();
        }

        private void increaseOpacity(object sender, RoutedEventArgs e)
        {
            if (opacity == 100)
            {
                return;
            }
            opacity = opacity + 5;
            updateOpacity();
            updateColourName();
            NotifyPropertyChanged();
        }

        private void decreaseOpacity(object sender, RoutedEventArgs e)
        {
            if (opacity == 0) {
                return;
            }
            opacity = opacity - 5;
            updateOpacity();
            updateColourName();
            NotifyPropertyChanged();
        }

        private void setPrevColour0(object sender, RoutedEventArgs e)
        {
            selection = hexToColor(colourPickerData.getDefaultPrevColour(0));
            updateColourName();
            NotifyPropertyChanged();
        }

        private void setPrevColour1(object sender, RoutedEventArgs e)
        {
            selection = hexToColor(colourPickerData.getDefaultPrevColour(1));
            updateColourName();
            NotifyPropertyChanged();
        }

        private void setPrevColour2(object sender, RoutedEventArgs e)
        {
            selection = hexToColor(colourPickerData.getDefaultPrevColour(2));
            updateColourName();
            NotifyPropertyChanged();
        }

        private void setPrevColour3(object sender, RoutedEventArgs e)
        {
            selection = hexToColor(colourPickerData.getDefaultPrevColour(3));
            updateColourName();
            NotifyPropertyChanged();
        }

        private void updateSelection() {
            selection = hexToColor(getOpacityHex() + colourPickerData.getColourHex(colourProfile, brightness));
            updateColourName();
            NotifyPropertyChanged();
        }

        private void setColour0(object sender, RoutedEventArgs e)
        {
            colourProfile = 0;
            brightness = DEFAULT_BRIGHTNESS;
            updateSelection();
        }

        private void setColour1(object sender, RoutedEventArgs e)
        {
            colourProfile = 1;
            brightness = DEFAULT_BRIGHTNESS;
            updateSelection();
        }

        private void setColour2(object sender, RoutedEventArgs e)
        {
            colourProfile = 2;
            brightness = DEFAULT_BRIGHTNESS;
            updateSelection();
        }

        private void setColour3(object sender, RoutedEventArgs e)
        {
            colourProfile = 3;
            brightness = DEFAULT_BRIGHTNESS;
            updateSelection();
        }
        private void setColour4(object sender, RoutedEventArgs e)
        {
            colourProfile = 4;
            brightness = DEFAULT_BRIGHTNESS;
            updateSelection();
        }
        private void setColour5(object sender, RoutedEventArgs e)
        {
            colourProfile = 5;
            brightness = DEFAULT_BRIGHTNESS;
            updateSelection();
        }
        private void setColour6(object sender, RoutedEventArgs e)
        {
            colourProfile = 6;
            brightness = DEFAULT_BRIGHTNESS;
            updateSelection();
        }

        private void setColour7(object sender, RoutedEventArgs e)
        {
            colourProfile = 7;
            brightness = DEFAULT_BRIGHTNESS;
            updateSelection();
        }
        private void setColour8(object sender, RoutedEventArgs e)
        {
            colourProfile = 8;
            brightness = DEFAULT_BRIGHTNESS;
            updateSelection();
        }
        private void setColour9(object sender, RoutedEventArgs e)
        {
            colourProfile = 9;
            brightness = DEFAULT_BRIGHTNESS;
            updateSelection();
        }
        private void setColour10(object sender, RoutedEventArgs e)
        {
            colourProfile = 10;
            brightness = DEFAULT_BRIGHTNESS;
            updateSelection();
        }
        private void setColour11(object sender, RoutedEventArgs e)
        {
            colourProfile = 11;
            brightness = DEFAULT_BRIGHTNESS;
            updateSelection();
        }

        private void setColour12(object sender, RoutedEventArgs e)
        {
            colourProfile = 12;
            brightness = 0;
            updateSelection();
        }

        private void setColour13(object sender, RoutedEventArgs e)
        {
            colourProfile = 12;
            brightness = DEFAULT_BRIGHTNESS;
            updateSelection();
        }

        private void setColour14(object sender, RoutedEventArgs e)
        {
            colourProfile = 12;
            brightness = 10;
            updateSelection();
        }
        public event EventHandler<ThemeColorArg> themeColorSelected;
        private void selectColour(object sender, RoutedEventArgs e)
        {
            if (selectingBackground)
            {
                backgroundSelection = selection;
                selection = hexToColor(selectionHex);
                backgroundSelectionChanged?.Invoke(this, EventArgs.Empty);
                selectingBackground = false;
            }
            else if (themeChoosing)
            {
                ThemeColorArg themeColorArg = new ThemeColorArg();
                themeColorArg.colorSelected = selection;
                themeColorArg.colorHex = selection.ToString();
                selection = hexToColor(selectionHex);
                themeColorSelected?.Invoke(this, themeColorArg);
                themeChoosing = false;
            }
            else
            {

                selectionHex = selection.ToString();
                colourPickerData.addColourToPrevColours(selection.ToString());
               
                updateColourSelection?.Invoke(this, EventArgs.Empty);

            }
            if (ColourPickerMenu.IsOpen) { ColourPickerMenu.IsOpen = false; }
            colourManagerClosed?.Invoke(this, EventArgs.Empty);
            NotifyPropertyChanged();
        }

        private void cancelColourPick(object sender, RoutedEventArgs e)
        {
            if (selectingBackground) { selectingBackground = false; }
            if (ColourPickerMenu.IsOpen) { ColourPickerMenu.IsOpen = false; }
            colourManagerClosed?.Invoke(this, EventArgs.Empty);
            selection = hexToColor(selectionHex);
        }

        public void close() {
            if (ColourPickerMenu.IsOpen) { ColourPickerMenu.IsOpen = false; }
        }

        public String getColourHex(int colourProfile, int brightness) {
            return colourPickerData.getColourHex(colourProfile, brightness);
        }

        private int cycleCount = 0;
        public String[] colorRecycleListHex = new String[5] {"#DAD870",
        "#FFCD58","#FF9636","#FF5C4D","CD5C5C"};
        public void AutoChangeColor(Color[] colors)
        {
            /*Random random = new Random();
            Byte[] bufferBytes = new Byte[4];
            random.NextBytes(bufferBytes);
            selection = Color.FromArgb(bufferBytes[0], bufferBytes[1], bufferBytes[2], bufferBytes[3]);*/

            if (cycleCount == 5)
            {
                cycleCount = 0;
            }
            //selection = hexToColor(colorRecycleListHex[cycleCount++]);
            selection = colors[cycleCount++];
        }

        public void updateColorTheme(Color[] recycleColor)
        {
            //this.colorRecycleListHex = recycleColor;

        }
        public class ThemeColorArg : EventArgs
        {
            public Color colorSelected { get; set; }
            public string colorHex { get; set; }
            public Color[] recycleColor { get; set; }
        }
        public class ColourPickerData
        {
            public int width { get; set; }
            public int height { get; set; }
            public int horizontalOffset { get; set; }
            public int verticalOffset { get; set; }
            public String[] prevColours { get; set; }

            public static String[] defaultPrevColours = { "#ffcdff59", "#ff4ffff6", "#ffff728e", "#ff1d283f" };

            public String[,] colourData { get; set; }
            public SerializationInfo BaseUri { get; private set; }

            public String getColourHex(int colourProfile, int brightness) {

                return colourData[colourProfile, brightness];
            }


            public void setColourDataTemp() {
                colourData = new string[13, 11] {
                    { "000000", "330000", "660000","990000", "CC0000", "FF0000", "FF3333", "FF6666", "FF9999", "FFCCCC", "FFFFFF"},
                    { "000000", "331900", "663300","994C00", "CC6600", "FF8000", "FF9933", "FFB266", "FFCC99", "FFE5CC", "FFFFFF"},
                    { "000000", "333300", "666600","999900", "CCCC00", "FFFF00", "FFFF33", "FFFF66", "FFFF99", "FFFFCC", "FFFFFF"},
                    { "000000", "193300", "336600","4C9900", "66CC00", "80FF00", "99FF33", "B2FF66", "CCFF99", "E5FFCC", "FFFFFF"},
                    { "000000", "003300", "006600","009900", "00CC00", "00FF00", "33FF33", "66FF66", "99FF99", "CCFFCC", "FFFFFF"},
                    { "000000", "003319", "006633","00994C", "00CC66", "00FF80", "33FF99", "66FFB2", "99FFCC", "CCFFE5", "FFFFFF"},
                    { "000000", "003333", "006666","009999", "00CCCC", "00FFFF", "33FFFF", "66FFFF", "99FFFF", "CCFFFF", "FFFFFF"},
                    { "000000", "001933", "003366","004C99", "0066CC", "0080FF", "3399FF", "66B2FF", "99CCFF", "CCE5FF", "FFFFFF"},
                    { "000000", "000033", "000066","000099", "0000CC", "0000FF", "3333FF", "6666FF", "9999FF", "CCCCFF", "FFFFFF"},
                    { "000000", "190033", "330066","4C0099", "6600CC", "7F00FF", "9933FF", "B266FF", "CC99FF", "E5CCFF", "FFFFFF"},
                    { "000000", "330033", "660066","990099", "CC00CC", "FF00FF", "FF33FF", "FF66FF", "FF99FF", "FFCCFF", "FFFFFF"},
                    { "000000", "330019", "660033","99004C", "CC0066", "FF007F", "FF3399", "FF66B2", "FF99CC", "FFCCE5", "FFFFFF"},
                    { "000000", "000000", "202020","404040", "606060", "808080", "A0A0A0", "C0C0C0", "E0E0E0", "FFFFFF", "FFFFFF"}
                                    };
                }

                //public async System.Threading.Tasks.Task readColourData()
                //{
                //    colourData = new string[13, 11];

                //    StorageFolder storageFolder =
                //    ApplicationData.Current.LocalFolder;
                //    StorageFile sampleFile =
                //        await storageFolder.GetFileAsync(@"Assets\background.png");

                //    string text = await FileIO.ReadTextAsync(sampleFile);

                //    Debug.WriteLine(text);


                //    //var lines = "".Split("\n");

                //    //for (int i = 0; i < 13; i++)
                //    //{
                //    //    var fields = lines[i].Split(",");

                //    //    int j = 0;
                //    //    foreach (var field in fields)
                //    //    {
                //    //        colourData[i, j] = field;
                //    //        j++;
                //    //    }
                //    //}

                //    //for (int i = 0; i < 13; i++)
                //    //{
                //    //    for (int j = 0; j < 13; j++)
                //    //    {
                //    //        Debug.WriteLine("x: " + i + " y: " + j + " " + colourData[i, j]);
                //    //    }

                //    //}
                //}


                public String[] getDefaultPrevColours()
            {
                return defaultPrevColours;
            }

            public String getDefaultPrevColour(int i)
            {
                return defaultPrevColours[i];
            }

            public ColourPickerData getColourPickerData()
            {
                int w = 850;
                int h = 650;

                var cpd = new ColourPickerData()
                {
                    width = w,
                    height = h,
                    horizontalOffset = (int)(Window.Current.Bounds.Width - w) / 2,
                    verticalOffset = (int)(Window.Current.Bounds.Height - h) / 2,
                    prevColours = defaultPrevColours,

                };

                return cpd;
            }

            public void addColourToPrevColours(String c)
            {
                if (c.Equals(defaultPrevColours[0]))
                {
                    return;
                }

                for (int i = (defaultPrevColours.Length - 1); i > 0; i--)
                {
                    defaultPrevColours[i] = defaultPrevColours[i - 1];
                }
                defaultPrevColours[0] = c;
            }


        }


    }


}

