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
    //Colour management system. Stores colour palette data and background data.
    //There are 13 different colour profiles, each with 10 brightnesses and 20 opacities.
    //Same system is used for all colour selections in program.
    //Eduardo Battistini
    public sealed partial class ColourManager : UserControl, INotifyPropertyChanged
    {

        private int BRIGHTNESS = 1;
        private int PROFILE = 0;
        private int OPACITY = 2;

        public int switchID = 0;

        public Color[] colourPalette = new Color[5];
        public String[] colourPaletteHex = new String[5];
        public int[,] colourPaletteData = new int[5,3];

        private int profileHolder;
        private int brightnessHolder;
        private int opacityHolder;

        public bool selectingBackground = false;
        public bool editingPalette = false;
        public int editID = 0;

        public Color backgroundSelection;
        private int bgProfile;
        private int bgBrightness;
        private int bgOpacity;
        public String backgroundHex;

        public Color selection { get; set; }

        public String selectionHex { get; set; }
        public int brightness { get; set; }
        public int opacity { get; set; }
        public int colourProfile { get; set; }
        public String colourName { get; set; }

        public static Color defaultColour { get; set; }

        private int DEFAULT_BRIGHTNESS = 5;

        ColourPickerData colourPickerData = new ColourPickerData();

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler updateColourSelection;
        public event EventHandler colourManagerClosed;
        public event EventHandler backgroundSelectionChanged;
        public event EventHandler paletteEdited;

        public ColourManager()
        {
            colourPickerData.setColourDataTemp();
            bgBrightness = 1;
            bgProfile = 12;
            bgOpacity = 100;

            backgroundSelection = hexToColor(getOpacityHex(bgOpacity) + colourPickerData.getColourHex(bgProfile, bgBrightness));
            brightness = 5;
            opacity = 100;
            colourProfile = 5;
            this.InitializeComponent();
            DataContext = colourPickerData.getColourPickerData();

            defaultColour = hexToColor(getOpacityHex(opacity) + colourPickerData.getColourHex(colourProfile, brightness));
            selection = defaultColour;
            selectionHex = selection.ToString();
            updateColourSelection?.Invoke(this, EventArgs.Empty);
            updateColourName();
        }

        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void updateColour(int profile, int brightness, int opacity)
        {
            if (selectingBackground)
            {
                backgroundSelection = hexToColor(getOpacityHex(opacity) + colourPickerData.getColourHex(profile, brightness));
                backgroundSelectionChanged?.Invoke(this, EventArgs.Empty);
                if (ColourPickerMenu.IsOpen) { ColourPickerMenu.IsOpen = false; }
                colourManagerClosed?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                selection = hexToColor(getOpacityHex(opacity) + colourPickerData.getColourHex(profile, brightness));
                selectionHex = selection.ToString();
                this.brightness = brightness;
                this.colourProfile = profile;
            }
            updateColourName();
            updateColourSelection?.Invoke(this, EventArgs.Empty);
            NotifyPropertyChanged();
        }

        public void close()
        {
            if (ColourPickerMenu.IsOpen) { ColourPickerMenu.IsOpen = false; }
        }

        public String getColourHex(int colourProfile, int brightness)
        {
            return colourPickerData.getColourHex(colourProfile, brightness);
        }

        public Color getColour(int profile, int brightness, int opacity)
        {
            return hexToColor(getOpacityHex(opacity) + colourPickerData.getColourHex(profile, brightness));
        }

        public Color getColour()
        {
            return selection;
        }

        public Color getBackgroundColour()
        {
            return backgroundSelection;
        }

        public void openMenu() {
            updateColourName();
            NotifyPropertyChanged();
            if (!ColourPickerMenu.IsOpen) { ColourPickerMenu.IsOpen = true; }
        }

        private void updateColourName() {
            colourName = ColorHelper.ToDisplayName(selection);
        }

        public static Color hexToColor(string hex)
        {
            if (hex.IndexOf('#') != -1)
                hex = hex.Replace("#", "");

            byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));

            return Color.FromArgb(a, r, g, b);
        }


        public String getOpacityHex(int opacity) {
            int newOpacity = (int)(opacity * 0.01 * 255);
            String newOpacityStr = Convert.ToString(newOpacity, 16);

            if (newOpacityStr.Length == 1)
            {
                newOpacityStr = newOpacityStr.Insert(0, "0");
            }

            return newOpacityStr;
        }

        private void updateSelection()
        {
            selection = getColour(colourProfile, brightness, opacity);
            updateColourName();
            NotifyPropertyChanged();
        }

        public void nextColour()
        {
            //used in autoswitch functionality
            switchID++;
            if (switchID == 5)
            {
                switchID = 0;

            }

            selection = colourPalette[switchID];
            colourProfile = colourPaletteData[switchID, PROFILE];
            brightness = colourPaletteData[switchID, BRIGHTNESS];
            opacity = colourPaletteData[switchID, OPACITY];


            updateColourSelection?.Invoke(this, EventArgs.Empty);
        }

        public void setBGColour(int p, int b, int o)
        {
            bgProfile = p;
            bgBrightness = b;
            bgOpacity = o;
            backgroundSelection = hexToColor(getOpacityHex(o) + colourPickerData.getColourHex(p, b));
            backgroundSelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public int[] getBGColour()
        {
            return new int[] { bgProfile, bgBrightness, bgOpacity };
        }

        public event EventHandler toggleAutoswitch;

        private void updateOpacity() {
            selection = hexToColor(getOpacityHex(opacity) + selection.ToString().Substring(3));
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
            if (opacity >= 100)
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

        private void selectColour(object sender, RoutedEventArgs e)
        {
            if (selectingBackground)
            {
                backgroundSelection = selection;
                bgProfile = colourProfile;
                bgBrightness = brightness;
                bgOpacity = opacity;
                resetSelection();
                backgroundSelectionChanged?.Invoke(this, EventArgs.Empty);
                selectingBackground = false;
            }
            else if (editingPalette)
            {
                colourPaletteData[editID, PROFILE] = colourProfile;
                colourPaletteData[editID, BRIGHTNESS] = brightness;
                colourPaletteData[editID, OPACITY] = opacity;
                colourPaletteHex[editID] = selection.ToString();
                colourPalette[editID] = selection;
                resetSelection();
                paletteEdited?.Invoke(this, EventArgs.Empty);    
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

        public void saveSelectionData() {
            profileHolder = colourProfile;
            brightnessHolder = brightness;
            opacityHolder = opacity;
        }

        private void resetSelection() {
            colourProfile = profileHolder;
            brightness = brightnessHolder;
            opacity = opacityHolder;
            selection = hexToColor(selectionHex);
        }

        public void loadColourData() {
            if (editingPalette) {
                selection = colourPalette[editID];
                colourProfile = colourPaletteData[editID, PROFILE];
                brightness = colourPaletteData[editID, BRIGHTNESS];
                opacity = colourPaletteData[editID, OPACITY];
            } else if (selectingBackground) {
                selection = backgroundSelection;
                colourProfile = bgProfile;
                brightness = bgBrightness;
                opacity = bgOpacity;
            }
        }

        public class ColourPickerData
        {
            public int width { get; set; }
            public int height { get; set; }
            public int horizontalOffset { get; set; }
            public int verticalOffset { get; set; }

            public String[,] colourData { get; set; }

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
                    

                };

                return cpd;
            }


        }


    }


}

