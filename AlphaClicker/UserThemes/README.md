# Template Theme for Auto Clicker

## Overview
This repository contains a template theme for the Auto Clicker application. This theme provides a base design that users can customize to create their own unique themes.

## Theme File
The main theme file is `TemplateTheme.xaml`, which includes a collection of `SolidColorBrush` definitions for various UI elements such as:
- Title color
- Background color
- Text color
- Button colors
- TextBox colors
- Circular selection colors

## How to Use
1. **Copy the Template**: 
   - To create your own theme, copy `TemplateTheme.xaml` and rename it to your desired theme name (e.g., `MyCustomTheme.xaml`).

2. **Modify the Colors**: 
   - Open the new theme file in a text editor or an IDE that supports XAML.
   - Change the color values of the `SolidColorBrush` elements to customize the appearance of the application.

3. **Load Your Theme**: 
   - Place your new theme file in the `UserThemes` directory of your application.
   - When the application starts, it will automatically detect and load your custom theme.

## Important Notes
- Make sure to maintain the structure of the `ResourceDictionary` for proper functionality.
- If you encounter any issues, verify that the color codes are valid and that there are no syntax errors in the XAML.

## License
This template is provided as-is. Feel free to modify and distribute as per your needs.
